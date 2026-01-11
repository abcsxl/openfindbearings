namespace OrderApi.Models.Entities
{
    public enum OrderStatus
    {
        Created = 1,        // 已创建
        Paid = 2,           // 已支付  
        Shipped = 3,        // 已发货
        Completed = 4,      // 已完成
        Cancelled = 5,      // 已取消
        Refunded = 6        // 已退款
    }

    public enum PaymentStatus
    {
        Pending = 1,        // 待支付
        Paid = 2,           // 已支付
        Failed = 3,         // 支付失败
        Refunded = 4        // 已退款
    }

    public enum ShippingStatus
    {
        NotShipped = 1,     // 未发货
        Shipped = 2,        // 已发货
        Delivered = 3,      // 已送达
        Returned = 4        // 已退货
    }
}
