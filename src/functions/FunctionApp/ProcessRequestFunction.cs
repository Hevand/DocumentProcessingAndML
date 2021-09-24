using Azure.AI.FormRecognizer;
using Common;
using Common.Model;
using Common.Repositories;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FunctionApp
{
    public class ProcessRequestFunction
    {
        IDocumentRepository _docRepository;
        IResultRepository _resultRepository;

        private readonly string _formRecognizerKey;
        private readonly string _formRecognizerUri;

        public ProcessRequestFunction()
        {
            string cosmosDbconnection = System.Environment.GetEnvironmentVariable("CosmosDB", EnvironmentVariableTarget.Process);
            this._resultRepository = new ResultRepository(new CosmosDBConnectionSettings() { ConnectionString = cosmosDbconnection });

            string storageAccountConnection = System.Environment.GetEnvironmentVariable("StorageAccount", EnvironmentVariableTarget.Process);
            this._docRepository = new DocumentRepository(new Azure.Storage.Blobs.BlobServiceClient(storageAccountConnection));

            this._formRecognizerKey = System.Environment.GetEnvironmentVariable("formrecognizerkey");
            this._formRecognizerUri = System.Environment.GetEnvironmentVariable("formrecognizeruri");
        }

        [FunctionName("ProcessUploadedDocuments")]
        public async Task Run([CosmosDBTrigger(
            databaseName: Constants.RequestsDatabase,
            collectionName: Constants.RequestsContainer,
            ConnectionStringSetting = "CosmosDb",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists =true)]IReadOnlyList<Document> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                foreach (var document in input)
                {
                    try
                    {
                        DateTime startTime = DateTime.UtcNow;
                        ProcessingRequest request = ParseRequest(document, log);

                        using (Stream binaryContent = await DownloadAttachment(request, log))
                        {
                            using (Stream ocrContent = await ConvertToText(request, binaryContent, log))
                            {
                                var attachmentAsText = await UploadDocument(request, $"{request.Id}.txt", ocrContent, log);

                                using (Stream modelResult = await Abstract(request, ocrContent, log))
                                {
                                    var attachmentAbstractionAsJson  = await UploadDocument(request, $"{request.Id}.json", modelResult, log);

                                    //Create result
                                    await CreateResult(request, startTime, new[] { attachmentAsText, attachmentAbstractionAsJson }, log);                                    
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.LogError(e, "Error in processing request, aborting operation.");
                    }
                }
            }
        }

        private static ProcessingRequest ParseRequest(Document i, ILogger log)
        {
            var request = JsonConvert.DeserializeObject<ProcessingRequest>(i.ToString());

            if (request == null)
            {
                string msg = $"Received document could not be deserialized as ProcessingRequest. Original input: {i}";
                log.LogError(msg);
                throw new InvalidCastException(msg);
            }

            log.LogInformation($"Received message for {request.Name} ({request.Id})");

            return request;
        }

        private async Task CreateResult(ProcessingRequest request, DateTime startTime, Common.Model.Attachment[] attachments, ILogger log)
        {
            ProcessingResult pr = new ProcessingResult()
            {
                Id = request.Id,
                Name = request.Name,
                User = request.User,
                StartedOn = startTime,
                CompletedOn = DateTime.UtcNow,
                Type = request.Type,
                Attachments = attachments
            };

            await this._resultRepository.Add(pr);

            log.LogInformation($"Updated result for {request.Name} ({request.Id})!");
        }

        private async Task<Common.Model.Attachment> UploadDocument(ProcessingRequest request, string fileName, Stream modelResult, ILogger log)
        {
            modelResult.Seek(0, SeekOrigin.Begin);
            
            var attachment =  await _docRepository.UploadFile(request.Id, fileName, modelResult);

            log.LogInformation($"Uploaded document for {request.Name} ({request.Id}) - name: {attachment.FileName}");

            return attachment;
        }

        private async Task<Stream> Abstract(ProcessingRequest request, Stream ocrContent, ILogger log)
        {
            //ocrContent.Seek(0, SeekOrigin.Begin);

            //string payload = null;

            //using (StreamReader sr = new StreamReader(ocrContent)) 
            //{
            //    payload = await sr.ReadToEndAsync();
            //}

            //ModelProcessingRequest modelRequest = new ModelProcessingRequest()
            //{
            //    Id = request.Id,
            //    Content = payload
            //};

            //invoke service
            string response = "[]";

            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            await sw.WriteAsync(response);
            sw.Flush();

            return ms;
        }

        private async Task<Stream> ConvertToText(ProcessingRequest request, Stream binaryContent, ILogger log)
        {
            binaryContent.Seek(0, SeekOrigin.Begin);

            FormRecognizerClient client = new FormRecognizerClient(new Uri(_formRecognizerUri), new Azure.AzureKeyCredential(_formRecognizerKey));

            Stopwatch sw = new Stopwatch();
            sw.Start();
            var result = await client.StartRecognizeContentAsync(binaryContent).WaitForCompletionAsync();
            sw.Stop();

            log.LogInformation($"Form recognizer completed processing for {request.Name} ({request.Id}) in {sw.Elapsed}");

            MemoryStream output = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(output);

            foreach (var form in result.Value)
            {
                log.LogInformation($"Form recognizer for {request.Name} ({request.Id}), page {form.PageNumber}. This page contains {form.Lines.Count} lines and {form.Tables.Count} tables");

                foreach (var line in form.Lines)
                {
                    await streamWriter.WriteLineAsync(line.Text);
                }

                await streamWriter.FlushAsync();
            }

            log.LogInformation($"Applied OCR for {request.Name} ({request.Id}) - {output.Length} bytes");

            return output;
        }

        private async Task<Stream> DownloadAttachment(ProcessingRequest request, ILogger log)
        {
            Stream binaryContent = null;
            if (request != null && request.Attachments != null && request.Attachments.Count > 0)
            {
                binaryContent = await _docRepository.DownloadFile(request.Attachments.First());
            }

            if (binaryContent == null)
            {
                string msg = $"File download failed for {request.Name} ({request.Id})";
                log.LogError(msg);
                throw new FileNotFoundException(msg);
            }                                                   
            log.LogInformation($"Downloaded file (1) for {request.Name} ({request.Id}) - {binaryContent.Length} bytes");

            return binaryContent;
        }
    }
}
