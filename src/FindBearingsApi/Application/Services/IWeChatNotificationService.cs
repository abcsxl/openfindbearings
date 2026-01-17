using FindBearingsApi.Domain.Entities;

namespace FindBearingsApi.Application.Services
{
    public interface IWeChatNotificationService
    {
        /// <summary>
        /// 通用推送
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        Task SendPushAsync(string openid, string content);

        /// <summary>
        /// 专门给管理员推送
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SendToAdminAsync(Message message);
    }
}
