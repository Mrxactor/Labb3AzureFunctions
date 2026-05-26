// Program.cs
// Main startup file for the CRM Minimal API. This file defines endpoints for managing customers in Cosmos DB.

using CrmApi.Models;
using CrmApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<CosmosDbService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/customers", async (Customer customer, CosmosDbService cosmosDbService) =>
{
    if (string.IsNullOrWhiteSpace(customer.Name))
    {
        return Results.BadRequest("Customer name is required.");
    }

    if (customer.SalesPerson == null || string.IsNullOrWhiteSpace(customer.SalesPerson.Email))
    {
        return Results.BadRequest("The customer must have a responsible salesperson with an email address.");
    }

    Customer createdCustomer = await cosmosDbService.AddCustomerAsync(customer);

    return Results.Created($"/customers/{createdCustomer.id}", createdCustomer);
});

app.MapGet("/customers", async (CosmosDbService cosmosDbService) =>
{
    List<Customer> customers = await cosmosDbService.GetAllCustomersAsync();

    return Results.Ok(customers);
});

app.MapGet("/customers/{id}", async (string id, CosmosDbService cosmosDbService) =>
{
    Customer? customer = await cosmosDbService.GetCustomerByIdAsync(id);

    if (customer == null)
    {
        return Results.NotFound("Customer was not found.");
    }

    return Results.Ok(customer);
});

app.MapPut("/customers/{id}", async (string id, Customer customer, CosmosDbService cosmosDbService) =>
{
    Customer? updatedCustomer = await cosmosDbService.UpdateCustomerAsync(id, customer);

    if (updatedCustomer == null)
    {
        return Results.NotFound("Customer was not found and could not be updated.");
    }

    return Results.Ok(updatedCustomer);
});

app.MapDelete("/customers/{id}", async (string id, CosmosDbService cosmosDbService) =>
{
    bool deleted = await cosmosDbService.DeleteCustomerAsync(id);

    if (!deleted)
    {
        return Results.NotFound("Customer was not found and could not be deleted.");
    }

    return Results.Ok("Customer has been deleted.");
});

app.MapGet("/customers/search", async (string? customerName, string? salesPersonName, CosmosDbService cosmosDbService) =>
{
    List<Customer> customers = await cosmosDbService.SearchCustomersAsync(customerName, salesPersonName);

    if (customers.Count == 0)
    {
        return Results.NotFound("No customers were found.");
    }

    return Results.Ok(customers);
});

app.Run();