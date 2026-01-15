using System.Text.Json.Serialization;

namespace FindBearingsApi.Application.Common
{
    /// <summary>
    /// 统一 API 响应格式
    /// </summary>
    public class ApiResponse<T>
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 业务状态码（200=成功, 400=参数错误, 401=未授权等）
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 消息描述
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 返回数据
        /// </summary>
        [JsonPropertyName("data")]
        public T? Data { get; set; }

        // 静态工厂方法：成功
        public static ApiResponse<T> Ok(T data, string message = "操作成功", int code = 200)
            => new() { Success = true, Code = code, Message = message, Data = data };

        // 静态工厂方法：失败
        public static ApiResponse<T> Fail(string message, int code = 400)
            => new() { Success = false, Code = code, Message = message };
    }

    //public class ApiResponse<T>
    //{
    //    public int Code { get; set; }
    //    public string Msg { get; set; }
    //    public T Data { get; set; }

    //    public static ApiResponse<T> Ok(T data)
    //    {
    //        return new ApiResponse<T> { Code = 200, Msg = "success", Data = data };
    //    }

    //    public static ApiResponse<T> Fail(string msg, int code = 400)
    //    {
    //        return new ApiResponse<T> { Code = code, Msg = msg, Data = default! };
    //    }
    //}
}
