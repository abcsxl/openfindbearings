using MediatR;
using Microsoft.Extensions.Logging;
using OpenFindBearings.Notification.Core.Notifications;
using OpenFindBearings.Shared.Domain.Events;

namespace OpenFindBearings.Notification.Core.Events;

/// <summary>
/// 库存变更事件处理器
/// </summary>
public class StockChangedEventHandler : INotificationHandler<StockChangedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<StockChangedEventHandler> _logger;

    public StockChangedEventHandler(
        INotificationService notificationService,
        ILogger<StockChangedEventHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(StockChangedEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "处理库存变更事件: InventoryId={InventoryId}, OldQuantity={OldQuantity}, NewQuantity={NewQuantity}",
            @event.InventoryId,
            @event.OldQuantity,
            @event.NewQuantity);

        // 发送邮件通知给供应商
        var notification = new NotificationMessage
        {
            Email = @event.SupplierEmail ?? "supplier@example.com",
            Type = NotificationType.Email,
            Template = NotificationTemplate.StockChanged,
            Subject = "库存变更通知",
            Content = $"您的库存 {@event.InventoryId} 已从 {@event.OldQuantity} 变更为 {@event.NewQuantity}",
            TemplateData = new Dictionary<string, object>
            {
                { "InventoryId", @event.InventoryId },
                { "OldQuantity", @event.OldQuantity },
                { "NewQuantity", @event.NewQuantity },
                { "BearingModel", @event.BearingModel ?? "N/A" },
                { "Reason", @event.Reason ?? "系统操作" }
            }
        };

        await _notificationService.SendNotificationAsync(notification, cancellationToken);
    }
}
