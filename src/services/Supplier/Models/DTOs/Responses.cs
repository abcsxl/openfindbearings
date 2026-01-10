namespace Supplier.Models.DTOs
{
    // 基础响应
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

    // 分页响应
    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    }

    // 供应商响应
    public class SupplierResponse
    {
        public long Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public SupplierStatus Status { get; set; }
        public SupplierType Type { get; set; }
        public double Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }

    public class SupplierDetailResponse : SupplierResponse
    {
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? BusinessLicense { get; set; }
        public string? TaxId { get; set; }
        public int TotalTransactions { get; set; }
        public int SuccessfulTransactions { get; set; }
        public List<ProductResponse> Products { get; set; } = new();
        public List<CertificateResponse> Certificates { get; set; } = new();
    }

    // 产品响应
    public class ProductResponse
    {
        public long Id { get; set; }
        public string BearingNumber { get; set; } = string.Empty;
        public string? Specification { get; set; }
        public string? Material { get; set; }
        public string? Brand { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int MinimumOrder { get; set; }
        public bool IsActive { get; set; }
    }

    // 证书响应
    public class CertificateResponse
    {
        public long Id { get; set; }
        public string CertificateType { get; set; } = string.Empty;
        public string CertificateNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? IssuingAuthority { get; set; }
        public string? FileUrl { get; set; }
    }
}
