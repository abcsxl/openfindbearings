using FindBearingsApi.Application.DTOs.Shared;
using FindBearingsApi.Domain.Entities;

namespace FindBearingsApi.Application.DTOs.Messages
{
    public record MessageDetailDto(
        long Id,
        MessageType Type,
        string BearingModel,
        int Quantity,
        string? Description,
        string ContactInfo,           // ← 敏感信息，仅登录用户可见
        DateTime CreatedAt,
        UserSummaryDto Publisher      // 发布者简要信息
    );
}
