using NotificationApi.Models.DTOs;

namespace NotificationApi.Services
{
    public interface INotificationService
    {
        // 核心通知功能
        Task<SendNotificationResult> SendNotificationAsync(SendNotificationRequest request);
        Task<SendNotificationResult> SendNotificationByTemplateAsync(string templateCode, Dictionary<string, object> templateData, string recipient);
        Task<List<SendNotificationResult>> SendBulkNotificationsAsync(List<SendNotificationRequest> requests);

        // 通知管理
        Task<NotificationDetailResponse?> GetNotificationAsync(long id);
        Task<PagedResponse<NotificationResponse>> GetNotificationsAsync(NotificationQuery query);
        Task<bool> MarkAsReadAsync(long notificationId);
        Task<bool> DeleteNotificationAsync(long id);

        // 模板管理
        Task<TemplateResponse?> GetTemplateAsync(string code);
        Task<string> RenderTemplateAsync(string template, Dictionary<string, object> data);

        // 业务事件处理
        Task HandleQuotationCreatedAsync(dynamic eventData);
        Task HandleOrderCreatedAsync(dynamic eventData);
        Task HandleOrderStatusChangedAsync(dynamic eventData);
        Task HandlePaymentProcessedAsync(dynamic eventData);

        // 运维功能
        Task<List<SendNotificationResult>> ProcessPendingNotificationsAsync(int batchSize = 100);
        Task<List<SendNotificationResult>> RetryFailedNotificationsAsync(int maxRetryCount = 3);
        Task<NotificationStatistics> GetStatisticsAsync();
    }
}
