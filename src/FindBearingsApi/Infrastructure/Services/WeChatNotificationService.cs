using FindBearingsApi.Application.Services;
using FindBearingsApi.Domain.Entities;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace FindBearingsApi.Infrastructure.Services
{
    // 模板消息请求体
    public class WeChatTemplateMessage
    {
        public string Touser { get; set; } = string.Empty;
        public string TemplateId { get; set; } = string.Empty;
        public string? Page { get; set; }
        public object Data { get; set; } = new { };
    }

    public class WeChatNotificationOptions
    {
        public string NewMessageTemplateId { get; set; } = string.Empty;
    }

    public class WeChatNotificationService : IWeChatNotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly IWeChatTokenService _tokenService;
        private readonly IAdminService _adminService;
        private readonly string _templateId;

        public WeChatNotificationService(
            HttpClient httpClient,
            IWeChatTokenService tokenService,
            IAdminService adminService,
            IOptions<WeChatNotificationOptions> options)
            {
                _httpClient = httpClient;
                _tokenService = tokenService;
                _adminService = adminService;
                _templateId = options.Value.NewMessageTemplateId;
            }

        public async Task SendPushAsync(string openid, string content)
        {
            // 调用微信 API 发送订阅消息
            if (string.IsNullOrEmpty(openid)) return;

            var message = new WeChatTemplateMessage
            {
                Touser = openid,
                TemplateId = _templateId,
                Page = "pages/index/index", // 用户点击通知后跳转的小程序页面
                Data = new
                {
                    thing1 = new { value = content },      // 根据你申请的模板字段调整
                    time2 = new { value = DateTime.Now.ToString("yyyy-MM-dd HH:mm") }
                }
            };

            await SendTemplateMessageAsync(message);
        }

        public async Task SendToAdminAsync(Message message)
        {
            var adminOpenIds = await _adminService.GetAdminOpenIdsAsync();
            var content = $"【新消息】{message.Type}：{message.BearingModel} x{message.Quantity}";

            foreach (var openId in adminOpenIds)
            {
                try
                {
                    await SendPushAsync(openId, content);
                }
                catch (Exception ex)
                {
                    // 建议：记录日志，但不要抛出异常影响主流程
                    // _logger?.LogError(ex, "微信推送失败，OpenId: {OpenId}", openId);
                }
            }
        }

        private async Task SendTemplateMessageAsync(WeChatTemplateMessage message)
        {
            try
            {
                var accessToken = await _tokenService.GetAccessTokenAsync();
                var url = $"https://api.weixin.qq.com/cgi-bin/message/subscribe/send?access_token={accessToken}";

                var json = JsonSerializer.Serialize(message, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var response = await _httpClient.PostAsync(url, content);
                var responseText = await response.Content.ReadAsStringAsync();

                // 可选：记录日志
                // Console.WriteLine($"WeChat Push Result: {responseText}");
            }
            catch (Exception ex)
            {
                // 关键：不要让推送失败影响主业务流程！
                // 建议：记录日志，但不抛出异常
                // _logger?.LogError(ex, "微信推送失败");
            }
        }
    }
}
