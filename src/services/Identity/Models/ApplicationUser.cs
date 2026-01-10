using Microsoft.AspNetCore.Identity;

namespace Identity.Models
{
    public class ApplicationUser : IdentityUser<long>
    {
        public string DisplayName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }

        public UserType UserType { get; set; } = UserType.SupplierUser;

        // 供应商特定属性
        public long? SupplierId { get; set; }
        public string? CompanyName { get; set; }
        public string? ContactPerson { get; set; }
        public string? BusinessLicense { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; } = false;
    }

    public class ApplicationRole : IdentityRole<long>
    {
        public string Description { get; set; } = string.Empty;
        public RoleType RoleType { get; set; } = RoleType.System;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum UserType
    {
        SystemAdmin,
        SupplierAdmin,
        SupplierUser,
        BuyerAdmin,
        BuyerUser,
        Guest
    }

    public enum RoleType
    {
        System,
        Supplier,
        Buyer,
        Custom
    }
}
