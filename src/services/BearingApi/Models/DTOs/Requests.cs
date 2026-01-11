using BearingApi.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace BearingApi.Models.DTOs
{
    public class CreateBearingRequest
    {
        [Required(ErrorMessage = "轴承型号是必填的")]
        [StringLength(50, ErrorMessage = "轴承型号长度不能超过50个字符")]
        public string BearingNumber { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "显示名称长度不能超过200个字符")]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "替代型号长度不能超过500个字符")]
        public string? AlternateNumbers { get; set; }

        [StringLength(50, ErrorMessage = "标准号长度不能超过50个字符")]
        public string? StandardNumber { get; set; }

        [Required(ErrorMessage = "轴承类型是必填的")]
        public BearingType Type { get; set; }

        public BearingCategory Category { get; set; } = BearingCategory.Standard;

        [StringLength(100, ErrorMessage = "应用领域长度不能超过100个字符")]
        public string? ApplicationArea { get; set; }

        // 尺寸参数
        [Range(0.1, 10000, ErrorMessage = "内径值应在0.1-10000之间")]
        public decimal? InnerDiameter { get; set; }

        [Range(0.1, 10000, ErrorMessage = "外径值应在0.1-10000之间")]
        public decimal? OuterDiameter { get; set; }

        [Range(0.1, 1000, ErrorMessage = "宽度值应在0.1-1000之间")]
        public decimal? Width { get; set; }

        [Range(0, 1000, ErrorMessage = "重量值应在0-1000之间")]
        public decimal? Weight { get; set; }

        // 载荷参数
        [Range(0, 1000000, ErrorMessage = "动载荷值应在0-1000000之间")]
        public decimal? DynamicLoadRating { get; set; }

        [Range(0, 1000000, ErrorMessage = "静载荷值应在0-1000000之间")]
        public decimal? StaticLoadRating { get; set; }

        [Range(0, 1000000, ErrorMessage = "极限转速值应在0-1000000之间")]
        public decimal? LimitingSpeed { get; set; }

        // 材料信息
        [StringLength(100, ErrorMessage = "材料长度不能超过100个字符")]
        public string? Material { get; set; }

        [StringLength(100, ErrorMessage = "保持架材料长度不能超过100个字符")]
        public string? CageMaterial { get; set; }

        [StringLength(50, ErrorMessage = "密封类型长度不能超过50个字符")]
        public string? SealType { get; set; }

        [StringLength(100, ErrorMessage = "润滑方式长度不能超过100个字符")]
        public string? Lubrication { get; set; }

        // 品牌和标准
        [StringLength(100, ErrorMessage = "品牌长度不能超过100个字符")]
        public string? Brand { get; set; }

        [StringLength(100, ErrorMessage = "产地长度不能超过100个字符")]
        public string? Origin { get; set; }

        [StringLength(50, ErrorMessage = "标准长度不能超过50个字符")]
        public string? Standard { get; set; }

        // 技术文档
        [StringLength(2000, ErrorMessage = "技术规格长度不能超过2000个字符")]
        public string? Specification { get; set; }

        [StringLength(1000, ErrorMessage = "安装指南长度不能超过1000个字符")]
        public string? InstallationGuide { get; set; }

        [StringLength(1000, ErrorMessage = "维护指南长度不能超过1000个字符")]
        public string? MaintenanceGuide { get; set; }

        // 多媒体
        [Url(ErrorMessage = "图片URL格式不正确")]
        public string? ImageUrl { get; set; }

        public bool? IsVerified { get; set; } = false;
    }

    public class UpdateBearingRequest
    {
        [StringLength(50, ErrorMessage = "轴承型号长度不能超过50个字符")]
        public string? BearingNumber { get; set; }

        [StringLength(200, ErrorMessage = "显示名称长度不能超过200个字符")]
        public string? DisplayName { get; set; }

        [StringLength(500, ErrorMessage = "替代型号长度不能超过500个字符")]
        public string? AlternateNumbers { get; set; }

        public BearingType? Type { get; set; }
        public BearingCategory? Category { get; set; }

        [StringLength(100, ErrorMessage = "应用领域长度不能超过100个字符")]
        public string? ApplicationArea { get; set; }

        // 尺寸参数
        [Range(0.1, 10000, ErrorMessage = "内径值应在0.1-10000之间")]
        public decimal? InnerDiameter { get; set; }

        [Range(0.1, 10000, ErrorMessage = "外径值应在0.1-10000之间")]
        public decimal? OuterDiameter { get; set; }

        [Range(0.1, 1000, ErrorMessage = "宽度值应在0.1-1000之间")]
        public decimal? Width { get; set; }

        // 载荷参数
        [Range(0, 1000000, ErrorMessage = "动载荷值应在0-1000000之间")]
        public decimal? DynamicLoadRating { get; set; }

        [Range(0, 1000000, ErrorMessage = "静载荷值应在0-1000000之间")]
        public decimal? StaticLoadRating { get; set; }

        // 材料信息
        [StringLength(100, ErrorMessage = "材料长度不能超过100个字符")]
        public string? Material { get; set; }

        [StringLength(100, ErrorMessage = "品牌长度不能超过100个字符")]
        public string? Brand { get; set; }

        public BearingStatus? Status { get; set; }
        public bool? IsVerified { get; set; }
    }

    public class BearingQuery
    {
        public string? BearingNumber { get; set; }
        public string? Brand { get; set; }
        public BearingType? Type { get; set; }
        public BearingCategory? Category { get; set; }
        public BearingStatus? Status { get; set; }
        public bool? IsVerified { get; set; }

        public DateTime? CreatedAfter { get; set; }  // 新增
        public DateTime? CreatedBefore { get; set; }  // 新增
        public DateTime? ExpiresBefore { get; set; }  // 新增
        public bool? HasMatches { get; set; }  // 新增
        public bool? HasQuotations { get; set; }  // 新增
        public long? RequesterId { get; set; }  // 新增
        public string? RequesterType { get; set; }  // 新增
        public string? Specification { get; set; }

        // 尺寸筛选
        public decimal? MinInnerDiameter { get; set; }
        public decimal? MaxInnerDiameter { get; set; }
        public decimal? MinOuterDiameter { get; set; }
        public decimal? MaxOuterDiameter { get; set; }

        // 分页和排序
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class BearingSearchRequest
    {
        public string? Keywords { get; set; }
        public string? BearingNumber { get; set; }
        public string? Brand { get; set; }
        public BearingType? Type { get; set; }
        public BearingCategory? Category { get; set; }

        // 尺寸筛选
        public decimal? MinInnerDiameter { get; set; }
        public decimal? MaxInnerDiameter { get; set; }
        public decimal? MinOuterDiameter { get; set; }
        public decimal? MaxOuterDiameter { get; set; }

        // 搜索选项
        public bool SearchAlternateNumbers { get; set; } = true;
        public bool SearchSpecifications { get; set; } = false;

        // 分页和排序
        public string? SortBy { get; set; } = "Relevance";
        public bool SortDescending { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class AddSpecificationRequest
    {
        [Required(ErrorMessage = "参数名称是必填的")]
        [StringLength(100, ErrorMessage = "参数名称长度不能超过100个字符")]
        public string ParameterName { get; set; } = string.Empty;

        [Required(ErrorMessage = "参数值是必填的")]
        [StringLength(200, ErrorMessage = "参数值长度不能超过200个字符")]
        public string ParameterValue { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "单位长度不能超过20个字符")]
        public string? Unit { get; set; }

        [StringLength(500, ErrorMessage = "描述长度不能超过500个字符")]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; } = 0;
    }

    public class AddImageRequest
    {
        [Required(ErrorMessage = "图片URL是必填的")]
        [Url(ErrorMessage = "图片URL格式不正确")]
        public string ImageUrl { get; set; } = string.Empty;

        [Url(ErrorMessage = "缩略图URL格式不正确")]
        public string? ThumbnailUrl { get; set; }

        [StringLength(200, ErrorMessage = "描述长度不能超过200个字符")]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; } = 0;
        public bool IsPrimary { get; set; } = false;
    }

    public class AddDocumentRequest
    {
        [Required(ErrorMessage = "文档类型是必填的")]
        [StringLength(50, ErrorMessage = "文档类型长度不能超过50个字符")]
        public string DocumentType { get; set; } = string.Empty;

        [Required(ErrorMessage = "文档URL是必填的")]
        [Url(ErrorMessage = "文档URL格式不正确")]
        public string DocumentUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "文件名是必填的")]
        [StringLength(200, ErrorMessage = "文件名长度不能超过200个字符")]
        public string FileName { get; set; } = string.Empty;

        [Range(0, long.MaxValue, ErrorMessage = "文件大小必须大于0")]
        public long FileSize { get; set; }

        [StringLength(500, ErrorMessage = "描述长度不能超过500个字符")]
        public string? Description { get; set; }
    }

    public class BearingParameters
    {
        public decimal? InnerDiameter { get; set; }
        public decimal? OuterDiameter { get; set; }
        public decimal? Width { get; set; }
        public decimal? DynamicLoadRating { get; set; }
        public decimal? StaticLoadRating { get; set; }
        public decimal? LimitingSpeed { get; set; }
        public string? Material { get; set; }
        public string? Brand { get; set; }
        public BearingType? Type { get; set; }
    }

    public class VerifyBearingRequest
    {
        [Required(ErrorMessage = "验证级别是必填的")]
        public VerificationLevel Level { get; set; }

        [StringLength(1000, ErrorMessage = "备注长度不能超过1000个字符")]
        public string? Notes { get; set; }
    }
}
