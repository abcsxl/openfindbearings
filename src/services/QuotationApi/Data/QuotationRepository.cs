using Microsoft.EntityFrameworkCore;
using QuotationApi.Models.DTOs;
using QuotationApi.Models.Entities;

namespace QuotationApi.Data
{
    public class QuotationRepository : IQuotationRepository
    {
        private readonly QuotationDbContext _context;
        private readonly ILogger<QuotationRepository> _logger;

        public QuotationRepository(QuotationDbContext context, ILogger<QuotationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // 基础 CRUD
        public async Task<Quotation> AddAsync(Quotation quotation)
        {
            await _context.Quotations.AddAsync(quotation);
            await _context.SaveChangesAsync();
            _logger.LogInformation("报价单添加成功: {QuotationNumber}", quotation.QuotationNumber);
            return quotation;
        }

        public async Task<Quotation?> GetByIdAsync(long id)
        {
            return await _context.Quotations.FindAsync(id);
        }

        public async Task<Quotation?> GetByIdWithIncludesAsync(long id)
        {
            return await _context.Quotations
                .Include(q => q.Items)
                .Include(q => q.Attachments)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<Quotation?> GetByQuotationNumberAsync(string quotationNumber)
        {
            return await _context.Quotations
                .FirstOrDefaultAsync(q => q.QuotationNumber == quotationNumber);
        }

        public async Task<List<Quotation>> GetListAsync(QuotationQuery query)
        {
            var quotations = _context.Quotations.AsQueryable();

            // 应用筛选条件
            if (query.DemandId.HasValue)
                quotations = quotations.Where(q => q.DemandId == query.DemandId.Value);

            if (query.SupplierId.HasValue)
                quotations = quotations.Where(q => q.SupplierId == query.SupplierId.Value);

            if (query.Status.HasValue)
                quotations = quotations.Where(q => q.Status == query.Status.Value);

            if (query.IsRecommended.HasValue)
                quotations = quotations.Where(q => q.IsRecommended == query.IsRecommended.Value);

            if (!string.IsNullOrEmpty(query.BearingNumber))
                quotations = quotations.Where(q => q.BearingNumber.Contains(query.BearingNumber));

            if (!string.IsNullOrEmpty(query.Brand))
                quotations = quotations.Where(q => q.Brand == query.Brand);

            if (query.MinPrice.HasValue)
                quotations = quotations.Where(q => q.UnitPrice >= query.MinPrice.Value);

            if (query.MaxPrice.HasValue)
                quotations = quotations.Where(q => q.UnitPrice <= query.MaxPrice.Value);

            if (query.StartDate.HasValue)
                quotations = quotations.Where(q => q.CreatedAt >= query.StartDate.Value);

            if (query.EndDate.HasValue)
                quotations = quotations.Where(q => q.CreatedAt <= query.EndDate.Value);

            // 排序
            quotations = query.SortBy?.ToLower() switch
            {
                "price" => query.SortOrder?.ToLower() == "asc"
                    ? quotations.OrderBy(q => q.UnitPrice)
                    : quotations.OrderByDescending(q => q.UnitPrice),
                "createdat" => query.SortOrder?.ToLower() == "asc"
                    ? quotations.OrderBy(q => q.CreatedAt)
                    : quotations.OrderByDescending(q => q.CreatedAt),
                "matchscore" => query.SortOrder?.ToLower() == "asc"
                    ? quotations.OrderBy(q => q.MatchScore)
                    : quotations.OrderByDescending(q => q.MatchScore),
                _ => quotations.OrderByDescending(q => q.CreatedAt)
            };

            // 分页
            if (query.PageSize > 0)
            {
                quotations = quotations
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize);
            }

            return await quotations.ToListAsync();
        }

        public async Task<int> GetCountAsync(QuotationQuery query)
        {
            var quotations = _context.Quotations.AsQueryable();

            if (query.DemandId.HasValue)
                quotations = quotations.Where(q => q.DemandId == query.DemandId.Value);

            if (query.SupplierId.HasValue)
                quotations = quotations.Where(q => q.SupplierId == query.SupplierId.Value);

            if (query.Status.HasValue)
                quotations = quotations.Where(q => q.Status == query.Status.Value);

            if (!string.IsNullOrEmpty(query.BearingNumber))
                quotations = quotations.Where(q => q.BearingNumber.Contains(query.BearingNumber));

            return await quotations.CountAsync();
        }

        public async Task<Quotation> UpdateAsync(Quotation quotation)
        {
            _context.Quotations.Update(quotation);
            await _context.SaveChangesAsync();
            _logger.LogInformation("报价单更新成功: {QuotationNumber}", quotation.QuotationNumber);
            return quotation;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var quotation = await GetByIdAsync(id);
            if (quotation == null) return false;

            _context.Quotations.Remove(quotation);
            await _context.SaveChangesAsync();
            _logger.LogInformation("报价单删除成功: {QuotationId}", id);
            return true;
        }

        // 业务查询
        public async Task<List<Quotation>> GetByDemandIdAsync(long demandId)
        {
            return await _context.Quotations
                .Where(q => q.DemandId == demandId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Quotation>> GetBySupplierIdAsync(long supplierId)
        {
            return await _context.Quotations
                .Where(q => q.SupplierId == supplierId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Quotation>> GetPendingQuotationsAsync()
        {
            return await _context.Quotations
                .Where(q => q.Status == QuotationStatus.Pending)
                .OrderBy(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Quotation>> GetExpiredQuotationsAsync(DateTime cutoffDate)
        {
            return await _context.Quotations
                .Where(q => q.ExpiresAt <= cutoffDate &&
                           q.Status != QuotationStatus.Expired &&
                           q.Status != QuotationStatus.Withdrawn)
                .ToListAsync();
        }

        public async Task<List<Quotation>> GetExpiringQuotationsAsync(DateTime beforeDate)
        {
            return await _context.Quotations
                .Where(q => q.ExpiresAt <= beforeDate &&
                           q.ExpiresAt > DateTime.UtcNow &&
                           q.Status == QuotationStatus.Submitted)
                .ToListAsync();
        }

        public async Task<List<Quotation>> GetRecommendedQuotationsAsync(long demandId, int limit = 10)
        {
            return await _context.Quotations
                .Where(q => q.DemandId == demandId && q.IsRecommended)
                .OrderByDescending(q => q.MatchScore)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<PagedResponse<Quotation>> SearchAsync(QuotationSearchRequest request)
        {
            var query = _context.Quotations.AsQueryable();

            if (!string.IsNullOrEmpty(request.BearingNumber))
                query = query.Where(q => q.BearingNumber.Contains(request.BearingNumber));

            if (!string.IsNullOrEmpty(request.Brand))
                query = query.Where(q => q.Brand == request.Brand);

            if (!string.IsNullOrEmpty(request.SupplierName))
                query = query.Where(q => q.SupplierName != null && q.SupplierName.Contains(request.SupplierName));

            if (request.DemandId.HasValue)
                query = query.Where(q => q.DemandId == request.DemandId.Value);

            if (request.MinPrice.HasValue)
                query = query.Where(q => q.UnitPrice >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                query = query.Where(q => q.UnitPrice <= request.MaxPrice.Value);

            if (!string.IsNullOrEmpty(request.Status))
            {
                if (Enum.TryParse<QuotationStatus>(request.Status, out var status))
                    query = query.Where(q => q.Status == status);
            }

            if (request.IsRecommended.HasValue)
                query = query.Where(q => q.IsRecommended == request.IsRecommended.Value);

            if (request.StartDate.HasValue)
                query = query.Where(q => q.CreatedAt >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                query = query.Where(q => q.CreatedAt <= request.EndDate.Value);

            var totalCount = await query.CountAsync();

            // 排序
            query = request.SortBy?.ToLower() switch
            {
                "price" => request.SortOrder?.ToLower() == "asc"
                    ? query.OrderBy(q => q.UnitPrice)
                    : query.OrderByDescending(q => q.UnitPrice),
                "createdat" => request.SortOrder?.ToLower() == "asc"
                    ? query.OrderBy(q => q.CreatedAt)
                    : query.OrderByDescending(q => q.CreatedAt),
                "matchscore" => request.SortOrder?.ToLower() == "asc"
                    ? query.OrderBy(q => q.MatchScore)
                    : query.OrderByDescending(q => q.MatchScore),
                _ => query.OrderByDescending(q => q.CreatedAt)
            };

            // 分页
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResponse<Quotation>
            {
                Items = items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<List<Quotation>> GetByBearingNumberAsync(string bearingNumber)
        {
            return await _context.Quotations
                .Where(q => q.BearingNumber == bearingNumber)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Quotation>> FindSimilarQuotationsAsync(string bearingNumber, int limit = 10)
        {
            return await _context.Quotations
                .Where(q => q.BearingNumber == bearingNumber)
                .OrderByDescending(q => q.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        // 统计
        public async Task<int> GetTotalQuotationsCountAsync()
        {
            return await _context.Quotations.CountAsync();
        }

        public async Task<int> GetQuotationsCountByStatusAsync(QuotationStatus status)
        {
            return await _context.Quotations.CountAsync(q => q.Status == status);
        }

        public async Task<Dictionary<QuotationStatus, int>> GetQuotationStatsByStatusAsync()
        {
            return await _context.Quotations
                .GroupBy(q => q.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetQuotationStatsByBrandAsync(int topN = 10)
        {
            var stats = await _context.Quotations
                .Where(q => q.Brand != null)
                .GroupBy(q => q.Brand!)
                .Select(g => new { Brand = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(topN)
                .ToListAsync();

            return stats.ToDictionary(x => x.Brand, x => x.Count);
        }

        public async Task<decimal> GetAverageResponseTimeAsync(long demandId)
        {
            var quotations = await GetByDemandIdAsync(demandId);
            var submittedQuotations = quotations.Where(q => q.SubmittedAt.HasValue).ToList();

            if (!submittedQuotations.Any()) return 0;

            var totalHours = submittedQuotations
                .Sum(q => (q.SubmittedAt!.Value - q.CreatedAt).TotalHours);

            return (decimal)(totalHours / submittedQuotations.Count);
        }

        // 业务操作
        public async Task<bool> UpdateStatusAsync(long quotationId, QuotationStatus newStatus)
        {
            var quotation = await GetByIdAsync(quotationId);
            if (quotation == null) return false;

            quotation.Status = newStatus;
            quotation.UpdatedAt = DateTime.UtcNow;

            switch (newStatus)
            {
                case QuotationStatus.Submitted:
                    quotation.SubmittedAt = DateTime.UtcNow;
                    break;
                case QuotationStatus.Accepted:
                    quotation.AcceptedAt = DateTime.UtcNow;
                    break;
                case QuotationStatus.Rejected:
                    quotation.RejectedAt = DateTime.UtcNow;
                    break;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SubmitQuotationAsync(long quotationId)
        {
            return await UpdateStatusAsync(quotationId, QuotationStatus.Submitted);
        }

        public async Task<bool> AcceptQuotationAsync(long quotationId)
        {
            return await UpdateStatusAsync(quotationId, QuotationStatus.Accepted);
        }

        public async Task<bool> RejectQuotationAsync(long quotationId)
        {
            return await UpdateStatusAsync(quotationId, QuotationStatus.Rejected);
        }

        public async Task<bool> WithdrawQuotationAsync(long quotationId)
        {
            return await UpdateStatusAsync(quotationId, QuotationStatus.Withdrawn);
        }

        public async Task<bool> ExpireQuotationAsync(long quotationId)
        {
            var quotation = await GetByIdAsync(quotationId);
            if (quotation == null) return false;

            quotation.Status = QuotationStatus.Expired;
            quotation.UpdatedAt = DateTime.UtcNow;
            quotation.ExpiresAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetRecommendedAsync(long quotationId, bool isRecommended, decimal? matchScore = null)
        {
            var quotation = await GetByIdAsync(quotationId);
            if (quotation == null) return false;

            quotation.IsRecommended = isRecommended;
            if (matchScore.HasValue)
                quotation.MatchScore = matchScore.Value;

            quotation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // 项目管理
        public async Task<List<QuotationItem>> GetQuotationItemsAsync(long quotationId)
        {
            return await _context.QuotationItems
                .Where(i => i.QuotationId == quotationId)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();
        }

        public async Task<QuotationItem> AddQuotationItemAsync(QuotationItem item)
        {
            await _context.QuotationItems.AddAsync(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> RemoveQuotationItemAsync(long itemId)
        {
            var item = await _context.QuotationItems.FindAsync(itemId);
            if (item == null) return false;

            _context.QuotationItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        // 附件管理
        public async Task<List<QuotationAttachment>> GetQuotationAttachmentsAsync(long quotationId)
        {
            return await _context.QuotationAttachments
                .Where(a => a.QuotationId == quotationId)
                .OrderByDescending(a => a.UploadedAt)
                .ToListAsync();
        }

        public async Task<QuotationAttachment> AddQuotationAttachmentAsync(QuotationAttachment attachment)
        {
            await _context.QuotationAttachments.AddAsync(attachment);
            await _context.SaveChangesAsync();
            return attachment;
        }

        public async Task<bool> RemoveQuotationAttachmentAsync(long attachmentId)
        {
            var attachment = await _context.QuotationAttachments.FindAsync(attachmentId);
            if (attachment == null) return false;

            _context.QuotationAttachments.Remove(attachment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
