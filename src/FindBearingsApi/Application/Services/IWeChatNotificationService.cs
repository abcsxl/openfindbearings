namespace FindBearingsApi.Application.Services
{
    public interface IWeChatNotificationService
    {
        Task SendPushAsync(string openid, string content);
    }
}
