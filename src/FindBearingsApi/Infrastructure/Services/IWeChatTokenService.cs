namespace FindBearingsApi.Infrastructure.Services
{
    public interface IWeChatTokenService
    {
        Task<string> GetAccessTokenAsync();
    }
}
