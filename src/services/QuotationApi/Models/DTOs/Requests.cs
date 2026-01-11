using QuotationApi.Models.DTOs;
using QuotationApi.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace QuotationApi.Models.DTOs
{
    // 创建报价请求
    public class CreateQuotationRequest
    {
        [Required]
        public long DemandId { get; set; }

        [Required]
        public long SupplierId { get; set; }

        [Required]
        [StringLength(100)]
        public string BearingNumber { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "CNY";

        [Range(1, 365)]
        public int DeliveryDays { get; set; } = 7;

        [StringLength(500)]
        public string? DeliveryAddress { get; set; }

        [StringLength(50)]
        public string? Incoterms { get; set; }

        [StringLength(100)]
        public string? QualityStandard { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime? ExpiresAt { get; set; }

        [StringLength(50)]
        public string Type { get; set; } = "Standard";
    }

    // 更新报价请求
    public class UpdateQuotationRequest
    {
        [Range(0.01, double.MaxValue)]
        public decimal? UnitPrice { get; set; }

        [Range(1, int.MaxValue)]
        public int? Quantity { get; set; }

        [StringLength(10)]
        public string? Currency { get; set; }

        [Range(1, 365)]
        public int? DeliveryDays { get; set; }

        [StringLength(500)]
        public string? DeliveryAddress { get; set; }

        [StringLength(50)]
        public string? Incoterms { get; set; }

        [StringLength(100)]
        public string? QualityStandard { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime? ExpiresAt { get; set; }
    }

    // 查询报价请求
    public class QuotationQuery
    {
        public long? DemandId { get; set; }
        public long? SupplierId { get; set; }
        //public string? Status { get; set; }
        public QuotationStatus? Status { get; set; }  // ✅ 改为枚举类型
        public bool? IsRecommended { get; set; }
        public string? BearingNumber { get; set; }
        public string? Brand { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinMatchScore { get; set; }
        public decimal? MaxMatchScore { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortOrder { get; set; } = "DESC";

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

       
    }

    // 搜索报价请求
    public class QuotationSearchRequest
    {
        [StringLength(100)]
        public string? BearingNumber { get; set; }

        [StringLength(100)]
        public string? Brand { get; set; }

        [StringLength(100)]
        public string? SupplierName { get; set; }

        public long? DemandId { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        [Range(0, 100)]
        public int? MinMatchScore { get; set; }

        public bool? IsRecommended { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(50)]
        public string? SortBy { get; set; } = "Relevance";

        [StringLength(10)]
        public string? SortOrder { get; set; } = "DESC";

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;
    }

    // 业务操作请求
    public class SubmitQuotationRequest
    {
        [StringLength(1000)]
        public string? SubmissionNotes { get; set; }
    }

    public class AcceptQuotationRequest
    {
        [Required]
        public long CustomerId { get; set; }
    }

    public class RejectQuotationRequest
    {
        [StringLength(1000)]
        public string? RejectionReason { get; set; }
    }

    public class WithdrawQuotationRequest
    {
        [StringLength(1000)]
        public string? Reason { get; set; }
    }

    public class SetRecommendedRequest
    {
        [Required]
        public bool IsRecommended { get; set; }

        [Range(0, 1)]
        public decimal? MatchScore { get; set; }
    }

    // 附件管理请求
    public class AddAttachmentRequest
    {
        [Required]
        [StringLength(50)]
        public string AttachmentType { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FileUrl { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        [Range(0, long.MaxValue)]
        public long FileSize { get; set; }
    }

    // 项目管理请求
    public class AddQuotationItemRequest
    {
        [Required]
        [StringLength(100)]
        public string BearingNumber { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [StringLength(100)]
        public string? Brand { get; set; }

        [StringLength(100)]
        public string? Material { get; set; }

        [StringLength(50)]
        public string? Standard { get; set; }

        public int DisplayOrder { get; set; } = 0;
    }

    // 批量操作请求
    public class BulkQuotationRequest
    {
        [Required]
        [MinLength(1)]
        [MaxLength(100)]
        public List<CreateQuotationRequest> Quotations { get; set; } = new List<CreateQuotationRequest>();

        public bool StopOnError { get; set; } = false;
    }

    // 导出请求
    public class ExportQuotationRequest
    {
        [Required]
        public QuotationQuery Query { get; set; } = new QuotationQuery();

        [Required]
        [StringLength(10)]
        public string Format { get; set; } = "xlsx";

        public List<string>? Columns { get; set; }

        public bool IncludeAttachments { get; set; } = false;
    }

    // 价格趋势查询请求
    public class PriceTrendRequest
    {
        [Required]
        [StringLength(100)]
        public string BearingNumber { get; set; } = string.Empty;

        [Range(1, 365)]
        public int Days { get; set; } = 30;

        public string? Granularity { get; set; } = "daily"; // daily, weekly, monthly
    }
}
