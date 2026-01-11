using OrderApi.Models.DTOs;
using OrderApi.Models.Entities;

namespace OrderApi.Data
{
    public interface IOrderRepository
    {
        // 基础CRUD
        Task<Order> AddAsync(Order order);
        Task<Order?> GetByIdAsync(long id);
        Task<Order?> GetByIdWithIncludesAsync(long id);
        Task<Order?> GetByOrderNumberAsync(string orderNumber);
        Task<List<Order>> GetListAsync(OrderQuery query);
        Task<int> GetCountAsync(OrderQuery query);
        Task<Order> UpdateAsync(Order order);
        Task<bool> DeleteAsync(long id);

        // 业务查询
        Task<List<Order>> GetByQuotationIdAsync(long quotationId);
        Task<List<Order>> GetByDemandIdAsync(long demandId);
        Task<List<Order>> GetBySupplierIdAsync(long supplierId);
        Task<List<Order>> GetPendingOrdersAsync();
        Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task<List<Order>> GetExpiringOrdersAsync(DateTime beforeDate);

        // 统计
        Task<int> GetTotalOrdersCountAsync();
        Task<int> GetOrdersCountByStatusAsync(OrderStatus status);
        Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<OrderStatus, int>> GetOrderStatsByStatusAsync();
        Task<Dictionary<string, int>> GetOrderStatsByBrandAsync(int topN = 10);

        // 业务操作
        Task<bool> UpdateStatusAsync(long orderId, OrderStatus newStatus);
        Task<bool> UpdatePaymentStatusAsync(long orderId, PaymentStatus newStatus);
        Task<bool> UpdateShippingStatusAsync(long orderId, ShippingStatus newStatus);

        // 项目管理
        Task<List<OrderItem>> GetOrderItemsAsync(long orderId);
        Task<OrderItem> AddOrderItemAsync(OrderItem item);
        Task<bool> RemoveOrderItemAsync(long itemId);

        // 附件管理
        Task<List<OrderAttachment>> GetOrderAttachmentsAsync(long orderId);
        Task<OrderAttachment> AddOrderAttachmentAsync(OrderAttachment attachment);
        Task<bool> RemoveOrderAttachmentAsync(long attachmentId);
    }
}
