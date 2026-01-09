using Dapr.Actors;
using OpenFindBearings.Shared.Dapr.Actors.Models;

namespace OpenFindBearings.Shared.Dapr.Actors;

/// <summary>
/// 询价匹配 Actor 接口
/// 每个 Actor 实例对应一个询价请求，负责异步匹配供应商
/// </summary>
public interface IInquiryMatchActor : IActor
{
    /// <summary>
    /// 获取匹配状态
    /// </summary>
    Task<InquiryMatchActorState> GetStateAsync();

    /// <summary>
    /// 开始匹配流程
    /// </summary>
    Task<bool> StartMatchingAsync(InquiryRequest inquiry);

    /// <summary>
    /// 添加候选供应商
    /// </summary>
    Task AddCandidateAsync(MatchCandidate candidate);

    /// <summary>
    /// 批量添加候选供应商
    /// </summary>
    Task AddCandidatesAsync(List<MatchCandidate> candidates);

    /// <summary>
    /// 获取所有候选供应商
    /// </summary>
    Task<List<MatchCandidate>> GetCandidatesAsync();

    /// <summary>
    /// 选择最佳匹配
    /// </summary>
    Task<MatchCandidate?> SelectBestMatchAsync();

    /// <summary>
    /// 完成匹配（选择供应商）
    /// </summary>
    Task<bool> CompleteMatchAsync(long supplierId, string reason);

    /// <summary>
    /// 取消匹配
    /// </summary>
    Task<bool> CancelMatchAsync(string reason);

    /// <summary>
    /// 更新匹配进度
    /// </summary>
    Task UpdateProgressAsync(MatchProgress progress);

    /// <summary>
    /// 设置匹配超时
    /// </summary>
    Task SetTimeoutAsync(TimeSpan timeout);
}
