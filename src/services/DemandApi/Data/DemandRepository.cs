using Demand.Models;
using Demand.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Demand.Data
{
    public class DemandRepository : IDemandRepository
    {
        private readonly DemandDbContext _context;
        private readonly ILogger<DemandRepository> _logger;

        public DemandRepository(DemandDbContext context, ILogger<DemandRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Models.Demand?> GetByIdAsync(long id)
        {
            return await _context.Demands
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Models.Demand?> GetByIdWithIncludesAsync(long id)
        {
            return await _context.Demands
                .Include(d => d.Matches)
                .Include(d => d.Views.OrderByDescending(v => v.ViewedAt).Take(10))
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<List<Models.Demand>> GetListAsync(DemandQuery query)
        {
            var demands = _context.Demands.AsQueryable();

            // 应用筛选条件
            if (!string.IsNullOrEmpty(query.BearingNumber))
                demands = demands.Where(d => d.BearingNumber.Contains(query.BearingNumber));

            if (!string.IsNullOrEmpty(query.Brand))
                demands = demands.Where(d => d.Brand != null && d.Brand.Contains(query.Brand));

            if (!string.IsNullOrEmpty(query.Specification))
                demands = demands.Where(d => d.Specification != null && d.Specification.Contains(query.Specification));

            if (query.Status.HasValue)
                demands = demands.Where(d => d.Status == query.Status.Value);

            if (query.Type.HasValue)
                demands = demands.Where(d => d.Type == query.Type.Value);

            if (query.Priority.HasValue)
                demands = demands.Where(d => d.Priority == query.Priority.Value);

            if (query.RequesterId.HasValue)
                demands = demands.Where(d => d.RequesterId == query.RequesterId.Value);

            if (!string.IsNullOrEmpty(query.RequesterType))
                demands = demands.Where(d => d.RequesterType == query.RequesterType);

            if (query.CreatedAfter.HasValue)
                demands = demands.Where(d => d.CreatedAt >= query.CreatedAfter.Value);

            if (query.CreatedBefore.HasValue)
                demands = demands.Where(d => d.CreatedAt <= query.CreatedBefore.Value);

            if (query.ExpiresBefore.HasValue)
                demands = demands.Where(d => d.ExpiresAt != null && d.ExpiresAt <= query.ExpiresBefore.Value);

            if (query.HasMatches.HasValue)
                demands = query.HasMatches.Value
                    ? demands.Where(d => d.TotalMatches > 0)
                    : demands.Where(d => d.TotalMatches == 0);

            if (query.HasQuotations.HasValue)
                demands = query.HasQuotations.Value
                    ? demands.Where(d => d.TotalQuotations > 0)
                    : demands.Where(d => d.TotalQuotations == 0);

            // 应用排序
            demands = query.SortBy?.ToLower() switch
            {
                "bearingnumber" => query.SortDescending ? demands.OrderByDescending(d => d.BearingNumber) : demands.OrderBy(d => d.BearingNumber),
                "requiredquantity" => query.SortDescending ? demands.OrderByDescending(d => d.RequiredQuantity) : demands.OrderBy(d => d.RequiredQuantity),
                "maxprice" => query.SortDescending ? demands.OrderByDescending(d => d.MaxPrice) : demands.OrderBy(d => d.MaxPrice),
                "totalmatches" => query.SortDescending ? demands.OrderByDescending(d => d.TotalMatches) : demands.OrderBy(d => d.TotalMatches),
                "expiresat" => query.SortDescending ? demands.OrderByDescending(d => d.ExpiresAt) : demands.OrderBy(d => d.ExpiresAt),
                _ => query.SortDescending ? demands.OrderByDescending(d => d.CreatedAt) : demands.OrderBy(d => d.CreatedAt)
            };

            // 分页
            if (query.PageSize > 0)
            {
                demands = demands
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize);
            }

            return await demands.ToListAsync();
        }

        public async Task<int> GetCountAsync(DemandQuery query)
        {
            var demands = _context.Demands.AsQueryable();

            // 应用相同的筛选条件
            if (!string.IsNullOrEmpty(query.BearingNumber))
                demands = demands.Where(d => d.BearingNumber.Contains(query.BearingNumber));

            if (query.Status.HasValue)
                demands = demands.Where(d => d.Status == query.Status.Value);

            if (query.Type.HasValue)
                demands = demands.Where(d => d.Type == query.Type.Value);

            if (query.Priority.HasValue)
                demands = demands.Where(d => d.Priority == query.Priority.Value);

            if (query.RequesterId.HasValue)
                demands = demands.Where(d => d.RequesterId == query.RequesterId.Value);

            if (!string.IsNullOrEmpty(query.RequesterType))
                demands = demands.Where(d => d.RequesterType == query.RequesterType);

            if (query.CreatedAfter.HasValue)
                demands = demands.Where(d => d.CreatedAt >= query.CreatedAfter.Value);

            if (query.CreatedBefore.HasValue)
                demands = demands.Where(d => d.CreatedAt <= query.CreatedBefore.Value);

            if (query.ExpiresBefore.HasValue)
                demands = demands.Where(d => d.ExpiresAt != null && d.ExpiresAt <= query.ExpiresBefore.Value);

            if (query.HasMatches.HasValue)
                demands = query.HasMatches.Value
                    ? demands.Where(d => d.TotalMatches > 0)
                    : demands.Where(d => d.TotalMatches == 0);

            if (query.HasQuotations.HasValue)
                demands = query.HasQuotations.Value
                    ? demands.Where(d => d.TotalQuotations > 0)
                    : demands.Where(d => d.TotalQuotations == 0);

            return await demands.CountAsync();
        }

        public async Task<Models.Demand> AddAsync(Models.Demand demand)
        {
            demand.CreatedAt = DateTime.UtcNow;
            demand.UpdatedAt = DateTime.UtcNow;

            _context.Demands.Add(demand);
            await _context.SaveChangesAsync();

            _logger.LogInformation("需求创建成功: {DemandId}", demand.Id);
            return demand;
        }

        public async Task<Models.Demand> UpdateAsync(Models.Demand demand)
        {
            demand.UpdatedAt = DateTime.UtcNow;
            _context.Demands.Update(demand);
            await _context.SaveChangesAsync();

            _logger.LogInformation("需求更新成功: {DemandId}", demand.Id);
            return demand;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var demand = await GetByIdAsync(id);
            if (demand == null) return false;

            _context.Demands.Remove(demand);
            await _context.SaveChangesAsync();

            _logger.LogInformation("需求删除成功: {DemandId}", id);
            return true;
        }

        public async Task<DemandMatch?> GetMatchAsync(long demandId, long supplierId)
        {
            return await _context.DemandMatches
                .FirstOrDefaultAsync(m => m.DemandId == demandId && m.SupplierId == supplierId);
        }

        public async Task<List<DemandMatch>> GetMatchesForDemandAsync(long demandId)
        {
            return await _context.DemandMatches
                .Where(m => m.DemandId == demandId)
                .OrderByDescending(m => m.MatchScore)
                .ToListAsync();
        }

        public async Task<List<DemandMatch>> GetMatchesForSupplierAsync(long supplierId)
        {
            return await _context.DemandMatches
                .Where(m => m.SupplierId == supplierId)
                .Include(m => m.Demand)
                .OrderByDescending(m => m.MatchScore)
                .ToListAsync();
        }

        public async Task<DemandMatch> AddMatchAsync(DemandMatch match)
        {
            match.CreatedAt = DateTime.UtcNow;
            _context.DemandMatches.Add(match);
            await _context.SaveChangesAsync();

            // 更新需求的匹配计数
            var demand = await GetByIdAsync(match.DemandId);
            if (demand != null)
            {
                demand.TotalMatches = await _context.DemandMatches
                    .CountAsync(m => m.DemandId == match.DemandId);
                await UpdateAsync(demand);
            }

            _logger.LogInformation("需求匹配添加成功: DemandId={DemandId}, SupplierId={SupplierId}",
                match.DemandId, match.SupplierId);
            return match;
        }

        public async Task<DemandMatch> UpdateMatchAsync(DemandMatch match)
        {
            _context.DemandMatches.Update(match);
            await _context.SaveChangesAsync();

            _logger.LogInformation("需求匹配更新成功: MatchId={MatchId}", match.Id);
            return match;
        }

        public async Task<bool> RemoveMatchAsync(long matchId)
        {
            var match = await _context.DemandMatches.FindAsync(matchId);
            if (match == null) return false;

            _context.DemandMatches.Remove(match);
            await _context.SaveChangesAsync();

            // 更新需求的匹配计数
            var demand = await GetByIdAsync(match.DemandId);
            if (demand != null)
            {
                demand.TotalMatches = await _context.DemandMatches
                    .CountAsync(m => m.DemandId == match.DemandId);
                await UpdateAsync(demand);
            }

            _logger.LogInformation("需求匹配删除成功: MatchId={MatchId}", matchId);
            return true;
        }

        public async Task AddViewAsync(DemandView view)
        {
            view.ViewedAt = DateTime.UtcNow;
            _context.DemandViews.Add(view);
            await _context.SaveChangesAsync();

            // 更新需求的查看计数
            var demand = await GetByIdAsync(view.DemandId);
            if (demand != null)
            {
                demand.TotalViews = await _context.DemandViews
                    .CountAsync(v => v.DemandId == view.DemandId);
                await UpdateAsync(demand);
            }
        }

        public async Task<List<DemandView>> GetViewsForDemandAsync(long demandId, int limit = 50)
        {
            return await _context.DemandViews
                .Where(v => v.DemandId == demandId)
                .OrderByDescending(v => v.ViewedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetViewCountAsync(long demandId)
        {
            return await _context.DemandViews
                .CountAsync(v => v.DemandId == demandId);
        }

        public async Task<int> GetActiveDemandsCountAsync()
        {
            return await _context.Demands
                .Where(d => d.Status == DemandStatus.Active)
                .CountAsync();
        }

        public async Task<int> GetTotalMatchesCountAsync()
        {
            return await _context.DemandMatches.CountAsync();
        }

        public async Task<Dictionary<DemandStatus, int>> GetDemandStatsByStatusAsync()
        {
            return await _context.Demands
                .GroupBy(d => d.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetPopularBearingNumbersAsync(int topN = 10)
        {
            return await _context.Demands
                .Where(d => d.Status == DemandStatus.Active)
                .GroupBy(d => d.BearingNumber)
                .Select(g => new { BearingNumber = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(topN)
                .ToDictionaryAsync(x => x.BearingNumber, x => x.Count);
        }
    }
}
