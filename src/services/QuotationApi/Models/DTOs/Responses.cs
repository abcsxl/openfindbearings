using QuotationApi.Models.Entities;

namespace QuotationApi.Models.DTOs
{
    // 基础响应类
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> SuccessResponse(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message ?? "Success"
            };
        }

        public static ApiResponse<T> ErrorResponse(string message)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message
            };
        }
    }


    // 分页响应类
    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    }

    // 报价单基础响应
    public class QuotationResponse
    {
        public long Id { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public long DemandId { get; set; }
        public long SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierContact { get; set; }
        public string? SupplierPhone { get; set; }
        public string? SupplierEmail { get; set; }
        public string BearingNumber { get; set; } = string.Empty;
        public string? BearingName { get; set; }
        public string? Brand { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "CNY";
        public int DeliveryDays { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? Incoterms { get; set; }
        public string? QualityStandard { get; set; }
        public int? WarrantyMonths { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRecommended { get; set; }
        public decimal? MatchScore { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? RejectedAt { get; set; }

        public string? CertificateRequirements { get; set; }
    }

    // 报价单详情响应
    public class QuotationDetailResponse
    {
        public long Id { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public long DemandId { get; set; }
        public long SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierContact { get; set; }
        public string? SupplierPhone { get; set; }
        public string? SupplierEmail { get; set; }
        public string BearingNumber { get; set; } = string.Empty;
        public string? BearingName { get; set; }
        public string? Brand { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "CNY";
        public int DeliveryDays { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? Incoterms { get; set; }
        public string? QualityStandard { get; set; }
        public int? WarrantyMonths { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRecommended { get; set; }
        public decimal? MatchScore { get; set; }
        public string? Notes { get; set; }
        public string? CertificateRequirements { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public List<QuotationItemResponse> Items { get; set; } = new List<QuotationItemResponse>();
        public List<QuotationAttachmentResponse> Attachments { get; set; } = new List<QuotationAttachmentResponse>();
    }

    // 报价项目响应
    public class QuotationItemResponse
    {
        public long Id { get; set; }
        public string BearingNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Brand { get; set; }
        public string? Material { get; set; }
        public string? Standard { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // 报价附件响应
    public class QuotationAttachmentResponse
    {
        public long Id { get; set; }
        public string AttachmentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    // 报价比较响应
    public class QuotationComparisonResponse
    {
        public long DemandId { get; set; }
        public string BearingNumber { get; set; } = string.Empty;
        public List<SupplierQuotation> SupplierQuotations { get; set; } = new List<SupplierQuotation>();
        public int TotalSuppliers { get; set; }
        public decimal? LowestPrice { get; set; }
        public decimal? HighestPrice { get; set; }
        public decimal? AveragePrice { get; set; }
        public decimal? PriceRange => HighestPrice - LowestPrice;
    }

    public class SupplierQuotation
    {
        public long SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int DeliveryDays { get; set; }
        public string? QualityStandard { get; set; }
        public int? WarrantyMonths { get; set; }
        public decimal? MatchScore { get; set; }
        public bool IsRecommended { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // 报价统计响应
    public class QuotationStatisticsResponse
    {
        public int TotalQuotations { get; set; }
        public int PendingQuotations { get; set; }
        public int SubmittedQuotations { get; set; }
        public int AcceptedQuotations { get; set; }
        public int RejectedQuotations { get; set; }
        public int ExpiredQuotations { get; set; }
        public decimal TotalQuotedAmount { get; set; }
        public decimal AverageUnitPrice { get; set; }
        public decimal AverageResponseTimeHours { get; set; }
        public decimal AcceptanceRate { get; set; }
        public Dictionary<string, int> StatusDistribution { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> BrandDistribution { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> MonthlyDistribution { get; set; } = new Dictionary<string, int>();
        public List<TopSupplierStat> TopSuppliers { get; set; } = new List<TopSupplierStat>();
        public List<TopBearingStat> TopBearings { get; set; } = new List<TopBearingStat>();
        public Dictionary<string, int> QuotationsByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> QuotationsByMonth { get; set; } = new Dictionary<string, int>();
    }

    public class TopSupplierStat
    {
        public long SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int TotalQuotations { get; set; }
        public int AcceptedQuotations { get; set; }
        public decimal AcceptanceRate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class TopBearingStat
    {
        public string BearingNumber { get; set; } = string.Empty;
        public int QuotationCount { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int TotalQuantity { get; set; }
    }

    // 价格趋势响应
    public class PriceTrendResponse
    {
        public string BearingNumber { get; set; } = string.Empty;
        public string BearingName { get; set; } = string.Empty;
        public List<PricePoint> PricePoints { get; set; } = new List<PricePoint>();
        public decimal CurrentPrice { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal PriceChangePercentage { get; set; }
        public string TrendDirection { get; set; } = "stable"; // rising, falling, stable
    }

    public class PricePoint
    {
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public int QuotationCount { get; set; }
    }

    // 验证结果响应
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public string? SuggestedCorrection { get; set; }
        public decimal? MatchScore { get; set; }
        public bool IsRecommended { get; set; }
    }

    // 批量操作响应
    public class BulkOperationResponse
    {
        public int TotalCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<BulkOperationItem> Results { get; set; } = new List<BulkOperationItem>();
        public DateTime CompletedAt { get; set; }
    }

    public class BulkOperationItem
    {
        public long Id { get; set; }
        public string Reference { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }
    }

    // 健康检查响应
    public class HealthCheckResponse
    {
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Service { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public Dictionary<string, HealthCheckDetail> Details { get; set; } = new Dictionary<string, HealthCheckDetail>();
    }

    public class HealthCheckDetail
    {
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TimeSpan? Duration { get; set; }
        public Dictionary<string, object>? Data { get; set; }
    }

    // 搜索建议响应
    public class SearchSuggestionResponse
    {
        public List<string> BearingNumbers { get; set; } = new List<string>();
        public List<string> Brands { get; set; } = new List<string>();
        public List<string> Suppliers { get; set; } = new List<string>();
        public List<RecentSearch> RecentSearches { get; set; } = new List<RecentSearch>();
    }

    public class RecentSearch
    {
        public string Query { get; set; } = string.Empty;
        public DateTime SearchedAt { get; set; }
        public int ResultCount { get; set; }
    }

    // 导出响应
    public class ExportResponse
    {
        public string ExportId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string Format { get; set; } = "xlsx";
        public int RecordCount { get; set; }
        public DateTime ExportedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }


    public class TopSupplier
    {
        public long SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int QuotationCount { get; set; }
        public int AcceptedCount { get; set; }
        public decimal AcceptanceRate { get; set; }
    }

    public class TopBearing
    {
        public string BearingNumber { get; set; } = string.Empty;
        public int QuotationCount { get; set; }
        public decimal AveragePrice { get; set; }
    }

    public class QuotationComparisonItem
    {
        public long QuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public long SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int DeliveryDays { get; set; }
        public decimal? MatchScore { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class Statistics
    {
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal AvgPrice { get; set; }
        public int Count { get; set; }
    }
}
