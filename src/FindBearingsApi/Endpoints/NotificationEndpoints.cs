using FindBearingsApi.Application.Common;
using FindBearingsApi.Application.DTOs.Notification;
using FindBearingsApi.Application.Services;

namespace FindBearingsApi.Endpoints
{
    public static class NotificationEndpoints
    {
        public static void MapNotificationEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/notifications").WithTags("Notifications");

            // GET /api/notifications —— 分页获取通知
            group.MapGet("/", async (
                 HttpContext ctx,
                 INotificationService service,
                [AsParameters] int page = 1,
                [AsParameters] int pageSize = 20) =>
            {
                var userId = ClaimsHelper.GetUserIdFromClaims(ctx);
                if (userId <= 0) return Results.Unauthorized();

                var paged = await service.GetNotificationsAsync(page, pageSize, userId);
                return Results.Ok(ApiResponse<PagedResponse<NotificationResponseDto>>.Ok(paged));
            })
            .RequireAuthorization();

            // PATCH /api/notifications/{id}/read —— 标记为已读
            group.MapPatch("/{id:long}/read", async (
                long id,
                HttpContext ctx,
                INotificationService service) =>
            {
                var userId = ClaimsHelper.GetUserIdFromClaims(ctx);
                if (userId <= 0) return Results.Unauthorized();

                var success = await service.MarkAsReadAsync(id, userId);
                if (!success) return Results.NotFound(ApiResponse<dynamic>.Fail("通知不存在", 404));

                return Results.Ok(ApiResponse<dynamic>.Ok(null, "已标记为已读"));
            })
            .RequireAuthorization();

            // PATCH /api/notifications/read-all —— 全部标记为已读
            group.MapPatch("read-all", async (
                HttpContext ctx,
                INotificationService service) =>
            {
                var userId = ClaimsHelper.GetUserIdFromClaims(ctx);
                if (userId <= 0) return Results.Unauthorized();

                var count = await service.MarkAllAsReadAsync(userId);
                return Results.Ok(ApiResponse<dynamic>.Ok(new { count }, $"已标记 {count} 条为已读"));
            })
            .RequireAuthorization();
        }
    }
}
