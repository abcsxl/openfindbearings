using BearingApi.Models.Entities;

namespace BearingApi.Models.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }

        public static ApiResponse<T> SuccessResponse(T data) => new()
        {
            Success = true,
            Data = data
        };

        public static ApiResponse<T> ErrorResponse(string message) => new()
        {
            Success = false,
            Message = message
        };
    }

    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    }

    public class BearingResponse
    {
        public long Id { get; set; }
        public string BearingNumber { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public BearingType Type { get; set; }
        public BearingCategory Category { get; set; }
        public decimal? InnerDiameter { get; set; }
        public decimal? OuterDiameter { get; set; }
        public decimal? Width { get; set; }
        public BearingStatus Status { get; set; }
        public bool IsVerified { get; set; }
        public int ViewCount { get; set; }
        public int SupplierCount { get; set; }
        public DateTime CreatedAt { get; set; }

        public int SearchCount { get; set; }
    }

    public class BearingDetailResponse : BearingResponse
    {
        public string? AlternateNumbers { get; set; }
        public string? StandardNumber { get; set; }
        public string? ApplicationArea { get; set; }
        public decimal? Weight { get; set; }
        public decimal? DynamicLoadRating { get; set; }
        public decimal? StaticLoadRating { get; set; }
        public decimal? LimitingSpeed { get; set; }
        public string? Material { get; set; }
        public string? CageMaterial { get; set; }
        public string? SealType { get; set; }
        public string? Lubrication { get; set; }
        public string? Origin { get; set; }
        public string? Standard { get; set; }
        public string? Specification { get; set; }
        public string? InstallationGuide { get; set; }
        public string? MaintenanceGuide { get; set; }
        public string? ImageUrl { get; set; }
        public VerificationLevel VerificationLevel { get; set; }
        public int SearchCount { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }

        public List<BearingSpecificationResponse> Specifications { get; set; } = new();
        public List<BearingImageResponse> Images { get; set; } = new();
        public List<BearingDocumentResponse> Documents { get; set; } = new();
    }

    public class BearingSpecificationResponse
    {
        public long Id { get; set; }
        public string ParameterName { get; set; } = string.Empty;
        public string ParameterValue { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class BearingImageResponse
    {
        public long Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class BearingDocumentResponse
    {
        public long Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? Description { get; set; }
    }

    public class BearingStatisticsResponse
    {
        public int TotalBearings { get; set; }
        public int VerifiedBearings { get; set; }
        public int ActiveBearings { get; set; }
        public int TotalViews { get; set; }
        public int TotalSearches { get; set; }
        public Dictionary<string, int> TypeDistribution { get; set; } = new();
        public Dictionary<string, int> BrandDistribution { get; set; } = new();
        public Dictionary<string, int> PopularSearches { get; set; } = new();
    }

    public class ValidationResponse
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public string? SuggestedCorrection { get; set; }
    }

    public class SearchSuggestionResponse
    {
        public List<string> BearingNumbers { get; set; } = new();
        public List<string> Brands { get; set; } = new();
        public List<string> Types { get; set; } = new();
    }

    public class ImportResultResponse
    {
        public int TotalRecords { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<ImportErrorResponse> Errors { get; set; } = new();
        public TimeSpan Duration { get; set; }
    }

    public class ImportErrorResponse
    {
        public int RowNumber { get; set; }
        public string? BearingNumber { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string? FieldName { get; set; }
    }
}
