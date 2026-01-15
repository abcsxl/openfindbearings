using FindBearingsApi.Application.Common;

namespace FindBearingsApi.Domain.Entities
{
    public enum UserRole
    {
        Member = 1,
        Admin = 2
    }

    public class User
    {
        public long Id { get; set; }
        public string OpenId { get; set; } = string.Empty; // 微信唯一标识
        public string Nickname { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Member; // 默认普通会员
        public DateTime CreatedAt { get; set; } = DateTimeHelper.UtcNow();

        // 导航属性
        public ICollection<Message> Messages { get; } = new List<Message>();
        public ICollection<Interest> Interests { get; } = new List<Interest>();
        public ICollection<Notification> Notifications { get; } = new List<Notification>();
    }
}