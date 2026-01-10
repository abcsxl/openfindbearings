using Demand.Models;
using Demand.Models.DTOs;

namespace Demand.Data
{
    public interface IDemandRepository
    {
        // Demand CRUD
        Task<Models.Demand?> GetByIdAsync(long id);
        Task<Models.Demand?> GetByIdWithIncludesAsync(long id);
        Task<List<Models.Demand>> GetListAsync(DemandQuery query);
        Task<int> GetCountAsync(DemandQuery query);
        Task<Models.Demand> AddAsync(Models.Demand demand);
        Task<Models.Demand> UpdateAsync(Models.Demand demand);
        Task<bool> DeleteAsync(long id);

        // DemandMatch 操作
        Task<DemandMatch?> GetMatchAsync(long demandId, long supplierId);
        Task<List<DemandMatch>> GetMatchesForDemandAsync(long demandId);
        Task<List<DemandMatch>> GetMatchesForSupplierAsync(long supplierId);
        Task<DemandMatch> AddMatchAsync(DemandMatch match);
        Task<DemandMatch> UpdateMatchAsync(DemandMatch match);
        Task<bool> RemoveMatchAsync(long matchId);

        // DemandView 操作
        Task AddViewAsync(DemandView view);
        Task<List<DemandView>> GetViewsForDemandAsync(long demandId, int limit = 50);
        Task<int> GetViewCountAsync(long demandId);

        // 统计方法
        Task<int> GetActiveDemandsCountAsync();
        Task<int> GetTotalMatchesCountAsync();
        Task<Dictionary<DemandStatus, int>> GetDemandStatsByStatusAsync();
        Task<Dictionary<string, int>> GetPopularBearingNumbersAsync(int topN = 10);
    }
}
