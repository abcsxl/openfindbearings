namespace NotificationApi.Models.Entities
{
    public enum NotificationType
    {
        Email = 1,
        Sms = 2,
        InApp = 3,
        Push = 4
    }

    public enum NotificationStatus
    {
        Pending = 1,
        Sent = 2,
        Failed = 3,
        Delivered = 4,
        Read = 5
    }

    public enum NotificationPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Urgent = 4
    }
}
