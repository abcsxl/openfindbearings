using NotificationApi.Models.Entities;

namespace NotificationApi.Models.DTOs
{
    public class SendNotificationRequest
    {
        public NotificationType Type { get; set; }
        public string Recipient { get; set; } = string.Empty;
        public string? RecipientName { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? TemplateCode { get; set; }
        public Dictionary<string, object>? TemplateData { get; set; }
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        public long? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public DateTime? ScheduledAt { get; set; }
    }

    public class CreateTemplateRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public NotificationType NotificationType { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Variables { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class NotificationQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; }
        public NotificationType? Type { get; set; }
        public NotificationStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int Skip => (PageNumber - 1) * PageSize;
    }

    public class SendNotificationByTemplateRequest
    {
        public string TemplateCode { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty;
        public Dictionary<string, object> TemplateData { get; set; } = new();
    }

    public class TestTemplateRequest
    {
        public string TestRecipient { get; set; } = string.Empty;
        public Dictionary<string, object> TemplateData { get; set; } = new();
    }
    public class NotificationStatistics
    {
        public int TotalCount { get; set; }
        public int SentCount { get; set; }
        public int FailedCount { get; set; }
        public int PendingCount { get; set; }
        public double SuccessRate { get; set; }
        public int TodayCount { get; set; }
        public int ThisWeekCount { get; set; }
        public int ThisMonthCount { get; set; }
        public Dictionary<string, int> TypeDistribution { get; set; } = new();
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
    }

    public class UpdateTemplateRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Subject { get; set; }
        public string? Content { get; set; }
        public string? Variables { get; set; }
        public bool? IsActive { get; set; }
    }
}
