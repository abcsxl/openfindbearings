using FindBearingsApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FindBearingsApi.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; } = default!;
        public DbSet<Interest> Interests { get; set; } = default!;

        public DbSet<Notification> Notifications { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === User ===
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OpenId).IsUnique();
                entity.Property(e => e.OpenId).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Nickname).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Avatar).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            // === Message ===
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.BearingModel);

                entity.HasOne(m => m.User)
                      .WithMany(u => u.Messages) // ← 关键：指定反向导航
                      .HasForeignKey(m => m.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // 用户删除时级联删除消息
            });

            // === Interest ===
            modelBuilder.Entity<Interest>(entity =>
            {
                entity.HasKey(i => new { i.UserId, i.MessageId }); // 复合主键

                entity.HasOne(i => i.User)
                      .WithMany(u => u.Interests) // ← 关键：指定反向导航
                      .HasForeignKey(i => i.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.Message)
                      .WithMany(m => m.Interests) // ← 关键：指定反向导航
                      .HasForeignKey(i => i.MessageId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // === Notification ===
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.HasIndex(n => n.UserId);
                entity.HasIndex(n => n.IsRead);

                entity.HasOne(n => n.User)
                      .WithMany(u => u.Notifications) // ← 关键：指定反向导航
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(n => n.Message)
                      .WithMany() // ← 这里可以保持空，因为 Message 不需要 Notifications 导航
                      .HasForeignKey(n => n.MessageId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // === 表名小写 ===
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                entityType.SetTableName(entityType.GetTableName()?.ToLowerInvariant());
            }
        }

        public IQueryable<Notification> GetNotificationsForUser(long userId)
        {
            return Notifications
                .Include(n => n.Message) // 加载关联消息
                .Where(n => n.UserId == userId && !n.Message!.IsDeleted); // 过滤已删除消息
        }
    }
}