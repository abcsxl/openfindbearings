using Microsoft.EntityFrameworkCore;
using OrderApi.Models.DTOs;
using OrderApi.Models.Entities;

namespace OrderApi.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(OrderDbContext context, ILogger<OrderRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Order> AddAsync(Order order)
        {
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("订单创建成功: {OrderNumber}", order.OrderNumber);
            return order;
        }

        public async Task<Order?> GetByIdAsync(long id)
        {
            return await _context.Orders.FindAsync(id);
        }

        public async Task<Order?> GetByIdWithIncludesAsync(long id)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Attachments)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<List<Order>> GetListAsync(OrderQuery query)
        {
            var orders = _context.Orders.AsQueryable();

            // 应用筛选条件
            if (query.QuotationId.HasValue)
                orders = orders.Where(o => o.QuotationId == query.QuotationId.Value);

            if (query.DemandId.HasValue)
                orders = orders.Where(o => o.DemandId == query.DemandId.Value);

            if (query.SupplierId.HasValue)
                orders = orders.Where(o => o.SupplierId == query.SupplierId.Value);

            if (!string.IsNullOrEmpty(query.OrderNumber))
                orders = orders.Where(o => o.OrderNumber.Contains(query.OrderNumber));

            if (!string.IsNullOrEmpty(query.BearingNumber))
                orders = orders.Where(o => o.BearingNumber.Contains(query.BearingNumber));

            if (!string.IsNullOrEmpty(query.Brand))
                orders = orders.Where(o => o.Brand == query.Brand);

            if (query.Status.HasValue)
                orders = orders.Where(o => o.Status == query.Status.Value);

            if (query.PaymentStatus.HasValue)
                orders = orders.Where(o => o.PaymentStatus == query.PaymentStatus.Value);

            if (query.ShippingStatus.HasValue)
                orders = orders.Where(o => o.ShippingStatus == query.ShippingStatus.Value);

            if (query.MinAmount.HasValue)
                orders = orders.Where(o => o.TotalAmount >= query.MinAmount.Value);

            if (query.MaxAmount.HasValue)
                orders = orders.Where(o => o.TotalAmount <= query.MaxAmount.Value);

            if (query.StartDate.HasValue)
                orders = orders.Where(o => o.CreatedAt >= query.StartDate.Value);

            if (query.EndDate.HasValue)
                orders = orders.Where(o => o.CreatedAt <= query.EndDate.Value);

            // 排序
            orders = query.SortBy?.ToLower() switch
            {
                "amount" => query.SortDescending ?
                    orders.OrderByDescending(o => o.TotalAmount) :
                    orders.OrderBy(o => o.TotalAmount),
                "createdat" => query.SortDescending ?
                    orders.OrderByDescending(o => o.CreatedAt) :
                    orders.OrderBy(o => o.CreatedAt),
                "deliverydate" => query.SortDescending ?
                    orders.OrderByDescending(o => o.EstimatedDeliveryDate) :
                    orders.OrderBy(o => o.EstimatedDeliveryDate),
                _ => query.SortDescending ?
                    orders.OrderByDescending(o => o.CreatedAt) :
                    orders.OrderBy(o => o.CreatedAt)
            };

            // 分页
            if (query.PageSize > 0)
            {
                orders = orders
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize);
            }

            return await orders.ToListAsync();
        }

        public async Task<int> GetCountAsync(OrderQuery query)
        {
            var orders = _context.Orders.AsQueryable();

            if (query.QuotationId.HasValue)
                orders = orders.Where(o => o.QuotationId == query.QuotationId.Value);

            if (query.Status.HasValue)
                orders = orders.Where(o => o.Status == query.Status.Value);

            if (query.StartDate.HasValue)
                orders = orders.Where(o => o.CreatedAt >= query.StartDate.Value);

            return await orders.CountAsync();
        }

        public async Task<Order> UpdateAsync(Order order)
        {
            order.UpdatedAt = DateTime.UtcNow;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("订单更新成功: {OrderNumber}", order.OrderNumber);
            return order;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var order = await GetByIdAsync(id);
            if (order == null) return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("订单删除成功: {OrderId}", id);
            return true;
        }

        public async Task<List<Order>> GetByQuotationIdAsync(long quotationId)
        {
            return await _context.Orders
                .Where(o => o.QuotationId == quotationId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Order>> GetByDemandIdAsync(long demandId)
        {
            return await _context.Orders
                .Where(o => o.DemandId == demandId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Order>> GetBySupplierIdAsync(long supplierId)
        {
            return await _context.Orders
                .Where(o => o.SupplierId == supplierId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Order>> GetPendingOrdersAsync()
        {
            return await _context.Orders
                .Where(o => o.Status == OrderStatus.Created || o.Status == OrderStatus.Paid)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _context.Orders
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Order>> GetExpiringOrdersAsync(DateTime beforeDate)
        {
            return await _context.Orders
                .Where(o => o.EstimatedDeliveryDate <= beforeDate &&
                           o.EstimatedDeliveryDate > DateTime.UtcNow &&
                           o.Status == OrderStatus.Paid)
                .ToListAsync();
        }

        public async Task<int> GetTotalOrdersCountAsync()
        {
            return await _context.Orders.CountAsync();
        }

        public async Task<int> GetOrdersCountByStatusAsync(OrderStatus status)
        {
            return await _context.Orders.CountAsync(o => o.Status == status);
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Orders.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(o => o.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(o => o.CreatedAt <= endDate.Value);

            return await query.SumAsync(o => o.TotalAmount);
        }

        public async Task<Dictionary<OrderStatus, int>> GetOrderStatsByStatusAsync()
        {
            return await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetOrderStatsByBrandAsync(int topN = 10)
        {
            return await _context.Orders
                .Where(o => !string.IsNullOrEmpty(o.Brand))
                .GroupBy(o => o.Brand)
                .Select(g => new { Brand = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(topN)
                .ToDictionaryAsync(x => x.Brand, x => x.Count);
        }

        public async Task<bool> UpdateStatusAsync(long orderId, OrderStatus newStatus)
        {
            var order = await GetByIdAsync(orderId);
            if (order == null) return false;

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            // 设置时间戳
            switch (newStatus)
            {
                case OrderStatus.Paid:
                    order.PaidAt = DateTime.UtcNow;
                    break;
                case OrderStatus.Shipped:
                    order.ShippedAt = DateTime.UtcNow;
                    break;
                case OrderStatus.Completed:
                    order.CompletedAt = DateTime.UtcNow;
                    break;
                case OrderStatus.Cancelled:
                    order.CancelledAt = DateTime.UtcNow;
                    break;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePaymentStatusAsync(long orderId, PaymentStatus newStatus)
        {
            var order = await GetByIdAsync(orderId);
            if (order == null) return false;

            order.PaymentStatus = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            if (newStatus == PaymentStatus.Paid)
                order.PaidAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateShippingStatusAsync(long orderId, ShippingStatus newStatus)
        {
            var order = await GetByIdAsync(orderId);
            if (order == null) return false;

            order.ShippingStatus = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            if (newStatus == ShippingStatus.Shipped)
                order.ShippedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<OrderItem>> GetOrderItemsAsync(long orderId)
        {
            return await _context.OrderItems
                .Where(i => i.OrderId == orderId)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();
        }

        public async Task<OrderItem> AddOrderItemAsync(OrderItem item)
        {
            _context.OrderItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> RemoveOrderItemAsync(long itemId)
        {
            var item = await _context.OrderItems.FindAsync(itemId);
            if (item == null) return false;

            _context.OrderItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<OrderAttachment>> GetOrderAttachmentsAsync(long orderId)
        {
            return await _context.OrderAttachments
                .Where(a => a.OrderId == orderId)
                .OrderByDescending(a => a.UploadedAt)
                .ToListAsync();
        }

        public async Task<OrderAttachment> AddOrderAttachmentAsync(OrderAttachment attachment)
        {
            _context.OrderAttachments.Add(attachment);
            await _context.SaveChangesAsync();
            return attachment;
        }

        public async Task<bool> RemoveOrderAttachmentAsync(long attachmentId)
        {
            var attachment = await _context.OrderAttachments.FindAsync(attachmentId);
            if (attachment == null) return false;

            _context.OrderAttachments.Remove(attachment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
