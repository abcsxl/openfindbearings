namespace BearingApi.Models.Entities
{
    public class BearingView
    {
        public long Id { get; set; }
        public long DemandId { get; set; }  // 被查看的轴承需求ID
        public long ViewerId { get; set; }  // 查看者ID
        public string? ViewerType { get; set; }  // 查看者类型
        public string? ViewerIp { get; set; }  // 查看者IP
        public string? UserAgent { get; set; }  // 用户代理
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

        // 导航属性
        public virtual Bearing? Demand { get; set; }
    }
}
