namespace SupplierApi.Models
{
    // 事件类
    public class SupplierCreatedEvent
    {
        public long SupplierId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class SupplierApprovedEvent
    {
        public long SupplierId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public DateTime ApprovedAt { get; set; }
    }

    public class SupplierSuspendedEvent
    {
        public long SupplierId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public DateTime SuspendedAt { get; set; }
    }

    public class ProductAddedEvent
    {
        public long SupplierId { get; set; }
        public long ProductId { get; set; }
        public string BearingNumber { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
