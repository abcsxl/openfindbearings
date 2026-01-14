namespace FindBearingsApi.Domain.Entities
{
    public class Interest
    {
        public long UserId { get; set; }
        public long MessageId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 导航属性
        public User? User { get; set; }
        public Message? Message { get; set; }
    }
}
