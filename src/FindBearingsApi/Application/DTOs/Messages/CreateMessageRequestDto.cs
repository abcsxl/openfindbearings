using FindBearingsApi.Domain.Entities;

namespace FindBearingsApi.Application.DTOs.Messages
{
    public class CreateMessageRequestDto
    {
        public MessageType Type { get; set; } = MessageType.Demand;
        public string BearingModel { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public string? Description { get; set; }
    }
}
