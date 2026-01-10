using Microsoft.EntityFrameworkCore;
using Supplier.Models;

namespace Supplier.Data
{
    public class SupplierDbContext : DbContext
    {
        public SupplierDbContext(DbContextOptions<SupplierDbContext> options) : base(options) { }

        public DbSet<Models.Supplier> Suppliers { get; set; }
        public DbSet<SupplierProduct> SupplierProducts { get; set; }
        public DbSet<SupplierCertificate> SupplierCertificates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置Supplier实体
            modelBuilder.Entity<Models.Supplier>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CompanyName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.ContactPerson).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.BusinessLicense).HasMaxLength(50);
                entity.Property(e => e.TaxId).HasMaxLength(30);

                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.CompanyName);
                entity.HasIndex(e => e.Status);
            });

            // 配置SupplierProduct实体
            modelBuilder.Entity<SupplierProduct>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BearingNumber).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Specification).HasMaxLength(500);
                entity.Property(e => e.Material).HasMaxLength(100);
                entity.Property(e => e.Brand).HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");

                entity.HasIndex(e => e.SupplierId);
                entity.HasIndex(e => e.BearingNumber);
                entity.HasIndex(e => new { e.SupplierId, e.BearingNumber }).IsUnique();
            });

            // 配置SupplierCertificate实体
            modelBuilder.Entity<SupplierCertificate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CertificateType).HasMaxLength(100).IsRequired();
                entity.Property(e => e.CertificateNumber).HasMaxLength(50).IsRequired();
                entity.Property(e => e.IssuingAuthority).HasMaxLength(200);

                entity.HasIndex(e => e.SupplierId);
                entity.HasIndex(e => e.CertificateNumber).IsUnique();
            });
        }
    }
}
