using BearingApi.Models.DTOs;
using BearingApi.Models.Entities;

namespace BearingApi.Data
{
    public interface IBearingRepository
    {
        // Bearing CRUD
        Task<Bearing?> GetByIdAsync(long id);
        Task<Bearing?> GetByBearingNumberAsync(string bearingNumber);
        Task<Bearing?> GetByIdWithIncludesAsync(long id);
        Task<List<Bearing>> GetListAsync(BearingQuery query);
        Task<int> GetCountAsync(BearingQuery query);
        Task<Bearing> AddAsync(Bearing bearing);
        Task<Bearing> UpdateAsync(Bearing bearing);
        Task<bool> DeleteAsync(long id);

        // 搜索和查询
        Task<List<Bearing>> SearchAsync(BearingSearchRequest request);
        Task<List<Bearing>> FindSimilarBearingsAsync(string bearingNumber, int limit = 10);
        Task<List<Bearing>> GetBearingsByParametersAsync(BearingParameters parameters);

        // 规格管理
        Task<List<BearingSpecification>> GetSpecificationsAsync(long bearingId);
        Task<BearingSpecification> AddSpecificationAsync(BearingSpecification spec);
        Task<bool> RemoveSpecificationAsync(long specId);

        // 图片管理
        Task<List<BearingImage>> GetImagesAsync(long bearingId);
        Task<BearingImage> AddImageAsync(BearingImage image);
        Task<bool> SetPrimaryImageAsync(long imageId);
        Task<bool> RemoveImageAsync(long imageId);

        // 文档管理
        Task<List<BearingDocument>> GetDocumentsAsync(long bearingId);
        Task<BearingDocument> AddDocumentAsync(BearingDocument document);
        Task<bool> RemoveDocumentAsync(long documentId);

        // 匹配管理
        Task<BearingMatch?> GetMatchAsync(long demandId, long supplierId);
        Task<List<BearingMatch>> GetMatchesForDemandAsync(long demandId);
        Task<List<BearingMatch>> GetMatchesForSupplierAsync(long supplierId);
        Task<BearingMatch> AddMatchAsync(BearingMatch match);
        Task<BearingMatch> UpdateMatchAsync(BearingMatch match);
        Task<bool> RemoveMatchAsync(long matchId);

        // 查看记录管理
        Task AddViewAsync(BearingView view);
        Task<List<BearingView>> GetViewsForDemandAsync(long demandId, int limit = 50);
        Task<int> GetViewCountAsync(long demandId);

        // 统计方法
        Task<int> GetTotalBearingsCountAsync();
        Task<int> GetActiveBearingsCountAsync();
        Task<int> GetTotalMatchesCountAsync();
        Task<Dictionary<BearingType, int>> GetBearingStatsByTypeAsync();
        Task<Dictionary<string, int>> GetPopularBrandsAsync(int topN = 10);
        Task<Dictionary<string, int>> GetPopularBearingNumbersAsync(int topN = 10);
        Task<Dictionary<BearingStatus, int>> GetDemandStatsByStatusAsync();
        Task<List<Bearing>> GetRecentlyAddedAsync(int limit = 20);
        Task<List<Bearing>> GetPendingApprovalAsync();
        Task<List<Bearing>> GetActiveBearingsAsync();

        // 计数方法
        Task IncrementViewCountAsync(long bearingId);
        Task IncrementSearchCountAsync(long bearingId);
    }
}
