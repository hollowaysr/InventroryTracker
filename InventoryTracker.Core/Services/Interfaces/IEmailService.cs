using System.ComponentModel.DataAnnotations;

namespace InventoryTracker.Core.Services.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string subject, string body, byte[]? attachment = null, string? attachmentName = null);
    Task<bool> SendBulkEmailAsync(IEnumerable<string> toEmails, string subject, string body, byte[]? attachment = null, string? attachmentName = null);
}

public class EmailRequest
{
    [Required]
    [EmailAddress]
    public string ToEmail { get; set; } = string.Empty;
    
    [Required]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    public string Body { get; set; } = string.Empty;
    
    public byte[]? Attachment { get; set; }
    public string? AttachmentName { get; set; }
}
