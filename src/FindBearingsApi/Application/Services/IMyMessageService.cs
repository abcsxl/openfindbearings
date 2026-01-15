using FindBearingsApi.Application.Common;
using FindBearingsApi.Application.DTOs.Messages;

namespace FindBearingsApi.Application.Services
{
    public interface IMyMessageService
    {
        Task<PagedResponse<MyMessageResponseDto>> GetMyMessagesAsync(int page, int pageSize, long currentUserId);
        Task<(bool Exists, bool AlreadyDeleted)> DeleteMyMessageAsync(long id, long currentUserId);
    }
}
