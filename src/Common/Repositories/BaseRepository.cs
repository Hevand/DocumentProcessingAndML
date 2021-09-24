using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public abstract class BaseRepository
    {
        protected string EndpointUri;
        protected string PrimaryKey;

        protected CosmosClient cosmosClient;
        protected Database Database;
        protected Container Container;

        public BaseRepository(CosmosDBConnectionSettings settings, string databaseId, string containerId) : base()
        {
            this.cosmosClient = new CosmosClient(settings.ConnectionString);

            this.Database = this.cosmosClient.GetDatabase(databaseId);
            this.Container = this.Database.GetContainer(containerId);
        }

        public async Task CreateIfNotExists<T>(T item, string partitionKeyValue)
        {
            var response = await Container.UpsertItemAsync<T>(
                item,
                new PartitionKey(partitionKeyValue));
        }
    }
}
