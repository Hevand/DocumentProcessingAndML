using Common.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public interface IResultRepository
    {
        Task Add(ProcessingResult result);
        Task<IEnumerable<ProcessingResult>> ListByUser(string userName);
        Task DeleteByUser(string userName);
    }
}