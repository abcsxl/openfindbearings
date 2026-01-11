using BearingApi.Data;
using BearingApi.Models.DTOs;
using BearingApi.Models.Entities;
using Dapr.Client;
using Microsoft.EntityFrameworkCore;

namespace BearingApi.Services
{
    public class BearingService : IBearingService
    {
        private readonly IBearingRepository _repository;
        private readonly IBearingValidationService _validationService;
        private readonly BearingDbContext _context;
        private readonly DaprClient _daprClient;
        private readonly ILogger<BearingService> _logger;

        public BearingService(
            IBearingRepository repository,
            IBearingValidationService validationService,
            BearingDbContext context,
            DaprClient daprClient,
            ILogger<BearingService> logger)
        {
            _repository = repository;
            _validationService = validationService;
            _context = context;
            _daprClient = daprClient;
            _logger = logger;
        }

        public async Task<Bearing> CreateBearingAsync(CreateBearingRequest request)
        {
            // 验证轴承型号
            var validationResult = await _validationService.ValidateBearingNumberAsync(request.BearingNumber);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException($"轴承型号验证失败: {string.Join(", ", validationResult.Errors)}");
            }

            var bearing = new Bearing
            {
                BearingNumber = request.BearingNumber,
                AlternateNumbers = request.AlternateNumbers,
                StandardNumber = request.StandardNumber,
                DisplayName = request.DisplayName,
                Type = request.Type,
                Category = request.Category,
                ApplicationArea = request.ApplicationArea,
                InnerDiameter = request.InnerDiameter,
                OuterDiameter = request.OuterDiameter,
                Width = request.Width,
                Weight = request.Weight,
                DynamicLoadRating = request.DynamicLoadRating,
                StaticLoadRating = request.StaticLoadRating,
                LimitingSpeed = request.LimitingSpeed,
                Material = request.Material,
                CageMaterial = request.CageMaterial,
                SealType = request.SealType,
                Lubrication = request.Lubrication,
                Brand = request.Brand,
                Origin = request.Origin,
                Standard = request.Standard,
                Specification = request.Specification,
                InstallationGuide = request.InstallationGuide,
                MaintenanceGuide = request.MaintenanceGuide,
                ImageUrl = request.ImageUrl,
                Status = BearingStatus.Active,
                IsVerified = request.IsVerified ?? false,
                VerificationLevel = request.IsVerified == true ? VerificationLevel.Basic : VerificationLevel.Pending
            };

            var result = await _repository.AddAsync(bearing);

            // 发布轴承创建事件
            await _daprClient.PublishEventAsync("pubsub", "bearing-created",
                new BearingCreatedEvent
                {
                    BearingId = result.Id,
                    BearingNumber = result.BearingNumber,
                    Brand = result.Brand,
                    Type = result.Type,
                    CreatedAt = result.CreatedAt
                });

            _logger.LogInformation("轴承创建成功: {BearingNumber}", result.BearingNumber);
            return result;
        }

        public async Task<Bearing?> GetBearingAsync(long id)
        {
            var bearing = await _repository.GetByIdAsync(id);
            if (bearing != null)
            {
                await _repository.IncrementViewCountAsync(id);
            }
            return bearing;
        }

        public async Task<BearingDetailResponse?> GetBearingDetailAsync(long id)
        {
            var bearing = await _repository.GetByIdWithIncludesAsync(id);
            if (bearing == null) return null;

            // 增加查看计数
            await _repository.IncrementViewCountAsync(id);

            return new BearingDetailResponse
            {
                Id = bearing.Id,
                BearingNumber = bearing.BearingNumber,
                AlternateNumbers = bearing.AlternateNumbers,
                StandardNumber = bearing.StandardNumber,
                DisplayName = bearing.DisplayName,
                Type = bearing.Type,
                Category = bearing.Category,
                ApplicationArea = bearing.ApplicationArea,
                InnerDiameter = bearing.InnerDiameter,
                OuterDiameter = bearing.OuterDiameter,
                Width = bearing.Width,
                Weight = bearing.Weight,
                DynamicLoadRating = bearing.DynamicLoadRating,
                StaticLoadRating = bearing.StaticLoadRating,
                LimitingSpeed = bearing.LimitingSpeed,
                Material = bearing.Material,
                CageMaterial = bearing.CageMaterial,
                SealType = bearing.SealType,
                Lubrication = bearing.Lubrication,
                Brand = bearing.Brand,
                Origin = bearing.Origin,
                Standard = bearing.Standard,
                Specification = bearing.Specification,
                InstallationGuide = bearing.InstallationGuide,
                MaintenanceGuide = bearing.MaintenanceGuide,
                ImageUrl = bearing.ImageUrl,
                Status = bearing.Status,
                IsVerified = bearing.IsVerified,
                VerificationLevel = bearing.VerificationLevel,
                ViewCount = bearing.ViewCount,
                SearchCount = bearing.SearchCount,
                SupplierCount = bearing.SupplierCount,
                CreatedAt = bearing.CreatedAt,
                UpdatedAt = bearing.UpdatedAt,
                VerifiedAt = bearing.VerifiedAt,
                Specifications = bearing.Specifications.Select(s => new BearingSpecificationResponse
                {
                    Id = s.Id,
                    ParameterName = s.ParameterName,
                    ParameterValue = s.ParameterValue,
                    Unit = s.Unit,
                    Description = s.Description,
                    DisplayOrder = s.DisplayOrder
                }).ToList(),
                Images = bearing.Images.Select(i => new BearingImageResponse
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    ThumbnailUrl = i.ThumbnailUrl,
                    Description = i.Description,
                    DisplayOrder = i.DisplayOrder,
                    IsPrimary = i.IsPrimary
                }).ToList(),
                Documents = bearing.Documents.Select(d => new BearingDocumentResponse
                {
                    Id = d.Id,
                    DocumentType = d.DocumentType,
                    DocumentUrl = d.DocumentUrl,
                    FileName = d.FileName,
                    FileSize = d.FileSize,
                    Description = d.Description
                }).ToList()
            };
        }

        public async Task<PagedResponse<BearingResponse>> GetBearingsAsync(BearingQuery query)
        {
            var bearings = await _repository.GetListAsync(query);
            var totalCount = await _repository.GetCountAsync(query);

            return new PagedResponse<BearingResponse>
            {
                Items = bearings.Select(b => new BearingResponse
                {
                    Id = b.Id,
                    BearingNumber = b.BearingNumber,
                    DisplayName = b.DisplayName,
                    Brand = b.Brand,
                    Type = b.Type,
                    Category = b.Category,
                    InnerDiameter = b.InnerDiameter,
                    OuterDiameter = b.OuterDiameter,
                    Width = b.Width,
                    Status = b.Status,
                    IsVerified = b.IsVerified,
                    ViewCount = b.ViewCount,
                    SupplierCount = b.SupplierCount,
                    CreatedAt = b.CreatedAt
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<PagedResponse<BearingResponse>> SearchBearingsAsync(BearingSearchRequest request)
        {
            var bearings = await _repository.SearchAsync(request);
            var totalCount = bearings.Count; // 注意：这里需要优化计数查询

            // 增加搜索计数
            foreach (var bearing in bearings)
            {
                await _repository.IncrementSearchCountAsync(bearing.Id);
            }

            return new PagedResponse<BearingResponse>
            {
                Items = bearings.Select(b => new BearingResponse
                {
                    Id = b.Id,
                    BearingNumber = b.BearingNumber,
                    DisplayName = b.DisplayName,
                    Brand = b.Brand,
                    Type = b.Type,
                    Category = b.Category,
                    InnerDiameter = b.InnerDiameter,
                    OuterDiameter = b.OuterDiameter,
                    Width = b.Width,
                    Status = b.Status,
                    IsVerified = b.IsVerified,
                    ViewCount = b.ViewCount,
                    SupplierCount = b.SupplierCount,
                    CreatedAt = b.CreatedAt
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<List<BearingResponse>> FindSimilarBearingsAsync(string bearingNumber, int limit = 10)
        {
            var bearings = await _repository.FindSimilarBearingsAsync(bearingNumber, limit);
            return bearings.Select(b => new BearingResponse
            {
                Id = b.Id,
                BearingNumber = b.BearingNumber,
                DisplayName = b.DisplayName,
                Brand = b.Brand,
                Type = b.Type,
                InnerDiameter = b.InnerDiameter,
                OuterDiameter = b.OuterDiameter,
                Width = b.Width,
                Status = b.Status,
                IsVerified = b.IsVerified,
                ViewCount = b.ViewCount,
                SupplierCount = b.SupplierCount,
                CreatedAt = b.CreatedAt
            }).ToList();
        }

        // 其他方法实现...
        public async Task<Bearing> UpdateBearingAsync(long id, UpdateBearingRequest request)
        {
            var bearing = await _repository.GetByIdAsync(id);
            if (bearing == null)
                throw new KeyNotFoundException("轴承不存在");

            // 更新字段
            if (!string.IsNullOrEmpty(request.BearingNumber))
                bearing.BearingNumber = request.BearingNumber;
            if (!string.IsNullOrEmpty(request.DisplayName))
                bearing.DisplayName = request.DisplayName;
            if (request.InnerDiameter.HasValue)
                bearing.InnerDiameter = request.InnerDiameter.Value;
            // ... 其他字段更新

            return await _repository.UpdateAsync(bearing);
        }

        public async Task<BearingStatistics> GetBearingStatisticsAsync()
        {
            var totalBearings = await _repository.GetTotalBearingsCountAsync();
            var typeDistribution = await _repository.GetBearingStatsByTypeAsync();
            var popularBrands = await _repository.GetPopularBrandsAsync(10);

            return new BearingStatistics
            {
                TotalBearings = totalBearings,
                VerifiedBearings = await _context.Bearings.CountAsync(b => b.IsVerified),
                ActiveBearings = await _context.Bearings.CountAsync(b => b.Status == BearingStatus.Active),
                TypeDistribution = typeDistribution,
                BrandDistribution = popularBrands,
                TotalViews = await _context.Bearings.SumAsync(b => b.ViewCount),
                TotalSearches = await _context.Bearings.SumAsync(b => b.SearchCount)
            };
        }

        // 简化其他方法实现...
        public Task<bool> DeleteBearingAsync(long id) => _repository.DeleteAsync(id);
        public Task<List<BearingSpecificationResponse>> GetBearingSpecificationsAsync(long bearingId)
            => Task.FromResult(new List<BearingSpecificationResponse>());
        public Task<Bearing> VerifyBearingAsync(long id, VerificationLevel level, string? notes = null)
            => Task.FromResult(new Bearing());

        // 占位符实现
        public Task<List<BearingResponse>> GetBearingsByParametersAsync(BearingParameters parameters)
            => Task.FromResult(new List<BearingResponse>());
        public Task<BearingSpecification> AddSpecificationAsync(long bearingId, AddSpecificationRequest request)
            => Task.FromResult(new BearingSpecification());
        public Task<bool> RemoveSpecificationAsync(long specId) => Task.FromResult(true);
        public Task<List<BearingImageResponse>> GetBearingImagesAsync(long bearingId)
            => Task.FromResult(new List<BearingImageResponse>());
        public Task<BearingImage> AddImageAsync(long bearingId, AddImageRequest request)
            => Task.FromResult(new BearingImage());
        public Task<bool> SetPrimaryImageAsync(long imageId) => Task.FromResult(true);
        public Task<bool> RemoveImageAsync(long imageId) => Task.FromResult(true);
        public Task<List<BearingDocumentResponse>> GetBearingDocumentsAsync(long bearingId)
            => Task.FromResult(new List<BearingDocumentResponse>());
        public Task<BearingDocument> AddDocumentAsync(long bearingId, AddDocumentRequest request)
            => Task.FromResult(new BearingDocument());
        public Task<bool> RemoveDocumentAsync(long documentId) => Task.FromResult(true);
        public Task<Bearing> UpdateBearingStatusAsync(long id, BearingStatus status)
            => Task.FromResult(new Bearing());
        public Task<List<BearingTrend>> GetBearingTrendsAsync(DateTime startDate, DateTime endDate)
            => Task.FromResult(new List<BearingTrend>());
        public Task<List<PopularBearing>> GetPopularBearingsAsync(int limit = 10)
            => Task.FromResult(new List<PopularBearing>());
        public Task<ImportResult> ImportBearingsAsync(Stream dataStream, string fileType)
            => Task.FromResult(new ImportResult());
        public Task<Stream> ExportBearingsAsync(BearingQuery query, string format)
            => Task.FromResult(Stream.Null);
    }
}
