using System.Net;
using System.Net.Mail;
using CrmCustomerNotifier.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CrmCustomerNotifier.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendCustomerNotificationAsync(Customer customer)
    {
        string smtpHost = _configuration["MailSettings:SmtpHost"] ?? string.Empty;
        string smtpPortText = _configuration["MailSettings:SmtpPort"] ?? "587";
        string smtpUsername = _configuration["MailSettings:SmtpUsername"] ?? string.Empty;
        string smtpPassword = _configuration["MailSettings:SmtpPassword"] ?? string.Empty;
        string fromEmail = _configuration["MailSettings:FromEmail"] ?? string.Empty;
        string fromName = _configuration["MailSettings:FromName"] ?? "CRM System";

        if (string.IsNullOrWhiteSpace(customer.SalesPerson.Email))
        {
            _logger.LogWarning("Kunden saknar email till ansvarig säljare. Inget mail skickades.");
            return;
        }

        if (string.IsNullOrWhiteSpace(smtpHost) ||
            string.IsNullOrWhiteSpace(smtpUsername) ||
            string.IsNullOrWhiteSpace(smtpPassword) ||
            string.IsNullOrWhiteSpace(fromEmail))
        {
            _logger.LogWarning("Mailinställningar saknas i local.settings.json. Inget mail skickades.");
            return;
        }

        int smtpPort = int.Parse(smtpPortText);

        string subject = $"Ny eller uppdaterad kund: {customer.Name}";

        string body = $"""
        Hej {customer.SalesPerson.Name},

        Du har blivit ansvarig säljare för följande kund:

        Kunduppgifter:
        Id: {customer.Id}
        Namn: {customer.Name}
        Titel: {customer.Title}
        Telefon: {customer.Phone}
        Email: {customer.Email}
        Adress: {customer.Address}

        Ansvarig säljare:
        Namn: {customer.SalesPerson.Name}
        Telefon: {customer.SalesPerson.Phone}
        Email: {customer.SalesPerson.Email}

        Detta mail skickades automatiskt från CRM-systemet.
        """;

        using SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(smtpUsername, smtpPassword)
        };

        using MailMessage mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = body
        };

        mailMessage.To.Add(customer.SalesPerson.Email);

        await smtpClient.SendMailAsync(mailMessage);

        _logger.LogInformation("Email skickades till ansvarig säljare: {SalesPersonEmail}", customer.SalesPerson.Email);
    }
}