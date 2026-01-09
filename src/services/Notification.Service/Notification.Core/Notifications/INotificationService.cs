namespace OpenFindBearings.Notification.Core.Notifications;

/// <summary>
/// 通知服务接口
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// 发送通知
    /// </summary>
    Task SendNotificationAsync(NotificationMessage notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送批量通知
    /// </summary>
    Task SendBulkNotificationsAsync(List<NotificationMessage> notifications, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送邮件
    /// </summary>
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送短信
    /// </summary>
    Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// 渲染通知模板
    /// </summary>
    Task<string> RenderTemplateAsync(NotificationTemplate template, Dictionary<string, object> data, CancellationToken cancellationToken = default);
}
