using FindBearingsApi.Application.Common;
using FindBearingsApi.Application.DTOs.Messages;

namespace FindBearingsApi.Application.Services
{
    public interface IMessageService
    {
        Task<MessageResponseDto> CreateMessageAsync(CreateMessageRequestDto request, long currentUserId);
        Task<PagedResponse<MessageResponseDto>> GetMessagesAsync(GetMessageListRequestDto request);
        Task<bool> DeleteMessageAsync(long id, long currentUserId);
        Task<bool> MarkAsInterestedAsync(long messageId, long currentUserId);
        Task RemoveInterestAsync(long messageId, long currentUserId);
        Task<MessageDetailDto?> GetMessageByIdAsync(long id, long currentUserId);
    }
}
