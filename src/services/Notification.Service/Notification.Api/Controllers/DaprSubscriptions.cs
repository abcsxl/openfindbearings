using Dapr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenFindBearings.Shared.Domain.Events;

namespace OpenFindBearings.Notification.Api.Controllers;

/// <summary>
/// Dapr 事件订阅控制器
/// 处理来自 Dapr Pub/Sub 的事件
/// </summary>
[ApiController]
public class DaprSubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DaprSubscriptionsController> _logger;

    public DaprSubscriptionsController(IMediator mediator, ILogger<DaprSubscriptionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// 订阅库存变更事件
    /// Topic: stock-changed
    /// </summary>
    [Topic("pubsub", "stock-changed")]
    [HttpPost("stock-changed")]
    public async Task<IActionResult> HandleStockChanged([FromBody] StockChangedEvent @event)
    {
        _logger.LogInformation(
            "收到库存变更事件: InventoryId={InventoryId}, OldQuantity={OldQuantity}, NewQuantity={NewQuantity}",
            @event.InventoryId,
            @event.OldQuantity,
            @event.NewQuantity);

        await _mediator.Publish(@event);
        return Ok();
    }

    /// <summary>
    /// 订阅询价创建事件
    /// Topic: inquiry-created
    /// </summary>
    [Topic("pubsub", "inquiry-created")]
    [HttpPost("inquiry-created")]
    public async Task<IActionResult> HandleInquiryCreated([FromBody] InquiryCreatedEvent @event)
    {
        _logger.LogInformation(
            "收到询价创建事件: InquiryId={InquiryId}, UserId={UserId}",
            @event.InquiryId,
            @event.UserId);

        await _mediator.Publish(@event);
        return Ok();
    }

    /// <summary>
    /// 订阅报价创建事件
    /// Topic: quotation-created
    /// </summary>
    [Topic("pubsub", "quotation-created")]
    [HttpPost("quotation-created")]
    public async Task<IActionResult> HandleQuotationCreated([FromBody] QuotationCreatedEvent @event)
    {
        _logger.LogInformation(
            "收到报价创建事件: QuotationId={QuotationId}, InquiryId={InquiryId}",
            @event.QuotationId,
            @event.InquiryId);

        await _mediator.Publish(@event);
        return Ok();
    }

    /// <summary>
    /// 订阅匹配完成事件
    /// Topic: match-completed
    /// </summary>
    [Topic("pubsub", "match-completed")]
    [HttpPost("match-completed")]
    public async Task<IActionResult> HandleMatchCompleted([FromBody] MatchCompletedEvent @event)
    {
        _logger.LogInformation(
            "收到匹配完成事件: InquiryId={InquiryId}, MatchCount={MatchCount}",
            @event.InquiryId,
            @event.MatchCount);

        await _mediator.Publish(@event);
        return Ok();
    }

    /// <summary>
    /// 订阅订单确认事件
    /// Topic: order-confirmed
    /// </summary>
    [Topic("pubsub", "order-confirmed")]
    [HttpPost("order-confirmed")]
    public async Task<IActionResult> HandleOrderConfirmed([FromBody] OrderConfirmedEvent @event)
    {
        _logger.LogInformation(
            "收到订单确认事件: OrderId={OrderId}, InquiryId={InquiryId}",
            @event.OrderId,
            @event.InquiryId);

        await _mediator.Publish(@event);
        return Ok();
    }
}
