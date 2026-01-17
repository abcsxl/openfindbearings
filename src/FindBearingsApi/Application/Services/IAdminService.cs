namespace FindBearingsApi.Application.Services
{
    public interface IAdminService
    {
        /// <summary>
        /// 获取所有管理员的微信 OpenId 列表（带缓存）
        /// </summary>
        Task<List<string>> GetAdminOpenIdsAsync();
    }
}
