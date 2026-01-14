using FindBearingsApi.Domain.Entities;

namespace FindBearingsApi.Application.DTOs.Messages
{
    public record MyMessageResponseDto(
        long Id,
        MessageType Type,
        string BearingModel,
        int Quantity,
        string? Description,
        bool IsDeleted,        // 显示是否已被删除
        DateTime CreatedAt,
        DateTime? DeletedAt    // 可选：删除时间（用于恢复？）
    );
}
