using QuotationApi.Models.Entities;

namespace QuotationApi.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(QuotationDbContext context)
        {
            // 检查是否已有数据
            if (context.Quotations.Any())
            {
                return; // 数据库已经 seeded
            }

            var quotations = new List<Quotation>
        {
            new Quotation
            {
                QuotationNumber = "QT202401010001",
                DemandId = 1,
                SupplierId = 1,
                SupplierName = "优质轴承供应商",
                SupplierContact = "张经理",
                SupplierPhone = "13800138000",
                SupplierEmail = "zhang@example.com",
                BearingNumber = "6201-2RS",
                BearingName = "深沟球轴承 6201-2RS",
                Brand = "SKF",
                UnitPrice = 25.50m,
                Quantity = 100,
                TotalAmount = 2550.00m,
                Currency = "CNY",
                DeliveryDays = 7,
                EstimatedDeliveryDate = DateTime.UtcNow.AddDays(7),
                DeliveryAddress = "上海市浦东新区张江高科技园区",
                Incoterms = "FOB",
                QualityStandard = "ISO9001",
                CertificateRequirements = "原厂质保书",
                WarrantyMonths = 12,
                Status = QuotationStatus.Submitted,
                Type = QuotationType.Standard,
                Notes = "量大优惠，可提供样品",
                IsRecommended = true,
                MatchScore = 0.85m,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                ExpiresAt = DateTime.UtcNow.AddDays(28),
                SubmittedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Quotation
            {
                QuotationNumber = "QT202401010002",
                DemandId = 1,
                SupplierId = 2,
                SupplierName = "快速轴承贸易",
                SupplierContact = "李经理",
                BearingNumber = "6201-2RS",
                BearingName = "深沟球轴承 6201-2RS",
                Brand = "NSK",
                UnitPrice = 23.80m,
                Quantity = 100,
                TotalAmount = 2380.00m,
                Currency = "CNY",
                DeliveryDays = 5,
                EstimatedDeliveryDate = DateTime.UtcNow.AddDays(5),
                DeliveryAddress = "北京市朝阳区CBD",
                Incoterms = "CIF",
                QualityStandard = "ISO9001",
                WarrantyMonths = 12,
                Status = QuotationStatus.Submitted,
                Type = QuotationType.Urgent,
                IsRecommended = false,
                MatchScore = 0.72m,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                ExpiresAt = DateTime.UtcNow.AddDays(29),
                SubmittedAt = DateTime.UtcNow
            }
        };

            await context.Quotations.AddRangeAsync(quotations);
            await context.SaveChangesAsync();
        }
    }
}
