using FindBearingsApi.Domain.Entities;

namespace FindBearingsApi.Application.DTOs.Notification
{
    public record NotificationResponseDto(
        long Id,
        string Content,
        bool IsRead,
        DateTime CreatedAt,
        MessageSummaryDto Message // 只返回关键信息
    );

    public record MessageSummaryDto(
        long Id,
        MessageType Type,
        string BearingModel,
        int Quantity
    );
}
