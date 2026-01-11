using Microsoft.EntityFrameworkCore;
using BearingApi.Models.Entities;
using BearingApi.Models.DTOs;
using System.Linq.Dynamic.Core;

namespace BearingApi.Data
{
    public class BearingRepository : IBearingRepository
    {
        private readonly BearingDbContext _context;
        private readonly ILogger<BearingRepository> _logger;

        public BearingRepository(BearingDbContext context, ILogger<BearingRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Bearing CRUD 操作
        public async Task<Bearing?> GetByIdAsync(long id)
        {
            return await _context.Bearings
                .Include(b => b.Specifications)
                .Include(b => b.Images)
                .Include(b => b.Documents)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Bearing?> GetByBearingNumberAsync(string bearingNumber)
        {
            return await _context.Bearings
                .Include(b => b.Specifications)
                .Include(b => b.Images)
                .Include(b => b.Documents)
                .FirstOrDefaultAsync(b => b.BearingNumber == bearingNumber);
        }

        public async Task<Bearing?> GetByIdWithIncludesAsync(long id)
        {
            return await _context.Bearings
                .Include(b => b.Specifications)
                .Include(b => b.Images)
                .Include(b => b.Documents)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<Bearing>> GetListAsync(BearingQuery query)
        {
            var bearings = _context.Bearings.AsQueryable();

            // 应用筛选条件
            if (!string.IsNullOrEmpty(query.BearingNumber))
                bearings = bearings.Where(b => b.BearingNumber.Contains(query.BearingNumber));

            if (!string.IsNullOrEmpty(query.Brand))
                bearings = bearings.Where(b => b.Brand != null && b.Brand.Contains(query.Brand));

            if (!string.IsNullOrEmpty(query.Specification))
                bearings = bearings.Where(b => b.Specification != null && b.Specification.Contains(query.Specification));

            if (query.Status.HasValue)
                bearings = bearings.Where(b => b.Status == query.Status.Value);

            if (query.Type.HasValue)
                bearings = bearings.Where(b => b.Type == query.Type.Value);

            if (query.Category.HasValue)
                bearings = bearings.Where(b => b.Category == query.Category.Value);

            if (query.RequesterId.HasValue)
                bearings = bearings.Where(b => b.RequesterId == query.RequesterId.Value);

            if (!string.IsNullOrEmpty(query.RequesterType))
                bearings = bearings.Where(b => b.RequesterType == query.RequesterType);

            if (query.CreatedAfter.HasValue)
                bearings = bearings.Where(b => b.CreatedAt >= query.CreatedAfter.Value);

            if (query.CreatedBefore.HasValue)
                bearings = bearings.Where(b => b.CreatedAt <= query.CreatedBefore.Value);

            if (query.ExpiresBefore.HasValue)
                bearings = bearings.Where(b => b.ExpiresAt != null && b.ExpiresAt <= query.ExpiresBefore.Value);

            if (query.HasMatches.HasValue)
                bearings = query.HasMatches.Value
                    ? bearings.Where(b => b.TotalMatches > 0)
                    : bearings.Where(b => b.TotalMatches == 0);

            if (query.HasQuotations.HasValue)
                bearings = query.HasQuotations.Value
                    ? bearings.Where(b => b.TotalQuotations > 0)
                    : bearings.Where(b => b.TotalQuotations == 0);

            // 应用排序
            bearings = query.SortBy?.ToLower() switch
            {
                "bearingnumber" => query.SortDescending ?
                    bearings.OrderByDescending(b => b.BearingNumber) :
                    bearings.OrderBy(b => b.BearingNumber),
                "brand" => query.SortDescending ?
                    bearings.OrderByDescending(b => b.Brand) :
                    bearings.OrderBy(b => b.Brand),
                "createdat" => query.SortDescending ?
                    bearings.OrderByDescending(b => b.CreatedAt) :
                    bearings.OrderBy(b => b.CreatedAt),
                "viewcount" => query.SortDescending ?
                    bearings.OrderByDescending(b => b.ViewCount) :
                    bearings.OrderBy(b => b.ViewCount),
                "rating" => query.SortDescending ?
                    bearings.OrderByDescending(b => b.Rating) :
                    bearings.OrderBy(b => b.Rating),
                _ => query.SortDescending ?
                    bearings.OrderByDescending(b => b.CreatedAt) :
                    bearings.OrderBy(b => b.CreatedAt)
            };

            // 分页
            if (query.PageSize > 0)
            {
                bearings = bearings
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize);
            }

            return await bearings.ToListAsync();
        }

        public async Task<int> GetCountAsync(BearingQuery query)
        {
            var bearings = _context.Bearings.AsQueryable();

            // 应用相同的筛选条件
            if (!string.IsNullOrEmpty(query.BearingNumber))
                bearings = bearings.Where(b => b.BearingNumber.Contains(query.BearingNumber));

            if (query.Status.HasValue)
                bearings = bearings.Where(b => b.Status == query.Status.Value);

            if (query.Type.HasValue)
                bearings = bearings.Where(b => b.Type == query.Type.Value);

            if (query.Category.HasValue)
                bearings = bearings.Where(b => b.Category == query.Category.Value);

            if (query.RequesterId.HasValue)
                bearings = bearings.Where(b => b.RequesterId == query.RequesterId.Value);

            if (!string.IsNullOrEmpty(query.RequesterType))
                bearings = bearings.Where(b => b.RequesterType == query.RequesterType);

            if (query.CreatedAfter.HasValue)
                bearings = bearings.Where(b => b.CreatedAt >= query.CreatedAfter.Value);

            if (query.CreatedBefore.HasValue)
                bearings = bearings.Where(b => b.CreatedAt <= query.CreatedBefore.Value);

            return await bearings.CountAsync();
        }

        public async Task<Bearing> AddAsync(Bearing bearing)
        {
            bearing.CreatedAt = DateTime.UtcNow;
            bearing.UpdatedAt = DateTime.UtcNow;

            _context.Bearings.Add(bearing);
            await _context.SaveChangesAsync();

            _logger.LogInformation("轴承添加成功: {BearingNumber}", bearing.BearingNumber);
            return bearing;
        }

        public async Task<Bearing> UpdateAsync(Bearing bearing)
        {
            bearing.UpdatedAt = DateTime.UtcNow;
            _context.Bearings.Update(bearing);
            await _context.SaveChangesAsync();

            _logger.LogInformation("轴承更新成功: {BearingId}", bearing.Id);
            return bearing;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var bearing = await GetByIdAsync(id);
            if (bearing == null) return false;

            _context.Bearings.Remove(bearing);
            await _context.SaveChangesAsync();

            _logger.LogInformation("轴承删除成功: {BearingId}", id);
            return true;
        }
        #endregion

        #region 搜索和匹配功能
        public async Task<List<Bearing>> SearchAsync(BearingSearchRequest request)
        {
            var query = _context.Bearings.AsQueryable();

            if (!string.IsNullOrEmpty(request.Keywords))
            {
                var keywords = request.Keywords.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var keyword in keywords)
                {
                    query = query.Where(b =>
                        b.BearingNumber.ToLower().Contains(keyword) ||
                        b.DisplayName.ToLower().Contains(keyword) ||
                        (b.AlternateNumbers != null && b.AlternateNumbers.ToLower().Contains(keyword)) ||
                        (b.Brand != null && b.Brand.ToLower().Contains(keyword)) ||
                        (b.Specification != null && b.Specification.ToLower().Contains(keyword))
                    );
                }
            }

            if (!string.IsNullOrEmpty(request.BearingNumber))
                query = query.Where(b => b.BearingNumber.Contains(request.BearingNumber));

            if (!string.IsNullOrEmpty(request.Brand))
                query = query.Where(b => b.Brand != null && b.Brand.Contains(request.Brand));

            if (request.Type.HasValue)
                query = query.Where(b => b.Type == request.Type.Value);

            if (request.Category.HasValue)
                query = query.Where(b => b.Category == request.Category.Value);

            // 尺寸范围筛选
            if (request.MinInnerDiameter.HasValue)
                query = query.Where(b => b.InnerDiameter >= request.MinInnerDiameter.Value);
            if (request.MaxInnerDiameter.HasValue)
                query = query.Where(b => b.InnerDiameter <= request.MaxInnerDiameter.Value);
            if (request.MinOuterDiameter.HasValue)
                query = query.Where(b => b.OuterDiameter >= request.MinOuterDiameter.Value);
            if (request.MaxOuterDiameter.HasValue)
                query = query.Where(b => b.OuterDiameter <= request.MaxOuterDiameter.Value);

            // 排序
            query = request.SortBy?.ToLower() switch
            {
                "relevance" => query.OrderByDescending(b =>
                    (b.BearingNumber == request.Keywords ? 100 : 0) +
                    (b.ViewCount * 0.1) +
                    (b.SearchCount * 0.05)),
                "popular" => request.SortDescending ?
                    query.OrderByDescending(b => b.ViewCount) :
                    query.OrderBy(b => b.ViewCount),
                _ => request.SortDescending ?
                    query.OrderByDescending(b => b.CreatedAt) :
                    query.OrderBy(b => b.CreatedAt)
            };

            // 分页
            if (request.PageSize > 0)
            {
                query = query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize);
            }

            return await query.ToListAsync();
        }

        public async Task<List<Bearing>> FindSimilarBearingsAsync(string bearingNumber, int limit = 10)
        {
            // 基于型号相似度查找相似轴承
            var bearings = await _context.Bearings
                .Where(b => b.BearingNumber.StartsWith(bearingNumber.Substring(0, Math.Min(3, bearingNumber.Length))) ||
                           (b.AlternateNumbers != null && b.AlternateNumbers.Contains(bearingNumber)))
                .OrderByDescending(b => b.ViewCount)
                .Take(limit)
                .ToListAsync();

            return bearings;
        }

        public async Task<List<Bearing>> GetBearingsByParametersAsync(BearingParameters parameters)
        {
            var query = _context.Bearings.AsQueryable();

            if (parameters.InnerDiameter.HasValue)
                query = query.Where(b => b.InnerDiameter == parameters.InnerDiameter.Value);

            if (parameters.OuterDiameter.HasValue)
                query = query.Where(b => b.OuterDiameter == parameters.OuterDiameter.Value);

            if (parameters.Width.HasValue)
                query = query.Where(b => b.Width == parameters.Width.Value);

            if (!string.IsNullOrEmpty(parameters.Brand))
                query = query.Where(b => b.Brand == parameters.Brand);

            if (parameters.Type.HasValue)
                query = query.Where(b => b.Type == parameters.Type.Value);

            return await query
                .OrderBy(b => b.BearingNumber)
                .ToListAsync();
        }
        #endregion

        #region 规格管理
        public async Task<List<BearingSpecification>> GetSpecificationsAsync(long bearingId)
        {
            return await _context.BearingSpecifications
                .Where(s => s.BearingId == bearingId)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
        }

        public async Task<BearingSpecification> AddSpecificationAsync(BearingSpecification spec)
        {
            _context.BearingSpecifications.Add(spec);
            await _context.SaveChangesAsync();
            return spec;
        }

        public async Task<bool> RemoveSpecificationAsync(long specId)
        {
            var spec = await _context.BearingSpecifications.FindAsync(specId);
            if (spec == null) return false;

            _context.BearingSpecifications.Remove(spec);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region 图片管理
        public async Task<List<BearingImage>> GetImagesAsync(long bearingId)
        {
            return await _context.BearingImages
                .Where(i => i.BearingId == bearingId)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();
        }

        public async Task<BearingImage> AddImageAsync(BearingImage image)
        {
            _context.BearingImages.Add(image);
            await _context.SaveChangesAsync();
            return image;
        }

        public async Task<bool> SetPrimaryImageAsync(long imageId)
        {
            var image = await _context.BearingImages.FindAsync(imageId);
            if (image == null) return false;

            // 清除其他主图
            var otherPrimaryImages = await _context.BearingImages
                .Where(i => i.BearingId == image.BearingId && i.IsPrimary && i.Id != imageId)
                .ToListAsync();

            foreach (var otherImage in otherPrimaryImages)
            {
                otherImage.IsPrimary = false;
            }

            image.IsPrimary = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveImageAsync(long imageId)
        {
            var image = await _context.BearingImages.FindAsync(imageId);
            if (image == null) return false;

            _context.BearingImages.Remove(image);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region 文档管理
        public async Task<List<BearingDocument>> GetDocumentsAsync(long bearingId)
        {
            return await _context.BearingDocuments
                .Where(d => d.BearingId == bearingId)
                .ToListAsync();
        }

        public async Task<BearingDocument> AddDocumentAsync(BearingDocument document)
        {
            _context.BearingDocuments.Add(document);
            await _context.SaveChangesAsync();
            return document;
        }

        public async Task<bool> RemoveDocumentAsync(long documentId)
        {
            var document = await _context.BearingDocuments.FindAsync(documentId);
            if (document == null) return false;

            _context.BearingDocuments.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region 统计和计数方法
        public async Task IncrementViewCountAsync(long bearingId)
        {
            var bearing = await GetByIdAsync(bearingId);
            if (bearing != null)
            {
                bearing.ViewCount++;
                await UpdateAsync(bearing);
            }
        }

        public async Task IncrementSearchCountAsync(long bearingId)
        {
            var bearing = await GetByIdAsync(bearingId);
            if (bearing != null)
            {
                bearing.SearchCount++;
                await UpdateAsync(bearing);
            }
        }

        public async Task<int> GetTotalBearingsCountAsync()
        {
            return await _context.Bearings.CountAsync();
        }

        public async Task<Dictionary<BearingType, int>> GetBearingStatsByTypeAsync()
        {
            return await _context.Bearings
                .GroupBy(b => b.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetPopularBrandsAsync(int topN = 10)
        {
            return await _context.Bearings
                .Where(b => !string.IsNullOrEmpty(b.Brand))
                .GroupBy(b => b.Brand)
                .Select(g => new { Brand = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(topN)
                .ToDictionaryAsync(x => x.Brand, x => x.Count);
        }

        public async Task<List<Bearing>> GetRecentlyAddedAsync(int limit = 20)
        {
            return await _context.Bearings
                .Where(b => b.Status == BearingStatus.Active)
                .OrderByDescending(b => b.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Bearing>> GetPendingApprovalAsync()
        {
            return await _context.Bearings
                .Where(b => b.Status == BearingStatus.Pending)
                .OrderBy(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Bearing>> GetActiveBearingsAsync()
        {
            return await _context.Bearings
                .Where(b => b.Status == BearingStatus.Active)
                .OrderByDescending(b => b.Rating)
                .ToListAsync();
        }

        public async Task<int> GetActiveBearingsCountAsync()
        {
            return await _context.Bearings
                .Where(b => b.Status == BearingStatus.Active)
                .CountAsync();
        }

        public async Task<int> GetTotalMatchesCountAsync()
        {
            return await _context.BearingMatches.CountAsync();
        }

        public async Task<Dictionary<BearingStatus, int>> GetDemandStatsByStatusAsync()
        {
            return await _context.Bearings
                .GroupBy(b => b.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetPopularBearingNumbersAsync(int topN = 10)
        {
            return await _context.Bearings
                .Where(b => b.Status == BearingStatus.Active)
                .GroupBy(b => b.BearingNumber)
                .Select(g => new { BearingNumber = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(topN)
                .ToDictionaryAsync(x => x.BearingNumber, x => x.Count);
        }
        #endregion

        #region 匹配管理
        public async Task<BearingMatch?> GetMatchAsync(long demandId, long supplierId)
        {
            return await _context.BearingMatches
                .FirstOrDefaultAsync(m => m.DemandId == demandId && m.SupplierId == supplierId);
        }

        public async Task<List<BearingMatch>> GetMatchesForDemandAsync(long demandId)
        {
            return await _context.BearingMatches
                .Where(m => m.DemandId == demandId)
                .OrderByDescending(m => m.MatchScore)
                .ToListAsync();
        }

        public async Task<List<BearingMatch>> GetMatchesForSupplierAsync(long supplierId)
        {
            return await _context.BearingMatches
                .Where(m => m.SupplierId == supplierId)
                .Include(m => m.Demand)
                .OrderByDescending(m => m.MatchScore)
                .ToListAsync();
        }

        public async Task<BearingMatch> AddMatchAsync(BearingMatch match)
        {
            match.CreatedAt = DateTime.UtcNow;
            _context.BearingMatches.Add(match);
            await _context.SaveChangesAsync();

            // 更新需求的匹配计数
            var bearing = await GetByIdAsync(match.DemandId);
            if (bearing != null)
            {
                bearing.TotalMatches = await _context.BearingMatches
                    .CountAsync(m => m.DemandId == match.DemandId);
                await UpdateAsync(bearing);
            }

            return match;
        }

        public async Task<BearingMatch> UpdateMatchAsync(BearingMatch match)
        {
            _context.BearingMatches.Update(match);
            await _context.SaveChangesAsync();
            return match;
        }

        public async Task<bool> RemoveMatchAsync(long matchId)
        {
            var match = await _context.BearingMatches.FindAsync(matchId);
            if (match == null) return false;

            _context.BearingMatches.Remove(match);
            await _context.SaveChangesAsync();

            // 更新需求的匹配计数
            var bearing = await GetByIdAsync(match.DemandId);
            if (bearing != null)
            {
                bearing.TotalMatches = await _context.BearingMatches
                    .CountAsync(m => m.DemandId == match.DemandId);
                await UpdateAsync(bearing);
            }

            return true;
        }
        #endregion

        #region 查看记录管理
        public async Task AddViewAsync(BearingView view)
        {
            view.ViewedAt = DateTime.UtcNow;
            _context.BearingViews.Add(view);
            await _context.SaveChangesAsync();

            // 更新查看计数
            var bearing = await GetByIdAsync(view.DemandId);
            if (bearing != null)
            {
                bearing.TotalViews = await _context.BearingViews
                    .CountAsync(v => v.DemandId == view.DemandId);
                await UpdateAsync(bearing);
            }
        }

        public async Task<List<BearingView>> GetViewsForDemandAsync(long demandId, int limit = 50)
        {
            return await _context.BearingViews
                .Where(v => v.DemandId == demandId)
                .OrderByDescending(v => v.ViewedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetViewCountAsync(long demandId)
        {
            return await _context.BearingViews
                .CountAsync(v => v.DemandId == demandId);
        }
        #endregion
    }
}
