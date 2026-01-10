namespace Identity.Models
{
    public class TokenResult
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
    }

    public class TokenRequest
    {
        public string grant_type { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string refresh_token { get; set; } = string.Empty;
        public string scope { get; set; } = string.Empty;
    }

    // 添加在 AccountController.cs 文件底部
    public class RegisterSupplierRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? BusinessLicense { get; set; }
    }

    public class RegisterResponse
    {
        public long UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool RequiresVerification { get; set; }
    }

    //public class LoginRequest
    //{
    //    public string Email { get; set; } = string.Empty;
    //    public string Password { get; set; } = string.Empty;
    //    public bool RememberMe { get; set; }
    //}

    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public UserInfo User { get; set; } = new();
    }

    public class UserInfo
    {
        public long Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public UserType UserType { get; set; }
        public string? CompanyName { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }

        public static ApiResponse<T> SuccessResponse(T data) => new()
        {
            Success = true,
            Data = data
        };

        public static ApiResponse<T> ErrorResponse(string message) => new()
        {
            Success = false,
            Message = message
        };
    }

    // 用户注册事件
    public class UserRegisteredEvent
    {
        public long UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public DateTime RegisteredAt { get; set; }
    }

    // 用户档案
    public class UserProfile
    {
        public long Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public UserType UserType { get; set; }
        public string? CompanyName { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
