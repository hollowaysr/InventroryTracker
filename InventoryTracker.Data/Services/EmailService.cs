using InventoryTracker.Core.Services.Interfaces;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InventoryTracker.Data.Services;

public class EmailService : IEmailService
{
    private readonly EmailClient _emailClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _fromEmail;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        var connectionString = _configuration.GetConnectionString("AzureCommunicationServices");
        _fromEmail = _configuration["Email:FromAddress"] ?? "noreply@inventorytracker.com";
        
        if (!string.IsNullOrEmpty(connectionString))
        {
            _emailClient = new EmailClient(connectionString);
        }
        else
        {
            _logger.LogWarning("Azure Communication Services connection string not found. Email functionality will be disabled.");
        }
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, byte[]? attachment = null, string? attachmentName = null)
    {
        try
        {
            if (_emailClient == null)
            {
                _logger.LogWarning("Email client not configured. Cannot send email to {ToEmail}", toEmail);
                return false;
            }

            var emailMessage = new EmailMessage(
                senderAddress: _fromEmail,
                content: new EmailContent(subject)
                {
                    PlainText = body,
                    Html = $"<html><body><pre>{body}</pre></body></html>"
                },
                recipients: new EmailRecipients(new List<EmailAddress> { new(toEmail) }));

            if (attachment != null && !string.IsNullOrEmpty(attachmentName))
            {
                emailMessage.Attachments.Add(new EmailAttachment(attachmentName, "application/octet-stream", new BinaryData(attachment)));
            }

            var operation = await _emailClient.SendAsync(Azure.WaitUntil.Completed, emailMessage);
            
            _logger.LogInformation("Email sent successfully to {ToEmail} with subject {Subject}", toEmail, subject);
            return operation.HasCompleted && !operation.HasValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail} with subject {Subject}", toEmail, subject);
            return false;
        }
    }

    public async Task<bool> SendBulkEmailAsync(IEnumerable<string> toEmails, string subject, string body, byte[]? attachment = null, string? attachmentName = null)
    {
        var tasks = toEmails.Select(email => SendEmailAsync(email, subject, body, attachment, attachmentName));
        var results = await Task.WhenAll(tasks);
        
        var successCount = results.Count(r => r);
        var totalCount = results.Length;
        
        _logger.LogInformation("Bulk email sent: {SuccessCount}/{TotalCount} emails delivered successfully", successCount, totalCount);
        
        return successCount > 0;
    }
}
