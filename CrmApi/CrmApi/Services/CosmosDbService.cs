// CosmosDbService.cs
// Service-klassen som hanterar all kommunikation mellan vårt CRM API och Cosmos DB.

using CrmApi.Models;
using Microsoft.Azure.Cosmos;

namespace CrmApi.Services;

public class CosmosDbService
{
    private readonly Container _container;

    public CosmosDbService(IConfiguration configuration)
    {
        string accountEndpoint = configuration["CosmosDb:AccountEndpoint"]!;
        string accountKey = configuration["CosmosDb:AccountKey"]!;
        string databaseName = configuration["CosmosDb:DatabaseName"]!;
        string containerName = configuration["CosmosDb:ContainerName"]!;

        CosmosClient cosmosClient = new CosmosClient(accountEndpoint, accountKey);

        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    public async Task<Customer> AddCustomerAsync(Customer customer)
    {
        await _container.CreateItemAsync(customer, new PartitionKey(customer.id));

        return customer;
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        List<Customer> customers = new();

        QueryDefinition query = new QueryDefinition("SELECT * FROM c");

        FeedIterator<Customer> resultSet = _container.GetItemQueryIterator<Customer>(query);

        while (resultSet.HasMoreResults)
        {
            FeedResponse<Customer> response = await resultSet.ReadNextAsync();
            customers.AddRange(response);
        }

        return customers;
    }

    public async Task<Customer?> GetCustomerByIdAsync(string id)
    {
        try
        {
            ItemResponse<Customer> response = await _container.ReadItemAsync<Customer>(
                id,
                new PartitionKey(id)
            );

            return response.Resource;
        }
        catch (CosmosException exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Customer?> UpdateCustomerAsync(string id, Customer customer)
    {
        Customer? existingCustomer = await GetCustomerByIdAsync(id);

        if (existingCustomer == null)
        {
            return null;
        }

        customer.id = id;

        ItemResponse<Customer> response = await _container.ReplaceItemAsync(
            customer,
            id,
            new PartitionKey(id)
        );

        return response.Resource;
    }

    public async Task<bool> DeleteCustomerAsync(string id)
    {
        Customer? existingCustomer = await GetCustomerByIdAsync(id);

        if (existingCustomer == null)
        {
            return false;
        }

        await _container.DeleteItemAsync<Customer>(
            id,
            new PartitionKey(id)
        );

        return true;
    }

    public async Task<List<Customer>> SearchCustomersAsync(string? customerName, string? salesPersonName)
    {
        List<Customer> customers = new();

        string queryText = "SELECT * FROM c WHERE 1 = 1";

        if (!string.IsNullOrWhiteSpace(customerName))
        {
            queryText += " AND CONTAINS(LOWER(c.Name), LOWER(@customerName))";
        }

        if (!string.IsNullOrWhiteSpace(salesPersonName))
        {
            queryText += " AND CONTAINS(LOWER(c.SalesPerson.Name), LOWER(@salesPersonName))";
        }

        QueryDefinition query = new QueryDefinition(queryText);

        if (!string.IsNullOrWhiteSpace(customerName))
        {
            query.WithParameter("@customerName", customerName);
        }

        if (!string.IsNullOrWhiteSpace(salesPersonName))
        {
            query.WithParameter("@salesPersonName", salesPersonName);
        }

        FeedIterator<Customer> resultSet = _container.GetItemQueryIterator<Customer>(query);

        while (resultSet.HasMoreResults)
        {
            FeedResponse<Customer> response = await resultSet.ReadNextAsync();
            customers.AddRange(response);
        }

        return customers;
    }
}