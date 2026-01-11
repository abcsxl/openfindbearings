using BearingApi.Models.DTOs;
using BearingApi.Models.Entities;
using BearingApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BearingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BearingController : ControllerBase
    {
        private readonly IBearingService _bearingService;
        private readonly IBearingValidationService _validationService;
        private readonly ILogger<BearingController> _logger;

        public BearingController(
            IBearingService bearingService,
            IBearingValidationService validationService,
            ILogger<BearingController> logger)
        {
            _bearingService = bearingService;
            _validationService = validationService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "SystemAdmin,BearingAdmin,SupplierAdmin")]
        public async Task<ActionResult<ApiResponse<BearingResponse>>> CreateBearing([FromBody] CreateBearingRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<BearingResponse>.ErrorResponse("请求数据无效"));

                var userId = long.Parse(User.FindFirst("sub")?.Value ?? "0");
                var userType = User.FindFirst("user_type")?.Value ?? "System";

                var bearing = await _bearingService.CreateBearingAsync(request);

                _logger.LogInformation("轴承创建成功: {BearingNumber} by {UserId}", bearing.BearingNumber, userId);

                return Ok(ApiResponse<BearingResponse>.SuccessResponse(new BearingResponse
                {
                    Id = bearing.Id,
                    BearingNumber = bearing.BearingNumber,
                    DisplayName = bearing.DisplayName,
                    Brand = bearing.Brand,
                    Type = bearing.Type,
                    Category = bearing.Category,
                    InnerDiameter = bearing.InnerDiameter,
                    OuterDiameter = bearing.OuterDiameter,
                    Width = bearing.Width,
                    Status = bearing.Status,
                    IsVerified = bearing.IsVerified,
                    ViewCount = bearing.ViewCount,
                    SupplierCount = bearing.SupplierCount,
                    CreatedAt = bearing.CreatedAt
                }));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "轴承创建失败: 参数错误");
                return BadRequest(ApiResponse<BearingResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "轴承创建失败");
                return StatusCode(500, ApiResponse<BearingResponse>.ErrorResponse("创建失败"));
            }
        }

        [HttpGet("{id:long}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<BearingDetailResponse>>> GetBearing(long id)
        {
            try
            {
                var bearing = await _bearingService.GetBearingDetailAsync(id);
                if (bearing == null)
                    return NotFound(ApiResponse<BearingDetailResponse>.ErrorResponse("轴承不存在"));

                return Ok(ApiResponse<BearingDetailResponse>.SuccessResponse(bearing));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取轴承详情失败");
                return StatusCode(500, ApiResponse<BearingDetailResponse>.ErrorResponse("获取失败"));
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<PagedResponse<BearingResponse>>>> GetBearings(
            [FromQuery] BearingQuery query)
        {
            try
            {
                var result = await _bearingService.GetBearingsAsync(query);
                return Ok(ApiResponse<PagedResponse<BearingResponse>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取轴承列表失败");
                return StatusCode(500, ApiResponse<PagedResponse<BearingResponse>>.ErrorResponse("获取失败"));
            }
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "SystemAdmin,BearingAdmin,SupplierAdmin")]
        public async Task<ActionResult<ApiResponse<BearingResponse>>> UpdateBearing(
            long id, [FromBody] UpdateBearingRequest request)
        {
            try
            {
                var bearing = await _bearingService.UpdateBearingAsync(id, request);

                return Ok(ApiResponse<BearingResponse>.SuccessResponse(new BearingResponse
                {
                    Id = bearing.Id,
                    BearingNumber = bearing.BearingNumber,
                    DisplayName = bearing.DisplayName,
                    Brand = bearing.Brand,
                    Type = bearing.Type,
                    Category = bearing.Category,
                    InnerDiameter = bearing.InnerDiameter,
                    OuterDiameter = bearing.OuterDiameter,
                    Width = bearing.Width,
                    Status = bearing.Status,
                    IsVerified = bearing.IsVerified,
                    ViewCount = bearing.ViewCount,
                    SupplierCount = bearing.SupplierCount,
                    CreatedAt = bearing.CreatedAt
                }));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<BearingResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新轴承失败");
                return StatusCode(500, ApiResponse<BearingResponse>.ErrorResponse("更新失败"));
            }
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "SystemAdmin,BearingAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteBearing(long id)
        {
            try
            {
                var result = await _bearingService.DeleteBearingAsync(id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("轴承不存在"));

                return Ok(ApiResponse<bool>.SuccessResponse(true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除轴承失败");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("删除失败"));
            }
        }

        [HttpPost("search")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<PagedResponse<BearingResponse>>>> SearchBearings(
            [FromBody] BearingSearchRequest request)
        {
            try
            {
                var result = await _bearingService.SearchBearingsAsync(request);
                return Ok(ApiResponse<PagedResponse<BearingResponse>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜索轴承失败");
                return StatusCode(500, ApiResponse<PagedResponse<BearingResponse>>.ErrorResponse("搜索失败"));
            }
        }

        [HttpGet("similar/{bearingNumber}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<BearingResponse>>>> FindSimilarBearings(
            string bearingNumber, [FromQuery] int limit = 10)
        {
            try
            {
                var bearings = await _bearingService.FindSimilarBearingsAsync(bearingNumber, limit);
                return Ok(ApiResponse<List<BearingResponse>>.SuccessResponse(bearings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查找相似轴承失败");
                return StatusCode(500, ApiResponse<List<BearingResponse>>.ErrorResponse("查找失败"));
            }
        }

        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<ValidationResponse>>> ValidateBearingNumber(
            [FromBody] ValidateBearingNumberRequest request)
        {
            try
            {
                var result = await _validationService.ValidateBearingNumberAsync(request.BearingNumber);

                var response = new ValidationResponse
                {
                    IsValid = result.IsValid,
                    Errors = result.Errors,
                    Warnings = result.Warnings,
                    SuggestedCorrection = result.SuggestedCorrection
                };

                return Ok(ApiResponse<ValidationResponse>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证轴承型号失败");
                return StatusCode(500, ApiResponse<ValidationResponse>.ErrorResponse("验证失败"));
            }
        }

        [HttpGet("suggestions")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<SearchSuggestionResponse>>> GetSuggestions(
     [FromQuery] string query, [FromQuery] int limit = 10)
        {
            try
            {
                var bearingNumbers = await _validationService.SuggestBearingNumbersAsync(query);
                var standardNumbers = await _validationService.GetStandardBearingNumbersAsync();

                var response = new SearchSuggestionResponse
                {
                    BearingNumbers = bearingNumbers.Take(limit).ToList(),
                    Brands = GetBrandSuggestions(query),
                    Types = GetTypeSuggestions(query)
                };

                return Ok(ApiResponse<SearchSuggestionResponse>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取搜索建议失败");
                return StatusCode(500, ApiResponse<SearchSuggestionResponse>.ErrorResponse("获取失败"));
            }
        }

        [HttpGet("statistics")]
        [Authorize(Roles = "SystemAdmin,BearingAdmin")]
        public async Task<ActionResult<ApiResponse<BearingStatisticsResponse>>> GetStatistics()
        {
            try
            {
                var statistics = await _bearingService.GetBearingStatisticsAsync();

                var response = new BearingStatisticsResponse
                {
                    TotalBearings = statistics.TotalBearings,
                    VerifiedBearings = statistics.VerifiedBearings,
                    ActiveBearings = statistics.ActiveBearings,
                    TotalViews = statistics.TotalViews,
                    TotalSearches = statistics.TotalSearches,
                    TypeDistribution = statistics.TypeDistribution.ToDictionary(
                        kvp => kvp.Key.ToString(), kvp => kvp.Value),
                    BrandDistribution = statistics.BrandDistribution,
                    PopularSearches = statistics.PopularSearches
                };

                return Ok(ApiResponse<BearingStatisticsResponse>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取统计信息失败");
                return StatusCode(500, ApiResponse<BearingStatisticsResponse>.ErrorResponse("获取失败"));
            }
        }

        [HttpPost("{id:long}/verify")]
        [Authorize(Roles = "SystemAdmin,BearingAdmin")]
        public async Task<ActionResult<ApiResponse<BearingResponse>>> VerifyBearing(
            long id, [FromBody] VerifyBearingRequest request)
        {
            try
            {
                var bearing = await _bearingService.VerifyBearingAsync(id, request.Level, request.Notes);

                return Ok(ApiResponse<BearingResponse>.SuccessResponse(new BearingResponse
                {
                    Id = bearing.Id,
                    BearingNumber = bearing.BearingNumber,
                    DisplayName = bearing.DisplayName,
                    Brand = bearing.Brand,
                    Type = bearing.Type,
                    Category = bearing.Category,
                    InnerDiameter = bearing.InnerDiameter,
                    OuterDiameter = bearing.OuterDiameter,
                    Width = bearing.Width,
                    Status = bearing.Status,
                    IsVerified = bearing.IsVerified,
                    ViewCount = bearing.ViewCount,
                    SupplierCount = bearing.SupplierCount,
                    CreatedAt = bearing.CreatedAt
                }));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<BearingResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证轴承失败");
                return StatusCode(500, ApiResponse<BearingResponse>.ErrorResponse("验证失败"));
            }
        }

        [HttpPost("{id:long}/status")]
        [Authorize(Roles = "SystemAdmin,BearingAdmin")]
        public async Task<ActionResult<ApiResponse<BearingResponse>>> UpdateBearingStatus(
            long id, [FromBody] UpdateBearingStatusRequest request)
        {
            try
            {
                var bearing = await _bearingService.UpdateBearingStatusAsync(id, request.Status);

                return Ok(ApiResponse<BearingResponse>.SuccessResponse(new BearingResponse
                {
                    Id = bearing.Id,
                    BearingNumber = bearing.BearingNumber,
                    DisplayName = bearing.DisplayName,
                    Brand = bearing.Brand,
                    Type = bearing.Type,
                    Category = bearing.Category,
                    InnerDiameter = bearing.InnerDiameter,
                    OuterDiameter = bearing.OuterDiameter,
                    Width = bearing.Width,
                    Status = bearing.Status,
                    IsVerified = bearing.IsVerified,
                    ViewCount = bearing.ViewCount,
                    SupplierCount = bearing.SupplierCount,
                    CreatedAt = bearing.CreatedAt
                }));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<BearingResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新轴承状态失败");
                return StatusCode(500, ApiResponse<BearingResponse>.ErrorResponse("更新失败"));
            }
        }

        [HttpGet("{id:long}/specifications")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<BearingSpecificationResponse>>>> GetSpecifications(long id)
        {
            try
            {
                var specifications = await _bearingService.GetBearingSpecificationsAsync(id);
                return Ok(ApiResponse<List<BearingSpecificationResponse>>.SuccessResponse(specifications));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取规格参数失败");
                return StatusCode(500, ApiResponse<List<BearingSpecificationResponse>>.ErrorResponse("获取失败"));
            }
        }

        [HttpPost("{id:long}/specifications")]
        [Authorize(Roles = "SystemAdmin,BearingAdmin")]
        public async Task<ActionResult<ApiResponse<BearingSpecificationResponse>>> AddSpecification(
            long id, [FromBody] AddSpecificationRequest request)
        {
            try
            {
                var specification = await _bearingService.AddSpecificationAsync(id, request);

                return Ok(ApiResponse<BearingSpecificationResponse>.SuccessResponse(
                    new BearingSpecificationResponse
                    {
                        Id = specification.Id,
                        ParameterName = specification.ParameterName,
                        ParameterValue = specification.ParameterValue,
                        Unit = specification.Unit,
                        Description = specification.Description,
                        DisplayOrder = specification.DisplayOrder
                    }));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<BearingSpecificationResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加规格参数失败");
                return StatusCode(500, ApiResponse<BearingSpecificationResponse>.ErrorResponse("添加失败"));
            }
        }

        [HttpDelete("specifications/{specId:long}")]
        [Authorize(Roles = "SystemAdmin,BearingAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveSpecification(long specId)
        {
            try
            {
                var result = await _bearingService.RemoveSpecificationAsync(specId);
                return Ok(ApiResponse<bool>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除规格参数失败");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("删除失败"));
            }
        }

        [HttpGet("popular")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<BearingResponse>>>> GetPopularBearings(
     [FromQuery] int limit = 10)
        {
            try
            {
                var popularBearings = await _bearingService.GetPopularBearingsAsync(limit);
                // 转换类型
                var bearingResponses = popularBearings.Select(pb => new BearingResponse
                {
                    Id = pb.BearingId,
                    BearingNumber = pb.BearingNumber,
                    DisplayName = pb.Brand, // 或其他合适的显示名称
                    Brand = pb.Brand,
                    ViewCount = pb.ViewCount,
                    SearchCount = pb.SearchCount,
                    SupplierCount = pb.SupplierCount
                }).ToList();

                return Ok(ApiResponse<List<BearingResponse>>.SuccessResponse(bearingResponses));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取热门轴承失败");
                return StatusCode(500, ApiResponse<List<BearingResponse>>.ErrorResponse("获取失败"));
            }
        }

        [HttpPost("import")]
        [Authorize(Roles = "SystemAdmin,BearingAdmin")]
        public async Task<ActionResult<ApiResponse<ImportResultResponse>>> ImportBearings(
            IFormFile file, [FromQuery] string fileType = "csv")
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(ApiResponse<ImportResultResponse>.ErrorResponse("文件不能为空"));

                using var stream = file.OpenReadStream();
                var result = await _bearingService.ImportBearingsAsync(stream, fileType);

                var response = new ImportResultResponse
                {
                    TotalRecords = result.TotalRecords,
                    SuccessCount = result.SuccessCount,
                    ErrorCount = result.ErrorCount,
                    Errors = result.Errors.Select(e => new ImportErrorResponse
                    {
                        RowNumber = e.RowNumber,
                        BearingNumber = e.BearingNumber,
                        ErrorMessage = e.ErrorMessage,
                        FieldName = e.FieldName
                    }).ToList(),
                    Duration = result.Duration
                };

                return Ok(ApiResponse<ImportResultResponse>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "导入轴承数据失败");
                return StatusCode(500, ApiResponse<ImportResultResponse>.ErrorResponse("导入失败"));
            }
        }

        [HttpGet("export")]
        [Authorize(Roles = "SystemAdmin,BearingAdmin")]
        public async Task<IActionResult> ExportBearings(
            [FromQuery] BearingQuery query, [FromQuery] string format = "csv")
        {
            try
            {
                var stream = await _bearingService.ExportBearingsAsync(query, format);
                var fileName = $"bearings-{DateTime.Now:yyyyMMddHHmmss}.{format}";

                return File(stream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "导出轴承数据失败");
                return StatusCode(500, "导出失败");
            }
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public ActionResult<ApiResponse<string>> HealthCheck()
        {
            return Ok(ApiResponse<string>.SuccessResponse("Bearing API is running"));
        }

        // 辅助方法
        private List<string> GetBrandSuggestions(string query)
        {
            var brands = new List<string> { "SKF", "NSK", "FAG", "TIMKEN", "NTN", "KOYO", "NACHI", "ZWZ", "HRB", "LYC" };
            return brands.Where(b => b.Contains(query, StringComparison.OrdinalIgnoreCase)).Take(5).ToList();
        }

        private List<string> GetTypeSuggestions(string query)
        {
            var types = Enum.GetNames(typeof(BearingType));
            return types.Where(t => t.Contains(query, StringComparison.OrdinalIgnoreCase)).Take(5).ToList();
        }
    }

    // 辅助请求DTO
    public class ValidateBearingNumberRequest
    {
        [Required(ErrorMessage = "轴承型号是必填的")]
        public string BearingNumber { get; set; } = string.Empty;
    }

    public class UpdateBearingStatusRequest
    {
        [Required(ErrorMessage = "状态是必填的")]
        public BearingStatus Status { get; set; }
    }
}
