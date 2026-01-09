using OpenFindBearings.Shared.Domain.Events;

namespace OpenFindBearings.Shared.Domain.Entities;

/// <summary>
/// 实体基类
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// 获取当前实体的领域事件
    /// </summary>
    private readonly List<DomainEvent> _domainEvents = new();

    /// <summary>
    /// 获取只读领域事件集合
    /// </summary>
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// 添加领域事件
    /// </summary>
    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// 移除领域事件
    /// </summary>
    protected void RemoveDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// 清除所有领域事件
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

/// <summary>
/// 带有 Id 的实体基类
/// </summary>
/// <typeparam name="TId">Id 类型</typeparam>
public abstract class Entity<TId> : Entity
{
    /// <summary>
    /// 实体唯一标识
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <summary>
    /// 判断两个实体是否相等
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (Id is null || other.Id is null)
            return false;

        return Id.Equals(other.Id);
    }

    /// <summary>
    /// 获取哈希码
    /// </summary>
    public override int GetHashCode()
    {
        return Id?.GetHashCode() ?? 0;
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }
}
