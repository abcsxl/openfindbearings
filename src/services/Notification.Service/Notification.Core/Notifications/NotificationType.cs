namespace OpenFindBearings.Notification.Core.Notifications;

/// <summary>
/// 通知类型
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// 邮件
    /// </summary>
    Email = 1,

    /// <summary>
    /// 短信
    /// </summary>
    Sms = 2,

    /// <summary>
    /// 推送通知
    /// </summary>
    Push = 3,

    /// <summary>
    /// 站内消息
    /// </summary>
    InApp = 4
}

/// <summary>
/// 通知模板
/// </summary>
public enum NotificationTemplate
{
    /// <summary>
    /// 库存变更通知
    /// </summary>
    StockChanged = 1,

    /// <summary>
    /// 询价创建通知
    /// </summary>
    InquiryCreated = 2,

    /// <summary>
    /// 报价创建通知
    /// </summary>
    QuotationCreated = 3,

    /// <summary>
    /// 匹配完成通知
    /// </summary>
    MatchCompleted = 4,

    /// <summary>
    /// 订单确认通知
    /// </summary>
    OrderConfirmed = 5,

    /// <summary>
    /// 欢迎邮件
    /// </summary>
    Welcome = 6
}
