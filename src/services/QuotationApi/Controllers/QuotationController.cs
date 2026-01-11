using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuotationApi.Models.DTOs;
using QuotationApi.Services;

namespace QuotationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuotationController : ControllerBase
    {
        private readonly IQuotationService _quotationService;
        private readonly ILogger<QuotationController> _logger;

        public QuotationController(IQuotationService quotationService, ILogger<QuotationController> logger)
        {
            _quotationService = quotationService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Supplier,Admin")]
        public async Task<ActionResult<ApiResponse<QuotationDetailResponse>>> CreateQuotation(
            [FromBody] CreateQuotationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<QuotationDetailResponse>.ErrorResponse("请求数据无效"));

                var quotation = await _quotationService.CreateQuotationAsync(request);

                _logger.LogInformation("报价单创建成功: {QuotationNumber} by {UserId}",
                    quotation.QuotationNumber, GetCurrentUserId());

                return CreatedAtAction(nameof(GetQuotation), new { id = quotation.Id },
                    ApiResponse<QuotationDetailResponse>.SuccessResponse(quotation));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "创建报价单参数错误");
                return BadRequest(ApiResponse<QuotationDetailResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建报价单失败");
                return StatusCode(500, ApiResponse<QuotationDetailResponse>.ErrorResponse("创建失败"));
            }
        }

        [HttpGet("{id:long}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<QuotationDetailResponse>>> GetQuotation(long id)
        {
            try
            {
                var quotation = await _quotationService.GetQuotationAsync(id);
                if (quotation == null)
                    return NotFound(ApiResponse<QuotationDetailResponse>.ErrorResponse("报价单不存在"));

                return Ok(ApiResponse<QuotationDetailResponse>.SuccessResponse(quotation));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取报价单详情失败");
                return StatusCode(500, ApiResponse<QuotationDetailResponse>.ErrorResponse("获取失败"));
            }
        }

        [HttpGet("number/{quotationNumber}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<QuotationDetailResponse>>> GetQuotationByNumber(string quotationNumber)
        {
            try
            {
                var quotation = await _quotationService.GetQuotationByNumberAsync(quotationNumber);
                if (quotation == null)
                    return NotFound(ApiResponse<QuotationDetailResponse>.ErrorResponse("报价单不存在"));

                return Ok(ApiResponse<QuotationDetailResponse>.SuccessResponse(quotation));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取报价单详情失败");
                return StatusCode(500, ApiResponse<QuotationDetailResponse>.ErrorResponse("获取失败"));
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<PagedResponse<QuotationResponse>>>> GetQuotations(
            [FromQuery] QuotationQuery query)
        {
            try
            {
                var result = await _quotationService.GetQuotationsAsync(query);
                return Ok(ApiResponse<PagedResponse<QuotationResponse>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取报价单列表失败");
                return StatusCode(500, ApiResponse<PagedResponse<QuotationResponse>>.ErrorResponse("获取失败"));
            }
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "Supplier,Admin")]
        public async Task<ActionResult<ApiResponse<QuotationDetailResponse>>> UpdateQuotation(
            long id, [FromBody] UpdateQuotationRequest request)
        {
            try
            {
                var quotation = await _quotationService.UpdateQuotationAsync(id, request);
                return Ok(ApiResponse<QuotationDetailResponse>.SuccessResponse(quotation));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<QuotationDetailResponse>.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<QuotationDetailResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新报价单失败");
                return StatusCode(500, ApiResponse<QuotationDetailResponse>.ErrorResponse("更新失败"));
            }
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Supplier,Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteQuotation(long id)
        {
            try
            {
                var result = await _quotationService.DeleteQuotationAsync(id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("报价单不存在"));

                return Ok(ApiResponse<bool>.SuccessResponse(true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除报价单失败");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("删除失败"));
            }
        }

        [HttpPost("{id:long}/submit")]
        [Authorize(Roles = "Supplier,Admin")]
        public async Task<ActionResult<ApiResponse<QuotationDetailResponse>>> SubmitQuotation(
            long id, [FromBody] SubmitQuotationRequest? request = null)
        {
            try
            {
                var quotation = await _quotationService.SubmitQuotationAsync(id, request?.SubmissionNotes);
                return Ok(ApiResponse<QuotationDetailResponse>.SuccessResponse(quotation));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<QuotationDetailResponse>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<QuotationDetailResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "提交报价单失败");
                return StatusCode(500, ApiResponse<QuotationDetailResponse>.ErrorResponse("提交失败"));
            }
        }

        [HttpPost("{id:long}/accept")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<ActionResult<ApiResponse<QuotationDetailResponse>>> AcceptQuotation(
            long id, [FromBody] AcceptQuotationRequest request)
        {
            try
            {
                var quotation = await _quotationService.AcceptQuotationAsync(id, request.CustomerId);
                return Ok(ApiResponse<QuotationDetailResponse>.SuccessResponse(quotation));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<QuotationDetailResponse>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<QuotationDetailResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "接受报价单失败");
                return StatusCode(500, ApiResponse<QuotationDetailResponse>.ErrorResponse("接受失败"));
            }
        }

        [HttpPost("{id:long}/reject")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<ActionResult<ApiResponse<QuotationDetailResponse>>> RejectQuotation(
            long id, [FromBody] RejectQuotationRequest request)
        {
            try
            {
                var quotation = await _quotationService.RejectQuotationAsync(id, request.RejectionReason);
                return Ok(ApiResponse<QuotationDetailResponse>.SuccessResponse(quotation));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<QuotationDetailResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "拒绝报价单失败");
                return StatusCode(500, ApiResponse<QuotationDetailResponse>.ErrorResponse("拒绝失败"));
            }
        }

        [HttpPost("{id:long}/withdraw")]
        [Authorize(Roles = "Supplier,Admin")]
        public async Task<ActionResult<ApiResponse<QuotationDetailResponse>>> WithdrawQuotation(
            long id, [FromBody] WithdrawQuotationRequest? request = null)
        {
            try
            {
                var quotation = await _quotationService.WithdrawQuotationAsync(id, request?.Reason);
                return Ok(ApiResponse<QuotationDetailResponse>.SuccessResponse(quotation));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<QuotationDetailResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "撤回报价单失败");
                return StatusCode(500, ApiResponse<QuotationDetailResponse>.ErrorResponse("撤回失败"));
            }
        }

        [HttpPost("{id:long}/expire")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<QuotationDetailResponse>>> ExpireQuotation(long id)
        {
            try
            {
                var quotation = await _quotationService.ExpireQuotationAsync(id);
                return Ok(ApiResponse<QuotationDetailResponse>.SuccessResponse(quotation));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<QuotationDetailResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "过期报价单失败");
                return StatusCode(500, ApiResponse<QuotationDetailResponse>.ErrorResponse("过期失败"));
            }
        }

        [HttpPost("{id:long}/recommend")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<QuotationDetailResponse>>> SetRecommended(
            long id, [FromBody] SetRecommendedRequest request)
        {
            try
            {
                var quotation = await _quotationService.SetRecommendedAsync(id, request.IsRecommended, request.MatchScore);
                return Ok(ApiResponse<QuotationDetailResponse>.SuccessResponse(quotation));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<QuotationDetailResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置推荐状态失败");
                return StatusCode(500, ApiResponse<QuotationDetailResponse>.ErrorResponse("设置失败"));
            }
        }

        [HttpPost("search")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<PagedResponse<QuotationResponse>>>> SearchQuotations(
            [FromBody] QuotationSearchRequest request)
        {
            try
            {
                var result = await _quotationService.SearchQuotationsAsync(request);
                return Ok(ApiResponse<PagedResponse<QuotationResponse>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜索报价单失败");
                return StatusCode(500, ApiResponse<PagedResponse<QuotationResponse>>.ErrorResponse("搜索失败"));
            }
        }

        [HttpGet("demand/{demandId:long}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<QuotationResponse>>>> GetQuotationsByDemand(long demandId)
        {
            try
            {
                var quotations = await _quotationService.GetQuotationsByDemandAsync(demandId);
                return Ok(ApiResponse<List<QuotationResponse>>.SuccessResponse(quotations));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取需求相关报价单失败");
                return StatusCode(500, ApiResponse<List<QuotationResponse>>.ErrorResponse("获取失败"));
            }
        }

        [HttpGet("supplier/{supplierId:long}")]
        [Authorize(Roles = "Supplier,Admin")]
        public async Task<ActionResult<ApiResponse<List<QuotationResponse>>>> GetQuotationsBySupplier(long supplierId)
        {
            try
            {
                var quotations = await _quotationService.GetQuotationsBySupplierAsync(supplierId);
                return Ok(ApiResponse<List<QuotationResponse>>.SuccessResponse(quotations));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取供应商报价单失败");
                return StatusCode(500, ApiResponse<List<QuotationResponse>>.ErrorResponse("获取失败"));
            }
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<List<QuotationResponse>>>> GetPendingQuotations()
        {
            try
            {
                var quotations = await _quotationService.GetPendingQuotationsAsync();
                return Ok(ApiResponse<List<QuotationResponse>>.SuccessResponse(quotations));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取待处理报价单失败");
                return StatusCode(500, ApiResponse<List<QuotationResponse>>.ErrorResponse("获取失败"));
            }
        }

        [HttpGet("expired")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<List<QuotationResponse>>>> GetExpiredQuotations()
        {
            try
            {
                var quotations = await _quotationService.GetExpiredQuotationsAsync();
                return Ok(ApiResponse<List<QuotationResponse>>.SuccessResponse(quotations));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取过期报价单失败");
                return StatusCode(500, ApiResponse<List<QuotationResponse>>.ErrorResponse("获取失败"));
            }
        }

        [HttpGet("demand/{demandId:long}/recommended")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<QuotationResponse>>>> GetRecommendedQuotations(
            long demandId, [FromQuery] int limit = 10)
        {
            try
            {
                var quotations = await _quotationService.GetRecommendedQuotationsAsync(demandId, limit);
                return Ok(ApiResponse<List<QuotationResponse>>.SuccessResponse(quotations));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取推荐报价单失败");
                return StatusCode(500, ApiResponse<List<QuotationResponse>>.ErrorResponse("获取失败"));
            }
        }

        [HttpGet("demand/{demandId:long}/bearing/{bearingNumber}/compare")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<QuotationComparisonResponse>>> CompareQuotations(
            long demandId, string bearingNumber)
        {
            try
            {
                var comparison = await _quotationService.CompareQuotationsAsync(demandId, bearingNumber);
                return Ok(ApiResponse<QuotationComparisonResponse>.SuccessResponse(comparison));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "比较报价单失败");
                return StatusCode(500, ApiResponse<QuotationComparisonResponse>.ErrorResponse("比较失败"));
            }
        }

        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<QuotationStatisticsResponse>>> GetStatistics()
        {
            try
            {
                var statistics = await _quotationService.GetQuotationStatisticsAsync();
                return Ok(ApiResponse<QuotationStatisticsResponse>.SuccessResponse(statistics));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取统计信息失败");
                return StatusCode(500, ApiResponse<QuotationStatisticsResponse>.ErrorResponse("获取失败"));
            }
        }

        [HttpGet("supplier/{supplierId:long}/statistics")]
        [Authorize(Roles = "Supplier,Admin")]
        public async Task<ActionResult<ApiResponse<QuotationStatisticsResponse>>> GetSupplierStatistics(long supplierId)
        {
            try
            {
                var statistics = await _quotationService.GetSupplierStatisticsAsync(supplierId);
                return Ok(ApiResponse<QuotationStatisticsResponse>.SuccessResponse(statistics));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取供应商统计信息失败");
                return StatusCode(500, ApiResponse<QuotationStatisticsResponse>.ErrorResponse("获取失败"));
            }
        }

        [HttpGet("trends/{bearingNumber}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<Dictionary<string, decimal>>>> GetPriceTrends(
            string bearingNumber, [FromQuery] int days = 30)
        {
            try
            {
                var trends = await _quotationService.GetPriceTrendsAsync(bearingNumber, days);
                return Ok(ApiResponse<Dictionary<string, decimal>>.SuccessResponse(trends));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取价格趋势失败");
                return StatusCode(500, ApiResponse<Dictionary<string, decimal>>.ErrorResponse("获取失败"));
            }
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "Supplier,Admin")]
        public async Task<ActionResult<ApiResponse<List<QuotationResponse>>>> CreateBulkQuotations(
            [FromBody] List<CreateQuotationRequest> requests)
        {
            try
            {
                var results = await _quotationService.CreateBulkQuotationsAsync(requests);
                return Ok(ApiResponse<List<QuotationResponse>>.SuccessResponse(results));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量创建报价单失败");
                return StatusCode(500, ApiResponse<List<QuotationResponse>>.ErrorResponse("批量创建失败"));
            }
        }

        [HttpPost("expire-old")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> ExpireOldQuotations([FromQuery] int days = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-days);
                var result = await _quotationService.ExpireOldQuotationsAsync(cutoffDate);
                return Ok(ApiResponse<bool>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "过期旧报价单失败");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("过期失败"));
            }
        }

        [HttpPost("send-reminders")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> SendQuotationReminders()
        {
            try
            {
                var result = await _quotationService.SendQuotationRemindersAsync();
                return Ok(ApiResponse<bool>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送报价提醒失败");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("发送失败"));
            }
        }

        // 附件管理
        [HttpPost("{id:long}/attachments")]
        [Authorize(Roles = "Supplier,Admin")]
        public async Task<ActionResult<ApiResponse<QuotationAttachmentResponse>>> AddAttachment(
            long id, [FromBody] AddAttachmentRequest request)
        {
            try
            {
                var attachment = await _quotationService.AddAttachmentAsync(id, request);
                return Ok(ApiResponse<QuotationAttachmentResponse>.SuccessResponse(attachment));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<QuotationAttachmentResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加附件失败");
                return StatusCode(500, ApiResponse<QuotationAttachmentResponse>.ErrorResponse("添加失败"));
            }
        }

        [HttpDelete("attachments/{attachmentId:long}")]
        [Authorize(Roles = "Supplier,Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveAttachment(long attachmentId)
        {
            try
            {
                var result = await _quotationService.RemoveAttachmentAsync(attachmentId);
                return Ok(ApiResponse<bool>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除附件失败");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("删除失败"));
            }
        }

        [HttpGet("{id:long}/attachments")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<QuotationAttachmentResponse>>>> GetAttachments(long id)
        {
            try
            {
                var attachments = await _quotationService.GetAttachmentsAsync(id);
                return Ok(ApiResponse<List<QuotationAttachmentResponse>>.SuccessResponse(attachments));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取附件列表失败");
                return StatusCode(500, ApiResponse<List<QuotationAttachmentResponse>>.ErrorResponse("获取失败"));
            }
        }

        // 项目管理
        [HttpPost("{id:long}/items")]
        [Authorize(Roles = "Supplier,Admin")]
        public async Task<ActionResult<ApiResponse<QuotationItemResponse>>> AddQuotationItem(
            long id, [FromBody] AddQuotationItemRequest request)
        {
            try
            {
                var item = await _quotationService.AddQuotationItemAsync(id, request);
                return Ok(ApiResponse<QuotationItemResponse>.SuccessResponse(item));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<QuotationItemResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加报价项目失败");
                return StatusCode(500, ApiResponse<QuotationItemResponse>.ErrorResponse("添加失败"));
            }
        }

        [HttpDelete("items/{itemId:long}")]
        [Authorize(Roles = "Supplier,Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveQuotationItem(long itemId)
        {
            try
            {
                var result = await _quotationService.RemoveQuotationItemAsync(itemId);
                return Ok(ApiResponse<bool>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除报价项目失败");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("删除失败"));
            }
        }

        [HttpGet("{id:long}/items")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<QuotationItemResponse>>>> GetQuotationItems(long id)
        {
            try
            {
                var items = await _quotationService.GetQuotationItemsAsync(id);
                return Ok(ApiResponse<List<QuotationItemResponse>>.SuccessResponse(items));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取报价项目失败");
                return StatusCode(500, ApiResponse<List<QuotationItemResponse>>.ErrorResponse("获取失败"));
            }
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public ActionResult<ApiResponse<string>> HealthCheck()
        {
            return Ok(ApiResponse<string>.SuccessResponse("Quotation API is running"));
        }

        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
