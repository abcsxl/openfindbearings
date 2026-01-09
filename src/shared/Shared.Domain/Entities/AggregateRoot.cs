namespace OpenFindBearings.Shared.Domain.Entities;

/// <summary>
/// 聚合根基类
/// </summary>
/// <typeparam name="TId">聚合根标识类型</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
{
    /// <summary>
    /// 聚合根的版本号，用于乐观并发控制
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// 增加版本号
    /// </summary>
    protected void IncrementVersion()
    {
        Version++;
    }
}
