using Common.Model;
using Common.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IRequestRepository _requestRepository;
        private readonly IResultRepository _resultRepository;

        public string UserName { get; set; }

        public IEnumerable<ProcessingSummary> RequestSummary { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IRequestRepository requestRepository, IResultRepository resultRepository)
        {
            _logger = logger;
            _requestRepository = requestRepository;
            _resultRepository = resultRepository;
        }

        public async Task<IActionResult> OnGet()
        {
          string userName = User.Identity.Name; // for readability in POC.

            var requests = await _requestRepository.ListByUser(userName);
            var results = await _resultRepository.ListByUser(userName);

            UserName = userName;

            RequestSummary = GenerateSummary(requests, results);

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            string userName = User.Identity.Name; // for readability in POC.

            await _requestRepository.DeleteByUser(userName);
            await _resultRepository.DeleteByUser(userName);

            return Redirect("/");
        }



        private IEnumerable<ProcessingSummary> GenerateSummary(IEnumerable<ProcessingRequest> processingRequests, IEnumerable<ProcessingResult> processingResults)
        {
            Dictionary<string, ProcessingSummary> result = new Dictionary<string, ProcessingSummary>();

            if (processingRequests != null)
            {
                foreach (var r in processingRequests)
                {
                    result.Add(r.Id, new ProcessingSummary()
                    {
                        Id = r.Id,
                        Type = r.Type,
                        Name = r.Name,
                        RequestedOn = r.RequestedOn,
                        Status = ProcessingStatus.Requested
                    });
                }
            }

            if (processingResults != null)
            {
                foreach (var r in processingResults)
                {
                    if (result.ContainsKey(r.Id))
                    {
                        result[r.Id].Status = ProcessingStatus.Completed;
                        result[r.Id].CompletedOn = r.CompletedOn;
                    }
                }
            }

            return result.Values.AsEnumerable();
        }
    }
}
