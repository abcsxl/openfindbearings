using Dapr.Client;
using Microsoft.Extensions.Options;
using NotificationApi.Data;
using NotificationApi.Models.Configuration;
using NotificationApi.Models.DTOs;
using NotificationApi.Models.Entities;

namespace NotificationApi.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger<NotificationService> _logger;
        private readonly DaprClient _daprClient;
        private readonly NotificationConfig _config;

        public NotificationService(
            INotificationRepository repository,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILogger<NotificationService> logger,
            DaprClient daprClient,
            IOptions<NotificationConfig> config)
        {
            _repository = repository;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = logger;
            _daprClient = daprClient;
            _config = config.Value;
        }

        public async Task<SendNotificationResult> SendNotificationAsync(SendNotificationRequest request)
        {
            try
            {
                _logger.LogInformation("发送通知: {Type} 给 {Recipient}", request.Type, request.Recipient);

                // 生成通知编号
                var notificationNumber = await GenerateNotificationNumberAsync();

                // 创建通知记录
                var notification = new Notification
                {
                    NotificationNumber = notificationNumber,
                    Type = request.Type,
                    Priority = request.Priority,
                    Status = NotificationStatus.Pending,
                    Recipient = request.Recipient,
                    RecipientName = request.RecipientName,
                    Subject = request.Subject,
                    Content = request.Content,
                    TemplateCode = request.TemplateCode,
                    RelatedEntityId = request.RelatedEntityId,
                    RelatedEntityType = request.RelatedEntityType,
                    Metadata = request.TemplateData != null ? System.Text.Json.JsonSerializer.Serialize(request.TemplateData) : null,
                    ScheduledAt = request.ScheduledAt,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var savedNotification = await _repository.AddNotificationAsync(notification);

                // 发送通知
                var sendResult = await SendNotificationInternalAsync(savedNotification);
                var result = new SendNotificationResult
                {
                    Success = sendResult.Success,
                    Message = sendResult.Message,
                    Error = sendResult.Error,
                    NotificationId = savedNotification.Id.ToString(),
                    SentAt = sendResult.SentAt
                };

                // 更新状态
                if (result.Success)
                {
                    savedNotification.Status = NotificationStatus.Sent;
                    savedNotification.SentAt = DateTime.UtcNow;
                }
                else
                {
                    savedNotification.Status = NotificationStatus.Failed;
                    savedNotification.ErrorMessage = result.Error;
                    savedNotification.RetryCount++;
                }

                await _repository.UpdateNotificationAsync(savedNotification);

                _logger.LogInformation("通知发送完成: {NotificationNumber}, 结果: {Success}", notificationNumber, result.Success);

                return new SendNotificationResult
                {
                    Success = result.Success,
                    Message = result.Message,
                    Error = result.Error,
                    NotificationId = savedNotification.Id.ToString(),
                    SentAt = result.SentAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送通知失败");
                return new SendNotificationResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<SendNotificationResult> SendNotificationByTemplateAsync(string templateCode, Dictionary<string, object> templateData, string recipient)
        {
            try
            {
                // 获取模板
                var template = await _repository.GetTemplateByCodeAsync(templateCode);
                if (template == null)
                    throw new ArgumentException($"模板不存在: {templateCode}");

                if (!template.IsActive)
                    throw new InvalidOperationException($"模板已停用: {templateCode}");

                // 渲染模板
                var subject = await RenderTemplateAsync(template.Subject, templateData);
                var content = await RenderTemplateAsync(template.Content, templateData);

                var request = new SendNotificationRequest
                {
                    Type = template.NotificationType,
                    Recipient = recipient,
                    Subject = subject,
                    Content = content,
                    TemplateCode = templateCode,
                    TemplateData = templateData
                };

                return await SendNotificationAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "通过模板发送通知失败: {TemplateCode}", templateCode);
                return new SendNotificationResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<List<SendNotificationResult>> SendBulkNotificationsAsync(List<SendNotificationRequest> requests)
        {
            var results = new List<SendNotificationResult>();

            foreach (var request in requests)
            {
                try
                {
                    var result = await SendNotificationAsync(request);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "批量发送通知失败");
                    results.Add(new SendNotificationResult
                    {
                        Success = false,
                        Error = ex.Message
                    });
                }
            }

            _logger.LogInformation("批量通知发送完成: 成功 {SuccessCount}, 失败 {FailedCount}",
                results.Count(r => r.Success),
                results.Count(r => !r.Success));

            return results;
        }

        public async Task<NotificationDetailResponse?> GetNotificationAsync(long id)
        {
            var notification = await _repository.GetNotificationByIdAsync(id);
            return notification == null ? null : MapToDetailResponse(notification);
        }

        public async Task<PagedResponse<NotificationResponse>> GetNotificationsAsync(NotificationQuery query)
        {
            var notifications = await _repository.GetNotificationsAsync(
                query.Search, query.Type, query.Status, query.StartDate, query.EndDate, query.Skip, query.PageSize);

            var totalCount = await _repository.GetNotificationsCountAsync(
                query.Search, query.Type, query.Status, query.StartDate, query.EndDate);

            return new PagedResponse<NotificationResponse>
            {
                Items = notifications.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<bool> MarkAsReadAsync(long notificationId)
        {
            return await _repository.UpdateNotificationStatusAsync(notificationId, NotificationStatus.Read);
        }

        public async Task<bool> DeleteNotificationAsync(long id)
        {
            return await _repository.DeleteNotificationAsync(id);
        }

        public async Task<TemplateResponse?> GetTemplateAsync(string code)
        {
            var template = await _repository.GetTemplateByCodeAsync(code);
            return template == null ? null : MapToTemplateResponse(template);
        }

        public async Task<string> RenderTemplateAsync(string template, Dictionary<string, object> data)
        {
            var result = template;
            foreach (var (key, value) in data)
            {
                result = result.Replace($"{{{{{key}}}}}", value?.ToString() ?? string.Empty);
            }
            return result;
        }

        public async Task HandleQuotationCreatedAsync(dynamic eventData)
        {
            try
            {
                var quotationId = (long)eventData.QuotationId;
                var supplierId = (long)eventData.SupplierId;
                var bearingNumber = (string)eventData.BearingNumber;

                await SendNotificationByTemplateAsync("QUOTATION_CREATED", new Dictionary<string, object>
                {
                    ["QuotationId"] = quotationId,
                    ["BearingNumber"] = bearingNumber,
                    ["CreatedAt"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")
                }, $"supplier_{supplierId}@example.com");

                _logger.LogInformation("处理报价创建事件成功: {QuotationId}", quotationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理报价创建事件失败");
            }
        }

        public async Task HandleOrderCreatedAsync(dynamic eventData)
        {
            try
            {
                var orderId = (long)eventData.OrderId;
                var customerId = (long)eventData.CustomerId;

                await SendNotificationByTemplateAsync("ORDER_CREATED", new Dictionary<string, object>
                {
                    ["OrderId"] = orderId,
                    ["CreatedAt"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")
                }, $"customer_{customerId}@example.com");

                _logger.LogInformation("处理订单创建事件成功: {OrderId}", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理订单创建事件失败");
            }
        }

        public async Task HandleOrderStatusChangedAsync(dynamic eventData)
        {
            try
            {
                var orderId = (long)eventData.OrderId;
                var newStatus = (string)eventData.NewStatus;
                var customerId = (long)eventData.CustomerId;

                var templateCode = newStatus.ToUpper() switch
                {
                    "PAID" => "ORDER_PAID",
                    "SHIPPED" => "ORDER_SHIPPED",
                    "COMPLETED" => "ORDER_COMPLETED",
                    _ => null
                };

                if (templateCode != null)
                {
                    await SendNotificationByTemplateAsync(templateCode, new Dictionary<string, object>
                    {
                        ["OrderId"] = orderId,
                        ["NewStatus"] = newStatus,
                        ["UpdatedAt"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")
                    }, $"customer_{customerId}@example.com");
                }

                _logger.LogInformation("处理订单状态变更事件成功: {OrderId}", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理订单状态变更事件失败");
            }
        }

        public async Task HandlePaymentProcessedAsync(dynamic eventData)
        {
            try
            {
                var orderId = (long)eventData.OrderId;
                var status = (string)eventData.Status;
                var customerId = (long)eventData.CustomerId;

                var templateCode = status == "SUCCESS" ? "PAYMENT_SUCCESS" : "PAYMENT_FAILED";

                await SendNotificationByTemplateAsync(templateCode, new Dictionary<string, object>
                {
                    ["OrderId"] = orderId,
                    ["Status"] = status,
                    ["ProcessedAt"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")
                }, $"customer_{customerId}@example.com");

                _logger.LogInformation("处理支付事件成功: {OrderId}", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理支付事件失败");
            }
        }

        public async Task<List<SendNotificationResult>> ProcessPendingNotificationsAsync(int batchSize = 100)
        {
            var pendingNotifications = await _repository.GetPendingNotificationsAsync(batchSize);
            var results = new List<SendNotificationResult>();

            _logger.LogInformation("开始处理 {Count} 个待发送通知", pendingNotifications.Count);

            foreach (var notification in pendingNotifications)
            {
                try
                {
                    var sendResult = await SendNotificationInternalAsync(notification);

                    // 创建 SendNotificationResult
                    var result = new SendNotificationResult
                    {
                        Success = sendResult.Success,
                        Message = sendResult.Message,
                        Error = sendResult.Error,
                        NotificationId = notification.Id.ToString(),
                        SentAt = sendResult.SentAt
                    };

                    results.Add(result);

                    // 更新状态
                    if (sendResult.Success)
                    {
                        notification.Status = NotificationStatus.Sent;
                        notification.SentAt = DateTime.UtcNow;
                    }
                    else
                    {
                        notification.Status = NotificationStatus.Failed;
                        notification.ErrorMessage = sendResult.Error;
                        notification.RetryCount++;
                    }

                    await _repository.UpdateNotificationAsync(notification);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "处理待发送通知失败: {NotificationId}", notification.Id);
                    results.Add(new SendNotificationResult
                    {
                        Success = false,
                        Error = ex.Message
                    });
                }
            }

            _logger.LogInformation("待发送通知处理完成: 成功 {SuccessCount}, 失败 {FailedCount}",
                results.Count(r => r.Success),
                results.Count(r => !r.Success));

            return results;
        }

        public async Task<List<SendNotificationResult>> RetryFailedNotificationsAsync(int maxRetryCount = 3)
        {
            var failedNotifications = await _repository.GetFailedNotificationsAsync(maxRetryCount);
            var results = new List<SendNotificationResult>();

            _logger.LogInformation("开始重试 {Count} 个失败通知", failedNotifications.Count);

            foreach (var notification in failedNotifications)
            {
                try
                {
                    var sendResult = await SendNotificationInternalAsync(notification);

                    // 修复这里：创建 SendNotificationResult
                    var result = new SendNotificationResult
                    {
                        Success = sendResult.Success,
                        Message = sendResult.Message,
                        Error = sendResult.Error,
                        NotificationId = notification.Id.ToString(),
                        SentAt = sendResult.SentAt
                    };

                    results.Add(result);

                    if (sendResult.Success)
                    {
                        notification.Status = NotificationStatus.Sent;
                        notification.SentAt = DateTime.UtcNow;
                    }
                    else
                    {
                        notification.RetryCount++;
                        notification.ErrorMessage = sendResult.Error;
                    }

                    notification.UpdatedAt = DateTime.UtcNow;
                    await _repository.UpdateNotificationAsync(notification);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "重试失败通知失败: {NotificationId}", notification.Id);
                    results.Add(new SendNotificationResult
                    {
                        Success = false,
                        Error = ex.Message
                    });
                }
            }

            return results;
        }

        public async Task<NotificationStatistics> GetStatisticsAsync()
        {
            // 获取统计信息
            var notifications = await _repository.GetNotificationsAsync();
            var totalCount = notifications.Count;
            var sentCount = notifications.Count(n => n.Status == NotificationStatus.Sent);
            var failedCount = notifications.Count(n => n.Status == NotificationStatus.Failed);
            var pendingCount = notifications.Count(n => n.Status == NotificationStatus.Pending);

            return new NotificationStatistics
            {
                TotalCount = totalCount,
                SentCount = sentCount,
                FailedCount = failedCount,
                PendingCount = pendingCount,
                SuccessRate = totalCount > 0 ? (double)sentCount / totalCount * 100 : 0
            };
        }

        // ========== 私有方法 ==========

        private async Task<SendResult> SendNotificationInternalAsync(Notification notification)
        {
            return notification.Type switch
            {
                NotificationType.Email => await SendEmailNotificationAsync(notification),
                NotificationType.Sms => await SendSmsNotificationAsync(notification),
                _ => new SendResult
                {
                    Success = false,
                    Error = $"不支持的发送类型: {notification.Type}"
                }
            };
        }

        private async Task<SendResult> SendEmailNotificationAsync(Notification notification)
        {
            try
            {
                var result = await _emailSender.SendEmailAsync(
                    notification.Recipient,
                    notification.RecipientName,
                    notification.Subject,
                    notification.Content
                );

                return new SendResult
                {
                    Success = result.Success,
                    Message = result.Message,
                    Error = result.Error,
                    SentAt = result.SentAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送邮件通知失败: {NotificationId}", notification.Id);
                return new SendResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private async Task<SendResult> SendSmsNotificationAsync(Notification notification)
        {
            try
            {
                var result = await _smsSender.SendSmsAsync(notification.Recipient, notification.Content);
                return new SendResult
                {
                    Success = result.Success,
                    Message = result.Message,
                    Error = result.Error,
                    SentAt = result.SentAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送短信通知失败: {NotificationId}", notification.Id);
                return new SendResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private async Task<string> GenerateNotificationNumberAsync()
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var count = await _repository.GetNotificationsCountAsync(startDate: DateTime.UtcNow.Date);
            return $"NT{today}{count + 1:0000}";
        }

        private NotificationResponse MapToResponse(Notification notification)
        {
            return new NotificationResponse
            {
                Id = notification.Id,
                NotificationNumber = notification.NotificationNumber,
                Type = notification.Type,
                Priority = notification.Priority,
                Status = notification.Status,
                Recipient = notification.Recipient,
                RecipientName = notification.RecipientName,
                Subject = notification.Subject,
                Content = notification.Content,
                TemplateCode = notification.TemplateCode,
                RetryCount = notification.RetryCount,
                ErrorMessage = notification.ErrorMessage,
                CreatedAt = notification.CreatedAt,
                SentAt = notification.SentAt,
                ReadAt = notification.ReadAt
            };
        }

        private NotificationDetailResponse MapToDetailResponse(Notification notification)
        {
            return new NotificationDetailResponse
            {
                Id = notification.Id,
                NotificationNumber = notification.NotificationNumber,
                Type = notification.Type,
                Priority = notification.Priority,
                Status = notification.Status,
                Recipient = notification.Recipient,
                RecipientName = notification.RecipientName,
                Subject = notification.Subject,
                Content = notification.Content,
                TemplateCode = notification.TemplateCode,
                RetryCount = notification.RetryCount,
                ErrorMessage = notification.ErrorMessage,
                CreatedAt = notification.CreatedAt,
                SentAt = notification.SentAt,
                ReadAt = notification.ReadAt,
                RelatedEntityId = notification.RelatedEntityId,
                RelatedEntityType = notification.RelatedEntityType,
                Metadata = notification.Metadata,
                DeliveredAt = notification.DeliveredAt,
                ScheduledAt = notification.ScheduledAt,
                UpdatedAt = notification.UpdatedAt
            };
        }

        private TemplateResponse MapToTemplateResponse(NotificationTemplate template)
        {
            return new TemplateResponse
            {
                Id = template.Id,
                Code = template.Code,
                Name = template.Name,
                Description = template.Description,
                NotificationType = template.NotificationType,
                Subject = template.Subject,
                Content = template.Content,
                Variables = template.Variables,
                IsActive = template.IsActive,
                Version = template.Version,
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt
            };
        }
    }
}
