using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using ContactBookLibrary.Models.Abstract;
using ContactBookLibrary.Models.Entities;

namespace ContactBookLibrary.Models.Concrete
{
  public class ContactRepository : IContactRepository
  {
    private Database _database;
    private Container _container;
    private CosmosClient _cosmosClient;
    public ContactRepository(IOptions<CosmosUtility> cosmosUtility)
    {
      _cosmosClient = new CosmosClient(cosmosUtility.Value.CosmosEndpoint, cosmosUtility.Value.CosmosKey);
      _database = _cosmosClient.GetDatabase(cosmosUtility.Value.DatabaseId);
      _container = _database.GetContainer(cosmosUtility.Value.ContainerId);
    }

    private async Task<List<Contact>> GetContacts(string sqlQuery)
    {
      QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
      FeedIterator<Contact> queryResultIterator = _container.GetItemQueryIterator<Contact>(queryDefinition);
      List<Contact> contactList = new List<Contact>();
      while (queryResultIterator.HasMoreResults)
      {
        FeedResponse<Contact> currentResultSet = await queryResultIterator.ReadNextAsync();
        foreach (var item in currentResultSet)
        {
          contactList.Add(item);
        }
        return contactList;
      }
      return null;
    }

    public async Task<Contact> CreateAsync(Contact contact)
    {
      contact.Id = Guid.NewGuid().ToString();
      ItemResponse<Contact> contactResponse = await _container.CreateItemAsync<Contact>(contact);
      if (contactResponse != null)
      {
        return contact;
      }
      return null;
    }

    public async Task<bool> DeleteAsync(string id, string contactName, string phone)
    {
      ItemResponse<Contact> contactResponse = await _container.DeleteItemAsync<Contact>(id, new PartitionKey(contactName));
      if (contactResponse != null) return true;
      return false;
    }

    public async Task<Contact> FindContactAsync(string id)
    {
      var sqlQuery = $"Select * from c where c.id='{id}'";
      var contactList = await GetContacts(sqlQuery);
      return contactList[0];
    }

    public async Task<List<Contact>> FindContactByPhoneAsync(string phone)
    {
      var sqlQuery = $"Select * from c where c.phone='{phone}'";
      var contactList = await GetContacts(sqlQuery);
      return contactList;
    }

    public async Task<List<Contact>> FindContactByContactNamePhoneAsync(string contactName, string phone)
    {
      var sqlQuery = $"Select * from c where c.contactName='{contactName}' and c.phone='{phone}'";
      var contactList = await GetContacts(sqlQuery);
      return contactList;
    }

    public async Task<List<Contact>> FindContactsByContactNameAsync(string contactName)
    {
      var sqlQuery = $"Select * from c where c.contactName='{contactName}'";
      var contactList = await GetContacts(sqlQuery);
      return contactList;
    }

    public async Task<List<Contact>> GetAllContactsAsync()
    {
      var sqlQuery = $"Select * from c";
      var contactList = await GetContacts(sqlQuery);
      return contactList;
    }

    public async Task<Contact> UpdateAsync(Contact contact)
    {
      ItemResponse<Contact> contactRespone = await _container.ReadItemAsync<Contact>(contact.Id, new PartitionKey(contact.ContactName));

      var contactResult = contactRespone.Resource;

      contactResult.Id = contact.Id;
      contactResult.ContactName = contact.ContactName;
      contactResult.Phone = contact.Phone;
      contactResult.ContactType = contact.ContactType;
      contactResult.Email = contact.Email;

      contactRespone = await _container.ReplaceItemAsync<Contact>(contactResult, contactResult.Id);

      if (contactRespone.Resource != null)
      {
        return contactRespone;
      }
      return null;
    }
  }
}