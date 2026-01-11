namespace QuotationApi.Models.Entities
{
    public enum QuotationStatus
    {
        Draft,          // 草稿
        Pending,        // 待审核
        Submitted,     // 已提交
        UnderReview,    // 审核中
        Approved,       // 已批准
        Rejected,       // 已拒绝
        Expired,        // 已过期
        Withdrawn,      // 已撤回
        Accepted,       // 已接受（客户接受）
        Cancelled       // 已取消
    }

    public enum QuotationType
    {
        Standard,       // 标准报价
        Urgent,         // 紧急报价
        Bulk,           // 批量报价
        Sample,         // 样品报价
        Custom          // 定制报价
    }

    public enum PriceTerm
    {
        EXW,            // 工厂交货
        FOB,            // 离岸价
        CIF,            // 到岸价
        CFR,            // 成本加运费
        DDP             // 完税后交货
    }

    public enum PaymentTerm
    {
        T30,            // 30天账期
        T60,            // 60天账期
        Advance,        // 预付款
        LC,             // 信用证
        DP,             // 付款交单
        CAD             // 凭单付现
    }

    public enum QuotationPriority
    {
        Low,            // 低优先级
        Normal,         // 普通
        High,           // 高优先级
        Urgent          // 紧急
    }
}
