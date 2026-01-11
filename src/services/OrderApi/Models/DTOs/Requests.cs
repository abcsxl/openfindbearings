using OrderApi.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderApi.Models.DTOs
{
    public class CreateOrderRequest
    {
        [Required]
        public long QuotationId { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class UpdateOrderRequest
    {
        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime? EstimatedDeliveryDate { get; set; }

        [StringLength(500)]
        public string? DeliveryAddress { get; set; }
    }

    public class OrderQuery
    {
        public long? QuotationId { get; set; }
        public long? DemandId { get; set; }
        public long? SupplierId { get; set; }
        public string? OrderNumber { get; set; }
        public string? BearingNumber { get; set; }
        public string? Brand { get; set; }
        public OrderStatus? Status { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public ShippingStatus? ShippingStatus { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class UpdateOrderStatusRequest
    {
        [Required]
        public OrderStatus Status { get; set; }

        [StringLength(1000)]
        public string? Reason { get; set; }
    }

    public class ProcessPaymentRequest
    {
        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(1000)]
        public string? PaymentNotes { get; set; }
    }

    public class UpdateShippingRequest
    {
        [Required]
        public ShippingStatus ShippingStatus { get; set; }

        [StringLength(100)]
        public string? TrackingNumber { get; set; }

        [StringLength(500)]
        public string? ShippingCompany { get; set; }

        [StringLength(1000)]
        public string? ShippingNotes { get; set; }
    }
}
