using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenFindBearings.Shared.Domain.Events;

namespace OpenFindBearings.Shared.Infrastructure.Messaging;

/// <summary>
/// MassTransit 消息总线配置扩展
/// </summary>
public static class MassTransitConfiguration
{
    /// <summary>
    /// 添加 MassTransit 和 RabbitMQ 配置
    /// </summary>
    public static IServiceCollection AddMassTransitWithRabbitMQ(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configure = null)
    {
        services.AddMassTransit(x =>
        {
            // 配置默认消费者
            configure?.Invoke(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqConfig = configuration.GetSection("RabbitMQ");

                var host = rabbitMqConfig["Host"] ?? "localhost";
                var username = rabbitMqConfig["Username"] ?? "admin";
                var password = rabbitMqConfig["Password"] ?? "admin";

                cfg.Host(host, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                // 配置消息序列化
                cfg.UseJsonSerializer();

                // 配置重试策略
                cfg.UseMessageRetry(r =>
                {
                    r.Interval(3, TimeSpan.FromSeconds(5));
                });

                // 配置端点
                cfg.ConfigureEndpoints(context);
            });
        });

        // 添加 MassTransit hosted service
        services.AddMassTransitHostedService();

        return services;
    }

    /// <summary>
    /// 配置消息发布
    /// </summary>
    public static void ConfigureMessagePublish(this IBusRegistrationConfigurator configurator)
    {
        // 注册所有集成事件
        configurator.AddConsumers(typeof(Shared.Infrastructure.Messaging.MassTransitConfiguration).Assembly);
    }
}

/// <summary>
/// 消息发布接口
/// </summary>
public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
}

/// <summary>
/// 消息发布实现
/// </summary>
public class MessagePublisher : IMessagePublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MessagePublisher> _logger;

    public MessagePublisher(IPublishEndpoint publishEndpoint, ILogger<MessagePublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            await _publishEndpoint.Publish(message, cancellationToken);
            _logger.LogInformation("消息发布成功: {MessageType}", typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "消息发布失败: {MessageType}", typeof(T).Name);
            throw;
        }
    }
}
