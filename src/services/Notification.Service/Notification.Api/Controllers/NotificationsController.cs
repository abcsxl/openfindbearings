using Dapr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenFindBearings.Shared.Domain.Events;

namespace OpenFindBearings.Notification.Api.Controllers;

/// <summary>
/// 通知控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(IMediator mediator, ILogger<NotificationsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// 发送测试通知
    /// </summary>
    [HttpPost("test")]
    public async Task<IActionResult> SendTestNotification([FromBody] TestNotificationRequest request)
    {
        _logger.LogInformation("发送测试通知: {Email}, {Message}", request.Email, request.Message);

        // 发布事件以进行测试
        await _mediator.Publish(new TestNotificationEvent
        {
            Email = request.Email,
            Message = request.Message,
            SentAt = DateTime.UtcNow
        });

        return Accepted(new { message = "通知已发送", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// 获取通知状态
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            service = "notification-service",
            status = "running",
            version = "1.0.0",
            daprEnabled = true,
            timestamp = DateTime.UtcNow
        });
    }
}

/// <summary>
/// 测试通知请求
/// </summary>
public record TestNotificationRequest
{
    public required string Email { get; init; }
    public required string Message { get; init; }
}

/// <summary>
/// 测试通知事件
/// </summary>
public class TestNotificationEvent : INotification
{
    public required string Email { get; set; }
    public required string Message { get; set; }
    public DateTime SentAt { get; set; }
}
