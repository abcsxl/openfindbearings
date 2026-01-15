using FindBearingsApi.Application.Common;
using FindBearingsApi.Application.DTOs.Messages;
using FindBearingsApi.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FindBearingsApi.Endpoints
{
    public static class MessageEndpoints
    {
        public static void MapMessageEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/messages").WithTags("Messages");

            // POST /api/messages — 需要登录
            group.MapPost("/", async (
                [FromBody] CreateMessageRequestDto request,
                IMessageService service,
                HttpContext ctx) =>
            {
                var userId = ClaimsHelper.GetUserIdFromClaims(ctx);
                if (userId <= 0) return Results.Unauthorized();

                var userExists = await app.Services.GetRequiredService<Infrastructure.Persistence.AppDbContext>()
                    .Users.AnyAsync(u => u.Id == userId);
                if (!userExists) return Results.BadRequest(new { code = 400, msg = "用户不存在", data = (object?)null });

                var dto = await service.CreateMessageAsync(request, userId);
                return Results.Ok(ApiResponse<MessageResponseDto>.Ok(dto));
            })
            .RequireAuthorization();

            // GET /api/messages — 匿名可访问
            group.MapGet("/", async (
                [AsParameters] GetMessageListRequestDto request,
                IMessageService service) =>
            {
                var paged = await service.GetMessagesAsync(request);
                return Results.Ok(ApiResponse<PagedResponse<MessageResponseDto>>.Ok(paged));
            });

            // DELETE /api/messages/{id} — 需要登录
            group.MapDelete("/{id:long}", async (
                long id,
                IMessageService service,
                HttpContext ctx) =>
            {
                var userId = ClaimsHelper.GetUserIdFromClaims(ctx);
                if (userId <= 0) return Results.Unauthorized();

                var success = await service.DeleteMessageAsync(id, userId);
                if (!success) return Results.NotFound(new { code = 404, msg = "消息不存在或无权删除" });

                return Results.Ok(new { code = 200, msg = "删除成功", data = new { id } });
            })
            .RequireAuthorization();

            // POST /api/messages/{id}/interested — 需要登录
            group.MapPost("/{id:long}/interested", async (
                long id,
                IMessageService service,
                HttpContext ctx) =>
            {
                var userId = ClaimsHelper.GetUserIdFromClaims(ctx);
                if (userId <= 0) return Results.Unauthorized();

                var success = await service.MarkAsInterestedAsync(id, userId);
                return Results.Ok(new { code = 200, msg = "success", data = new { interested = success } });
            })
            .RequireAuthorization();

            // DELETE /api/messages/{id}/interested — 需要登录
            group.MapDelete("/{id:long}/interested", async (
                long id,
                IMessageService service,
                HttpContext ctx) =>
            {
                var userId = ClaimsHelper.GetUserIdFromClaims(ctx);
                if (userId <= 0) return Results.Unauthorized();

                await service.RemoveInterestAsync(id, userId);
                return Results.Ok(new { code = 200, msg = "success", data = new { interested = false } });
            })
            .RequireAuthorization();

            // GET /api/messages/{id} — 需要登录（查看详情）
            group.MapGet("/{id:long}", async (
                long id,
                IMessageService service,
                HttpContext ctx) =>
            {
                var userId = ClaimsHelper.GetUserIdFromClaims(ctx);
                if (userId <= 0) return Results.Unauthorized();

                var dto = await service.GetMessageByIdAsync(id, userId);
                if (dto == null) return Results.NotFound(ApiResponse<dynamic>.Fail("消息不存在或已被删除", 404));

                return Results.Ok(ApiResponse<MessageDetailDto>.Ok(dto));
            })
            .RequireAuthorization();
        }

    }
}
