using Microsoft.AspNetCore.Mvc;
using NotificationApi.Models.DTOs;
using NotificationApi.Services;

namespace NotificationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpPost("send")]
        public async Task<ActionResult<ApiResponse<SendNotificationResult>>> SendNotification([FromBody] SendNotificationRequest request)
        {
            try
            {
                var result = await _notificationService.SendNotificationAsync(request);
                return ApiResponse<SendNotificationResult>.Success(result, "通知发送成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送通知失败");
                return BadRequest(ApiResponse<SendNotificationResult>.Error($"发送通知失败: {ex.Message}"));
            }
        }

        [HttpPost("send-by-template")]
        public async Task<ActionResult<ApiResponse<SendNotificationResult>>> SendNotificationByTemplate(
            [FromBody] SendNotificationByTemplateRequest request)
        {
            try
            {
                var result = await _notificationService.SendNotificationByTemplateAsync(
                    request.TemplateCode,
                    request.TemplateData,
                    request.Recipient);

                return ApiResponse<SendNotificationResult>.Success(result, "模板通知发送成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "通过模板发送通知失败");
                return BadRequest(ApiResponse<SendNotificationResult>.Error($"通过模板发送通知失败: {ex.Message}"));
            }
        }

        [HttpPost("send-bulk")]
        public async Task<ActionResult<ApiResponse<List<SendNotificationResult>>>> SendBulkNotifications([FromBody] List<SendNotificationRequest> requests)
        {
            try
            {
                var results = await _notificationService.SendBulkNotificationsAsync(requests);
                return ApiResponse<List<SendNotificationResult>>.Success(results, "批量通知发送完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量发送通知失败");
                return BadRequest(ApiResponse<List<SendNotificationResult>>.Error($"批量发送通知失败: {ex.Message}"));
            }
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ApiResponse<NotificationDetailResponse>>> GetNotification(long id)
        {
            try
            {
                var notification = await _notificationService.GetNotificationAsync(id);
                if (notification == null)
                    return NotFound(ApiResponse<NotificationDetailResponse>.Error("通知不存在"));

                return ApiResponse<NotificationDetailResponse>.Success(notification, "获取通知成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取通知失败: {NotificationId}", id);
                return BadRequest(ApiResponse<NotificationDetailResponse>.Error($"获取通知失败: {ex.Message}"));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<NotificationResponse>>>> GetNotifications([FromQuery] NotificationQuery query)
        {
            try
            {
                var result = await _notificationService.GetNotificationsAsync(query);
                return ApiResponse<PagedResponse<NotificationResponse>>.Success(result, "获取通知列表成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取通知列表失败");
                return BadRequest(ApiResponse<PagedResponse<NotificationResponse>>.Error($"获取通知列表失败: {ex.Message}"));
            }
        }

        [HttpPut("{id:long}/read")]
        public async Task<ActionResult<ApiResponse<object>>> MarkAsRead(long id)
        {
            try
            {
                var success = await _notificationService.MarkAsReadAsync(id);
                if (!success)
                    return NotFound(ApiResponse<object>.Error("通知不存在或状态不合法"));

                return ApiResponse<object>.Success(null, "标记为已读成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "标记通知为已读失败: {NotificationId}", id);
                return BadRequest(ApiResponse<object>.Error($"标记为已读失败: {ex.Message}"));
            }
        }

        [HttpDelete("{id:long}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteNotification(long id)
        {
            try
            {
                var success = await _notificationService.DeleteNotificationAsync(id);
                if (!success)
                    return NotFound(ApiResponse<object>.Error("通知不存在"));

                return ApiResponse<object>.Success(null, "删除通知成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除通知失败: {NotificationId}", id);
                return BadRequest(ApiResponse<object>.Error($"删除通知失败: {ex.Message}"));
            }
        }

        [HttpPost("process-pending")]
        public async Task<ActionResult<ApiResponse<List<SendNotificationResult>>>> ProcessPendingNotifications([FromQuery] int batchSize = 100)
        {
            try
            {
                var results = await _notificationService.ProcessPendingNotificationsAsync(batchSize);
                return ApiResponse<List<SendNotificationResult>>.Success(results, "处理待发送通知完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理待发送通知失败");
                return BadRequest(ApiResponse<List<SendNotificationResult>>.Error($"处理待发送通知失败: {ex.Message}"));
            }
        }

        [HttpPost("retry-failed")]
        public async Task<ActionResult<ApiResponse<List<SendNotificationResult>>>> RetryFailedNotifications([FromQuery] int maxRetryCount = 3)
        {
            try
            {
                var results = await _notificationService.RetryFailedNotificationsAsync(maxRetryCount);
                return ApiResponse<List<SendNotificationResult>>.Success(results, "重试失败通知完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重试失败通知失败");
                return BadRequest(ApiResponse<List<SendNotificationResult>>.Error($"重试失败通知失败: {ex.Message}"));
            }
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<NotificationStatistics>>> GetStatistics()
        {
            try
            {
                var statistics = await _notificationService.GetStatisticsAsync();
                return ApiResponse<NotificationStatistics>.Success(statistics, "获取统计信息成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取统计信息失败");
                return BadRequest(ApiResponse<NotificationStatistics>.Error($"获取统计信息失败: {ex.Message}"));
            }
        }

        [HttpPost("events/quotation-created")]
        public async Task<ActionResult<ApiResponse<object>>> HandleQuotationCreated([FromBody] dynamic eventData)
        {
            try
            {
                await _notificationService.HandleQuotationCreatedAsync(eventData);
                return ApiResponse<object>.Success(null, "处理报价创建事件成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理报价创建事件失败");
                return BadRequest(ApiResponse<object>.Error($"处理报价创建事件失败: {ex.Message}"));
            }
        }

        [HttpPost("events/order-created")]
        public async Task<ActionResult<ApiResponse<object>>> HandleOrderCreated([FromBody] dynamic eventData)
        {
            try
            {
                await _notificationService.HandleOrderCreatedAsync(eventData);
                return ApiResponse<object>.Success(null, "处理订单创建事件成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理订单创建事件失败");
                return BadRequest(ApiResponse<object>.Error($"处理订单创建事件失败: {ex.Message}"));
            }
        }

        [HttpPost("events/order-status-changed")]
        public async Task<ActionResult<ApiResponse<object>>> HandleOrderStatusChanged([FromBody] dynamic eventData)
        {
            try
            {
                await _notificationService.HandleOrderStatusChangedAsync(eventData);
                return ApiResponse<object>.Success(null, "处理订单状态变更事件成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理订单状态变更事件失败");
                return BadRequest(ApiResponse<object>.Error($"处理订单状态变更事件失败: {ex.Message}"));
            }
        }

        [HttpPost("events/payment-processed")]
        public async Task<ActionResult<ApiResponse<object>>> HandlePaymentProcessed([FromBody] dynamic eventData)
        {
            try
            {
                await _notificationService.HandlePaymentProcessedAsync(eventData);
                return ApiResponse<object>.Success(null, "处理支付事件成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理支付事件失败");
                return BadRequest(ApiResponse<object>.Error($"处理支付事件失败: {ex.Message}"));
            }
        }
    }
}
