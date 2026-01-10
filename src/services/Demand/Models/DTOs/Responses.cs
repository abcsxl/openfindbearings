namespace Demand.Models.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }

        public static ApiResponse<T> SuccessResponse(T data) => new()
        {
            Success = true,
            Data = data
        };

        public static ApiResponse<T> ErrorResponse(string message) => new()
        {
            Success = false,
            Message = message
        };
    }

    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    }

    public class DemandResponse
    {
        public long Id { get; set; }
        public string BearingNumber { get; set; } = string.Empty;
        public string? Specification { get; set; }
        public string? Brand { get; set; }
        public int RequiredQuantity { get; set; }
        public decimal? MaxPrice { get; set; }
        public DemandStatus Status { get; set; }
        public DemandPriority Priority { get; set; }
        public int TotalMatches { get; set; }
        public int TotalQuotations { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class DemandDetailResponse : DemandResponse
    {
        public long RequesterId { get; set; }
        public string RequesterType { get; set; } = string.Empty;
        public string? RequesterCompany { get; set; }
        public string? Material { get; set; }
        public string? Standard { get; set; }
        public decimal? MinPrice { get; set; }
        public string? DeliveryAddress { get; set; }
        public DateTime? RequiredByDate { get; set; }
        public string? AdditionalRequirements { get; set; }
        public DemandType Type { get; set; }
        public int TotalViews { get; set; }
        public long? SelectedSupplierId { get; set; }
        public long? SelectedQuotationId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public List<DemandMatchResponse> Matches { get; set; } = new();
        public List<DemandViewResponse> RecentViews { get; set; } = new();
    }

    public class DemandMatchResponse
    {
        public long Id { get; set; }
        public long SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public double MatchScore { get; set; }
        public MatchReason MatchReason { get; set; }
        public string? MatchDetails { get; set; }
        public bool IsNotified { get; set; }
        public bool HasResponded { get; set; }
        public bool IsInterested { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DemandViewResponse
    {
        public long? ViewerId { get; set; }
        public string ViewerType { get; set; } = string.Empty;
        public string? ViewerCompany { get; set; }
        public DateTime ViewedAt { get; set; }
    }

    public class MatchResultResponse
    {
        public long DemandId { get; set; }
        public int TotalSuppliersMatched { get; set; }
        public int TotalSuppliersNotified { get; set; }
        public List<SupplierMatch> Matches { get; set; } = new();
        public TimeSpan MatchingDuration { get; set; }
        public DateTime MatchedAt { get; set; }
    }

    public class SupplierMatch
    {
        public long SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public double MatchScore { get; set; }
        public MatchReason PrimaryReason { get; set; }
        public List<string> Strengths { get; set; } = new();
        public bool IsNotified { get; set; }
    }
}
