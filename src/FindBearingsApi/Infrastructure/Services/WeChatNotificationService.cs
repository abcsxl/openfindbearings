using FindBearingsApi.Application.Services;

namespace FindBearingsApi.Infrastructure.Services
{
    public class WeChatNotificationService : IWeChatNotificationService
    {
        public async Task SendPushAsync(string openid, string content)
        {
            // 调用微信 API 发送订阅消息
            //var url = $"https://api.weixin.qq.com/cgi-bin/message/template/send?access_token={accessToken}";
            //var payload = new
            //{
            //    touser = openid,
            //    template_id = "your_template_id",
            //    data = new { first = new { value = content } }
            //};

            //await HttpClient.PostAsJsonAsync(url, payload);
        }
    }
}
