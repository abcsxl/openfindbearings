namespace NotificationApi.Models.Configuration
{
    public class NotificationConfig
    {
        public EmailConfig Email { get; set; } = new();
        public SmsConfig Sms { get; set; } = new();
        public int MaxRetryCount { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 5;
        public int BatchSize { get; set; } = 100;
    }

    public class EmailConfig
    {
        public bool Enabled { get; set; } = true;
        public string SmtpServer { get; set; } = "smtp.office365.com";
        public int Port { get; set; } = 587;
        public bool UseSsl { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromAddress { get; set; } = "noreply@example.com";
        public string FromName { get; set; } = "通知服务";
        public int Timeout { get; set; } = 30000;
    }

    public class SmsConfig
    {
        public bool Enabled { get; set; } = false;
        public string Provider { get; set; } = "Aliyun";
        public string AccessKeyId { get; set; } = string.Empty;
        public string AccessKeySecret { get; set; } = string.Empty;
        public string SignName { get; set; } = "通知服务";
        public int Timeout { get; set; } = 10000;
    }
}
