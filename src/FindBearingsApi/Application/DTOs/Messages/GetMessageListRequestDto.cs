using FindBearingsApi.Domain.Entities;

namespace FindBearingsApi.Application.DTOs.Messages
{
    public class GetMessageListRequestDto
    {
        public string? Model { get; set; } // 轴承型号关键词
        public MessageType? Type { get; set; } // 类型筛选
        public int Page { get; set; } = 1;   // 页码（从1开始）
        public int PageSize { get; set; } = 10; // 每页数量（最大20）
    }
}
