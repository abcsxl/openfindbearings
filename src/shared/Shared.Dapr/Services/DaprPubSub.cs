using Dapr.Client;
using Microsoft.Extensions.Logging;
using OpenFindBearings.Shared.Domain.Events;

namespace OpenFindBearings.Shared.Dapr.Services;

/// <summary>
/// Dapr 发布订阅封装
/// </summary>
public interface IDaprPubSub
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : DomainEvent;
    Task PublishAsync<T>(string topic, T @event, CancellationToken cancellationToken = default) where T : class;
}

public class DaprPubSub : IDaprPubSub
{
    private const string PubSubName = "pubsub";
    private readonly DaprClient _daprClient;
    private readonly ILogger<DaprPubSub> _logger;

    public DaprPubSub(DaprClient daprClient, ILogger<DaprPubSub> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : DomainEvent
    {
        // 根据事件类型自动推断 topic 名称
        var topicName = ConvertEventNameToTopic(@event.EventType);
        await PublishAsync(topicName, @event, cancellationToken);
    }

    public async Task PublishAsync<T>(string topic, T @event, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            _logger.LogInformation("发布事件到主题 {Topic}: {EventType}", topic, typeof(T).Name);

            await _daprClient.PublishEventAsync(
                PubSubName,
                topic,
                @event,
                cancellationToken);

            _logger.LogDebug("事件发布成功: {EventType}, Topic: {Topic}", typeof(T).Name, topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "事件发布失败: {EventType}, Topic: {Topic}", typeof(T).Name, topic);
            throw;
        }
    }

    private string ConvertEventNameToTopic(string eventType)
    {
        // StockChangedEvent -> stock-changed
        // QuotationCreatedEvent -> quotation-created
        // InquiryCreatedEvent -> inquiry-created
        var result = eventType
            .Replace("Event", "")
            .Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x.ToString() : x.ToString())
            .Aggregate(string.Concat)
            .ToLower();

        return result;
    }
}
