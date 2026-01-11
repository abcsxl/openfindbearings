namespace QuotationApi.Models.Events
{
    public class QuotationCreatedEvent
    {
        public long QuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public long DemandId { get; set; }
        public long SupplierId { get; set; }
        public string BearingNumber { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class QuotationSubmittedEvent
    {
        public long QuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public long DemandId { get; set; }
        public long SupplierId { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    public class QuotationAcceptedEvent
    {
        public long QuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public long DemandId { get; set; }
        public long SupplierId { get; set; }
        public long CustomerId { get; set; }
        public DateTime AcceptedAt { get; set; }
    }

    public class QuotationExpiredEvent
    {
        public long QuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public DateTime ExpiredAt { get; set; }
    }
}
