using Dapr.Client;
using IdentityApi.Models;
using IdentityApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserService _userService;
        private readonly DaprClient _daprClient;  // 修正：拼写
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUserService userService,
            DaprClient daprClient,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userService = userService;
            _daprClient = daprClient;  // 修正
            _logger = logger;
        }

        [HttpPost("register/supplier")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<RegisterResponse>>> RegisterSupplier(
            [FromBody] RegisterSupplierRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse("请求数据无效"));

                // 检查用户是否存在
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                    return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse("邮箱已被注册"));

                // 创建用户
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    DisplayName = request.ContactName,
                    PhoneNumber = request.Phone,
                    UserType = UserType.SupplierUser,
                    CompanyName = request.CompanyName,
                    ContactPerson = request.ContactName,
                    BusinessLicense = request.BusinessLicense
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                    return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse(
                        string.Join(", ", result.Errors.Select(e => e.Description))));

                // 分配供应商角色
                await _userManager.AddToRoleAsync(user, "Supplier");

                // 发布用户注册事件
                await _daprClient.PublishEventAsync("pubsub", "user-registered",
                    new UserRegisteredEvent
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        UserType = user.UserType.ToString(),
                        CompanyName = user.CompanyName,
                        RegisteredAt = DateTime.UtcNow
                    });

                _logger.LogInformation("供应商注册成功: {Email}", user.Email);

                return Ok(ApiResponse<RegisterResponse>.SuccessResponse(new RegisterResponse
                {
                    UserId = user.Id,
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    RequiresVerification = true
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "供应商注册失败");
                return StatusCode(500, ApiResponse<RegisterResponse>.ErrorResponse("注册失败"));
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request) 
        {
            try
            {
                // 使用request.Email和request.Password
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                    return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse("用户名或密码错误"));

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!result.Succeeded)
                    return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse("用户名或密码错误"));

                if (!user.IsActive)
                    return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse("账户已被禁用"));

                // 生成JWT令牌（使用我们自己的服务，不依赖OpenIddict完整流程）
                var token = await _userService.GenerateJwtTokenAsync(user);

                _logger.LogInformation("用户登录成功: {Email}", user.Email);

                return Ok(ApiResponse<LoginResponse>.SuccessResponse(new LoginResponse
                {
                    AccessToken = token.AccessToken,
                    RefreshToken = token.RefreshToken,
                    ExpiresIn = token.ExpiresIn,
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Email = user.Email,
                        DisplayName = user.DisplayName,
                        UserType = user.UserType,
                        CompanyName = user.CompanyName
                    }
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户登录失败");
                return StatusCode(500, ApiResponse<LoginResponse>.ErrorResponse("登录失败"));
            }
        }

        [HttpPost("connect/token")]
        [AllowAnonymous]
        public async Task<IActionResult> Token([FromForm] TokenRequest request)
        {
            try
            {
                if (request.grant_type == "password")
                {
                    // 处理密码授权流程
                    var user = await _userManager.FindByNameAsync(request.username);
                    if (user == null)
                        return BadRequest(new { error = "invalid_grant" });

                    var result = await _signInManager.CheckPasswordSignInAsync(user, request.password, false);
                    if (!result.Succeeded)
                        return BadRequest(new { error = "invalid_grant" });

                    var token = await _userService.GenerateJwtTokenAsync(user);

                    return Ok(new
                    {
                        access_token = token.AccessToken,
                        refresh_token = token.RefreshToken,
                        expires_in = token.ExpiresIn,
                        token_type = "Bearer"
                    });
                }
                else if (request.grant_type == "refresh_token")
                {
                    // 处理刷新令牌
                    // 实现刷新令牌逻辑
                    return BadRequest(new { error = "unsupported_grant_type" });
                }

                return BadRequest(new { error = "unsupported_grant_type" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "令牌请求失败");
                return StatusCode(500, new { error = "server_error" });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserProfile>>> GetProfile()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId) || !long.TryParse(userId, out var id))
                return Unauthorized(ApiResponse<UserProfile>.ErrorResponse("未授权"));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<UserProfile>.ErrorResponse("用户不存在"));

            return Ok(ApiResponse<UserProfile>.SuccessResponse(new UserProfile
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                PhoneNumber = user.PhoneNumber,
                UserType = user.UserType,
                CompanyName = user.CompanyName,
                IsActive = user.IsActive,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt
            }));
        }
    }
}
