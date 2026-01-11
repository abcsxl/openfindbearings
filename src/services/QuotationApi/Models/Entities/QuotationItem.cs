using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuotationApi.Models.Entities
{
    public class QuotationItem
    {
        public long Id { get; set; }
        public long QuotationId { get; set; }

        [Required]
        [StringLength(100)]
        public string BearingNumber { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice => UnitPrice * Quantity;  // 计算属性，只读

        [StringLength(100)]
        public string? Brand { get; set; }

        [StringLength(100)]
        public string? Material { get; set; }

        [StringLength(50)]
        public string? Standard { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // 添加 CreatedAt

        public virtual Quotation Quotation { get; set; } = null!;
    }
}
