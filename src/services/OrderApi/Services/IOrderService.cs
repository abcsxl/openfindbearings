using OrderApi.Models.DTOs;

namespace OrderApi.Services
{
    public interface IOrderService
    {
        // 基础CRUD
        Task<OrderDetailResponse> CreateOrderAsync(CreateOrderRequest request);
        Task<OrderDetailResponse?> GetOrderAsync(long id);
        Task<OrderDetailResponse?> GetOrderByNumberAsync(string orderNumber);
        Task<PagedResponse<OrderResponse>> GetOrdersAsync(OrderQuery query);
        Task<OrderDetailResponse> UpdateOrderAsync(long id, UpdateOrderRequest request);
        Task<bool> DeleteOrderAsync(long id);

        // 业务操作
        Task<OrderDetailResponse> CreateFromQuotationAsync(long quotationId);
        Task<OrderDetailResponse> UpdateOrderStatusAsync(long id, UpdateOrderStatusRequest request);
        Task<OrderDetailResponse> ProcessPaymentAsync(long id, ProcessPaymentRequest request);
        Task<OrderDetailResponse> UpdateShippingAsync(long id, UpdateShippingRequest request);
        Task<OrderDetailResponse> CancelOrderAsync(long id, string? reason = null);
        Task<OrderDetailResponse> CompleteOrderAsync(long id);

        // 查询
        Task<List<OrderResponse>> GetOrdersByQuotationAsync(long quotationId);
        Task<List<OrderResponse>> GetOrdersByDemandAsync(long demandId);
        Task<List<OrderResponse>> GetOrdersBySupplierAsync(long supplierId);
        Task<List<OrderResponse>> GetPendingOrdersAsync();
        Task<List<OrderResponse>> GetExpiringOrdersAsync();
        Task<List<OrderResponse>> GetRecommendedOrdersAsync(int limit = 10);

        // 统计和分析
        Task<OrderStatisticsResponse> GetOrderStatisticsAsync();
        Task<OrderStatisticsResponse> GetSupplierStatisticsAsync(long supplierId);
        Task<Dictionary<string, decimal>> GetRevenueTrendsAsync(int days = 30);

        // 批量操作
        Task<List<OrderResponse>> CreateBulkOrdersAsync(List<CreateOrderRequest> requests);
        Task<bool> ExpireOldOrdersAsync(DateTime cutoffDate);
        Task<bool> SendOrderRemindersAsync();

        // 附件管理
        Task<OrderAttachmentResponse> AddAttachmentAsync(long orderId, AddAttachmentRequest request);
        Task<bool> RemoveAttachmentAsync(long attachmentId);
        Task<List<OrderAttachmentResponse>> GetAttachmentsAsync(long orderId);

        // 项目管理
        Task<OrderItemResponse> AddOrderItemAsync(long orderId, AddOrderItemRequest request);
        Task<bool> RemoveOrderItemAsync(long itemId);
        Task<List<OrderItemResponse>> GetOrderItemsAsync(long orderId);
    }
}
