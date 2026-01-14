using FindBearingsApi.Application.Common;
using FindBearingsApi.Application.DTOs.Messages;
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
    public class MyMessagesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MyMessagesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyMessages([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var currentUserId = GetUserIdFromToken();
            if (currentUserId <= 0)
                return Unauthorized(ApiResponse<dynamic>.Fail("未登录", 401));

            const int maxPageSize = 50;
            pageSize = Math.Min(pageSize, maxPageSize);
            var skip = (page - 1) * pageSize;

            var query = _context.Messages
                .Where(m => m.UserId == currentUserId)
                .OrderByDescending(m => m.CreatedAt);

            var total = await query.CountAsync();
            var messages = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(m => new MyMessageResponseDto(
                    m.Id,
                    m.Type,
                    m.BearingModel,
                    m.Quantity,
                    m.Description,
                    m.IsDeleted,
                    m.CreatedAt,
                    m.DeletedAt
                ))
                .ToListAsync();

            var paged = new PagedResponse<MyMessageResponseDto>
            {
                Items = messages,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = total
            };

            return Ok(ApiResponse<PagedResponse<MyMessageResponseDto>>.Ok(paged));
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteMyMessage(long id)
        {
            var currentUserId = GetUserIdFromToken();
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == currentUserId);

            if (message == null)
                return NotFound(ApiResponse<dynamic>.Fail("消息不存在或无权限", 404));

            if (message.IsDeleted)
                return Ok(ApiResponse<dynamic>.Ok(null, "消息已删除"));

            message.IsDeleted = true;
            message.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<dynamic>.Ok(null, "删除成功"));
        }

        private long GetUserIdFromToken()
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            return long.TryParse(userIdStr, out var id) ? id : 0;
        }
    }
}
