using FindBearingsApi.Application.Common;
using FindBearingsApi.Application.DTOs.Notification;
using FindBearingsApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FindBearingsApi.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<NotificationResponseDto>> GetNotificationsAsync(int page, int pageSize, long currentUserId)
        {
            const int maxPageSize = 50;
            pageSize = Math.Min(pageSize, maxPageSize);
            var skip = (page - 1) * pageSize;

            var query = _context.GetNotificationsForUser(currentUserId)
                .OrderByDescending(n => n.CreatedAt);

            var total = await query.CountAsync();
            var notifications = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(n => new NotificationResponseDto(
                    n.Id,
                    n.Content,
                    n.IsRead,
                    n.CreatedAt,
                    new MessageSummaryDto(
                        n.Message!.Id,
                        n.Message.Type,
                        n.Message.BearingModel,
                        n.Message.Quantity
                    )
                ))
                .ToListAsync();

            return new PagedResponse<NotificationResponseDto>
            {
                Items = notifications,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public async Task<bool> MarkAsReadAsync(long id, long currentUserId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == currentUserId);

            if (notification == null) return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> MarkAllAsReadAsync(long currentUserId)
        {
            var count = await _context.Notifications
                .Where(n => n.UserId == currentUserId && !n.IsRead)
                .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));
            return count;
        }
    }
}
