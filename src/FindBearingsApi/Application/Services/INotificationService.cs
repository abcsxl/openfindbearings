using FindBearingsApi.Application.Common;
using FindBearingsApi.Application.DTOs.Notification;

namespace FindBearingsApi.Application.Services
{
    public interface INotificationService
    {
        Task<PagedResponse<NotificationResponseDto>> GetNotificationsAsync(int page, int pageSize, long currentUserId);
        Task<bool> MarkAsReadAsync(long id, long currentUserId);
        Task<int> MarkAllAsReadAsync(long currentUserId);
    }
}
