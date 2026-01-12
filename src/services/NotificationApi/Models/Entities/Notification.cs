using System;

namespace NotificationApi.Models.Entities
{
    public class Notification
    {
        public long Id { get; set; }
        public string NotificationNumber { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
        public string Recipient { get; set; } = string.Empty;
        public string? RecipientName { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? TemplateCode { get; set; }
        public long? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public string? Metadata { get; set; }
        public int RetryCount { get; set; } = 0;
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? ScheduledAt { get; set; }
    }
}
