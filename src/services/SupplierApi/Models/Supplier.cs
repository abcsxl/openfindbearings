namespace SupplierApi.Models
{
    public class Supplier
    {
        public long Id { get; set; }
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

        // 供应商状态
        public SupplierStatus Status { get; set; } = SupplierStatus.Pending;
        public SupplierType Type { get; set; } = SupplierType.General;

        // 评分和信誉
        public double Rating { get; set; } = 0.0;
        public int TotalTransactions { get; set; } = 0;
        public int SuccessfulTransactions { get; set; } = 0;

        // 时间戳
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }

        // 导航属性
        public virtual ICollection<SupplierProduct> Products { get; set; } = new List<SupplierProduct>();
        public virtual ICollection<SupplierCertificate> Certificates { get; set; } = new List<SupplierCertificate>();
    }

    public class SupplierProduct
    {
        public long Id { get; set; }
        public long SupplierId { get; set; }
        public string BearingNumber { get; set; } = string.Empty;
        public string? Specification { get; set; }
        public string? Material { get; set; }
        public string? Brand { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int MinimumOrder { get; set; } = 1;
        public bool IsActive { get; set; } = true;

        public virtual Supplier Supplier { get; set; } = null!;
    }

    public class SupplierCertificate
    {
        public long Id { get; set; }
        public long SupplierId { get; set; }
        public string CertificateType { get; set; } = string.Empty;
        public string CertificateNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? IssuingAuthority { get; set; }
        public string? FileUrl { get; set; }

        public virtual Supplier Supplier { get; set; } = null!;
    }

    public enum SupplierStatus
    {
        Pending,    // 待审核
        Approved,    // 已批准
        Rejected,    // 已拒绝
        Suspended,   // 已暂停
        Inactive     // 未激活
    }

    public enum SupplierType
    {
        Manufacturer,  // 制造商
        Distributor,   // 分销商
        General        // 一般供应商
    }
}
