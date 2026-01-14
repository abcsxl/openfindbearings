using FindBearingsApi.Application.Common;
using FindBearingsApi.Application.DTOs.Auth;
using FindBearingsApi.Application.DTOs.Shared;
using FindBearingsApi.Domain.Entities;
using FindBearingsApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FindBearingsApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                return BadRequest(new { code = 400, msg = "缺少微信登录 code", data = (object?)null });

            // 🔁 模拟：调用微信 jscode2session 接口（实际开发需替换为真实 HTTP 调用）
            var (success, openid) = await SimulateWeChatLoginAsync(request.Code);
            if (!success)
                return StatusCode(500, new { code = 500, msg = "微信登录失败", data = (object?)null });

            // 🔍 查找或创建用户
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

            //// 🎫 生成模拟 Token（后续替换为真实 JWT）
            //var token = $"mock_jwt_token_for_user_{user.Id}";
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
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"]!)),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            //return Ok(new
            //{
            //    code = 200,
            //    msg = "success",
            //    data = new
            //    {
            //        token,
            //        user = new
            //        {
            //            user.Id,
            //            user.Nickname,
            //            user.Avatar,
            //            user.Role
            //        }
            //    }
            //});

            return Ok(
                ApiResponse<dynamic>.Ok(new
                {
                    token,
                    user = new UserSummaryDto(user.Id, user.Nickname, user.Avatar, user.Role)
                }));
        }

        // 🧪 模拟微信登录（实际项目中替换为 HttpClient 调用微信 API）
        private static async Task<(bool success, string openid)> SimulateWeChatLoginAsync(string code)
        {
            // 模拟网络延迟
            await Task.Delay(50);

            // 简单哈希生成唯一 openid（仅用于开发）
            var hash = Math.Abs(code.GetHashCode()).ToString();
            return (true, $"wx_openid_{hash}");
        }
    }
}