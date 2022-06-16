using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace ContactBookLibrary.Models
{
  public class CosmosDbContext
  {
    private readonly string _cosmosKey;
    private readonly string _cosmosEndpoint;
    private readonly string _databaseId;
    private readonly string _containerId;
    private Database _database;
    private Container _container;
    private CosmosClient _cosmosClient;

    public CosmosDbContext(IOptions<CosmosUtility> cosmosUtility)
    {
      _cosmosKey = cosmosUtility.Value.CosmosKey;
      _cosmosEndpoint = cosmosUtility.Value.CosmosEndpoint;
      _databaseId = cosmosUtility.Value.DatabaseId;
      _containerId = cosmosUtility.Value.ContainerId;

      _cosmosClient = new CosmosClient(_cosmosEndpoint, _cosmosKey);
    }

    public async Task<bool> CreateDatabaseIfNotExists()
    {
      DatabaseResponse databaseResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseId);
      //if (databaseResponse.StatusCode == System.Net.HttpStatusCode.Created)
      if (databaseResponse.Database != null)
      {
        _database = _cosmosClient.GetDatabase(_databaseId);
        return true;
      }
      return false;
    }
    public async Task<bool> CreateContainerIfNotExists()
    {
      ContainerResponse containerResponse = await _database.CreateContainerIfNotExistsAsync(_containerId, "/contactName");

      //if (containerResponse.StatusCode == System.Net.HttpStatusCode.Created)
      if (containerResponse.Container != null)
      {
        _container = _database.GetContainer(_containerId);
        return true;
      }
      return false;
    }

  }
}