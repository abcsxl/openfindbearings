using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderApi.Models.Entities
{
    public class Order
    {
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public long QuotationId { get; set; }

        [Required]
        public long DemandId { get; set; }

        [Required]
        public long SupplierId { get; set; }

        [StringLength(200)]
        public string? SupplierName { get; set; }

        [StringLength(100)]
        public string? SupplierContact { get; set; }

        [Required]
        [StringLength(100)]
        public string BearingNumber { get; set; } = string.Empty;

        [StringLength(200)]
        public string? BearingName { get; set; }

        [StringLength(100)]
        public string? Brand { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(3)]
        public string Currency { get; set; } = "CNY";

        public int DeliveryDays { get; set; } = 7;
        public DateTime? EstimatedDeliveryDate { get; set; }

        [StringLength(500)]
        public string? DeliveryAddress { get; set; }

        [StringLength(50)]
        public string? Incoterms { get; set; }

        [StringLength(100)]
        public string? QualityStandard { get; set; }

        public int WarrantyMonths { get; set; } = 12;

        public OrderStatus Status { get; set; } = OrderStatus.Created;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public ShippingStatus ShippingStatus { get; set; } = ShippingStatus.NotShipped;

        [StringLength(1000)]
        public string? Notes { get; set; }

        // 时间戳
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        // 导航属性
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public virtual ICollection<OrderAttachment> Attachments { get; set; } = new List<OrderAttachment>();
    }

    public class OrderItem
    {
        public long Id { get; set; }
        public long OrderId { get; set; }

        [Required]
        [StringLength(100)]
        public string BearingNumber { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice => UnitPrice * Quantity;

        [StringLength(100)]
        public string? Brand { get; set; }

        [StringLength(100)]
        public string? Material { get; set; }

        [StringLength(50)]
        public string? Standard { get; set; }

        public int DisplayOrder { get; set; }

        public virtual Order Order { get; set; } = null!;
    }

    public class OrderAttachment
    {
        public long Id { get; set; }
        public long OrderId { get; set; }

        [Required]
        [StringLength(50)]
        public string AttachmentType { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FileUrl { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public virtual Order Order { get; set; } = null!;
    }
}
