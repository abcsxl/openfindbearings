namespace FindBearingsApi.Application.Common
{
    public static class ApiResponseExtensions
    {
        public static ApiResponse<T> Success<T>(this T data, string message = "success", int code = 200)
            => ApiResponse<T>.Ok(data, message, code);

        public static ApiResponse<T> Error<T>(string message, int code = 400)
             => ApiResponse<T>.Fail(message, code);
    }
}
