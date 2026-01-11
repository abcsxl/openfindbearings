using BearingApi.Models.DTOs;
using BearingApi.Models.Entities;

namespace BearingApi.Services
{
    public interface IBearingService
    {
        // Bearing CRUD
        Task<Bearing> CreateBearingAsync(CreateBearingRequest request);
        Task<Bearing?> GetBearingAsync(long id);
        Task<BearingDetailResponse?> GetBearingDetailAsync(long id);
        Task<PagedResponse<BearingResponse>> GetBearingsAsync(BearingQuery query);
        Task<Bearing> UpdateBearingAsync(long id, UpdateBearingRequest request);
        Task<bool> DeleteBearingAsync(long id);

        // 搜索功能
        Task<PagedResponse<BearingResponse>> SearchBearingsAsync(BearingSearchRequest request);
        Task<List<BearingResponse>> FindSimilarBearingsAsync(string bearingNumber, int limit = 10);
        Task<List<BearingResponse>> GetBearingsByParametersAsync(BearingParameters parameters);

        // 规格管理
        Task<List<BearingSpecificationResponse>> GetBearingSpecificationsAsync(long bearingId);
        Task<BearingSpecification> AddSpecificationAsync(long bearingId, AddSpecificationRequest request);
        Task<bool> RemoveSpecificationAsync(long specId);

        // 图片管理
        Task<List<BearingImageResponse>> GetBearingImagesAsync(long bearingId);
        Task<BearingImage> AddImageAsync(long bearingId, AddImageRequest request);
        Task<bool> SetPrimaryImageAsync(long imageId);
        Task<bool> RemoveImageAsync(long imageId);

        // 文档管理
        Task<List<BearingDocumentResponse>> GetBearingDocumentsAsync(long bearingId);
        Task<BearingDocument> AddDocumentAsync(long bearingId, AddDocumentRequest request);
        Task<bool> RemoveDocumentAsync(long documentId);

        // 验证和状态管理
        Task<Bearing> VerifyBearingAsync(long id, VerificationLevel level, string? notes = null);
        Task<Bearing> UpdateBearingStatusAsync(long id, BearingStatus status);

        // 统计和分析
        Task<BearingStatistics> GetBearingStatisticsAsync();
        Task<List<BearingTrend>> GetBearingTrendsAsync(DateTime startDate, DateTime endDate);
        Task<List<PopularBearing>> GetPopularBearingsAsync(int limit = 10);

        // 数据导入/导出
        Task<ImportResult> ImportBearingsAsync(Stream dataStream, string fileType);
        Task<Stream> ExportBearingsAsync(BearingQuery query, string format);
    }

    public interface IBearingValidationService
    {
        Task<ValidationResult> ValidateBearingNumberAsync(string bearingNumber);
        Task<ValidationResult> ValidateBearingParametersAsync(BearingParameters parameters);
        Task<List<string>> SuggestBearingNumbersAsync(string partialNumber);
        Task<List<string>> GetStandardBearingNumbersAsync();
    }

    public class BearingStatistics
    {
        public int TotalBearings { get; set; }
        public int VerifiedBearings { get; set; }
        public int ActiveBearings { get; set; }
        public Dictionary<BearingType, int> TypeDistribution { get; set; } = new();
        public Dictionary<string, int> BrandDistribution { get; set; } = new();
        public Dictionary<string, int> PopularSearches { get; set; } = new();
        public int TotalViews { get; set; }
        public int TotalSearches { get; set; }
    }

    public class BearingTrend
    {
        public DateTime Date { get; set; }
        public int NewBearings { get; set; }
        public int UpdatedBearings { get; set; }
        public int TotalViews { get; set; }
        public int TotalSearches { get; set; }
    }

    public class PopularBearing
    {
        public long BearingId { get; set; }
        public string BearingNumber { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public int ViewCount { get; set; }
        public int SearchCount { get; set; }
        public int SupplierCount { get; set; }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public string? SuggestedCorrection { get; set; }
    }

    public class ImportResult
    {
        public int TotalRecords { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<ImportError> Errors { get; set; } = new();
        public TimeSpan Duration { get; set; }
    }

    public class ImportError
    {
        public int RowNumber { get; set; }
        public string? BearingNumber { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string? FieldName { get; set; }
    }
}
