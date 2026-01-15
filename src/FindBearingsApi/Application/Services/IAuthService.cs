using FindBearingsApi.Application.DTOs.Auth;
using FindBearingsApi.Application.DTOs.Shared;

namespace FindBearingsApi.Application.Services
{
    public interface IAuthService
    {
        Task<(string Token, UserSummaryDto User)> LoginAsync(LoginRequestDto request);
    }
}
