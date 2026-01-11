using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuotationApi.Models.Entities
{
    public class Quotation
    {
        public long Id { get; set; }

        // 基础信息
        [Required]
        [StringLength(50)]
        public string QuotationNumber { get; set; } = string.Empty;  // 报价单号

        [Required]
        public long DemandId { get; set; }  // 关联的需求ID

        [Required]
        public long SupplierId { get; set; }  // 供应商ID

        [StringLength(200)]
        public string? SupplierName { get; set; }  // 供应商名称（冗余存储）

        [StringLength(100)]
        public string? SupplierContact { get; set; }  // 供应商联系人

        [StringLength(20)]
        public string? SupplierPhone { get; set; }  // 供应商电话

        [StringLength(100)]
        public string? SupplierEmail { get; set; }  // 供应商邮箱

        // 报价信息
        [Required]
        [StringLength(100)]
        public string BearingNumber { get; set; } = string.Empty;  // 轴承型号

        [StringLength(200)]
        public string? BearingName { get; set; }  // 轴承名称

        [StringLength(100)]
        public string? Brand { get; set; }  // 品牌

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }  // 单价

        [Required]
        public int Quantity { get; set; }  // 数量

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }  // 总金额

        [StringLength(3)]
        public string Currency { get; set; } = "CNY";  // 货币

        // 交货信息
        public int DeliveryDays { get; set; } = 7;  // 交货天数
        public DateTime? EstimatedDeliveryDate { get; set; }  // 预计交货日期

        [StringLength(500)]
        public string? DeliveryAddress { get; set; }  // 交货地址

        [StringLength(100)]
        public string? Incoterms { get; set; }  // 贸易术语（FOB, CIF等）

        // 质量保证
        [StringLength(100)]
        public string? QualityStandard { get; set; }  // 质量标准

        [StringLength(200)]
        public string? CertificateRequirements { get; set; }  // 证书要求

        public int WarrantyMonths { get; set; } = 12;  // 质保月数

        // 状态管理
        public QuotationStatus Status { get; set; } = QuotationStatus.Pending;  // 报价状态
        public QuotationType Type { get; set; } = QuotationType.Standard;  // 报价类型

        [StringLength(1000)]
        public string? Notes { get; set; }  // 备注

        public bool IsRecommended { get; set; } = false;  // 是否推荐
        public decimal? MatchScore { get; set; }  // 匹配分数

        // 时间戳
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }  // 报价有效期
        public DateTime? SubmittedAt { get; set; }  // 提交时间
        public DateTime? AcceptedAt { get; set; }  // 接受时间
        public DateTime? RejectedAt { get; set; }  // 拒绝时间

        // 导航属性
        public virtual ICollection<QuotationItem> Items { get; set; } = new List<QuotationItem>();
        public virtual ICollection<QuotationAttachment> Attachments { get; set; } = new List<QuotationAttachment>();
    }

    //public class QuotationItem
    //{
    //    public long Id { get; set; }
    //    public long QuotationId { get; set; }

    //    [Required]
    //    [StringLength(100)]
    //    public string BearingNumber { get; set; } = string.Empty;

    //    [StringLength(200)]
    //    public string? Description { get; set; }

    //    [Required]
    //    public int Quantity { get; set; }

    //    [Required]
    //    [Column(TypeName = "decimal(18,2)")]
    //    public decimal UnitPrice { get; set; }

    //    [Column(TypeName = "decimal(18,2)")]
    //    public decimal TotalPrice => UnitPrice * Quantity;

    //    [StringLength(100)]
    //    public string? Brand { get; set; }

    //    [StringLength(100)]
    //    public string? Material { get; set; }

    //    [StringLength(50)]
    //    public string? Standard { get; set; }

    //    public int DisplayOrder { get; set; }

    //    public virtual Quotation Quotation { get; set; } = null!;
    //}

    public class QuotationAttachment
    {
        public long Id { get; set; }
        public long QuotationId { get; set; }

        [Required]
        [StringLength(50)]
        public string AttachmentType { get; set; } = string.Empty;  // 附件类型

        [Required]
        [StringLength(200)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FileUrl { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public virtual Quotation Quotation { get; set; } = null!;
    }
}
