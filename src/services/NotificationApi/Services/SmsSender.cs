using Microsoft.Extensions.Options;
using NotificationApi.Models.Configuration;

namespace NotificationApi.Services
{
    public class SmsSender : ISmsSender
    {
        private readonly SmsConfig _smsConfig;
        private readonly ILogger<SmsSender> _logger;

        public SmsSender(IOptions<NotificationConfig> config, ILogger<SmsSender> logger)
        {
            _smsConfig = config.Value.Sms;
            _logger = logger;
        }

        public async Task<SendResult> SendSmsAsync(string to, string message)
        {
            try
            {
                if (!_smsConfig.Enabled)
                {
                    return new SendResult { Success = false, Error = "短信发送功能已禁用" };
                }

                if (!await ValidatePhoneNumberAsync(to))
                {
                    return new SendResult { Success = false, Error = $"无效的手机号码: {to}" };
                }

                // TODO: 实现短信发送逻辑
                _logger.LogInformation("短信发送功能暂未实现: {To}, 内容: {Message}", to, message);

                return new SendResult
                {
                    Success = false,
                    Error = "短信功能暂未实现"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "短信发送失败: {To}", to);
                return new SendResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<SendResult> SendSmsAsync(string to, string templateCode, Dictionary<string, string> templateData)
        {
            // TODO: 实现模板短信发送
            return new SendResult
            {
                Success = false,
                Error = "短信功能暂未实现"
            };
        }

        public async Task<List<SendResult>> SendBulkSmsAsync(List<SmsMessage> messages)
        {
            var results = new List<SendResult>();
            foreach (var message in messages)
            {
                results.Add(new SendResult
                {
                    Success = false,
                    Error = "短信功能暂未实现"
                });
            }
            return results;
        }

        public async Task<bool> ValidatePhoneNumberAsync(string phoneNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phoneNumber))
                    return false;

                // 简单手机号验证
                return phoneNumber.Length >= 10 && phoneNumber.Length <= 15;
            }
            catch
            {
                return false;
            }
        }
    }
}
