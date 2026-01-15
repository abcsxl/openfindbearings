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
        /// <summary>
        /// 序号
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 求购 or 供应
        /// </summary>
        [Required]
        public MessageType Type { get; set; } 
        /// <summary>
        /// 轴承型号，如 "6204"
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string BearingModel { get; set; } = string.Empty;
        /// <summary>
        /// 数量
        /// </summary>
        [Required]
        public int Quantity { get; set; }
        /// <summary>
        /// 补充说明
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; } 
        /// <summary>
        /// 联系方式（电话/微信）
        /// </summary>
        [MaxLength(50)]
        public string ContactInfo { get; set; } = string.Empty;
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTimeHelper.UtcNow();
        /// <summary>
        /// 是否已删除
        /// </summary>
        public bool IsDeleted { get; internal set; } = false;
        /// <summary>
        /// 删除时间（可为空）
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        ///////////////// 导航属性
        /// <summary>
        /// 发布者 ID
        /// </summary>
        [Required]
        public long UserId { get; set; }
        /// <summary>
        /// 发布者
        /// </summary>
        public User? User { get; set; }
        public ICollection<Interest> Interests { get; } = new List<Interest>();

    }
}
