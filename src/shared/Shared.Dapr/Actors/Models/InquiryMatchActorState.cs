namespace OpenFindBearings.Shared.Dapr.Actors.Models;

/// <summary>
/// 询价匹配 Actor 状态模型
/// 每个 Actor 实例对应一个询价请求的匹配状态
/// </summary>
public class InquiryMatchActorState
{
    /// <summary>
    /// 询价 ID
    /// </summary>
    public long InquiryId { get; set; }

    /// <summary>
    /// 用户 ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 询价请求详情
    /// </summary>
    public InquiryRequest? Inquiry { get; set; }

    /// <summary>
    /// 匹配状态
    /// </summary>
    public MatchStatus Status { get; set; }

    /// <summary>
    /// 候选供应商列表
    /// </summary>
    public List<MatchCandidate> Candidates { get; set; } = new();

    /// <summary>
    /// 选中的供应商
    /// </summary>
    public MatchCandidate? SelectedCandidate { get; set; }

    /// <summary>
    /// 匹配开始时间
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// 匹配完成时间
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 匹配进度
    /// </summary>
    public MatchProgress Progress { get; set; }

    /// <summary>
    /// 匹配结果备注
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 检查是否已过期
    /// </summary>
    public bool IsExpired() => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

    /// <summary>
    /// 检查是否正在匹配中
    /// </summary>
    public bool IsMatching() => Status == MatchStatus.Matching && !IsExpired();

    /// <summary>
    /// 检查是否已完成
    /// </summary>
    public bool IsCompleted() => Status == MatchStatus.Completed ||
                                  Status == MatchStatus.Failed ||
                                  Status == MatchStatus.Cancelled;
}

/// <summary>
/// 询价请求
/// </summary>
public class InquiryRequest
{
    /// <summary>
    /// 轴承型号
    /// </summary>
    public string BearingModel { get; set; } = string.Empty;

    /// <summary>
    /// 品牌
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 期望价格
    /// </summary>
    public decimal? ExpectedPrice { get; set; }

    /// <summary>
    /// 期望交期（天数）
    /// </summary>
    public int? ExpectedDeliveryDays { get; set; }

    /// <summary>
    /// 质量要求
    /// </summary>
    public string? QualityRequirements { get; set; }

    /// <summary>
    /// 其他要求
    /// </summary>
    public string? AdditionalRequirements { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 匹配候选供应商
/// </summary>
public class MatchCandidate
{
    /// <summary>
    /// 供应商 ID
    /// </summary>
    public long SupplierId { get; set; }

    /// <summary>
    /// 供应商名称
    /// </summary>
    public string? SupplierName { get; set; }

    /// <summary>
    /// 库存 ID
    /// </summary>
    public long InventoryId { get; set; }

    /// <summary>
    /// 可用数量
    /// </summary>
    public int AvailableQuantity { get; set; }

    /// <summary>
    /// 单价
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 匹配分数（0-100）
    /// </summary>
    public double MatchScore { get; set; }

    /// <summary>
    /// 信用评分
    /// </summary>
    public double? CreditRating { get; set; }

    /// <summary>
    /// 交期（天数）
    /// </summary>
    public int? DeliveryDays { get; set; }

    /// <summary>
    /// 匹配原因
    /// </summary>
    public string? MatchReason { get; set; }

    /// <summary>
    /// 发现时间
    /// </summary>
    public DateTime DiscoveredAt { get; set; }

    /// <summary>
    /// 是否已通知
    /// </summary>
    public bool IsNotified { get; set; }
}

/// <summary>
/// 匹配状态
/// </summary>
public enum MatchStatus
{
    /// <summary>
    /// 等待开始
    /// </summary>
    Pending,

    /// <summary>
    /// 正在匹配
    /// </summary>
    Matching,

    /// <summary>
    /// 已完成
    /// </summary>
    Completed,

    /// <summary>
    /// 失败
    /// </summary>
    Failed,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled,

    /// <summary>
    /// 已超时
    /// </summary>
    Timeout
}

/// <summary>
/// 匹配进度
/// </summary>
public class MatchProgress
{
    /// <summary>
    /// 已搜索供应商数量
    /// </summary>
    public int SearchedCount { get; set; }

    /// <summary>
    /// 已找到候选数量
    /// </summary>
    public int FoundCount { get; set; }

    /// <summary>
    /// 已通知数量
    /// </summary>
    public int NotifiedCount { get; set; }

    /// <summary>
    /// 当前阶段
    /// </summary>
    public MatchStage Stage { get; set; }

    /// <summary>
    /// 进度百分比 (0-100)
    /// </summary>
    public int Percentage { get; set; }

    /// <summary>
    /// 当前阶段描述
    /// </summary>
    public string? StageDescription { get; set; }
}

/// <summary>
/// 匹配阶段
/// </summary>
public enum MatchStage
{
    /// <summary>
    /// 初始化
    /// </summary>
    Initializing,

    /// <summary>
    /// 搜索库存
    /// </summary>
    SearchingInventory,

    /// <summary>
    /// 评分排序
    /// </summary>
    ScoringCandidates,

    /// <summary>
    /// 通知供应商
    /// </summary>
    NotifyingSuppliers,

    /// <summary>
    /// 等待响应
    /// </summary>
    WaitingForResponse,

    /// <summary>
    /// 完成
    /// </summary>
    Completed
}
