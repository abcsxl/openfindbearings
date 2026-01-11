using Microsoft.EntityFrameworkCore;
using OrderApi.Models.Entities;

namespace OrderApi.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(OrderDbContext context)
        {
            // 确保数据库已创建
            await context.Database.EnsureCreatedAsync();

            // 检查是否已有数据
            if (await context.Orders.AnyAsync())
            {
                return; // 数据库已有数据
            }

            var orders = new List<Order>
            {
                new Order
                {
                    OrderNumber = "OD202412010001",
                    QuotationId = 1001,
                    DemandId = 2001,
                    SupplierId = 3001,
                    SupplierName = "上海轴承有限公司",
                    SupplierContact = "张经理 13800138001",
                    BearingNumber = "6204-2RS",
                    BearingName = "深沟球轴承 6204-2RS",
                    Brand = "SKF",
                    UnitPrice = 25.50m,
                    Quantity = 100,
                    TotalAmount = 2550.00m,
                    Currency = "CNY",
                    DeliveryDays = 7,
                    EstimatedDeliveryDate = DateTime.UtcNow.AddDays(5),
                    DeliveryAddress = "上海市浦东新区张江高科技园区",
                    Incoterms = "FOB Shanghai",
                    QualityStandard = "ISO 9001",
                    WarrantyMonths = 12,
                    Status = OrderStatus.Paid,
                    PaymentStatus = PaymentStatus.Paid,
                    ShippingStatus = ShippingStatus.Shipped,
                    Notes = "急需货物，请尽快安排发货",
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2),
                    PaidAt = DateTime.UtcNow.AddDays(-8),
                    ShippedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Order
                {
                    OrderNumber = "OD202412010002",
                    QuotationId = 1002,
                    DemandId = 2002,
                    SupplierId = 3002,
                    SupplierName = "北京精密轴承厂",
                    SupplierContact = "李工 13900139002",
                    BearingNumber = "6305-ZZ",
                    BearingName = "深沟球轴承 6305-ZZ",
                    Brand = "NSK",
                    UnitPrice = 18.75m,
                    Quantity = 200,
                    TotalAmount = 3750.00m,
                    Currency = "CNY",
                    DeliveryDays = 10,
                    EstimatedDeliveryDate = DateTime.UtcNow.AddDays(8),
                    DeliveryAddress = "北京市海淀区中关村科技园",
                    Incoterms = "EXW Beijing",
                    QualityStandard = "ISO/TS 16949",
                    WarrantyMonths = 18,
                    Status = OrderStatus.Created,
                    PaymentStatus = PaymentStatus.Pending,
                    ShippingStatus = ShippingStatus.NotShipped,
                    Notes = "需要提供原厂质保书",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3)
                }
            };

            await context.Orders.AddRangeAsync(orders);
            await context.SaveChangesAsync();
        }
    }
}
