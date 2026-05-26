using System.Text.Json;
using CrmCustomerNotifier.Models;
using CrmCustomerNotifier.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CrmCustomerNotifier;

public class CustomerChangedFunction
{
    private readonly EmailService _emailService;
    private readonly ILogger<CustomerChangedFunction> _logger;

    public CustomerChangedFunction(
        EmailService emailService,
        ILogger<CustomerChangedFunction> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    [Function("CustomerChangedFunction")]
    public async Task Run(
        [CosmosDBTrigger(
            databaseName: "CrmDatabase",
            containerName: "Customers",
            Connection = "CosmosDbConnection",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true)]
        IReadOnlyList<JsonElement> customers)
    {
        if (customers == null || customers.Count == 0)
        {
            return;
        }

        JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        foreach (JsonElement customerDocument in customers)
        {
            string customerJson = customerDocument.GetRawText();

            _logger.LogInformation("En kund har skapats eller uppdaterats i Cosmos DB:");
            _logger.LogInformation(customerJson);

            Customer? customer = JsonSerializer.Deserialize<Customer>(customerJson, jsonOptions);

            if (customer == null)
            {
                _logger.LogWarning("Kunden kunde inte läsas från Cosmos DB-dokumentet.");
                continue;
            }

            await _emailService.SendCustomerNotificationAsync(customer);
        }
    }
}