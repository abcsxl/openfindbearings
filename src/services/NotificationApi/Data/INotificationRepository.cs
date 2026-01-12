using NotificationApi.Models.Entities;

namespace NotificationApi.Data
{
    public interface INotificationRepository
    {
        // Notification CRUD
        Task<Notification> AddNotificationAsync(Notification notification);
        Task<Notification?> GetNotificationByIdAsync(long id);
        Task<Notification?> GetNotificationByNumberAsync(string notificationNumber);
        Task<List<Notification>> GetNotificationsAsync(string? recipient = null, NotificationType? type = null,
            NotificationStatus? status = null, DateTime? startDate = null, DateTime? endDate = null,
            int skip = 0, int take = 20);
        Task<int> GetNotificationsCountAsync(string? recipient = null, NotificationType? type = null,
            NotificationStatus? status = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<Notification> UpdateNotificationAsync(Notification notification);
        Task<bool> DeleteNotificationAsync(long id);

        // 业务查询
        Task<List<Notification>> GetPendingNotificationsAsync(int limit = 100);
        Task<List<Notification>> GetFailedNotificationsAsync(int maxRetryCount = 3);
        Task<bool> UpdateNotificationStatusAsync(long notificationId, NotificationStatus status, string? errorMessage = null);

        // Template CRUD
        Task<NotificationTemplate> AddTemplateAsync(NotificationTemplate template);
        Task<NotificationTemplate?> GetTemplateByIdAsync(long id);
        Task<NotificationTemplate?> GetTemplateByCodeAsync(string code);
        Task<List<NotificationTemplate>> GetTemplatesAsync(bool? isActive = null);
        Task<NotificationTemplate> UpdateTemplateAsync(NotificationTemplate template);
        Task<bool> DeleteTemplateAsync(long id);
    }
}
