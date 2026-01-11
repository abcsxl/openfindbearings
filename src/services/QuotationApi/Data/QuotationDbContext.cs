using Microsoft.EntityFrameworkCore;
using QuotationApi.Models.Entities;

namespace QuotationApi.Data
{
    public class QuotationDbContext : DbContext
    {
        public QuotationDbContext(DbContextOptions<QuotationDbContext> options) : base(options) { }

        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<QuotationItem> QuotationItems { get; set; }
        public DbSet<QuotationAttachment> QuotationAttachments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置 Quotation 实体
            modelBuilder.Entity<Quotation>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.QuotationNumber)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.BearingNumber)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.SupplierName)
                    .HasMaxLength(200);

                entity.Property(e => e.UnitPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalAmount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Currency)
                    .HasMaxLength(3)
                    .HasDefaultValue("CNY");

                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.Property(e => e.Type)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                // 索引
                entity.HasIndex(e => e.QuotationNumber).IsUnique();
                entity.HasIndex(e => e.DemandId);
                entity.HasIndex(e => e.SupplierId);
                entity.HasIndex(e => e.BearingNumber);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.ExpiresAt);
                entity.HasIndex(e => new { e.DemandId, e.SupplierId });

                // 关系
                entity.HasMany(q => q.Items)
                    .WithOne(i => i.Quotation)
                    .HasForeignKey(i => i.QuotationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(q => q.Attachments)
                    .WithOne(a => a.Quotation)
                    .HasForeignKey(a => a.QuotationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 配置 QuotationItem 实体
            modelBuilder.Entity<QuotationItem>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.BearingNumber)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(200);

                entity.Property(e => e.UnitPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Brand)
                    .HasMaxLength(100);

                entity.HasIndex(e => e.QuotationId);
                entity.HasIndex(e => e.BearingNumber);
            });

            // 配置 QuotationAttachment 实体
            modelBuilder.Entity<QuotationAttachment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.AttachmentType)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.FileName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(e => e.FileUrl)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(200);

                entity.HasIndex(e => e.QuotationId);
                entity.HasIndex(e => e.AttachmentType);
            });
        }
    }
}
