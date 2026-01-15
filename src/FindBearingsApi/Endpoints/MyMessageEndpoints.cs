using FindBearingsApi.Application.Common;
using FindBearingsApi.Application.DTOs.Messages;
using FindBearingsApi.Application.Services;

namespace FindBearingsApi.Endpoints
{
    public static class MyMessageEndpoints
    {
        public static void MapMyMessageEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/mymessages").WithTags("MyMessages");

            // GET /api/mymessages —— 分页获取我的消息
            group.MapGet("/", async (
                HttpContext ctx,
                IMyMessageService service,
                [AsParameters] int page = 1,
                [AsParameters] int pageSize = 20) =>
            {
                var userId = ClaimsHelper.GetUserIdFromClaims(ctx);
                if (userId <= 0) return Results.Unauthorized();

                var paged = await service.GetMyMessagesAsync(page, pageSize, userId);
                return Results.Ok(ApiResponse<PagedResponse<MyMessageResponseDto>>.Ok(paged));
            })
            .RequireAuthorization();

            // DELETE /api/mymessages/{id} —— 软删除消息
            group.MapDelete("/{id:long}", async (
                long id,
                HttpContext ctx,
                IMyMessageService service) =>
            {
                var userId = ClaimsHelper.GetUserIdFromClaims(ctx);
                if (userId <= 0) return Results.Unauthorized();

                var (exists, alreadyDeleted) = await service.DeleteMyMessageAsync(id, userId);

                if (!exists)
                    return Results.NotFound(ApiResponse<dynamic>.Fail("消息不存在或无权限", 404));

                if (alreadyDeleted)
                    return Results.Ok(ApiResponse<dynamic>.Ok(null, "消息已删除"));

                return Results.Ok(ApiResponse<dynamic>.Ok(null, "删除成功"));
            })
            .RequireAuthorization();
        }
    }
}
