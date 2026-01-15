using FindBearingsApi.Application.Common;
using FindBearingsApi.Application.DTOs.Auth;
using FindBearingsApi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FindBearingsApi.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/auth").WithTags("Auth");

            group.MapPost("/login", async (
                [FromBody] LoginRequestDto request,
                IAuthService authService) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(request.Code))
                        return Results.BadRequest(new { code = 400, msg = "缺少微信登录 code", data = (object?)null });

                    var (token, user) = await authService.LoginAsync(request);

                    //return Results.Json(
                    //    ApiResponse<dynamic>.Ok(new
                    //    {
                    //        token,
                    //        user
                    //    }, "success", 200)
                    //);
                    return Results.Json(new { token, user }.Success("success", 200));
                }
                catch (InvalidOperationException ex) when (ex.Message == "微信登录失败")
                {
                    return Results.Json(
                        ApiResponse<dynamic>.Fail("微信登录失败", 500),
                        statusCode: 500
                    );
                }
                catch (Exception)
                {
                    return Results.Json(
                        ApiResponse<dynamic>.Fail("登录异常", 500),
                        statusCode: 500
                    );
                }
            });
        }
    }
}
