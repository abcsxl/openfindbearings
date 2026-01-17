using FindBearingsApi.Application.Common;
using FindBearingsApi.Application.DTOs.Auth;
using FindBearingsApi.Application.DTOs.Shared;
using FindBearingsApi.Domain.Entities;
using FindBearingsApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FindBearingsApi.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<(string Token, UserSummaryDto User)> LoginAsync(LoginRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                throw new ArgumentException("缺少微信登录 code");

            var (success, openid) = await SimulateWeChatLoginAsync(request.Code);
            if (!success)
                throw new InvalidOperationException("微信登录失败");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.OpenId == openid);
            if (user == null)
            {
                user = new User
                {
                    OpenId = openid,
                    Nickname = $"用户_{Guid.NewGuid().ToString("N")[..8]}",
                    Avatar = "https://picsum.photos/150?text=User"
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            var token = GenerateJwtToken(user);
            var userDto = new UserSummaryDto(user.Id, user.Nickname, user.Avatar, user.Role);

            return (token, userDto);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Nickname),
                new Claim("role", user.Role.ToString())
            }),
                Expires = DateTimeHelper.UtcNow().AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"]!)),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        // 🧪 模拟微信登录（与原 Controller 完全一致）
        private static async Task<(bool success, string openid)> SimulateWeChatLoginAsync(string code)
        {
            await Task.Delay(50);
            var hash = Math.Abs(code.GetHashCode()).ToString();
            return (true, $"wx_openid_{hash}");
        }
    }
}
