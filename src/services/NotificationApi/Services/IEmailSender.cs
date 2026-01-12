namespace NotificationApi.Services
{
    public interface IEmailSender
    {
        Task<SendResult> SendEmailAsync(string to, string? toName, string subject, string body, List<string>? attachments = null);
        Task<SendResult> SendEmailAsync(string to, string subject, string body);
        Task<List<SendResult>> SendBulkEmailAsync(List<EmailMessage> messages);
        Task<bool> ValidateEmailAsync(string email);
    }

    public class EmailMessage
    {
        public string To { get; set; } = string.Empty;
        public string? ToName { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; } = true;
        public List<EmailAttachment>? Attachments { get; set; }
    }

    public class EmailAttachment
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }

    public class SendResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public string? MessageId { get; set; }
    }
}
