using Common.Model;
using Common.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Web.Pages
{
    public class AddDocumentModel : PageModel
    {
        private ILogger<IndexModel> _logger;
        private IRequestRepository _requestRepository;
        private IDocumentRepository _documentRepository;

        public AddDocumentModel(ILogger<IndexModel> logger, IRequestRepository requestRepository, IDocumentRepository documentRepository)
        {
            _logger = logger;
            _requestRepository = requestRepository;
            _documentRepository = documentRepository;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostSubmit(AddDocumentModelRequest formData)
        {
            //Validate

            ProcessingRequest processingRequest = new ProcessingRequest()
            {
                Id = Guid.NewGuid().ToString(),
                User = User.Identity.Name,
                Name = formData.Name,
                RequestedOn = DateTime.UtcNow,
                Type = formData.DocumentType == "Debt"
                    ? DocumentType.Debt
                    : DocumentType.Lease
            };

            int i = 0;
            foreach(var file in Request.Form.Files)
            {
                //Upload 
                MemoryStream ms = new MemoryStream();                
                file.CopyTo(ms);

                var attachment = await _documentRepository.UploadFile(
                    processingRequest.Id,
                    file.FileName,
                    ms
                    );

                //sequence
                attachment.Order = i++;

                //Add
                processingRequest.Attachments.Add(attachment);
            }

            await _requestRepository.Add(processingRequest);

            return Redirect("/");
        }
    }

    public class AddDocumentModelRequest
    {
        [BindProperty]
        public string Name { get; set; }        
        [BindProperty]
        public string DocumentType { get; set; }
    }
}
