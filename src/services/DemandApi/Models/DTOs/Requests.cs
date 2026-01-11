namespace Demand.Models.DTOs
{
    public class CreateDemandRequest
    {
        public string BearingNumber { get; set; } = string.Empty;
        public string? Specification { get; set; }
        public string? Material { get; set; }
        public string? Brand { get; set; }
        public string? Standard { get; set; }

        public int RequiredQuantity { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinPrice { get; set; }
        public string? DeliveryAddress { get; set; }
        public DateTime? RequiredByDate { get; set; }
        public string? AdditionalRequirements { get; set; }

        public DemandType Type { get; set; } = DemandType.Standard;
        public DemandPriority Priority { get; set; } = DemandPriority.Normal;
        public int? ValidityDays { get; set; } = 30;  // 需求有效期
    }

    public class UpdateDemandRequest
    {
        public string? BearingNumber { get; set; }
        public string? Specification { get; set; }
        public string? Material { get; set; }
        public string? Brand { get; set; }
        public int? RequiredQuantity { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinPrice { get; set; }
        public string? DeliveryAddress { get; set; }
        public DateTime? RequiredByDate { get; set; }
        public string? AdditionalRequirements { get; set; }
        public DemandStatus? Status { get; set; }
        public DemandPriority? Priority { get; set; }
    }

    public class DemandQuery
    {
        public string? BearingNumber { get; set; }
        public string? Brand { get; set; }
        public string? Specification { get; set; }
        public DemandStatus? Status { get; set; }
        public DemandType? Type { get; set; }
        public DemandPriority? Priority { get; set; }
        public long? RequesterId { get; set; }
        public string? RequesterType { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public DateTime? ExpiresBefore { get; set; }
        public bool? HasMatches { get; set; }
        public bool? HasQuotations { get; set; }

        // 分页和排序
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class MatchDemandRequest
    {
        public long DemandId { get; set; }
        public bool EnableAutoMatch { get; set; } = true;
        public List<long>? SpecificSupplierIds { get; set; }  // 指定匹配的供应商
        public MatchStrategy Strategy { get; set; } = MatchStrategy.Intelligent;
    }

    public class DemandSearchRequest
    {
        public string? Keywords { get; set; }
        public string? BearingNumber { get; set; }
        public string? Brand { get; set; }
        public string? Specification { get; set; }
        public string? Material { get; set; }
        public string? Standard { get; set; }
        public int? MinQuantity { get; set; }
        public int? MaxQuantity { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? Country { get; set; }
        public bool? InStockOnly { get; set; } = true;

        public string? SortBy { get; set; } = "Relevance";
        public bool SortDescending { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public enum MatchStrategy
    {
        Intelligent,    // 智能匹配
        Quick,         // 快速匹配
        Comprehensive  // 全面匹配
    }
}
