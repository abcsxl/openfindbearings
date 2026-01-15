using FindBearingsApi.Domain.Entities;

namespace FindBearingsApi.Application.Common
{
    public static class MessageTypeExtensions
    {
        public static string GetDisplayName(this MessageType type) => type switch
        {
            MessageType.Supply => "供应",
            MessageType.Demand => "求购",
            _ => type.ToString()
        };
    }
}
