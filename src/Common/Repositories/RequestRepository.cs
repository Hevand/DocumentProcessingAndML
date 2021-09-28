using Common.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public class RequestRepository : BaseRepository, IRequestRepository
    {
        public RequestRepository(CosmosDBConnectionSettings settings) : base(settings, Constants.RequestsDatabase, Constants.RequestsContainer)
        {

        }

        public async Task<IEnumerable<ProcessingRequest>> ListByUser(string userName)
        {
            var query = Container.GetItemQueryIterator<ProcessingRequest>($"select * from c where c.userName = \"{userName}\"");

            List<ProcessingRequest> result = new List<ProcessingRequest>();

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

        public async Task Add(ProcessingRequest request)
        {
            await base.CreateIfNotExists(request, request.User);
        }

        public async Task DeleteByUser(string userName)
        {
            var items = await ListByUser(userName);

            foreach (var i in items)
            {
                await Container.DeleteItemAsync<ProcessingRequest>(i.Id, new Microsoft.Azure.Cosmos.PartitionKey(i.User));
            }
        }

        public async Task<ProcessingRequest> ListByUserAndId(string id, string name)
        {
            return await Container.ReadItemAsync<ProcessingRequest>(id, new Microsoft.Azure.Cosmos.PartitionKey(name)); 
        }
    }
}
