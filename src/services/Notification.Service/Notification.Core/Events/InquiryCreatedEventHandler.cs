using MediatR;
using Microsoft.Extensions.Logging;
using OpenFindBearings.Notification.Core.Notifications;
using OpenFindBearings.Shared.Domain.Events;

namespace OpenFindBearings.Notification.Core.Events;

/// <summary>
/// 询价创建事件处理器
/// </summary>
public class InquiryCreatedEventHandler : INotificationHandler<InquiryCreatedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<InquiryCreatedEventHandler> _logger;

    public InquiryCreatedEventHandler(
        INotificationService notificationService,
        ILogger<InquiryCreatedEventHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(InquiryCreatedEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "处理询价创建事件: InquiryId={InquiryId}, UserId={UserId}",
            @event.InquiryId,
            @event.UserId);

        // 通知相关供应商（这里简化为发送通知给管理员）
        var notification = new NotificationMessage
        {
            Email = "admin@openfindbearings.com",
            Type = NotificationType.Email,
            Template = NotificationTemplate.InquiryCreated,
            Subject = "新询价创建通知",
            Content = $"用户 {@event.UserId} 创建了新的询价请求: {@event.BearingModel}",
            Priority = NotificationPriority.Normal,
            TemplateData = new Dictionary<string, object>
            {
                { "InquiryId", @event.InquiryId },
                { "UserId", @event.UserId },
                { "BearingModel", @event.BearingModel },
                { "Quantity", @event.Quantity }
            }
        };

        await _notificationService.SendNotificationAsync(notification, cancellationToken);
    }
}

/// <summary>
/// 报价创建事件处理器
/// </summary>
public class QuotationCreatedEventHandler : INotificationHandler<QuotationCreatedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<QuotationCreatedEventHandler> _logger;

    public QuotationCreatedEventHandler(
        INotificationService notificationService,
        ILogger<QuotationCreatedEventHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(QuotationCreatedEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "处理报价创建事件: QuotationId={QuotationId}, InquiryId={InquiryId}",
            @event.QuotationId,
            @event.InquiryId);

        // 通知询价用户有新报价
        var notification = new NotificationMessage
        {
            Email = @event.UserEmail ?? "user@example.com",
            Type = NotificationType.Email,
            Template = NotificationTemplate.QuotationCreated,
            Subject = "您有新的报价",
            Content = $"供应商 {@event.SupplierName} 为您的询价 {@event.InquiryId} 提供了报价",
            Priority = NotificationPriority.High,
            TemplateData = new Dictionary<string, object>
            {
                { "QuotationId", @event.QuotationId },
                { "InquiryId", @event.InquiryId },
                { "SupplierName", @event.SupplierName },
                { "Price", @event.Price },
                { "DeliveryDays", @event.DeliveryDays }
            }
        };

        await _notificationService.SendNotificationAsync(notification, cancellationToken);
    }
}

/// <summary>
/// 匹配完成事件处理器
/// </summary>
public class MatchCompletedEventHandler : INotificationHandler<MatchCompletedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<MatchCompletedEventHandler> _logger;

    public MatchCompletedEventHandler(
        INotificationService notificationService,
        ILogger<MatchCompletedEventHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(MatchCompletedEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "处理匹配完成事件: InquiryId={InquiryId}, MatchCount={MatchCount}",
            @event.InquiryId,
            @event.MatchCount);

        // 通知用户匹配完成
        var notification = new NotificationMessage
        {
            Email = @event.UserEmail ?? "user@example.com",
            Type = NotificationType.Email,
            Template = NotificationTemplate.MatchCompleted,
            Subject = "询价匹配完成",
            Content = $"您的询价 {@event.InquiryId} 已完成匹配，找到 {@event.MatchCount} 个合适的供应商",
            Priority = NotificationPriority.High,
            TemplateData = new Dictionary<string, object>
            {
                { "InquiryId", @event.InquiryId },
                { "MatchCount", @event.MatchCount },
                { "BestPrice", @event.BestPrice },
                { "BestDeliveryDays", @event.BestDeliveryDays }
            }
        };

        await _notificationService.SendNotificationAsync(notification, cancellationToken);
    }
}
