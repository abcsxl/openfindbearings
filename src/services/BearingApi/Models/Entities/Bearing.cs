namespace BearingApi.Models.Entities
{
    public class Bearing
    {
        public long Id { get; set; }

        // 基本信息
        public string BearingNumber { get; set; } = string.Empty;  // 轴承型号
        public string? AlternateNumbers { get; set; }              // 替代型号
        public string? StandardNumber { get; set; }               // 标准号
        public string DisplayName { get; set; } = string.Empty;    // 显示名称

        // 分类信息
        public BearingType Type { get; set; } = BearingType.DeepGrooveBallBearing;
        public BearingCategory Category { get; set; } = BearingCategory.Standard;
        public string? ApplicationArea { get; set; }               // 应用领域

        // 规格参数
        public decimal? InnerDiameter { get; set; }                // 内径(mm)
        public decimal? OuterDiameter { get; set; }               // 外径(mm)
        public decimal? Width { get; set; }                        // 宽度(mm)
        public decimal? Weight { get; set; }                      // 重量(kg)
        public decimal? DynamicLoadRating { get; set; }            // 动载荷(kN)
        public decimal? StaticLoadRating { get; set; }             // 静载荷(kN)
        public decimal? LimitingSpeed { get; set; }               // 极限转速(rpm)

        // 材料信息
        public string? Material { get; set; }                      // 材料
        public string? CageMaterial { get; set; }                  // 保持架材料
        public string? SealType { get; set; }                     // 密封类型
        public string? Lubrication { get; set; }                   // 润滑方式

        // 品牌和标准
        public string? Brand { get; set; }                         // 品牌
        public string? Origin { get; set; }                        // 产地
        public string? Standard { get; set; }                      // 标准(ISO/DIN等)

        // 技术文档
        public string? Specification { get; set; }                 // 技术规格
        public string? InstallationGuide { get; set; }            // 安装指南
        public string? MaintenanceGuide { get; set; }              // 维护指南

        // 多媒体
        public string? ImageUrl { get; set; }                       // 图片URL
        public string? DrawingUrl { get; set; }                     // 图纸URL
        public string? CatalogUrl { get; set; }                     // 产品手册URL

        // 状态管理
        public BearingStatus Status { get; set; } = BearingStatus.Active;
        public bool IsVerified { get; set; } = false;              // 是否已验证
        public VerificationLevel VerificationLevel { get; set; } = VerificationLevel.Pending;

        // 统计信息
        public int ViewCount { get; set; } = 0;
        public int SearchCount { get; set; } = 0;
        public int SupplierCount { get; set; } = 0;                // 供应商数量


        public DateTime? ExpiresAt { get; set; }  // 新增
        public double Rating { get; set; } = 0.0;  // 新增
        public long? RequesterId { get; set; }  // 新增
        public string? RequesterType { get; set; }  // 新增
        public int TotalMatches { get; set; } = 0;  // 新增
        public int TotalQuotations { get; set; } = 0;  // 新增
        public int TotalViews { get; set; } = 0;  // 新增


        // 时间戳
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? VerifiedAt { get; set; }

        // 导航属性
        public virtual ICollection<BearingSpecification> Specifications { get; set; } = new List<BearingSpecification>();
        public virtual ICollection<BearingImage> Images { get; set; } = new List<BearingImage>();
        public virtual ICollection<BearingDocument> Documents { get; set; } = new List<BearingDocument>();
    }

    public class BearingSpecification
    {
        public long Id { get; set; }
        public long BearingId { get; set; }

        public string ParameterName { get; set; } = string.Empty;  // 参数名称
        public string ParameterValue { get; set; } = string.Empty;  // 参数值
        public string? Unit { get; set; }                          // 单位
        public string? Description { get; set; }                   // 参数说明
        public int DisplayOrder { get; set; } = 0;                // 显示顺序

        public virtual Bearing Bearing { get; set; } = null!;
    }

    public class BearingImage
    {
        public long Id { get; set; }
        public long BearingId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsPrimary { get; set; } = false;               // 是否主图

        public virtual Bearing Bearing { get; set; } = null!;
    }

    public class BearingDocument
    {
        public long Id { get; set; }
        public long BearingId { get; set; }

        public string DocumentType { get; set; } = string.Empty;   // 文档类型
        public string DocumentUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; } = 0;
        public string? Description { get; set; }

        public virtual Bearing Bearing { get; set; } = null!;
    }
}
