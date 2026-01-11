using BearingApi.Models.Entities;

namespace BearingApi.Models.DTOs
{
    public class BearingCreatedEvent
    {
        public long BearingId { get; set; }
        public string BearingNumber { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public BearingType Type { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class BearingUpdatedEvent
    {
        public long BearingId { get; set; }
        public string BearingNumber { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public BearingType Type { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class BearingVerifiedEvent
    {
        public long BearingId { get; set; }
        public string BearingNumber { get; set; } = string.Empty;
        public VerificationLevel Level { get; set; }
        public DateTime VerifiedAt { get; set; }
        public string? VerifiedBy { get; set; }
    }

    public class BearingViewedEvent
    {
        public long BearingId { get; set; }
        public string BearingNumber { get; set; } = string.Empty;
        public long? ViewerId { get; set; }
        public string ViewerType { get; set; } = string.Empty;
        public DateTime ViewedAt { get; set; }
    }

    public class BearingSearchedEvent
    {
        public long? BearingId { get; set; }
        public string? BearingNumber { get; set; }
        public string SearchKeywords { get; set; } = string.Empty;
        public int ResultCount { get; set; }
        public DateTime SearchedAt { get; set; }
    }
}
