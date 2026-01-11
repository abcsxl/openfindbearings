using Dapr.Client;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Models.DTOs;
using OrderApi.Models.Entities;

namespace OrderApi.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;
        private readonly ILogger<OrderService> _logger;
        private readonly DaprClient _daprClient;
        private readonly OrderDbContext _context;

        public OrderService(
            IOrderRepository repository,
            ILogger<OrderService> logger,
            DaprClient daprClient,
            OrderDbContext context)
        {
            _repository = repository;
            _logger = logger;
            _daprClient = daprClient;
            _context = context;
        }

        public async Task<OrderDetailResponse> CreateOrderAsync(CreateOrderRequest request)
        {
            try
            {
                // 验证请求数据
                if (request.QuotationId <= 0)
                    throw new ArgumentException("报价单ID无效");

                // 检查是否已存在订单
                var existingOrders = await _repository.GetByQuotationIdAsync(request.QuotationId);
                if (existingOrders.Any())
                    throw new InvalidOperationException($"报价单 {request.QuotationId} 已存在订单");

                // 生成订单号
                var orderNumber = await GenerateOrderNumberAsync();

                var order = new Order
                {
                    OrderNumber = orderNumber,
                    QuotationId = request.QuotationId,
                    Notes = request.Notes,
                    Status = OrderStatus.Created,
                    PaymentStatus = PaymentStatus.Pending,
                    ShippingStatus = ShippingStatus.NotShipped,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // 从报价单获取详细信息
                await PopulateOrderFromQuotationAsync(order, request.QuotationId);

                var result = await _repository.AddAsync(order);

                // 发布订单创建事件
                await PublishOrderCreatedEventAsync(result);

                _logger.LogInformation("订单创建成功: {OrderNumber}", orderNumber);

                return MapToDetailResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建订单失败: {QuotationId}", request.QuotationId);
                throw;
            }
        }

        public async Task<OrderDetailResponse> CreateFromQuotationAsync(long quotationId)
        {
            try
            {
                // 检查是否已存在订单
                var existingOrders = await _repository.GetByQuotationIdAsync(quotationId);
                if (existingOrders.Any())
                    throw new InvalidOperationException($"报价单 {quotationId} 已存在订单");

                var orderNumber = await GenerateOrderNumberAsync();

                var order = new Order
                {
                    OrderNumber = orderNumber,
                    QuotationId = quotationId,
                    Status = OrderStatus.Created,
                    PaymentStatus = PaymentStatus.Pending,
                    ShippingStatus = ShippingStatus.NotShipped,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // 从报价单获取详细信息
                await PopulateOrderFromQuotationAsync(order, quotationId);

                var result = await _repository.AddAsync(order);

                // 发布订单创建事件
                await PublishOrderCreatedEventAsync(result);

                _logger.LogInformation("从报价单创建订单成功: {QuotationId} -> {OrderNumber}", quotationId, orderNumber);

                return MapToDetailResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从报价单创建订单失败: {QuotationId}", quotationId);
                throw;
            }
        }

        public async Task<OrderDetailResponse?> GetOrderAsync(long id)
        {
            try
            {
                var order = await _repository.GetByIdWithIncludesAsync(id);
                if (order == null) return null;

                return MapToDetailResponse(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单详情失败: {OrderId}", id);
                throw;
            }
        }

        public async Task<OrderDetailResponse?> GetOrderByNumberAsync(string orderNumber)
        {
            try
            {
                var order = await _repository.GetByOrderNumberAsync(orderNumber);
                if (order == null) return null;

                return await GetOrderAsync(order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单详情失败: {OrderNumber}", orderNumber);
                throw;
            }
        }

        public async Task<PagedResponse<OrderResponse>> GetOrdersAsync(OrderQuery query)
        {
            try
            {
                var orders = await _repository.GetListAsync(query);
                var totalCount = await _repository.GetCountAsync(query);

                return new PagedResponse<OrderResponse>
                {
                    Items = orders.Select(MapToResponse).ToList(),
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单列表失败");
                throw;
            }
        }

        public async Task<OrderDetailResponse> UpdateOrderAsync(long id, UpdateOrderRequest request)
        {
            try
            {
                var order = await _repository.GetByIdAsync(id);
                if (order == null)
                    throw new KeyNotFoundException("订单不存在");

                // 更新字段
                if (!string.IsNullOrEmpty(request.Notes))
                    order.Notes = request.Notes;

                if (request.EstimatedDeliveryDate.HasValue)
                    order.EstimatedDeliveryDate = request.EstimatedDeliveryDate.Value;

                if (!string.IsNullOrEmpty(request.DeliveryAddress))
                    order.DeliveryAddress = request.DeliveryAddress;

                order.UpdatedAt = DateTime.UtcNow;

                var result = await _repository.UpdateAsync(order);
                return MapToDetailResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新订单失败: {OrderId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteOrderAsync(long id)
        {
            try
            {
                return await _repository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除订单失败: {OrderId}", id);
                throw;
            }
        }

        public async Task<OrderDetailResponse> UpdateOrderStatusAsync(long id, UpdateOrderStatusRequest request)
        {
            try
            {
                var success = await _repository.UpdateStatusAsync(id, request.Status);
                if (!success)
                    throw new KeyNotFoundException("订单不存在");

                var order = await _repository.GetByIdAsync(id);
                if (order == null)
                    throw new KeyNotFoundException("订单不存在");

                _logger.LogInformation("订单状态更新: {OrderId} -> {Status}", id, request.Status);

                return MapToDetailResponse(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新订单状态失败: {OrderId}", id);
                throw;
            }
        }

        public async Task<OrderDetailResponse> ProcessPaymentAsync(long id, ProcessPaymentRequest request)
        {
            try
            {
                var order = await _repository.GetByIdAsync(id);
                if (order == null)
                    throw new KeyNotFoundException("订单不存在");

                // 验证支付金额
                if (request.Amount != order.TotalAmount)
                {
                    throw new ArgumentException($"支付金额 {request.Amount} 与订单金额 {order.TotalAmount} 不匹配");
                }

                // 更新支付状态
                order.PaymentStatus = PaymentStatus.Paid;
                order.PaidAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;

                // 如果支付成功，自动更新订单状态为已支付
                if (order.Status == OrderStatus.Created)
                {
                    order.Status = OrderStatus.Paid;
                }

                var result = await _repository.UpdateAsync(order);

                // 发布支付成功事件
                await PublishPaymentProcessedEventAsync(result);

                _logger.LogInformation("订单支付成功: {OrderNumber}, 金额: {Amount}", order.OrderNumber, request.Amount);

                return MapToDetailResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理支付失败: {OrderId}", id);
                throw;
            }
        }

        public async Task<OrderDetailResponse> UpdateShippingAsync(long id, UpdateShippingRequest request)
        {
            try
            {
                var order = await _repository.GetByIdAsync(id);
                if (order == null)
                    throw new KeyNotFoundException("订单不存在");

                order.ShippingStatus = request.ShippingStatus;
                order.UpdatedAt = DateTime.UtcNow;

                // 如果发货，更新发货时间
                if (request.ShippingStatus == ShippingStatus.Shipped)
                {
                    order.ShippedAt = DateTime.UtcNow;
                    order.Status = OrderStatus.Shipped;
                }

                var result = await _repository.UpdateAsync(order);

                // 发布发货事件
                await PublishShippingUpdatedEventAsync(result, request);

                _logger.LogInformation("订单发货状态更新: {OrderNumber} -> {Status}", order.OrderNumber, request.ShippingStatus);

                return MapToDetailResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新发货状态失败: {OrderId}", id);
                throw;
            }
        }

        public async Task<OrderDetailResponse> CancelOrderAsync(long id, string? reason = null)
        {
            try
            {
                var success = await _repository.UpdateStatusAsync(id, OrderStatus.Cancelled);
                if (!success)
                    throw new KeyNotFoundException("订单不存在");

                var order = await _repository.GetByIdAsync(id);
                if (order == null)
                    throw new KeyNotFoundException("订单不存在");

                if (!string.IsNullOrEmpty(reason))
                    order.Notes = reason;

                await _repository.UpdateAsync(order);

                // 发布订单取消事件
                await PublishOrderCancelledEventAsync(order);

                _logger.LogInformation("订单已取消: {OrderNumber}, 原因: {Reason}", order.OrderNumber, reason);

                return MapToDetailResponse(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消订单失败: {OrderId}", id);
                throw;
            }
        }

        public async Task<OrderDetailResponse> CompleteOrderAsync(long id)
        {
            try
            {
                var success = await _repository.UpdateStatusAsync(id, OrderStatus.Completed);
                if (!success)
                    throw new KeyNotFoundException("订单不存在");

                var order = await _repository.GetByIdAsync(id);
                if (order == null)
                    throw new KeyNotFoundException("订单不存在");

                order.CompletedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(order);

                // 发布订单完成事件
                await PublishOrderCompletedEventAsync(order);

                _logger.LogInformation("订单已完成: {OrderNumber}", order.OrderNumber);

                return MapToDetailResponse(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "完成订单失败: {OrderId}", id);
                throw;
            }
        }

        public async Task<List<OrderResponse>> GetOrdersByQuotationAsync(long quotationId)
        {
            try
            {
                var orders = await _repository.GetByQuotationIdAsync(quotationId);
                return orders.Select(MapToResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取报价单相关订单失败: {QuotationId}", quotationId);
                throw;
            }
        }

        public async Task<List<OrderResponse>> GetOrdersByDemandAsync(long demandId)
        {
            try
            {
                var orders = await _repository.GetByDemandIdAsync(demandId);
                return orders.Select(MapToResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取需求相关订单失败: {DemandId}", demandId);
                throw;
            }
        }

        public async Task<List<OrderResponse>> GetOrdersBySupplierAsync(long supplierId)
        {
            try
            {
                var orders = await _repository.GetBySupplierIdAsync(supplierId);
                return orders.Select(MapToResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取供应商相关订单失败: {SupplierId}", supplierId);
                throw;
            }
        }

        public async Task<List<OrderResponse>> GetPendingOrdersAsync()
        {
            try
            {
                var orders = await _repository.GetPendingOrdersAsync();
                return orders.Select(MapToResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取待处理订单失败");
                throw;
            }
        }

        public async Task<List<OrderResponse>> GetExpiringOrdersAsync()
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(3);
                var orders = await _repository.GetExpiringOrdersAsync(cutoffDate);
                return orders.Select(MapToResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取即将到期订单失败");
                throw;
            }
        }

        public async Task<List<OrderResponse>> GetRecommendedOrdersAsync(int limit = 10)
        {
            try
            {
                var orders = await _repository.GetListAsync(new OrderQuery
                {
                    PageSize = limit,
                    SortBy = "CreatedAt",
                    SortDescending = true
                });
                return orders.Select(MapToResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取推荐订单失败");
                throw;
            }
        }

        public async Task<OrderStatisticsResponse> GetOrderStatisticsAsync()
        {
            try
            {
                var totalOrders = await _repository.GetTotalOrdersCountAsync();
                var statusStats = await _repository.GetOrderStatsByStatusAsync();
                var brandStats = await _repository.GetOrderStatsByBrandAsync(10);
                var totalRevenue = await _repository.GetTotalRevenueAsync();

                return new OrderStatisticsResponse
                {
                    TotalOrders = totalOrders,
                    PendingOrders = statusStats.GetValueOrDefault(OrderStatus.Created, 0),
                    PaidOrders = statusStats.GetValueOrDefault(OrderStatus.Paid, 0),
                    ShippedOrders = statusStats.GetValueOrDefault(OrderStatus.Shipped, 0),
                    CompletedOrders = statusStats.GetValueOrDefault(OrderStatus.Completed, 0),
                    CancelledOrders = statusStats.GetValueOrDefault(OrderStatus.Cancelled, 0),
                    TotalRevenue = totalRevenue,
                    AverageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0,
                    StatusDistribution = statusStats.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
                    BrandDistribution = brandStats
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单统计失败");
                throw;
            }
        }

        public async Task<OrderStatisticsResponse> GetSupplierStatisticsAsync(long supplierId)
        {
            try
            {
                var orders = await _repository.GetBySupplierIdAsync(supplierId);

                return new OrderStatisticsResponse
                {
                    TotalOrders = orders.Count,
                    PendingOrders = orders.Count(o => o.Status == OrderStatus.Created),
                    PaidOrders = orders.Count(o => o.Status == OrderStatus.Paid),
                    ShippedOrders = orders.Count(o => o.Status == OrderStatus.Shipped),
                    CompletedOrders = orders.Count(o => o.Status == OrderStatus.Completed),
                    CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled),
                    TotalRevenue = orders.Where(o => o.TotalAmount > 0).Sum(o => o.TotalAmount),
                    AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0,
                    StatusDistribution = orders.GroupBy(o => o.Status)
                        .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                    BrandDistribution = orders.Where(o => !string.IsNullOrEmpty(o.Brand))
                        .GroupBy(o => o.Brand)
                        .ToDictionary(g => g.Key!, g => g.Count())
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取供应商统计失败: {SupplierId}", supplierId);
                throw;
            }
        }

        public async Task<Dictionary<string, decimal>> GetRevenueTrendsAsync(int days = 30)
        {
            try
            {
                var trends = new Dictionary<string, decimal>();
                var startDate = DateTime.UtcNow.AddDays(-days);

                for (int i = 0; i < days; i++)
                {
                    var date = startDate.AddDays(i);
                    var dailyRevenue = await _repository.GetTotalRevenueAsync(
                        date.Date, date.Date.AddDays(1).AddTicks(-1));

                    trends[date.ToString("yyyy-MM-dd")] = dailyRevenue;
                }

                return trends;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取收入趋势失败");
                throw;
            }
        }

        public async Task<List<OrderResponse>> CreateBulkOrdersAsync(List<CreateOrderRequest> requests)
        {
            try
            {
                var results = new List<OrderResponse>();

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    foreach (var request in requests)
                    {
                        try
                        {
                            var order = await CreateOrderAsync(request);
                            results.Add(MapToResponse(await _repository.GetByIdAsync(
                                await GetOrderIdFromResponse(order))));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "批量创建订单失败: {QuotationId}", request.QuotationId);
                        }
                    }

                    await transaction.CommitAsync();
                    return results;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "批量创建订单事务失败");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量创建订单失败");
                throw;
            }
        }

        public async Task<bool> ExpireOldOrdersAsync(DateTime cutoffDate)
        {
            try
            {
                var expiredOrders = await _repository.GetExpiringOrdersAsync(cutoffDate);
                var successCount = 0;

                foreach (var order in expiredOrders)
                {
                    try
                    {
                        await CancelOrderAsync(order.Id, "订单已过期");
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "过期订单处理失败: {OrderId}", order.Id);
                    }
                }

                _logger.LogInformation("成功处理 {Count} 个过期订单", successCount);
                return successCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理过期订单失败");
                throw;
            }
        }

        public async Task<bool> SendOrderRemindersAsync()
        {
            try
            {
                var expiringOrders = await GetExpiringOrdersAsync();

                foreach (var order in expiringOrders)
                {
                    try
                    {
                        await _daprClient.PublishEventAsync("pubsub", "order-reminder", new
                        {
                            OrderId = order.Id,
                            OrderNumber = order.OrderNumber,
                            SupplierId = order.SupplierId,
                            EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                            ReminderSentAt = DateTime.UtcNow
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "发送订单提醒失败: {OrderId}", order.Id);
                    }
                }

                return expiringOrders.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送订单提醒失败");
                throw;
            }
        }

        public async Task<OrderAttachmentResponse> AddAttachmentAsync(long orderId, AddAttachmentRequest request)
        {
            try
            {
                var order = await _repository.GetByIdAsync(orderId);
                if (order == null)
                    throw new KeyNotFoundException("订单不存在");

                var attachment = new OrderAttachment
                {
                    OrderId = orderId,
                    AttachmentType = request.AttachmentType,
                    FileName = request.FileName,
                    FileUrl = request.FileUrl,
                    Description = request.Description,
                    FileSize = request.FileSize,
                    UploadedAt = DateTime.UtcNow
                };

                var result = await _repository.AddOrderAttachmentAsync(attachment);

                return new OrderAttachmentResponse
                {
                    Id = result.Id,
                    AttachmentType = result.AttachmentType,
                    FileName = result.FileName,
                    FileUrl = result.FileUrl,
                    Description = result.Description,
                    FileSize = result.FileSize,
                    UploadedAt = result.UploadedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加附件失败: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> RemoveAttachmentAsync(long attachmentId)
        {
            try
            {
                return await _repository.RemoveOrderAttachmentAsync(attachmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除附件失败: {AttachmentId}", attachmentId);
                throw;
            }
        }

        public async Task<List<OrderAttachmentResponse>> GetAttachmentsAsync(long orderId)
        {
            try
            {
                var attachments = await _repository.GetOrderAttachmentsAsync(orderId);
                return attachments.Select(a => new OrderAttachmentResponse
                {
                    Id = a.Id,
                    AttachmentType = a.AttachmentType,
                    FileName = a.FileName,
                    FileUrl = a.FileUrl,
                    Description = a.Description,
                    FileSize = a.FileSize,
                    UploadedAt = a.UploadedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取附件列表失败: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<OrderItemResponse> AddOrderItemAsync(long orderId, AddOrderItemRequest request)
        {
            try
            {
                var order = await _repository.GetByIdAsync(orderId);
                if (order == null)
                    throw new KeyNotFoundException("订单不存在");

                var item = new OrderItem
                {
                    OrderId = orderId,
                    BearingNumber = request.BearingNumber,
                    Description = request.Description,
                    Quantity = request.Quantity,
                    UnitPrice = request.UnitPrice,
                    Brand = request.Brand,
                    Material = request.Material,
                    Standard = request.Standard,
                    DisplayOrder = request.DisplayOrder
                };

                var result = await _repository.AddOrderItemAsync(item);

                // 重新计算总金额
                await RecalculateOrderTotalAsync(orderId);

                return new OrderItemResponse
                {
                    Id = result.Id,
                    BearingNumber = result.BearingNumber,
                    Description = result.Description,
                    Quantity = result.Quantity,
                    UnitPrice = result.UnitPrice,
                    TotalPrice = result.TotalPrice,
                    Brand = result.Brand,
                    Material = result.Material,
                    Standard = result.Standard,
                    DisplayOrder = result.DisplayOrder
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加订单项失败: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> RemoveOrderItemAsync(long itemId)
        {
            try
            {
                var success = await _repository.RemoveOrderItemAsync(itemId);
                if (success)
                {
                    var item = await _context.OrderItems.FindAsync(itemId);
                    if (item != null)
                    {
                        await RecalculateOrderTotalAsync(item.OrderId);
                    }
                }
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除订单项失败: {ItemId}", itemId);
                throw;
            }
        }

        public async Task<List<OrderItemResponse>> GetOrderItemsAsync(long orderId)
        {
            try
            {
                var items = await _repository.GetOrderItemsAsync(orderId);
                return items.Select(i => new OrderItemResponse
                {
                    Id = i.Id,
                    BearingNumber = i.BearingNumber,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice,
                    Brand = i.Brand,
                    Material = i.Material,
                    Standard = i.Standard,
                    DisplayOrder = i.DisplayOrder
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单项失败: {OrderId}", orderId);
                throw;
            }
        }

        // ========== 私有辅助方法 ==========

        private async Task<string> GenerateOrderNumberAsync()
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var count = await _context.Orders
                .CountAsync(o => o.OrderNumber.StartsWith($"OD{today}"));

            return $"OD{today}{count + 1:0000}";
        }

        private async Task PopulateOrderFromQuotationAsync(Order order, long quotationId)
        {
            // 通过Dapr调用QuotationApi获取报价详情
            try
            {
                var quotation = await _daprClient.InvokeMethodAsync<QuotationDetailResponse>(
                    HttpMethod.Get,
                    "quotationapi",
                    $"/api/quotations/{quotationId}");

                if (quotation != null)
                {
                    order.DemandId = quotation.DemandId;
                    order.SupplierId = quotation.SupplierId;
                    order.SupplierName = quotation.SupplierName;
                    order.SupplierContact = quotation.SupplierContact;
                    order.BearingNumber = quotation.BearingNumber;
                    order.BearingName = quotation.BearingName;
                    order.Brand = quotation.Brand;
                    order.UnitPrice = quotation.UnitPrice;
                    order.Quantity = quotation.Quantity;
                    order.TotalAmount = quotation.TotalAmount;
                    order.Currency = quotation.Currency;
                    order.DeliveryDays = quotation.DeliveryDays;
                    order.EstimatedDeliveryDate = quotation.EstimatedDeliveryDate;
                    order.DeliveryAddress = quotation.DeliveryAddress;
                    order.Incoterms = quotation.Incoterms;
                    order.QualityStandard = quotation.QualityStandard;
                    order.WarrantyMonths = quotation.WarrantyMonths;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "获取报价详情失败: {QuotationId}", quotationId);
                throw new InvalidOperationException($"无法获取报价单 {quotationId} 的详细信息");
            }
        }

        private async Task RecalculateOrderTotalAsync(long orderId)
        {
            try
            {
                var order = await _repository.GetByIdAsync(orderId);
                if (order == null) return;

                var items = await _repository.GetOrderItemsAsync(orderId);
                order.TotalAmount = items.Sum(i => i.TotalPrice);

                await _repository.UpdateAsync(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重新计算订单总金额失败: {OrderId}", orderId);
                throw;
            }
        }

        private async Task<long> GetOrderIdFromResponse(OrderDetailResponse response)
        {
            try
            {
                var order = await _repository.GetByOrderNumberAsync(response.OrderNumber);
                return order?.Id ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从响应获取订单ID失败");
                return 0;
            }
        }

        // ========== 事件发布方法 ==========

        private async Task PublishOrderCreatedEventAsync(Order order)
        {
            try
            {
                await _daprClient.PublishEventAsync("pubsub", "order-created", new
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    QuotationId = order.QuotationId,
                    DemandId = order.DemandId,
                    SupplierId = order.SupplierId,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status.ToString(),
                    CreatedAt = order.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发布订单创建事件失败");
            }
        }

        private async Task PublishPaymentProcessedEventAsync(Order order)
        {
            try
            {
                await _daprClient.PublishEventAsync("pubsub", "order-payment-processed", new
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    Amount = order.TotalAmount,
                    PaymentStatus = order.PaymentStatus.ToString(),
                    PaidAt = order.PaidAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发布支付处理事件失败");
            }
        }

        private async Task PublishShippingUpdatedEventAsync(Order order, UpdateShippingRequest request)
        {
            try
            {
                await _daprClient.PublishEventAsync("pubsub", "order-shipping-updated", new
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    ShippingStatus = request.ShippingStatus.ToString(),
                    TrackingNumber = request.TrackingNumber,
                    ShippingCompany = request.ShippingCompany,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发布发货更新事件失败");
            }
        }

        private async Task PublishOrderCancelledEventAsync(Order order)
        {
            try
            {
                await _daprClient.PublishEventAsync("pubsub", "order-cancelled", new
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    CancelledAt = order.CancelledAt,
                    Reason = order.Notes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发布订单取消事件失败");
            }
        }

        private async Task PublishOrderCompletedEventAsync(Order order)
        {
            try
            {
                await _daprClient.PublishEventAsync("pubsub", "order-completed", new
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    CompletedAt = order.CompletedAt,
                    TotalAmount = order.TotalAmount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发布订单完成事件失败");
            }
        }

        // ========== 映射方法 ==========

        private OrderResponse MapToResponse(Order order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                QuotationId = order.QuotationId,
                DemandId = order.DemandId,
                SupplierId = order.SupplierId,
                SupplierName = order.SupplierName,
                SupplierContact = order.SupplierContact,
                BearingNumber = order.BearingNumber,
                BearingName = order.BearingName,
                Brand = order.Brand,
                UnitPrice = order.UnitPrice,
                Quantity = order.Quantity,
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                DeliveryDays = order.DeliveryDays,
                EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                DeliveryAddress = order.DeliveryAddress,
                Incoterms = order.Incoterms,
                QualityStandard = order.QualityStandard,
                WarrantyMonths = order.WarrantyMonths,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                ShippingStatus = order.ShippingStatus,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                PaidAt = order.PaidAt,
                ShippedAt = order.ShippedAt,
                CompletedAt = order.CompletedAt,
                CancelledAt = order.CancelledAt
            };
        }

        private OrderDetailResponse MapToDetailResponse(Order order)
        {
            var detailResponse = new OrderDetailResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                QuotationId = order.QuotationId,
                DemandId = order.DemandId,
                SupplierId = order.SupplierId,
                SupplierName = order.SupplierName,
                SupplierContact = order.SupplierContact,
                BearingNumber = order.BearingNumber,
                BearingName = order.BearingName,
                Brand = order.Brand,
                UnitPrice = order.UnitPrice,
                Quantity = order.Quantity,
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                DeliveryDays = order.DeliveryDays,
                EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                DeliveryAddress = order.DeliveryAddress,
                Incoterms = order.Incoterms,
                QualityStandard = order.QualityStandard,
                WarrantyMonths = order.WarrantyMonths,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                ShippingStatus = order.ShippingStatus,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                PaidAt = order.PaidAt,
                ShippedAt = order.ShippedAt,
                CompletedAt = order.CompletedAt,
                CancelledAt = order.CancelledAt
            };

            // 同步加载关联数据
            var items = _repository.GetOrderItemsAsync(order.Id).GetAwaiter().GetResult();
            var attachments = _repository.GetOrderAttachmentsAsync(order.Id).GetAwaiter().GetResult();

            detailResponse.Items = items.Select(i => new OrderItemResponse
            {
                Id = i.Id,
                BearingNumber = i.BearingNumber,
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice,
                Brand = i.Brand,
                Material = i.Material,
                Standard = i.Standard,
                DisplayOrder = i.DisplayOrder
            }).ToList();

            detailResponse.Attachments = attachments.Select(a => new OrderAttachmentResponse
            {
                Id = a.Id,
                AttachmentType = a.AttachmentType,
                FileName = a.FileName,
                FileUrl = a.FileUrl,
                Description = a.Description,
                FileSize = a.FileSize,
                UploadedAt = a.UploadedAt
            }).ToList();

            return detailResponse;
        }
    }

    // ========== DTO 类定义 ==========

    public class QuotationDetailResponse
    {
        public long Id { get; set; }
        public long DemandId { get; set; }
        public long SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierContact { get; set; }
        public string BearingNumber { get; set; } = string.Empty;
        public string? BearingName { get; set; }
        public string? Brand { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "CNY";
        public int DeliveryDays { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? Incoterms { get; set; }
        public string? QualityStandard { get; set; }
        public int WarrantyMonths { get; set; }
    }

    public class AddAttachmentRequest
    {
        public string AttachmentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long FileSize { get; set; }
    }

    public class AddOrderItemRequest
    {
        public string BearingNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Brand { get; set; }
        public string? Material { get; set; }
        public string? Standard { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> SuccessResponse(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message ?? "Success"
            };
        }

        public static ApiResponse<T> ErrorResponse(string message)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message
            };
        }
    }
}
