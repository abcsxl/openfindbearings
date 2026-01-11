namespace SupplierApi.Models.DTOs
{
    // 查询请求
    public class SupplierQueryRequest
    {
        public string? CompanyName { get; set; }
        public string? Email { get; set; }
        public SupplierStatus? Status { get; set; }
        public SupplierType? Type { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public SupplierQuery ToQuery() => new()
        {
            CompanyName = CompanyName,
            Email = Email,
            Status = Status,
            Type = Type,
            City = City,
            Country = Country,
            SortBy = SortBy,
            SortDescending = SortDescending,
            PageNumber = PageNumber,
            PageSize = PageSize
        };
    }

    public class SupplierQuery
    {
        public string? CompanyName { get; set; }
        public string? Email { get; set; }
        public SupplierStatus? Status { get; set; }
        public SupplierType? Type { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // 创建供应商请求
    public class CreateSupplierRequest
    {
        public string CompanyName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? BusinessLicense { get; set; }
        public string? TaxId { get; set; }
        public SupplierType SupplierType { get; set; } = SupplierType.General;
    }

    // 更新供应商请求
    public class UpdateSupplierRequest
    {
        public string? CompanyName { get; set; }
        public string? ContactPerson { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? BusinessLicense { get; set; }
        public string? TaxId { get; set; }
    }

    // 添加产品请求
    public class AddProductRequest
    {
        public string BearingNumber { get; set; } = string.Empty;
        public string? Specification { get; set; }
        public string? Material { get; set; }
        public string? Brand { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int MinimumOrder { get; set; } = 1;
    }

    // 添加证书请求
    public class AddCertificateRequest
    {
        public string CertificateType { get; set; } = string.Empty;
        public string CertificateNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? IssuingAuthority { get; set; }
        public string? FileUrl { get; set; }
    }
}
