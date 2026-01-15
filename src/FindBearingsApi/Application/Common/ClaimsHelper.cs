namespace FindBearingsApi.Application.Common
{
    public static class ClaimsHelper
    {
        public static long GetUserIdFromClaims(HttpContext ctx)
        {
            var userIdStr = ctx.User.FindFirst("userId")?.Value;
            return long.TryParse(userIdStr, out var id) ? id : 0;
        }
    }
}
