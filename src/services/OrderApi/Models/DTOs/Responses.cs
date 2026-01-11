using OrderApi.Models.Entities;

namespace OrderApi.Models.DTOs
{
    public class OrderResponse
    {
        public long Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public long QuotationId { get; set; }
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
        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public ShippingStatus ShippingStatus { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }

    public class OrderDetailResponse : OrderResponse
    {
        public List<OrderItemResponse> Items { get; set; } = new List<OrderItemResponse>();
        public List<OrderAttachmentResponse> Attachments { get; set; } = new List<OrderAttachmentResponse>();
    }

    public class OrderItemResponse
    {
        public long Id { get; set; }
        public string BearingNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Brand { get; set; }
        public string? Material { get; set; }
        public string? Standard { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class OrderAttachmentResponse
    {
        public long Id { get; set; }
        public string AttachmentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }

    public class OrderStatisticsResponse
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int PaidOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public Dictionary<string, int> StatusDistribution { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> BrandDistribution { get; set; } = new Dictionary<string, int>();
    }
}
