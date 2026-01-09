using Dapr.Actors;
using OpenFindBearings.Shared.Dapr.Actors.Models;

namespace OpenFindBearings.Shared.Dapr.Actors;

/// <summary>
/// 库存 Actor 接口
/// 每个 Actor 实例对应一个库存项，确保并发安全的库存操作
/// </summary>
public interface IInventoryActor : IActor
{
    /// <summary>
    /// 获取库存状态
    /// </summary>
    Task<InventoryActorState> GetStateAsync();

    /// <summary>
    /// 增加库存
    /// </summary>
    Task<int> AddStockAsync(int quantity, string reason);

    /// <summary>
    /// 减少库存
    /// </summary>
    Task<bool> ReduceStockAsync(int quantity, string reason);

    /// <summary>
    /// 预留库存
    /// </summary>
    Task<bool> ReserveStockAsync(long orderId, int quantity, TimeSpan validFor);

    /// <summary>
    /// 确认订单（从预留中扣减）
    /// </summary>
    Task<bool> ConfirmOrderAsync(long orderId);

    /// <summary>
    /// 取消预留
    /// </summary>
    Task<bool> CancelReservationAsync(long orderId);

    /// <summary>
    /// 获取所有预留
    /// </summary>
    Task<List<StockReservation>> GetReservationsAsync();
}
