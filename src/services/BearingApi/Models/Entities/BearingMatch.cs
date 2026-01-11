namespace BearingApi.Models.Entities
{
    public class BearingMatch
    {
        public long Id { get; set; }
        public long DemandId { get; set; }  // 需求方ID
        public long SupplierId { get; set; }  // 供应商ID
        public double MatchScore { get; set; }  // 匹配分数
        public BearingMatchStatus Status { get; set; } = BearingMatchStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // 导航属性
        public virtual Bearing? Demand { get; set; }
        public virtual Bearing? Supplier { get; set; }
    }

    public enum BearingMatchStatus
    {
        Pending,
        Accepted,
        Rejected,
        Expired
    }
}
