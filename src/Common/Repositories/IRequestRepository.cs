using Common.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public interface IRequestRepository
    {
        Task Add(ProcessingRequest request);
        Task<IEnumerable<ProcessingRequest>> ListByUser(string userName);
        Task DeleteByUser(string userName);
    }
}