using FindBearingsApi.Application.DTOs.Shared;
using FindBearingsApi.Domain.Entities;

namespace FindBearingsApi.Application.DTOs.Messages
{
    public record MessageResponseDto(
        long Id,
        MessageType Type,
        string BearingModel,
        int Quantity,
        string? Description,
        DateTime CreatedAt,
        UserSummaryDto User
    );
}
