using Common.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public class ResultRepository : BaseRepository, IResultRepository
    {

        public ResultRepository(CosmosDBConnectionSettings settings) : base(settings, Constants.ResultsDatabase, Constants.ResultsContainer)
        {

        }


        public async Task<IEnumerable<ProcessingResult>> ListByUser(string userName)
        {
            var query = Container.GetItemQueryIterator<ProcessingResult>($"select * from c where c.userName = \"{userName}\"");

            List<ProcessingResult> result = new List<ProcessingResult>();

            while (query.HasMoreResults)
            {
                var resp = await query.ReadNextAsync();

                foreach (var item in resp.Resource)
                {
                    if (item != null)
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        public async Task Add(ProcessingResult result)
        {
            await base.CreateIfNotExists(result, result.User);
        }

        public async Task DeleteByUser(string userName)
        {
            var items = await ListByUser(userName);

            foreach (var i in items)
            {
                await Container.DeleteItemAsync<ProcessingResult>(i.Id, new Microsoft.Azure.Cosmos.PartitionKey(i.User));
            }
        }
    }
}
