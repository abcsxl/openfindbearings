using Microsoft.EntityFrameworkCore;
using NotificationApi.Models.Entities;

namespace NotificationApi.Data
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context;
        private readonly ILogger<NotificationRepository> _logger;

        public NotificationRepository(NotificationDbContext context, ILogger<NotificationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Notification> AddNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<Notification?> GetNotificationByIdAsync(long id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public async Task<Notification?> GetNotificationByNumberAsync(string notificationNumber)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationNumber == notificationNumber);
        }

        public async Task<List<Notification>> GetNotificationsAsync(string? recipient = null, NotificationType? type = null,
            NotificationStatus? status = null, DateTime? startDate = null, DateTime? endDate = null, int skip = 0, int take = 20)
        {
            var query = _context.Notifications.AsQueryable();

            if (!string.IsNullOrEmpty(recipient))
                query = query.Where(n => n.Recipient.Contains(recipient));
            if (type.HasValue)
                query = query.Where(n => n.Type == type.Value);
            if (status.HasValue)
                query = query.Where(n => n.Status == status.Value);
            if (startDate.HasValue)
                query = query.Where(n => n.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(n => n.CreatedAt <= endDate.Value);

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetNotificationsCountAsync(string? recipient = null, NotificationType? type = null,
            NotificationStatus? status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Notifications.AsQueryable();

            if (!string.IsNullOrEmpty(recipient))
                query = query.Where(n => n.Recipient.Contains(recipient));
            if (type.HasValue)
                query = query.Where(n => n.Type == type.Value);
            if (status.HasValue)
                query = query.Where(n => n.Status == status.Value);
            if (startDate.HasValue)
                query = query.Where(n => n.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(n => n.CreatedAt <= endDate.Value);

            return await query.CountAsync();
        }

        public async Task<Notification> UpdateNotificationAsync(Notification notification)
        {
            notification.UpdatedAt = DateTime.UtcNow;
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<bool> DeleteNotificationAsync(long id)
        {
            var notification = await GetNotificationByIdAsync(id);
            if (notification == null) return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Notification>> GetPendingNotificationsAsync(int limit = 100)
        {
            return await _context.Notifications
                .Where(n => n.Status == NotificationStatus.Pending &&
                           (!n.ScheduledAt.HasValue || n.ScheduledAt <= DateTime.UtcNow))
                .OrderBy(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetFailedNotificationsAsync(int maxRetryCount = 3)
        {
            return await _context.Notifications
                .Where(n => n.Status == NotificationStatus.Failed && n.RetryCount < maxRetryCount)
                .OrderBy(n => n.CreatedAt)
                .Take(100)
                .ToListAsync();
        }

        public async Task<bool> UpdateNotificationStatusAsync(long notificationId, NotificationStatus status, string? errorMessage = null)
        {
            var notification = await GetNotificationByIdAsync(notificationId);
            if (notification == null) return false;

            notification.Status = status;
            notification.ErrorMessage = errorMessage;
            notification.UpdatedAt = DateTime.UtcNow;

            if (status == NotificationStatus.Sent)
                notification.SentAt = DateTime.UtcNow;
            else if (status == NotificationStatus.Read)
                notification.ReadAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // Template 方法
        public async Task<NotificationTemplate> AddTemplateAsync(NotificationTemplate template)
        {
            _context.NotificationTemplates.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<NotificationTemplate?> GetTemplateByIdAsync(long id)
        {
            return await _context.NotificationTemplates.FindAsync(id);
        }

        public async Task<NotificationTemplate?> GetTemplateByCodeAsync(string code)
        {
            return await _context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.Code == code);
        }

        public async Task<List<NotificationTemplate>> GetTemplatesAsync(bool? isActive = null)
        {
            var query = _context.NotificationTemplates.AsQueryable();
            if (isActive.HasValue)
                query = query.Where(t => t.IsActive == isActive.Value);

            return await query.OrderBy(t => t.Name).ToListAsync();
        }

        public async Task<NotificationTemplate> UpdateTemplateAsync(NotificationTemplate template)
        {
            template.UpdatedAt = DateTime.UtcNow;
            template.Version++;
            _context.NotificationTemplates.Update(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<bool> DeleteTemplateAsync(long id)
        {
            var template = await GetTemplateByIdAsync(id);
            if (template == null) return false;

            _context.NotificationTemplates.Remove(template);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
