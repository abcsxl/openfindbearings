using Dapr.Client;
using Demand.Data;
using Demand.Models;
using Demand.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Demand.Services
{
    public class DemandService : IDemandService
    {
        private readonly IDemandRepository _repository;
        private readonly IDemandMatchingService _matchingService;
        private readonly DemandDbContext _context;
        private readonly DaprClient _daprClient;
        private readonly ILogger<DemandService> _logger;

        public DemandService(
            IDemandRepository repository,
            IDemandMatchingService matchingService,
            DemandDbContext context,
            DaprClient daprClient,
            ILogger<DemandService> logger)
        {
            _repository = repository;
            _matchingService = matchingService;
            _context = context;
            _daprClient = daprClient;
            _logger = logger;
        }

        public async Task<Models.Demand> CreateDemandAsync(CreateDemandRequest request, long requesterId, string requesterType, string? requesterCompany)
        {
            var demand = new Models.Demand
            {
                RequesterId = requesterId,
                RequesterType = requesterType,
                RequesterCompany = requesterCompany,
                BearingNumber = request.BearingNumber,
                Specification = request.Specification,
                Material = request.Material,
                Brand = request.Brand,
                Standard = request.Standard,
                RequiredQuantity = request.RequiredQuantity,
                MaxPrice = request.MaxPrice,
                MinPrice = request.MinPrice,
                DeliveryAddress = request.DeliveryAddress,
                RequiredByDate = request.RequiredByDate,
                AdditionalRequirements = request.AdditionalRequirements,
                Type = request.Type,
                Priority = request.Priority,
                Status = DemandStatus.Active,
                ExpiresAt = DateTime.UtcNow.AddDays(request.ValidityDays ?? 30)
            };

            var result = await _repository.AddAsync(demand);

            // 发布需求创建事件
            await _daprClient.PublishEventAsync("pubsub", "demand-created",
                new DemandCreatedEvent
                {
                    DemandId = result.Id,
                    RequesterId = result.RequesterId,
                    BearingNumber = result.BearingNumber,
                    Brand = result.Brand,
                    Specification = result.Specification,
                    RequiredQuantity = result.RequiredQuantity,
                    DeliveryAddress = result.DeliveryAddress,
                    CreatedAt = result.CreatedAt
                });

            _logger.LogInformation("需求创建成功: {DemandId} by {RequesterId}", result.Id, requesterId);
            return result;
        }

        public async Task<Models.Demand?> GetDemandAsync(long id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<DemandDetailResponse?> GetDemandDetailAsync(long id)
        {
            var demand = await _repository.GetByIdWithIncludesAsync(id);
            if (demand == null) return null;

            return new DemandDetailResponse
            {
                Id = demand.Id,
                BearingNumber = demand.BearingNumber,
                Specification = demand.Specification,
                Brand = demand.Brand,
                RequiredQuantity = demand.RequiredQuantity,
                MaxPrice = demand.MaxPrice,
                MinPrice = demand.MinPrice,
                Status = demand.Status,
                Priority = demand.Priority,
                TotalMatches = demand.TotalMatches,
                TotalQuotations = demand.TotalQuotations,
                CreatedAt = demand.CreatedAt,
                ExpiresAt = demand.ExpiresAt,
                RequesterId = demand.RequesterId,
                RequesterType = demand.RequesterType,
                RequesterCompany = demand.RequesterCompany,
                Material = demand.Material,
                Standard = demand.Standard,
                DeliveryAddress = demand.DeliveryAddress,
                RequiredByDate = demand.RequiredByDate,
                AdditionalRequirements = demand.AdditionalRequirements,
                Type = demand.Type,
                TotalViews = demand.TotalViews,
                SelectedSupplierId = demand.SelectedSupplierId,
                SelectedQuotationId = demand.SelectedQuotationId,
                UpdatedAt = demand.UpdatedAt,
                ClosedAt = demand.ClosedAt,
                Matches = demand.Matches.Select(m => new DemandMatchResponse
                {
                    Id = m.Id,
                    SupplierId = m.SupplierId,
                    SupplierName = m.SupplierName,
                    MatchScore = m.MatchScore,
                    MatchReason = m.MatchReason,
                    MatchDetails = m.MatchDetails,
                    IsNotified = m.IsNotified,
                    HasResponded = m.HasResponded,
                    IsInterested = m.IsInterested,
                    CreatedAt = m.CreatedAt
                }).ToList(),
                RecentViews = demand.Views.Select(v => new DemandViewResponse
                {
                    ViewerId = v.ViewerId,
                    ViewerType = v.ViewerType,
                    ViewerCompany = v.ViewerCompany,
                    ViewedAt = v.ViewedAt
                }).ToList()
            };
        }

        public async Task<PagedResponse<DemandResponse>> GetDemandsAsync(DemandQuery query)
        {
            var demands = await _repository.GetListAsync(query);
            var totalCount = await _repository.GetCountAsync(query);

            return new PagedResponse<DemandResponse>
            {
                Items = demands.Select(d => new DemandResponse
                {
                    Id = d.Id,
                    BearingNumber = d.BearingNumber,
                    Specification = d.Specification,
                    Brand = d.Brand,
                    RequiredQuantity = d.RequiredQuantity,
                    MaxPrice = d.MaxPrice,
                    Status = d.Status,
                    Priority = d.Priority,
                    TotalMatches = d.TotalMatches,
                    TotalQuotations = d.TotalQuotations,
                    CreatedAt = d.CreatedAt,
                    ExpiresAt = d.ExpiresAt
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<Models.Demand> UpdateDemandAsync(long id, UpdateDemandRequest request)
        {
            var demand = await _repository.GetByIdAsync(id);
            if (demand == null)
                throw new KeyNotFoundException("需求不存在");

            // 更新字段
            if (!string.IsNullOrEmpty(request.BearingNumber))
                demand.BearingNumber = request.BearingNumber;

            if (!string.IsNullOrEmpty(request.Specification))
                demand.Specification = request.Specification;

            if (!string.IsNullOrEmpty(request.Material))
                demand.Material = request.Material;

            if (!string.IsNullOrEmpty(request.Brand))
                demand.Brand = request.Brand;

            if (request.RequiredQuantity.HasValue)
                demand.RequiredQuantity = request.RequiredQuantity.Value;

            if (request.MaxPrice.HasValue)
                demand.MaxPrice = request.MaxPrice.Value;

            if (request.MinPrice.HasValue)
                demand.MinPrice = request.MinPrice.Value;

            if (!string.IsNullOrEmpty(request.DeliveryAddress))
                demand.DeliveryAddress = request.DeliveryAddress;

            if (request.RequiredByDate.HasValue)
                demand.RequiredByDate = request.RequiredByDate.Value;

            if (!string.IsNullOrEmpty(request.AdditionalRequirements))
                demand.AdditionalRequirements = request.AdditionalRequirements;

            if (request.Status.HasValue)
                demand.Status = request.Status.Value;

            if (request.Priority.HasValue)
                demand.Priority = request.Priority.Value;

            return await _repository.UpdateAsync(demand);
        }

        public async Task<bool> DeleteDemandAsync(long id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<Models.Demand> UpdateDemandStatusAsync(long id, DemandStatus newStatus, string? reason = null)
        {
            var demand = await _repository.GetByIdAsync(id);
            if (demand == null)
                throw new KeyNotFoundException("需求不存在");

            var oldStatus = demand.Status;
            demand.Status = newStatus;
            demand.UpdatedAt = DateTime.UtcNow;

            if (newStatus == DemandStatus.Closed)
                demand.ClosedAt = DateTime.UtcNow;

            var result = await _repository.UpdateAsync(demand);

            // 发布状态变更事件
            await _daprClient.PublishEventAsync("pubsub", "demand-status-changed",
                new DemandStatusChangedEvent
                {
                    DemandId = result.Id,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    ChangedByUserId = result.RequesterId,
                    Reason = reason,
                    ChangedAt = DateTime.UtcNow
                });

            return result;
        }

        public async Task<Models.Demand> CloseDemandAsync(long id, string? reason = null)
        {
            return await UpdateDemandStatusAsync(id, DemandStatus.Closed, reason ?? "需求已完成");
        }

        public async Task<Models.Demand> CancelDemandAsync(long id, string? reason = null)
        {
            return await UpdateDemandStatusAsync(id, DemandStatus.Cancelled, reason ?? "需求已取消");
        }

        public async Task<Models.Demand> ExpireDemandAsync(long id)
        {
            return await UpdateDemandStatusAsync(id, DemandStatus.Expired, "需求已过期");
        }

        public async Task<MatchResultResponse> MatchDemandAsync(MatchDemandRequest request)
        {
            return await _matchingService.MatchDemandToSuppliersAsync(request.DemandId, request.Strategy);
        }

        public async Task<List<DemandMatch>> GetDemandMatchesAsync(long demandId)
        {
            return await _repository.GetMatchesForDemandAsync(demandId);
        }

        public async Task<bool> NotifySupplierAsync(long demandId, long supplierId)
        {
            var match = await _repository.GetMatchAsync(demandId, supplierId);
            if (match == null) return false;

            match.IsNotified = true;
            match.NotifiedAt = DateTime.UtcNow;
            await _repository.UpdateMatchAsync(match);

            // 发布供应商通知事件
            var demand = await _repository.GetByIdAsync(demandId);
            if (demand != null)
            {
                await _daprClient.PublishEventAsync("pubsub", "supplier-notified",
                    new SupplierNotificationEvent
                    {
                        DemandId = demandId,
                        SupplierId = supplierId,
                        SupplierName = match.SupplierName,
                        SupplierEmail = "", // 需要从用户服务获取
                        BearingNumber = demand.BearingNumber,
                        Brand = demand.Brand,
                        RequiredQuantity = demand.RequiredQuantity,
                        MatchScore = match.MatchScore,
                        MatchReason = match.MatchReason,
                        NotifiedAt = DateTime.UtcNow
                    });
            }

            return true;
        }

        public async Task<bool> RecordSupplierResponseAsync(long demandId, long supplierId, bool isInterested)
        {
            var match = await _repository.GetMatchAsync(demandId, supplierId);
            if (match == null) return false;

            match.HasResponded = true;
            match.RespondedAt = DateTime.UtcNow;
            match.IsInterested = isInterested;
            await _repository.UpdateMatchAsync(match);

            return true;
        }

        public async Task RecordDemandViewAsync(long demandId, long? viewerId, string viewerType, string? viewerCompany, string? userAgent, string? ipAddress)
        {
            var view = new DemandView
            {
                DemandId = demandId,
                ViewerId = viewerId,
                ViewerType = viewerType,
                ViewerCompany = viewerCompany,
                UserAgent = userAgent,
                IpAddress = ipAddress
            };

            await _repository.AddViewAsync(view);
        }

        public async Task<PagedResponse<DemandResponse>> SearchDemandsAsync(DemandSearchRequest request)
        {
            // 转换搜索请求为查询
            var query = new DemandQuery
            {
                BearingNumber = request.BearingNumber,
                Brand = request.Brand,
                Specification = request.Specification,
                Status = DemandStatus.Active, // 默认只搜索活跃需求
                SortBy = request.SortBy,
                SortDescending = request.SortDescending,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return await GetDemandsAsync(query);
        }

        public async Task<DemandStatistics> GetDemandStatisticsAsync()
        {
            var totalDemands = await _repository.GetCountAsync(new DemandQuery());
            var activeDemands = await _repository.GetActiveDemandsCountAsync();
            var totalMatches = await _repository.GetTotalMatchesCountAsync();
            var statusDistribution = await _repository.GetDemandStatsByStatusAsync();
            var popularBearings = await _repository.GetPopularBearingNumbersAsync(10);

            return new DemandStatistics
            {
                TotalDemands = totalDemands,
                ActiveDemands = activeDemands,
                TotalMatches = totalMatches,
                StatusDistribution = statusDistribution,
                PopularBearings = popularBearings,
                AverageMatchesPerDemand = activeDemands > 0 ? totalMatches / activeDemands : 0,
                MatchSuccessRate = totalMatches > 0 ? (double)activeDemands / totalDemands * 100 : 0
            };
        }

        public async Task<List<DemandTrend>> GetDemandTrendsAsync(DateTime startDate, DateTime endDate)
        {
            var trends = new List<DemandTrend>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var nextDate = currentDate.AddDays(1);

                // 修复1：添加 await 和正确的 Where 条件
                var newDemands = await _context.Demands
                    .Where(d => d.CreatedAt >= currentDate && d.CreatedAt < nextDate)
                    .CountAsync();

                var closedDemands = await _context.Demands
                    .Where(d => d.ClosedAt != null && d.ClosedAt >= currentDate && d.ClosedAt < nextDate)
                    .CountAsync();

                // 修复2：正确的 DemandMatches 查询
                var matches = await _context.DemandMatches
                    .Where(m => m.CreatedAt >= currentDate && m.CreatedAt < nextDate)
                    .CountAsync();

                trends.Add(new DemandTrend
                {
                    Date = currentDate,
                    NewDemands = newDemands,
                    ClosedDemands = closedDemands,
                    TotalMatches = matches,
                    MatchRate = newDemands > 0 ? (double)matches / newDemands * 100 : 0
                });

                currentDate = nextDate;
            }

            return trends;
        }
    }
}
