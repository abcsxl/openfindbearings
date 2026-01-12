using NotificationApi.Models.Entities;

namespace NotificationApi.Models.DTOs
{
    public class SendNotificationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
        public string? NotificationId { get; set; }
        public DateTime? SentAt { get; set; }

        public SendNotificationResult() { }

        public SendNotificationResult(Services.SendResult sendResult, string notificationId = null)
        {
            Success = sendResult.Success;
            Message = sendResult.Message;
            Error = sendResult.Error;
            NotificationId = notificationId;
            SentAt = sendResult.SentAt;
        }
    }

    public class NotificationResponse
    {
        public long Id { get; set; }
        public string NotificationNumber { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public NotificationPriority Priority { get; set; }
        public NotificationStatus Status { get; set; }
        public string Recipient { get; set; } = string.Empty;
        public string? RecipientName { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? TemplateCode { get; set; }
        public int RetryCount { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class NotificationDetailResponse : NotificationResponse
    {
        public long? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public string? Metadata { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class TemplateResponse
    {
        public long Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public NotificationType NotificationType { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Variables { get; set; }
        public bool IsActive { get; set; }
        public int Version { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }

    public class ApiResponse<T>
    {
        public bool _Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> Success(T data, string? message = null)
        {
            return new ApiResponse<T> { _Success = true, Data = data, Message = message };
        }

        public static ApiResponse<T> Error(string message)
        {
            return new ApiResponse<T> { _Success = false, Message = message };
        }
    }
}
