using FindBearingsApi.Application.Common;
using FindBearingsApi.Application.DTOs.Messages;
using FindBearingsApi.Application.DTOs.Shared;
using FindBearingsApi.Application.Services;
using FindBearingsApi.Domain.Entities;
using FindBearingsApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FindBearingsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MessagesController> _logger;
        private readonly IRecommendationService _recommendationService;

        public bool IsDeleted { get; set; } = false;

        public MessagesController(
            AppDbContext context,
            ILogger<MessagesController> logger,
            IRecommendationService recommendationService)
        {
            _context = context;
            _logger = logger;
            _recommendationService = recommendationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage([FromBody] CreateMessageRequestDto request)
        {
            // 🔐 模拟：从 Token 获取当前用户 ID（实际应解析 JWT）
            var currentUserId = GetUserIdFromToken(); // 开发阶段固定为 1
            if (currentUserId <= 0)
                return Unauthorized(new { code = 401, msg = "未登录", data = (object?)null });

            // 验证用户是否存在
            var userExists = await _context.Users.AnyAsync(u => u.Id == currentUserId);
            if (!userExists)
                return BadRequest(new { code = 400, msg = "用户不存在", data = (object?)null });

            // 创建消息
            var message = new Message
            {
                UserId = currentUserId,
                Type = request.Type,
                BearingModel = request.BearingModel.Trim(),
                Quantity = request.Quantity,
                Description = request.Description?.Trim(),
                IsDeleted = false // 显式设置
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // 🔔 新增：查找感兴趣用户（可用于后续推送）
            var interestedUserIds = await _recommendationService.GetInterestedUserIdsAsync(message.BearingModel);

            // TODO: 这里可以：
            // - 存入 Notification 表
            // - 调用微信订阅消息 API
            // - 发送 RabbitMQ 消息到推送服务
            // 暂时先打印日志
            if (interestedUserIds.Any())
            {
                var notifications = interestedUserIds
                  .Where(uid => uid != currentUserId) // 不推给自己
                  .Select(uid => new Notification
                  {
                      UserId = uid,
                      MessageId = message.Id,
                      Content = $"有新的{(message.Type == MessageType.Demand ? "求购" : "供应")}：{message.BearingModel} x{message.Quantity}"
                  })
                  .ToList();

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();
                Console.WriteLine($"发现 {interestedUserIds.Count} 个用户对 {message.BearingModel} 感兴趣");
            }

            //return Ok(new
            //{
            //    code = 200,
            //    msg = "success",
            //    data = new
            //    {
            //        id = message.Id,
            //        type = message.Type,
            //        bearingModel = message.BearingModel,
            //        quantity = message.Quantity,
            //        description = message.Description,
            //        createdAt = message.CreatedAt
            //    }
            //});
            var responseData = new MessageResponseDto(
    message.Id,
    message.Type,
    message.BearingModel,
    message.Quantity,
    message.Description,
    message.CreatedAt,
    new UserSummaryDto(message.User!.Id, message.User.Nickname, message.User.Avatar, message.User.Role)
);

            return Ok(ApiResponse<MessageResponseDto>.Ok(responseData));
        }

        private long GetUserIdFromToken()
        {
            // 开发阶段：假设所有请求来自用户 ID=1
            //return 1;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var id) ? id : 0;
        }

        [HttpGet]
        [AllowAnonymous] // 允许未登录用户浏览（符合供求平台逻辑）
        public async Task<IActionResult> GetMessages([FromQuery] GetMessageListRequestDto request)
        {
            var query = _context.Messages
                .Include(m => m.User) // 加载发布者信息
                .Where(m => !m.IsDeleted)
                .AsQueryable();

            // 🔎 按型号模糊搜索（PostgreSQL 不区分大小写）
            if (!string.IsNullOrWhiteSpace(request.Model))
            {
                var keyword = request.Model.Trim();
                query = query.Where(m => EF.Functions.ILike(m.BearingModel, $"%{keyword}%"));
            }

            // 🧾 按类型筛选
            if (request.Type.HasValue)
            {
                query = query.Where(m => m.Type == request.Type.Value);
            }

            // 📅 按时间倒序
            query = query.OrderByDescending(m => m.CreatedAt);

            // 📐 分页安全处理
            var pageSize = Math.Min(Math.Max(request.PageSize, 1), 20);
            var skip = (Math.Max(request.Page, 1) - 1) * pageSize;

            var total = await query.CountAsync();
            var messages = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(m => new MessageResponseDto(
                    m.Id,
                    m.Type,
                    m.BearingModel,
                    m.Quantity,
                    m.Description,
                    m.CreatedAt,
                    new UserSummaryDto(m.User!.Id, m.User.Nickname, m.User.Avatar, m.User.Role)
                ))
                .ToListAsync();

            return Ok(ApiResponse<PagedResponse<MessageResponseDto>>.Ok(new PagedResponse<MessageResponseDto>
            {
                Items = messages,
                PageNumber = request.Page,
                PageSize = pageSize,
                TotalCount = total
            }));
        }

        [HttpDelete("{id:long}")]
        [Authorize]
        public async Task<IActionResult> DeleteMessage(long id)
        {
            var currentUserId = GetUserIdFromToken();
            if (currentUserId <= 0)
                return Unauthorized(new { code = 401, msg = "无效身份凭证" });

            // 🔍 查找消息并确保属于当前用户
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == currentUserId);

            if (message == null)
                return NotFound(new { code = 404, msg = "消息不存在或无权删除" });
            if (IsDeleted)
            {
                _context.Messages.Remove(message);
            }
            else
            {
                message.IsDeleted = true;
            }
            await _context.SaveChangesAsync();

            return Ok(new { code = 200, msg = "删除成功", data = new { id } });
        }

        [HttpPost("{id:long}/interested")]
        [Authorize]
        public async Task<IActionResult> MarkAsInterested(long id)
        {
            var currentUserId = GetUserIdFromToken();
            if (currentUserId <= 0)
                return Unauthorized(new { code = 401, msg = "未登录" });

            // 🔍 验证消息是否存在且未被删除（如果用了软删除需加 IsDeleted=false）
            var messageExists = await _context.Messages
                .Where(m => !m.IsDeleted)
                .AnyAsync(m => m.Id == id);

            if (!messageExists)
                return NotFound(new { code = 404, msg = "消息不存在" });

            // 🔁 幂等操作：尝试插入，冲突则忽略
            try
            {
                var interest = new Interest
                {
                    UserId = currentUserId,
                    MessageId = id
                };

                _context.Interests.Add(interest);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // 已存在，视为成功（幂等）
            }

            return Ok(new
            {
                code = 200,
                msg = "success",
                data = new { interested = true }
            });
        }

        // 判断是否唯一键冲突（PostgreSQL 错误码 23505）
        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            return ex.InnerException?.Message.Contains("23505") == true;
        }

        [HttpDelete("{id:long}/interested")]
        [Authorize]
        public async Task<IActionResult> RemoveInterest(long id)
        {
            var currentUserId = GetUserIdFromToken();
            var rowsAffected = await _context.Interests
                .Where(i => i.UserId == currentUserId && i.MessageId == id)
                .ExecuteDeleteAsync(); // EF Core 7+ 高效批量删除

            return Ok(new
            {
                code = 200,
                msg = "success",
                data = new { interested = false }
            });
        }
    }
}
