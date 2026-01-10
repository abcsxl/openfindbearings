using Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Data
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, long>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 配置用户表
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.DisplayName).HasMaxLength(100);
                entity.Property(u => u.CompanyName).HasMaxLength(200);
                entity.Property(u => u.ContactPerson).HasMaxLength(100);
                entity.Property(u => u.BusinessLicense).HasMaxLength(50);

                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.SupplierId);
                entity.HasIndex(u => u.UserType);
            });

            // 配置角色表
            builder.Entity<ApplicationRole>(entity =>
            {
                entity.Property(r => r.Description).HasMaxLength(500);
            });
        }
    }
}
