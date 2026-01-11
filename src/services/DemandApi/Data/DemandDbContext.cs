using Demand.Models;
using Microsoft.EntityFrameworkCore;

namespace Demand.Data
{
    public class DemandDbContext : DbContext
    {
        public DemandDbContext(DbContextOptions<DemandDbContext> options) : base(options) { }

        public DbSet<Models.Demand> Demands { get; set; }
        public DbSet<DemandMatch> DemandMatches { get; set; }
        public DbSet<DemandView> DemandViews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置Demand实体
            modelBuilder.Entity<Models.Demand>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.BearingNumber).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Specification).HasMaxLength(500);
                entity.Property(e => e.Material).HasMaxLength(100);
                entity.Property(e => e.Brand).HasMaxLength(100);
                entity.Property(e => e.Standard).HasMaxLength(50);
                entity.Property(e => e.DeliveryAddress).HasMaxLength(500);
                entity.Property(e => e.AdditionalRequirements).HasMaxLength(2000);
                entity.Property(e => e.RequesterType).HasMaxLength(20).IsRequired();
                entity.Property(e => e.RequesterCompany).HasMaxLength(200);

                entity.Property(e => e.MaxPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MinPrice).HasColumnType("decimal(18,2)");

                entity.HasIndex(e => e.BearingNumber);
                entity.HasIndex(e => e.Brand);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.RequesterId);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.ExpiresAt);

                // 关系配置
                entity.HasMany(d => d.Matches)
                    .WithOne(m => m.Demand)
                    .HasForeignKey(m => m.DemandId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(d => d.Views)
                    .WithOne(v => v.Demand)
                    .HasForeignKey(v => v.DemandId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 配置DemandMatch实体
            modelBuilder.Entity<DemandMatch>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.SupplierName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.MatchDetails).HasMaxLength(2000);

                entity.HasIndex(e => e.DemandId);
                entity.HasIndex(e => e.SupplierId);
                entity.HasIndex(e => e.MatchScore);
                entity.HasIndex(e => e.IsNotified);
                entity.HasIndex(e => e.HasResponded);

                // 唯一约束：同一个需求不能重复匹配同一个供应商
                entity.HasIndex(e => new { e.DemandId, e.SupplierId }).IsUnique();
            });

            // 配置DemandView实体
            modelBuilder.Entity<DemandView>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ViewerType).HasMaxLength(20).IsRequired();
                entity.Property(e => e.ViewerCompany).HasMaxLength(200);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.IpAddress).HasMaxLength(45);

                entity.HasIndex(e => e.DemandId);
                entity.HasIndex(e => e.ViewerId);
                entity.HasIndex(e => e.ViewedAt);
            });
        }
    }
}
