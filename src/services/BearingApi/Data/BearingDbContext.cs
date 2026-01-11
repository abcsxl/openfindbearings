using BearingApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BearingApi.Data
{
    public class BearingDbContext : DbContext
    {
        public BearingDbContext(DbContextOptions<BearingDbContext> options)
            : base(options)
        {
        }

        public DbSet<Bearing> Bearings { get; set; }
        public DbSet<BearingSpecification> BearingSpecifications { get; set; }
        public DbSet<BearingImage> BearingImages { get; set; }
        public DbSet<BearingDocument> BearingDocuments { get; set; }

        // 以下是缺少的 DbSet
        public DbSet<BearingMatch> BearingMatches { get; set; }  // 新增
        public DbSet<BearingView> BearingViews { get; set; }  // 新增

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置索引
            modelBuilder.Entity<Bearing>()
                .HasIndex(b => b.BearingNumber)
                .IsUnique();

            modelBuilder.Entity<Bearing>()
                .HasIndex(b => b.Status);

            modelBuilder.Entity<Bearing>()
                .HasIndex(b => b.Type);

            modelBuilder.Entity<Bearing>()
                .HasIndex(b => b.Category);

            modelBuilder.Entity<Bearing>()
                .HasIndex(b => b.Brand);

            // 配置关系
            modelBuilder.Entity<Bearing>()
                .HasMany(b => b.Specifications)
                .WithOne()
                .HasForeignKey(s => s.BearingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Bearing>()
                .HasMany(b => b.Images)
                .WithOne()
                .HasForeignKey(i => i.BearingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Bearing>()
                .HasMany(b => b.Documents)
                .WithOne()
                .HasForeignKey(d => d.BearingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
