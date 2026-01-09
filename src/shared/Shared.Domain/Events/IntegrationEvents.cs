namespace OpenFindBearings.Shared.Domain.Events;

/// <summary>
/// 库存变更事件
/// </summary>
public class StockChangedEvent : DomainEvent
{
    public long InventoryId { get; init; }
    public long CompanyId { get; init; }
    public long BearingModelId { get; init; }
    public int OldQuantity { get; init; }
    public int NewQuantity { get; init; }
    public string ChangeType { get; init; } = string.Empty; // In, Out, Adjust
    public decimal? Price { get; init; }

    /// <summary>
    /// 供应商邮箱
    /// </summary>
    public string? SupplierEmail { get; init; }

    /// <summary>
    /// 轴承型号
    /// </summary>
    public string? BearingModel { get; init; }

    /// <summary>
    /// 变更原因
    /// </summary>
    public string? Reason { get; init; }
}

/// <summary>
/// 新询价事件
/// </summary>
public class InquiryCreatedEvent : DomainEvent
{
    public long InquiryId { get; init; }
    public long BuyerCompanyId { get; init; }
    public long BearingModelId { get; init; }
    public string ModelNumber { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public bool IsUrgent { get; init; }
    public DateTime ValidUntil { get; init; }
    public string? Description { get; init; }

    /// <summary>
    /// 用户 ID
    /// </summary>
    public long UserId { get; init; }

    /// <summary>
    /// 轴承型号（用于通知）
    /// </summary>
    public string BearingModel => ModelNumber;
}

/// <summary>
/// 新报价事件
/// </summary>
public class QuotationCreatedEvent : DomainEvent
{
    public long QuotationId { get; init; }
    public long InquiryId { get; init; }
    public long SupplierCompanyId { get; init; }
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public int DeliveryPeriod { get; init; }
    public DateTime ExpiresAt { get; init; }

    /// <summary>
    /// 供应商名称
    /// </summary>
    public string SupplierName { get; init; } = string.Empty;

    /// <summary>
    /// 价格（别名）
    /// </summary>
    public decimal Price => UnitPrice;

    /// <summary>
    /// 交货天数
    /// </summary>
    public int DeliveryDays => DeliveryPeriod;

    /// <summary>
    /// 用户邮箱
    /// </summary>
    public string? UserEmail { get; init; }
}

/// <summary>
/// 匹配结果事件
/// </summary>
public class MatchCompletedEvent : DomainEvent
{
    public long InquiryId { get; init; }
    public List<MatchResult> Matches { get; init; } = new();

    /// <summary>
    /// 用户邮箱
    /// </summary>
    public string? UserEmail { get; init; }

    /// <summary>
    /// 匹配数量
    /// </summary>
    public int MatchCount => Matches.Count;

    /// <summary>
    /// 最佳价格
    /// </summary>
    public decimal? BestPrice => Matches.OrderBy(m => m.Price).FirstOrDefault()?.Price;

    /// <summary>
    /// 最快交期
    /// </summary>
    public int? BestDeliveryDays => Matches.Where(m => m.AvailableQuantity > 0).Min(m => m.DeliveryDays);
}

/// <summary>
/// 匹配结果
/// </summary>
public record MatchResult
{
    public long CompanyId { get; init; }
    public long InventoryId { get; init; }
    public decimal Score { get; init; }
    public int AvailableQuantity { get; init; }
    public decimal? Price { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// 交货天数
    /// </summary>
    public int DeliveryDays { get; init; }
}

/// <summary>
/// 订单确认事件
/// </summary>
public class OrderConfirmedEvent : DomainEvent
{
    public long OrderId { get; init; }
    public long InquiryId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
}

/// <summary>
/// 订单状态变更事件
/// </summary>
public class OrderStatusChangedEvent : DomainEvent
{
    public long OrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string OldStatus { get; init; } = string.Empty;
    public string NewStatus { get; init; } = string.Empty;
    public long BuyerCompanyId { get; init; }
    public long SupplierCompanyId { get; init; }
}

/// <summary>
/// 用户注册事件
/// </summary>
public class UserRegisteredEvent : DomainEvent
{
    public long UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? Email { get; init; }
}

/// <summary>
/// 企业认证申请事件
/// </summary>
public class CompanyCertificationAppliedEvent : DomainEvent
{
    public long CompanyId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string UnifiedSocialCreditCode { get; init; } = string.Empty;
}

/// <summary>
/// 企业认证审核完成事件
/// </summary>
public class CompanyCertificationAuditEvent : DomainEvent
{
    public long CompanyId { get; init; }
    public bool Approved { get; init; }
    public string? RejectReason { get; init; }
}

/// <summary>
/// 报价状态变更事件
/// </summary>
public class QuotationStatusChangedEvent : DomainEvent
{
    public long QuotationId { get; init; }
    public long InquiryId { get; init; }
    public string OldStatus { get; init; } = string.Empty;
    public string NewStatus { get; init; } = string.Empty;
    public long BuyerCompanyId { get; init; }
    public long SupplierCompanyId { get; init; }
}
