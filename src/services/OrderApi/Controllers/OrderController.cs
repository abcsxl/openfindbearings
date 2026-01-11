using Microsoft.AspNetCore.Mvc;
using OrderApi.Models.DTOs;
using OrderApi.Services;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<OrderDetailResponse>>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<OrderDetailResponse>.ErrorResponse("请求数据无效"));

                var order = await _orderService.CreateOrderAsync(request);

                _logger.LogInformation("订单创建成功: {OrderNumber}", order.OrderNumber);

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id },
                    ApiResponse<OrderDetailResponse>.SuccessResponse(order, "订单创建成功"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "创建订单参数错误");
                return BadRequest(ApiResponse<OrderDetailResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建订单失败");
                return StatusCode(500, ApiResponse<OrderDetailResponse>.ErrorResponse("创建订单失败"));
            }
        }

        [HttpPost("from-quotation/{quotationId:long}")]
        public async Task<ActionResult<ApiResponse<OrderDetailResponse>>> CreateFromQuotation(long quotationId)
        {
            try
            {
                var order = await _orderService.CreateFromQuotationAsync(quotationId);

                _logger.LogInformation("从报价单创建订单成功: {QuotationId} -> {OrderNumber}", quotationId, order.OrderNumber);

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id },
                    ApiResponse<OrderDetailResponse>.SuccessResponse(order, "订单创建成功"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "从报价单创建订单失败");
                return BadRequest(ApiResponse<OrderDetailResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从报价单创建订单失败");
                return StatusCode(500, ApiResponse<OrderDetailResponse>.ErrorResponse("创建订单失败"));
            }
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ApiResponse<OrderDetailResponse>>> GetOrder(long id)
        {
            try
            {
                var order = await _orderService.GetOrderAsync(id);
                if (order == null)
                    return NotFound(ApiResponse<OrderDetailResponse>.ErrorResponse("订单不存在"));

                return Ok(ApiResponse<OrderDetailResponse>.SuccessResponse(order));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单详情失败");
                return StatusCode(500, ApiResponse<OrderDetailResponse>.ErrorResponse("获取订单失败"));
            }
        }

        [HttpGet("number/{orderNumber}")]
        public async Task<ActionResult<ApiResponse<OrderDetailResponse>>> GetOrderByNumber(string orderNumber)
        {
            try
            {
                var order = await _orderService.GetOrderByNumberAsync(orderNumber);
                if (order == null)
                    return NotFound(ApiResponse<OrderDetailResponse>.ErrorResponse("订单不存在"));

                return Ok(ApiResponse<OrderDetailResponse>.SuccessResponse(order));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单详情失败");
                return StatusCode(500, ApiResponse<OrderDetailResponse>.ErrorResponse("获取订单失败"));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<OrderResponse>>>> GetOrders([FromQuery] OrderQuery query)
        {
            try
            {
                var orders = await _orderService.GetOrdersAsync(query);
                return Ok(ApiResponse<PagedResponse<OrderResponse>>.SuccessResponse(orders));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单列表失败");
                return StatusCode(500, ApiResponse<PagedResponse<OrderResponse>>.ErrorResponse("获取订单列表失败"));
            }
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<ApiResponse<OrderDetailResponse>>> UpdateOrder(long id, [FromBody] UpdateOrderRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<OrderDetailResponse>.ErrorResponse("请求数据无效"));

                var order = await _orderService.UpdateOrderAsync(id, request);
                return Ok(ApiResponse<OrderDetailResponse>.SuccessResponse(order, "订单更新成功"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<OrderDetailResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新订单失败");
                return StatusCode(500, ApiResponse<OrderDetailResponse>.ErrorResponse("更新订单失败"));
            }
        }

        [HttpDelete("{id:long}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteOrder(long id)
        {
            try
            {
                var result = await _orderService.DeleteOrderAsync(id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("订单不存在"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "订单删除成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除订单失败");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("删除订单失败"));
            }
        }

        [HttpPatch("{id:long}/status")]
        public async Task<ActionResult<ApiResponse<OrderDetailResponse>>> UpdateOrderStatus(long id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<OrderDetailResponse>.ErrorResponse("请求数据无效"));

                var order = await _orderService.UpdateOrderStatusAsync(id, request);
                return Ok(ApiResponse<OrderDetailResponse>.SuccessResponse(order, "订单状态更新成功"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<OrderDetailResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新订单状态失败");
                return StatusCode(500, ApiResponse<OrderDetailResponse>.ErrorResponse("更新订单状态失败"));
            }
        }

        [HttpPost("{id:long}/payment")]
        public async Task<ActionResult<ApiResponse<OrderDetailResponse>>> ProcessPayment(long id, [FromBody] ProcessPaymentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<OrderDetailResponse>.ErrorResponse("请求数据无效"));

                var order = await _orderService.ProcessPaymentAsync(id, request);
                return Ok(ApiResponse<OrderDetailResponse>.SuccessResponse(order, "支付处理成功"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<OrderDetailResponse>.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<OrderDetailResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理支付失败");
                return StatusCode(500, ApiResponse<OrderDetailResponse>.ErrorResponse("处理支付失败"));
            }
        }

        [HttpPatch("{id:long}/shipping")]
        public async Task<ActionResult<ApiResponse<OrderDetailResponse>>> UpdateShipping(long id, [FromBody] UpdateShippingRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<OrderDetailResponse>.ErrorResponse("请求数据无效"));

                var order = await _orderService.UpdateShippingAsync(id, request);
                return Ok(ApiResponse<OrderDetailResponse>.SuccessResponse(order, "发货状态更新成功"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<OrderDetailResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新发货状态失败");
                return StatusCode(500, ApiResponse<OrderDetailResponse>.ErrorResponse("更新发货状态失败"));
            }
        }

        [HttpPost("{id:long}/cancel")]
        public async Task<ActionResult<ApiResponse<OrderDetailResponse>>> CancelOrder(long id, [FromQuery] string? reason = null)
        {
            try
            {
                var order = await _orderService.CancelOrderAsync(id, reason);
                return Ok(ApiResponse<OrderDetailResponse>.SuccessResponse(order, "订单取消成功"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<OrderDetailResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消订单失败");
                return StatusCode(500, ApiResponse<OrderDetailResponse>.ErrorResponse("取消订单失败"));
            }
        }

        [HttpPost("{id:long}/complete")]
        public async Task<ActionResult<ApiResponse<OrderDetailResponse>>> CompleteOrder(long id)
        {
            try
            {
                var order = await _orderService.CompleteOrderAsync(id);
                return Ok(ApiResponse<OrderDetailResponse>.SuccessResponse(order, "订单完成"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<OrderDetailResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "完成订单失败");
                return StatusCode(500, ApiResponse<OrderDetailResponse>.ErrorResponse("完成订单失败"));
            }
        }

        [HttpGet("quotation/{quotationId:long}")]
        public async Task<ActionResult<ApiResponse<List<OrderResponse>>>> GetOrdersByQuotation(long quotationId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByQuotationAsync(quotationId);
                return Ok(ApiResponse<List<OrderResponse>>.SuccessResponse(orders));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取报价单相关订单失败");
                return StatusCode(500, ApiResponse<List<OrderResponse>>.ErrorResponse("获取订单失败"));
            }
        }

        [HttpGet("demand/{demandId:long}")]
        public async Task<ActionResult<ApiResponse<List<OrderResponse>>>> GetOrdersByDemand(long demandId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByDemandAsync(demandId);
                return Ok(ApiResponse<List<OrderResponse>>.SuccessResponse(orders));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取需求相关订单失败");
                return StatusCode(500, ApiResponse<List<OrderResponse>>.ErrorResponse("获取订单失败"));
            }
        }

        [HttpGet("supplier/{supplierId:long}")]
        public async Task<ActionResult<ApiResponse<List<OrderResponse>>>> GetOrdersBySupplier(long supplierId)
        {
            try
            {
                var orders = await _orderService.GetOrdersBySupplierAsync(supplierId);
                return Ok(ApiResponse<List<OrderResponse>>.SuccessResponse(orders));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取供应商相关订单失败");
                return StatusCode(500, ApiResponse<List<OrderResponse>>.ErrorResponse("获取订单失败"));
            }
        }

        [HttpGet("pending")]
        public async Task<ActionResult<ApiResponse<List<OrderResponse>>>> GetPendingOrders()
        {
            try
            {
                var orders = await _orderService.GetPendingOrdersAsync();
                return Ok(ApiResponse<List<OrderResponse>>.SuccessResponse(orders));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取待处理订单失败");
                return StatusCode(500, ApiResponse<List<OrderResponse>>.ErrorResponse("获取订单失败"));
            }
        }

        [HttpGet("expiring")]
        public async Task<ActionResult<ApiResponse<List<OrderResponse>>>> GetExpiringOrders()
        {
            try
            {
                var orders = await _orderService.GetExpiringOrdersAsync();
                return Ok(ApiResponse<List<OrderResponse>>.SuccessResponse(orders));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取即将到期订单失败");
                return StatusCode(500, ApiResponse<List<OrderResponse>>.ErrorResponse("获取订单失败"));
            }
        }

        [HttpGet("recommended")]
        public async Task<ActionResult<ApiResponse<List<OrderResponse>>>> GetRecommendedOrders([FromQuery] int limit = 10)
        {
            try
            {
                var orders = await _orderService.GetRecommendedOrdersAsync(limit);
                return Ok(ApiResponse<List<OrderResponse>>.SuccessResponse(orders));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取推荐订单失败");
                return StatusCode(500, ApiResponse<List<OrderResponse>>.ErrorResponse("获取订单失败"));
            }
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<OrderStatisticsResponse>>> GetStatistics()
        {
            try
            {
                var statistics = await _orderService.GetOrderStatisticsAsync();
                return Ok(ApiResponse<OrderStatisticsResponse>.SuccessResponse(statistics));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单统计失败");
                return StatusCode(500, ApiResponse<OrderStatisticsResponse>.ErrorResponse("获取统计失败"));
            }
        }

        [HttpGet("supplier/{supplierId:long}/statistics")]
        public async Task<ActionResult<ApiResponse<OrderStatisticsResponse>>> GetSupplierStatistics(long supplierId)
        {
            try
            {
                var statistics = await _orderService.GetSupplierStatisticsAsync(supplierId);
                return Ok(ApiResponse<OrderStatisticsResponse>.SuccessResponse(statistics));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取供应商统计失败");
                return StatusCode(500, ApiResponse<OrderStatisticsResponse>.ErrorResponse("获取统计失败"));
            }
        }

        [HttpGet("revenue-trends")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, decimal>>>> GetRevenueTrends([FromQuery] int days = 30)
        {
            try
            {
                var trends = await _orderService.GetRevenueTrendsAsync(days);
                return Ok(ApiResponse<Dictionary<string, decimal>>.SuccessResponse(trends));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取收入趋势失败");
                return StatusCode(500, ApiResponse<Dictionary<string, decimal>>.ErrorResponse("获取趋势失败"));
            }
        }

        [HttpPost("bulk")]
        public async Task<ActionResult<ApiResponse<List<OrderResponse>>>> CreateBulkOrders([FromBody] List<CreateOrderRequest> requests)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<List<OrderResponse>>.ErrorResponse("请求数据无效"));

                var orders = await _orderService.CreateBulkOrdersAsync(requests);
                return Ok(ApiResponse<List<OrderResponse>>.SuccessResponse(orders, "批量创建订单成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量创建订单失败");
                return StatusCode(500, ApiResponse<List<OrderResponse>>.ErrorResponse("批量创建订单失败"));
            }
        }

        [HttpPost("expire-old")]
        public async Task<ActionResult<ApiResponse<bool>>> ExpireOldOrders([FromQuery] int days = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-days);
                var result = await _orderService.ExpireOldOrdersAsync(cutoffDate);
                return Ok(ApiResponse<bool>.SuccessResponse(result, "过期订单处理完成"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理过期订单失败");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("处理过期订单失败"));
            }
        }

        [HttpPost("send-reminders")]
        public async Task<ActionResult<ApiResponse<bool>>> SendOrderReminders()
        {
            try
            {
                var result = await _orderService.SendOrderRemindersAsync();
                return Ok(ApiResponse<bool>.SuccessResponse(result, "订单提醒发送完成"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送订单提醒失败");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("发送提醒失败"));
            }
        }

        [HttpPost("{orderId:long}/attachments")]
        public async Task<ActionResult<ApiResponse<OrderAttachmentResponse>>> AddAttachment(long orderId, [FromBody] AddAttachmentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<OrderAttachmentResponse>.ErrorResponse("请求数据无效"));

                var attachment = await _orderService.AddAttachmentAsync(orderId, request);
                return Ok(ApiResponse<OrderAttachmentResponse>.SuccessResponse(attachment, "附件添加成功"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<OrderAttachmentResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加附件失败");
                return StatusCode(500, ApiResponse<OrderAttachmentResponse>.ErrorResponse("添加附件失败"));
            }
        }

        [HttpDelete("attachments/{attachmentId:long}")]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveAttachment(long attachmentId)
        {
            try
            {
                var result = await _orderService.RemoveAttachmentAsync(attachmentId);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("附件不存在"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "附件删除成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除附件失败");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("删除附件失败"));
            }
        }

        [HttpGet("{orderId:long}/attachments")]
        public async Task<ActionResult<ApiResponse<List<OrderAttachmentResponse>>>> GetAttachments(long orderId)
        {
            try
            {
                var attachments = await _orderService.GetAttachmentsAsync(orderId);
                return Ok(ApiResponse<List<OrderAttachmentResponse>>.SuccessResponse(attachments));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取附件列表失败");
                return StatusCode(500, ApiResponse<List<OrderAttachmentResponse>>.ErrorResponse("获取附件失败"));
            }
        }

        [HttpPost("{orderId:long}/items")]
        public async Task<ActionResult<ApiResponse<OrderItemResponse>>> AddOrderItem(long orderId, [FromBody] AddOrderItemRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<OrderItemResponse>.ErrorResponse("请求数据无效"));

                var item = await _orderService.AddOrderItemAsync(orderId, request);
                return Ok(ApiResponse<OrderItemResponse>.SuccessResponse(item, "订单项添加成功"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<OrderItemResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加订单项失败");
                return StatusCode(500, ApiResponse<OrderItemResponse>.ErrorResponse("添加订单项失败"));
            }
        }

        [HttpDelete("items/{itemId:long}")]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveOrderItem(long itemId)
        {
            try
            {
                var result = await _orderService.RemoveOrderItemAsync(itemId);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("订单项不存在"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "订单项删除成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除订单项失败");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("删除订单项失败"));
            }
        }

        [HttpGet("{orderId:long}/items")]
        public async Task<ActionResult<ApiResponse<List<OrderItemResponse>>>> GetOrderItems(long orderId)
        {
            try
            {
                var items = await _orderService.GetOrderItemsAsync(orderId);
                return Ok(ApiResponse<List<OrderItemResponse>>.SuccessResponse(items));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单项失败");
                return StatusCode(500, ApiResponse<List<OrderItemResponse>>.ErrorResponse("获取订单项失败"));
            }
        }

        [HttpGet("health")]
        public ActionResult<ApiResponse<string>> HealthCheck()
        {
            return Ok(ApiResponse<string>.SuccessResponse("Order API is running"));
        }
    }
}
