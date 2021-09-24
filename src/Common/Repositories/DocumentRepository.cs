using Azure.Storage.Blobs;
using Common.Model;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly BlobServiceClient _serviceClient;

        public DocumentRepository(BlobServiceClient serviceClient)
        {
            _serviceClient = serviceClient;
        }

        public async Task<Attachment> UploadFile(string engagementId, string originalFileName, Stream content)
        {
            var containerClient = _serviceClient.GetBlobContainerClient(engagementId);
            await containerClient.CreateIfNotExistsAsync();

            string fileName = originalFileName;
            var blobClient = containerClient.GetBlobClient(fileName);
            bool blobExists = await blobClient.ExistsAsync();

            for (int i = 0; i < 99 && blobExists; i++)
            {
                fileName = $"{Path.GetFileName(originalFileName)} ({i}).{Path.GetExtension(originalFileName)}";

                blobClient = containerClient.GetBlobClient(fileName);

                blobExists = await blobClient.ExistsAsync();
            }

            content.Seek(0, SeekOrigin.Begin);

            await blobClient.UploadAsync(content);

            return new Attachment()
            {
                ContainerName = engagementId,
                FileName = fileName,
                FileSize = (int)content.Length,
                Title = originalFileName,
                UploadedOn = DateTime.UtcNow,
                Order = 1
            };
        }


        public async Task<Stream> DownloadFile(Attachment attachment)
        {
            var containerClient = _serviceClient.GetBlobContainerClient(attachment.ContainerName);
            var blobClient = containerClient.GetBlobClient(attachment.FileName);

            MemoryStream ms = new MemoryStream();

            await blobClient.DownloadToAsync(ms);

            return ms;
        }
    }
}
