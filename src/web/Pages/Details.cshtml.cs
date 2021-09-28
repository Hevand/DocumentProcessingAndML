using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Model;
using Common.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Web.Pages
{
    public class DetailsModel : PageModel
    {
        private ILogger<IndexModel> _logger;
        private IRequestRepository _requestRepository;
        private IResultRepository _resultRepository;
        private IDocumentRepository _documentRepository;

        public ICollection<Hyperlink> OriginalDocuments = new List<Hyperlink>();

        public ICollection<Hyperlink> OCRDocuments = new List<Hyperlink>();

        public ICollection<Hyperlink> JsonDocuments = new List<Hyperlink>();

        public DetailsModel(ILogger<IndexModel> logger, IRequestRepository requestRepository, IResultRepository resultRepository, IDocumentRepository documentRepository)
        {
            _logger = logger;
            _requestRepository = requestRepository;
            _resultRepository = resultRepository;
            _documentRepository = documentRepository;
        }


        public async Task<IActionResult> OnGet(string id)
        {
            ProcessingRequest request = await _requestRepository.ListByUserAndId(id, User.Identity.Name);

            ProcessingResult result = await _resultRepository.ListbyUserAndId(id, 
                User.Identity.Name);

            OriginalDocuments = GenerateSASTokens(request.Attachments);

            OCRDocuments = GenerateSASTokens(result.Attachments.Where(a => a.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)));

            JsonDocuments = GenerateSASTokens(result.Attachments.Where(a => a.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)));

            return Page();
        }

        private ICollection<Hyperlink> GenerateSASTokens(IEnumerable<Attachment> attachments)
        {
            List<Hyperlink> result = new List<Hyperlink>();

            foreach( var a in attachments)
            {
                result.Add(new Hyperlink()
                {
                    Link = a.FileName,
                    Uri = _documentRepository.GetSASUri(a)
                });
            }

            return result;
        }
    }

    public class Hyperlink
    {
        public string Link { get; set; }
        public Uri Uri { get; set; }
    }
}
