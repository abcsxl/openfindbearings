namespace OpenFindBearings.Shared.Domain.Events;

/// <summary>
/// 领域事件基类
/// </summary>
public abstract class DomainEvent
{
    /// <summary>
    /// 事件发生时间
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// 事件类型名称
    /// </summary>
    public string EventType => GetType().Name;

    /// <summary>
    /// 事件唯一标识
    /// </summary>
    public Guid EventId => Guid.NewGuid();
}
