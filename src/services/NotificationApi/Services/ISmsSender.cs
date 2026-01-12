namespace NotificationApi.Services
{
    public interface ISmsSender
    {
        Task<SendResult> SendSmsAsync(string to, string message);
        Task<SendResult> SendSmsAsync(string to, string templateCode, Dictionary<string, string> templateData);
        Task<List<SendResult>> SendBulkSmsAsync(List<SmsMessage> messages);
        Task<bool> ValidatePhoneNumberAsync(string phoneNumber);
    }

    public class SmsMessage
    {
        public string To { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? TemplateCode { get; set; }
        public Dictionary<string, string>? TemplateData { get; set; }
    }
}
