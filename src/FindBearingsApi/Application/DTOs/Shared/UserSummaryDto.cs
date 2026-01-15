using FindBearingsApi.Domain.Entities;

namespace FindBearingsApi.Application.DTOs.Shared
{
    public record UserSummaryDto(
         long Id,
         string Nickname,
         string Avatar,
         UserRole Role = UserRole.Member
     );
}
