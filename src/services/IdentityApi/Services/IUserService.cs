using IdentityApi.Models;

namespace IdentityApi.Services
{
    public interface IUserService
    {
        Task<TokenResult> GenerateJwtTokenAsync(ApplicationUser user);
        Task<TokenResult> RefreshTokenAsync(string refreshToken);
        Task<ApplicationUser?> GetUserByIdAsync(long userId);
        Task<bool> ValidateCredentialsAsync(string email, string password);
    }

    public interface ITokenService
    {
        Task<string> GenerateAccessTokenAsync(ApplicationUser user);
        Task<string> GenerateRefreshTokenAsync();
        Task<bool> ValidateTokenAsync(string token);
    }
}
