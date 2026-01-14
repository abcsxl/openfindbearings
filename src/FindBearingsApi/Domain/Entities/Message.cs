using System.ComponentModel.DataAnnotations;

namespace FindBearingsApi.Domain.Entities
{
    public enum MessageType
    {
        /// <summary>
        /// 求购
        /// </summary>
        Demand = 1,
        /// <summary>
        /// 供应
        /// </summary>
        Supply = 2
    }

    public class Message
    {
        public long Id { get; set; }

        [Required]
        public MessageType Type { get; set; } // 求购 or 供应

        [Required]
        [MaxLength(100)]
        public string BearingModel { get; set; } = string.Empty; // 轴承型号，如 "6204"

        [Required]
        public int Quantity { get; set; } // 数量

        [MaxLength(500)]
        public string? Description { get; set; } // 补充说明

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // 创建时间
        public bool IsDeleted { get; internal set; } = false; // 是否已删除
        public DateTime? DeletedAt { get; set; } // 可为空的删除时间

        // 导航属性
        [Required]
        public long UserId { get; set; } // 发布者 ID
        public User? User { get; set; }
        public ICollection<Interest> Interests { get; } = new List<Interest>();

    }
}
