namespace OpenFindBearings.Shared.Dapr.Actors.Models;

/// <summary>
/// 库存 Actor 状态模型
/// 每个 Actor 实例对应一个库存项的状态
/// </summary>
public class InventoryActorState
{
    /// <summary>
    /// 库存项 ID
    /// </summary>
    public long InventoryId { get; set; }

    /// <summary>
    /// 总库存数量
    /// </summary>
    public int TotalQuantity { get; set; }

    /// <summary>
    /// 可用库存数量（未预留）
    /// </summary>
    public int AvailableQuantity { get; set; }

    /// <summary>
    /// 已预留库存数量
    /// </summary>
    public int ReservedQuantity { get; set; }

    /// <summary>
    /// 轴承型号
    /// </summary>
    public string? BearingModel { get; set; }

    /// <summary>
    /// 品牌
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// 活跃的库存预留列表
    /// </summary>
    public List<StockReservation> Reservations { get; set; } = new();
}

/// <summary>
/// 库存预留记录
/// </summary>
public class StockReservation
{
    /// <summary>
    /// 订单 ID
    /// </summary>
    public long OrderId { get; set; }

    /// <summary>
    /// 预留数量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 预留时间
    /// </summary>
    public DateTime ReservedAt { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 预留状态
    /// </summary>
    public ReservationStatus Status { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// 检查预留是否已过期
    /// </summary>
    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// 检查预留是否活跃
    /// </summary>
    public bool IsActive() => Status == ReservationStatus.Active && !IsExpired();
}

/// <summary>
/// 预留状态
/// </summary>
public enum ReservationStatus
{
    /// <summary>
    /// 活跃
    /// </summary>
    Active,

    /// <summary>
    /// 已确认（已扣减库存）
    /// </summary>
    Confirmed,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled,

    /// <summary>
    /// 已过期
    /// </summary>
    Expired
}
