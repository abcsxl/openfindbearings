using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationApi.Models.Configuration;

namespace NotificationApi.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfig _emailConfig;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<NotificationConfig> config, ILogger<EmailSender> logger)
        {
            _emailConfig = config.Value.Email;
            _logger = logger;
        }

        public async Task<SendResult> SendEmailAsync(string to, string? toName, string subject, string body, List<string>? attachments = null)
        {
            try
            {
                if (!_emailConfig.Enabled)
                {
                    return new SendResult { Success = false, Error = "邮件发送功能已禁用" };
                }

                if (!await ValidateEmailAsync(to))
                {
                    return new SendResult { Success = false, Error = $"无效的邮箱地址: {to}" };
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailConfig.FromName, _emailConfig.FromAddress));
                message.To.Add(new MailboxAddress(toName ?? to, to));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                if (body.Contains("</") || body.Contains("<html"))
                {
                    bodyBuilder.HtmlBody = body;
                }
                else
                {
                    bodyBuilder.TextBody = body;
                }

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, _emailConfig.UseSsl);
                await client.AuthenticateAsync(_emailConfig.Username, _emailConfig.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("邮件发送成功: {To}, 主题: {Subject}", to, subject);

                return new SendResult
                {
                    Success = true,
                    Message = "邮件发送成功",
                    SentAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "邮件发送失败: {To}", to);
                return new SendResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<SendResult> SendEmailAsync(string to, string subject, string body)
        {
            return await SendEmailAsync(to, null, subject, body, null);
        }

        public async Task<List<SendResult>> SendBulkEmailAsync(List<EmailMessage> messages)
        {
            var results = new List<SendResult>();

            using var client = new SmtpClient();

            try
            {
                await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, _emailConfig.UseSsl);
                await client.AuthenticateAsync(_emailConfig.Username, _emailConfig.Password);

                foreach (var message in messages)
                {
                    try
                    {
                        var result = await SendSingleEmailAsync(client, message);
                        results.Add(result);
                        await Task.Delay(100);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "批量发送邮件失败: {To}", message.To);
                        results.Add(new SendResult { Success = false, Error = ex.Message });
                    }
                }

                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量邮件发送连接失败");
                foreach (var message in messages)
                {
                    results.Add(new SendResult { Success = false, Error = "SMTP连接失败" });
                }
            }

            return results;
        }

        public async Task<bool> ValidateEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return false;

                // 简单格式验证
                return email.Contains("@") && email.Contains(".");
            }
            catch
            {
                return false;
            }
        }

        private async Task<SendResult> SendSingleEmailAsync(SmtpClient client, EmailMessage message)
        {
            try
            {
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(_emailConfig.FromName, _emailConfig.FromAddress));
                mimeMessage.To.Add(new MailboxAddress(message.ToName ?? message.To, message.To));
                mimeMessage.Subject = message.Subject;

                var bodyBuilder = new BodyBuilder();
                if (message.IsHtml)
                {
                    bodyBuilder.HtmlBody = message.Body;
                }
                else
                {
                    bodyBuilder.TextBody = message.Body;
                }

                mimeMessage.Body = bodyBuilder.ToMessageBody();
                await client.SendAsync(mimeMessage);

                return new SendResult
                {
                    Success = true,
                    Message = "邮件发送成功",
                    SentAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "单封邮件发送失败: {To}", message.To);
                return new SendResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }
    }
}
