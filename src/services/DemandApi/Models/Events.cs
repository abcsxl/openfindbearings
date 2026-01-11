namespace Demand.Models
{
    public class DemandCreatedEvent
    {
        public long DemandId { get; set; }
        public long RequesterId { get; set; }
        public string BearingNumber { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Specification { get; set; }
        public int RequiredQuantity { get; set; }
        public string? DeliveryAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DemandMatchedEvent
    {
        public long DemandId { get; set; }
        public int TotalMatches { get; set; }
        public List<long> MatchedSupplierIds { get; set; } = new();
        public DateTime MatchedAt { get; set; }
    }

    public class SupplierNotificationEvent
    {
        public long DemandId { get; set; }
        public long SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierEmail { get; set; } = string.Empty;
        public string BearingNumber { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public int RequiredQuantity { get; set; }
        public double MatchScore { get; set; }
        public MatchReason MatchReason { get; set; }
        public DateTime NotifiedAt { get; set; }
    }

    public class DemandStatusChangedEvent
    {
        public long DemandId { get; set; }
        public DemandStatus OldStatus { get; set; }
        public DemandStatus NewStatus { get; set; }
        public long ChangedByUserId { get; set; }
        public string? Reason { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
