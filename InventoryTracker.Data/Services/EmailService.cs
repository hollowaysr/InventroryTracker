using InventoryTracker.Core.Services.Interfaces;

namespace InventoryTracker.Data.Services
{
    public class EmailService : IEmailService
    {        public Task<bool> SendEmailAsync(string to, string subject, string body, byte[]? attachment = null, string? attachmentName = null)
        {
            // Stub implementation - always returns true
            // In a real implementation, this would send email via SMTP or email service
            return Task.FromResult(true);
        }

        public Task<bool> SendBulkEmailAsync(IEnumerable<string> toEmails, string subject, string body, byte[]? attachment = null, string? attachmentName = null)
        {
            // Stub implementation - always returns true
            // In a real implementation, this would send bulk emails via SMTP or email service
            return Task.FromResult(true);
        }
    }
}