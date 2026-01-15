using FindBearingsApi.Application.Common;

namespace FindBearingsApi.Domain.Entities
{
    public class Notification
    {
        public long Id { get; set; }
        public long UserId { get; set; }          // 接收者
        public long MessageId { get; set; }       // 关联的消息
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTimeHelper.UtcNow();

        // 导航属性
        public Message? Message { get; set; }    
        public User? User { get; set; }
    }
}
