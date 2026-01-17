using FindBearingsApi.Application.Common;
using FindBearingsApi.Application.DTOs.Messages;
using FindBearingsApi.Application.DTOs.Shared;
using FindBearingsApi.Domain.Entities;
using FindBearingsApi.Infrastructure.Persistence;
using FindBearingsApi.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FindBearingsApi.Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly AppDbContext _context;
        private readonly IRecommendationService _recommendationService;
        private readonly IWeChatNotificationService _weChatNotificationService;

        public MessageService(
            AppDbContext context,
            IRecommendationService recommendationService,
            IWeChatNotificationService weChatNotificationService)
        {
            _context = context;
            _recommendationService = recommendationService;
            _weChatNotificationService = weChatNotificationService;
        }

        public async Task<MessageResponseDto> CreateMessageAsync(CreateMessageRequestDto request, long currentUserId)
        {
            // 1. 【校验】轴承型号（你之前担心的那一步）
            if (!BearingModelValidator.IsValid(request.BearingModel))
                throw new ArgumentException("轴承型号格式不正确");

            // 2. 【核心】构建实体并保存
            var message = new Message
            {
                UserId = currentUserId,
                Type = request.Type,
                BearingModel = request.BearingModel.Trim(),
                Quantity = request.Quantity,
                Description = request.Description?.Trim(),
                IsDeleted = false
                // CreatedAt 会在 SaveChangesAsync 时由数据库自动生成
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // 3. 【增强】自动创建通知给管理员（假设管理员 UserId = 1）
            // 这里可以优化：如果管理员也在“感兴趣列表”里，就不用重复发了
            var adminNotification = new Notification
            {
                UserId = 1, // 👈 这里填管理员的用户ID
                MessageId = message.Id,
                Content = $"【管理员通知】新消息：用户{currentUserId}发布了新的{message.Type}：{message.BearingModel} x{message.Quantity}"
            };
            _context.Notifications.Add(adminNotification);

            // 4. 【增强】查找感兴趣用户并创建通知（你原来的逻辑）
            var interestedUserIds = await _recommendationService.GetInterestedUserIdsAsync(message.BearingModel);
            if (interestedUserIds.Any())
            {
                var notifications = interestedUserIds
                    .Where(uid => uid != currentUserId) // 排除自己
                    .Select(uid => new Notification
                    {
                        UserId = uid,
                        MessageId = message.Id,
                        Content = $"有新的{(message.Type == MessageType.Demand ? "求购" : "供应")}：{message.BearingModel} x{message.Quantity}"
                    })
                    .ToList();

                _context.Notifications.AddRange(notifications);
            }

            // 5. 【增强】推送微信模板消息给管理员（异步执行，不阻塞主流程）
            // 注意：这里用 _ = 来“火并”一个异步任务，表示“发了就行，不管结果”
            _ = Task.Run(async () =>
            {
                try
                {
                    await _weChatNotificationService.SendToAdminAsync(message);
                }
                catch { /* 记录日志，但不要影响主业务 */ }
            });

            // 6. 保存所有通知（站内信）
            await _context.SaveChangesAsync();

            // 7. 查询用户信息并返回
            var user = await _context.Users.FindAsync(message.UserId);

            return new MessageResponseDto(
                message.Id,
                message.Type,
                message.BearingModel,
                message.Quantity,
                message.Description,
                message.CreatedAt,
                new UserSummaryDto(user!.Id, user.Nickname, user.Avatar, user.Role)
            );
        }

        public async Task<PagedResponse<MessageResponseDto>> GetMessagesAsync(GetMessageListRequestDto request)
        {
            var query = _context.Messages
                .Include(m => m.User)
                .Where(m => !m.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Model))
            {
                var keyword = request.Model.Trim();
                query = query.Where(m => EF.Functions.ILike(m.BearingModel, $"%{keyword}%"));
            }

            if (request.Type.HasValue)
            {
                query = query.Where(m => m.Type == request.Type.Value);
            }

            query = query.OrderByDescending(m => m.CreatedAt);

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

            return new PagedResponse<MessageResponseDto>
            {
                Items = messages,
                PageNumber = request.Page,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public async Task<bool> DeleteMessageAsync(long id, long currentUserId)
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == currentUserId);

            if (message == null) return false;

            // 注意：原 Controller 有个 IsDeleted 字段但未使用，这里按软删除处理
            message.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAsInterestedAsync(long messageId, long currentUserId)
        {
            var exists = await _context.Interests
                .AnyAsync(i => i.UserId == currentUserId && i.MessageId == messageId);
            if (exists) return true;

            try
            {
                _context.Interests.Add(new Interest { UserId = currentUserId, MessageId = messageId });
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                return true; // 幂等
            }
        }

        public async Task RemoveInterestAsync(long messageId, long currentUserId)
        {
            await _context.Interests
                .Where(i => i.UserId == currentUserId && i.MessageId == messageId)
                .ExecuteDeleteAsync();
        }

        public async Task<MessageDetailDto?> GetMessageByIdAsync(long id, long currentUserId)
        {
            var message = await _context.Messages
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (message == null) return null;

            var user = message.User!;
            return new MessageDetailDto(
                message.Id,
                message.Type,
                message.BearingModel,
                message.Quantity,
                message.Description,
                message.ContactInfo,
                message.CreatedAt,
                new UserSummaryDto(user.Id, user.Nickname, user.Avatar, user.Role)
            );
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            return ex.InnerException?.Message.Contains("23505") == true;
        }
    }
}
