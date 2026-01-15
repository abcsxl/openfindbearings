using System.Text.RegularExpressions;

namespace FindBearingsApi.Application.Common
{
    public static class BearingModelValidator
    {
        public static bool IsValid(string model) => Regex.IsMatch(model, @"^[A-Z0-9\-]+$");
    }

    public static class IdGenerator
    {
        public static string GenerateMessageId() => $"MSG_{Guid.NewGuid().ToString("N")[..8]}";
    }
}
