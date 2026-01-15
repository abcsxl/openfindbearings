using FindBearingsApi.Application.Common;
using FindBearingsApi.Application.DTOs.Messages;
using FindBearingsApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FindBearingsApi.Application.Services
{
    public class MyMessageService : IMyMessageService
    {
        private readonly AppDbContext _context;

        public MyMessageService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<MyMessageResponseDto>> GetMyMessagesAsync(int page, int pageSize, long currentUserId)
        {
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

            return new PagedResponse<MyMessageResponseDto>
            {
                Items = messages,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public async Task<(bool Exists, bool AlreadyDeleted)> DeleteMyMessageAsync(long id, long currentUserId)
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == currentUserId);

            if (message == null)
                return (Exists: false, AlreadyDeleted: false);

            if (message.IsDeleted)
                return (Exists: true, AlreadyDeleted: true);

            message.IsDeleted = true;
            message.DeletedAt = DateTimeHelper.UtcNow();
            await _context.SaveChangesAsync();

            return (Exists: true, AlreadyDeleted: false);
        }
    }
}
