namespace OpenFindBearings.Notification.Core.Notifications;

/// <summary>
/// 通知消息
/// </summary>
public class NotificationMessage
{
    /// <summary>
    /// 接收人邮箱
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// 接收人手机号
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 用户 ID
    /// </summary>
    public long? UserId { get; set; }

    /// <summary>
    /// 通知类型
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// 通知模板
    /// </summary>
    public NotificationTemplate Template { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public required string Subject { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// 模板数据
    /// </summary>
    public Dictionary<string, object>? TemplateData { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 通知优先级
/// </summary>
public enum NotificationPriority
{
    /// <summary>
    /// 低
    /// </summary>
    Low = 0,

    /// <summary>
    /// 普通
    /// </summary>
    Normal = 1,

    /// <summary>
    /// 高
    /// </summary>
    High = 2,

    /// <summary>
    /// 紧急
    /// </summary>
    Urgent = 3
}
