using FindBearingsApi.Application.Common;
using FindBearingsApi.Application.DTOs.Notification;
using FindBearingsApi.Domain.Entities;
using FindBearingsApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FindBearingsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var currentUserId = GetUserIdFromToken();
            if (currentUserId <= 0)
                return Unauthorized(ApiResponse<dynamic>.Fail("未登录", 401));

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

            var paged = new PagedResponse<NotificationResponseDto>
            {
                Items = notifications,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = total
            };

            return Ok(ApiResponse<PagedResponse<NotificationResponseDto>>.Ok(paged));
        }

        [HttpPatch("{id:long}/read")]
        public async Task<IActionResult> MarkAsRead(long id)
        {
            var currentUserId = GetUserIdFromToken();
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == currentUserId);

            if (notification == null)
                return NotFound(ApiResponse<dynamic>.Fail("通知不存在", 404));

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<dynamic>.Ok(null, "已标记为已读"));
        }

        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var currentUserId = GetUserIdFromToken();
            var unreadCount = await _context.Notifications
                .Where(n => n.UserId == currentUserId && !n.IsRead)
                .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));

            return Ok(ApiResponse<dynamic>.Ok(new { count = unreadCount }, $"已标记 {unreadCount} 条为已读"));
        }

        private long GetUserIdFromToken()
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            return long.TryParse(userIdStr, out var id) ? id : 0;
        }
    }
}
