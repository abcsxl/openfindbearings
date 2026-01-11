using DemandApi.Models;
using DemandApi.Models.DTOs;

namespace DemandApi.Services
{
    public interface IDemandService
    {
        // Demand CRUD
        Task<Demand> CreateDemandAsync(CreateDemandRequest request, long requesterId, string requesterType, string? requesterCompany);
        Task<Demand?> GetDemandAsync(long id);
        Task<DemandDetailResponse?> GetDemandDetailAsync(long id);
        Task<PagedResponse<DemandResponse>> GetDemandsAsync(DemandQuery query);
        Task<Demand> UpdateDemandAsync(long id, UpdateDemandRequest request);
        Task<bool> DeleteDemandAsync(long id);

        // 状态管理
        Task<Demand> UpdateDemandStatusAsync(long id, DemandStatus newStatus, string? reason = null);
        Task<Demand> CloseDemandAsync(long id, string? reason = null);
        Task<Demand> CancelDemandAsync(long id, string? reason = null);
        Task<Demand> ExpireDemandAsync(long id);

        // 匹配管理
        Task<MatchResultResponse> MatchDemandAsync(MatchDemandRequest request);
        Task<List<DemandMatch>> GetDemandMatchesAsync(long demandId);
        Task<bool> NotifySupplierAsync(long demandId, long supplierId);
        Task<bool> RecordSupplierResponseAsync(long demandId, long supplierId, bool isInterested);

        // 查看管理
        Task RecordDemandViewAsync(long demandId, long? viewerId, string viewerType, string? viewerCompany, string? userAgent, string? ipAddress);

        // 搜索
        Task<PagedResponse<DemandResponse>> SearchDemandsAsync(DemandSearchRequest request);

        // 统计
        Task<DemandStatistics> GetDemandStatisticsAsync();
        Task<List<DemandTrend>> GetDemandTrendsAsync(DateTime startDate, DateTime endDate);
    }



    public class DemandStatistics
    {
        public int TotalDemands { get; set; }
        public int ActiveDemands { get; set; }
        public int TotalMatches { get; set; }
        public int PendingDemands { get; set; }
        public int ClosedDemands { get; set; }
        public Dictionary<DemandStatus, int> StatusDistribution { get; set; } = new();
        public Dictionary<string, int> PopularBearings { get; set; } = new();
        public int AverageMatchesPerDemand { get; set; }
        public double MatchSuccessRate { get; set; }
    }

    public class DemandTrend
    {
        public DateTime Date { get; set; }
        public int NewDemands { get; set; }
        public int ClosedDemands { get; set; }
        public int TotalMatches { get; set; }
        public double MatchRate { get; set; }
    }
}
