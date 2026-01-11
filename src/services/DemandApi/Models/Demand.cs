namespace DemandApi.Models
{
    public class Demand
    {
        public long Id { get; set; }
        public long RequesterId { get; set; }  // 需求方用户ID
        public string RequesterType { get; set; } = string.Empty;  // 需求方类型：Supplier/Buyer
        public string? RequesterCompany { get; set; }

        // 轴承需求信息
        public string BearingNumber { get; set; } = string.Empty;
        public string? Specification { get; set; }
        public string? Material { get; set; }
        public string? Brand { get; set; }
        public string? Standard { get; set; }

        // 需求详情
        public int RequiredQuantity { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinPrice { get; set; }
        public string? DeliveryAddress { get; set; }
        public DateTime? RequiredByDate { get; set; }
        public string? AdditionalRequirements { get; set; }

        // 需求状态
        public DemandStatus Status { get; set; } = DemandStatus.Active;
        public DemandType Type { get; set; } = DemandType.Standard;
        public DemandPriority Priority { get; set; } = DemandPriority.Normal;

        // 匹配信息
        public int TotalMatches { get; set; }
        public int TotalQuotations { get; set; }
        public int TotalViews { get; set; }
        public long? SelectedSupplierId { get; set; }
        public long? SelectedQuotationId { get; set; }

        // 时间戳
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        // 导航属性
        public virtual ICollection<DemandMatch> Matches { get; set; } = new List<DemandMatch>();
        public virtual ICollection<DemandView> Views { get; set; } = new List<DemandView>();
    }

    public class DemandMatch
    {
        public long Id { get; set; }
        public long DemandId { get; set; }
        public long SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;

        // 匹配评分
        public double MatchScore { get; set; }  // 0-100分
        public MatchReason MatchReason { get; set; }
        public string? MatchDetails { get; set; }  // JSON格式的匹配详情

        // 供应商响应
        public bool IsNotified { get; set; }
        public DateTime? NotifiedAt { get; set; }
        public bool HasResponded { get; set; }
        public DateTime? RespondedAt { get; set; }
        public bool IsInterested { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Demand Demand { get; set; } = null!;
    }

    public class DemandView
    {
        public long Id { get; set; }
        public long DemandId { get; set; }
        public long? ViewerId { get; set; }  // 查看者用户ID
        public string ViewerType { get; set; } = string.Empty;  // Supplier/Buyer/Anonymous
        public string? ViewerCompany { get; set; }
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }

        public virtual Demand Demand { get; set; } = null!;
    }

    public enum DemandStatus
    {
        Draft,          // 草稿
        Active,         // 活跃中
        Pending,        // 待处理
        Matched,        // 已匹配
        Negotiating,    // 谈判中
        Closed,         // 已关闭
        Expired,        // 已过期
        Cancelled       // 已取消
    }

    public enum DemandType
    {
        Standard,       // 标准需求
        Urgent,         // 紧急需求
        Bulk,           // 批量采购
        Sample,         // 样品需求
        Custom          // 定制需求
    }

    public enum DemandPriority
    {
        Low,            // 低优先级
        Normal,         // 普通优先级
        High,           // 高优先级
        Critical        // 紧急优先级
    }

    public enum MatchReason
    {
        ExactMatch,             // 精确匹配
        SimilarProduct,        // 类似产品
        LocationProximity,      // 地理位置近
        HistoricalCooperation,  // 历史合作
        PriceAdvantage,         // 价格优势
        QualityMatch,           // 质量匹配
        CapacityMatch          // 产能匹配
    }
}
