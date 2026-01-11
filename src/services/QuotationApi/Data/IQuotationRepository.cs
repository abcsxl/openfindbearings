using QuotationApi.Models.DTOs;
using QuotationApi.Models.Entities;

namespace QuotationApi.Data
{
    public interface IQuotationRepository
    {
        // 基础 CRUD
        Task<Quotation> AddAsync(Quotation quotation);
        Task<Quotation?> GetByIdAsync(long id);
        Task<Quotation?> GetByIdWithIncludesAsync(long id);
        Task<Quotation?> GetByQuotationNumberAsync(string quotationNumber);
        Task<List<Quotation>> GetListAsync(QuotationQuery query);
        Task<int> GetCountAsync(QuotationQuery query);
        Task<Quotation> UpdateAsync(Quotation quotation);
        Task<bool> DeleteAsync(long id);

        // 业务查询
        Task<List<Quotation>> GetByDemandIdAsync(long demandId);
        Task<List<Quotation>> GetBySupplierIdAsync(long supplierId);
        Task<List<Quotation>> GetByBearingNumberAsync(string bearingNumber);
        Task<List<Quotation>> GetPendingQuotationsAsync();
        Task<List<Quotation>> GetExpiredQuotationsAsync(DateTime cutoffDate);
        Task<List<Quotation>> GetExpiringQuotationsAsync(DateTime beforeDate);
        Task<List<Quotation>> GetRecommendedQuotationsAsync(long demandId, int limit = 10);

        // 搜索
        Task<PagedResponse<Quotation>> SearchAsync(QuotationSearchRequest request);
        Task<List<Quotation>> FindSimilarQuotationsAsync(string bearingNumber, int limit = 10);

        // 统计
        Task<int> GetTotalQuotationsCountAsync();
        Task<int> GetQuotationsCountByStatusAsync(QuotationStatus status);
        Task<Dictionary<QuotationStatus, int>> GetQuotationStatsByStatusAsync();
        Task<Dictionary<string, int>> GetQuotationStatsByBrandAsync(int topN = 10);
        Task<decimal> GetAverageResponseTimeAsync(long demandId);

        // 业务操作
        Task<bool> UpdateStatusAsync(long quotationId, QuotationStatus newStatus);
        Task<bool> SetRecommendedAsync(long quotationId, bool isRecommended, decimal? matchScore = null);
        Task<bool> SubmitQuotationAsync(long quotationId);
        Task<bool> AcceptQuotationAsync(long quotationId);
        Task<bool> RejectQuotationAsync(long quotationId);
        Task<bool> WithdrawQuotationAsync(long quotationId);
        Task<bool> ExpireQuotationAsync(long quotationId);

        // 项目管理
        Task<List<QuotationItem>> GetQuotationItemsAsync(long quotationId);
        Task<QuotationItem> AddQuotationItemAsync(QuotationItem item);
        Task<bool> RemoveQuotationItemAsync(long itemId);

        // 附件管理
        Task<List<QuotationAttachment>> GetQuotationAttachmentsAsync(long quotationId);
        Task<QuotationAttachment> AddQuotationAttachmentAsync(QuotationAttachment attachment);
        Task<bool> RemoveQuotationAttachmentAsync(long attachmentId);
    }
}
