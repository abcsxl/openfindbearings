using QuotationApi.Models.DTOs;
using QuotationApi.Models.Entities;

namespace QuotationApi.Services
{
    public interface IQuotationService
    {
        // 基础 CRUD
        Task<QuotationDetailResponse> CreateQuotationAsync(CreateQuotationRequest request);
        Task<QuotationDetailResponse?> GetQuotationAsync(long id);
        Task<QuotationDetailResponse?> GetQuotationByNumberAsync(string quotationNumber);
        Task<PagedResponse<QuotationResponse>> GetQuotationsAsync(QuotationQuery query);
        Task<QuotationDetailResponse> UpdateQuotationAsync(long id, UpdateQuotationRequest request);
        Task<bool> DeleteQuotationAsync(long id);

        // 业务操作
        Task<QuotationDetailResponse> SubmitQuotationAsync(long id, string? submissionNotes = null);
        Task<QuotationDetailResponse> AcceptQuotationAsync(long id, long customerId);
        Task<QuotationDetailResponse> RejectQuotationAsync(long id, string? rejectionReason = null);
        Task<QuotationDetailResponse> WithdrawQuotationAsync(long id, string? reason = null);
        Task<QuotationDetailResponse> ExpireQuotationAsync(long id);
        Task<QuotationDetailResponse> SetRecommendedAsync(long id, bool isRecommended, decimal? matchScore = null);

        // 搜索和查询
        Task<PagedResponse<QuotationResponse>> SearchQuotationsAsync(QuotationSearchRequest request);
        Task<List<QuotationResponse>> GetQuotationsByDemandAsync(long demandId);
        Task<List<QuotationResponse>> GetQuotationsBySupplierAsync(long supplierId);
        Task<List<QuotationResponse>> GetPendingQuotationsAsync();
        Task<List<QuotationResponse>> GetExpiredQuotationsAsync();
        Task<List<QuotationResponse>> GetRecommendedQuotationsAsync(long demandId, int limit = 10);

        // 比较和分析
        Task<QuotationComparisonResponse> CompareQuotationsAsync(long demandId, string bearingNumber);
        Task<QuotationStatisticsResponse> GetQuotationStatisticsAsync();
        Task<QuotationStatisticsResponse> GetSupplierStatisticsAsync(long supplierId);
        Task<Dictionary<string, decimal>> GetPriceTrendsAsync(string bearingNumber, int days = 30);

        // 批量操作
        Task<List<QuotationResponse>> CreateBulkQuotationsAsync(List<CreateQuotationRequest> requests);
        Task<bool> ExpireOldQuotationsAsync(DateTime cutoffDate);
        Task<bool> SendQuotationRemindersAsync();

        // 附件管理
        Task<QuotationAttachmentResponse> AddAttachmentAsync(long quotationId, AddAttachmentRequest request);
        Task<bool> RemoveAttachmentAsync(long attachmentId);
        Task<List<QuotationAttachmentResponse>> GetAttachmentsAsync(long quotationId);

        // 项目管理
        Task<QuotationItemResponse> AddQuotationItemAsync(long quotationId, AddQuotationItemRequest request);
        Task<bool> RemoveQuotationItemAsync(long itemId);
        Task<List<QuotationItemResponse>> GetQuotationItemsAsync(long quotationId);
    }

    public interface IQuotationValidationService
    {
        Task<ValidationResult> ValidateQuotationAsync(CreateQuotationRequest request);
        Task<ValidationResult> ValidateQuotationUpdateAsync(long quotationId, UpdateQuotationRequest request);
        Task<ValidationResult> ValidateQuotationSubmissionAsync(long quotationId);
        Task<ValidationResult> ValidateQuotationAcceptanceAsync(long quotationId, long customerId);
        Task<List<string>> ValidateQuotationNumberAsync(string quotationNumber);
        Task<decimal> CalculateMatchScoreAsync(Quotation quotation, long demandId);
    }

    //public class PagedResponse<T>
    //{
    //    public List<T> Items { get; set; } = new();
    //    public int TotalCount { get; set; }
    //    public int PageNumber { get; set; }
    //    public int PageSize { get; set; }
    //    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    //}

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public string? SuggestedCorrection { get; set; }
    }

    //// 请求 DTO
    //public class AddAttachmentRequest
    //{
    //    public string AttachmentType { get; set; } = string.Empty;
    //    public string FileName { get; set; } = string.Empty;
    //    public string FileUrl { get; set; } = string.Empty;
    //    public string? Description { get; set; }
    //    public long FileSize { get; set; }
    //}

    //public class AddQuotationItemRequest
    //{
    //    public string BearingNumber { get; set; } = string.Empty;
    //    public string? Description { get; set; }
    //    public int Quantity { get; set; }
    //    public decimal UnitPrice { get; set; }
    //    public string? Brand { get; set; }
    //    public string? Material { get; set; }
    //    public string? Standard { get; set; }
    //    public int DisplayOrder { get; set; } = 0;
    //}
}
