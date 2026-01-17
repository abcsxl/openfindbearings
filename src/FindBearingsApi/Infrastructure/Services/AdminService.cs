using FindBearingsApi.Application.Services;
using FindBearingsApi.Domain.Entities;
using FindBearingsApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FindBearingsApi.Infrastructure.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        // 缓存键和过期时间
        private const string CacheKey = "AdminOpenIds";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1); // 缓存1小时

        public AdminService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<string>> GetAdminOpenIdsAsync()
        {
            // 1. 尝试从缓存获取
            if (_cache.TryGetValue(CacheKey, out List<string> cachedOpenIds))
            {
                return cachedOpenIds;
            }

            // 2. 缓存未命中，从数据库查询
            var openIds = await _context.Users
                .Where(u => u.Role == UserRole.Admin)
                .Where(u => !string.IsNullOrEmpty(u.OpenId)) // 确保 OpenId 不为空
                .Select(u => u.OpenId)
                .ToListAsync();

            // 3. 存入缓存并返回
            _cache.Set(CacheKey, openIds, CacheDuration);
            return openIds;
        }
        /// <summary>
        /// 手动清除缓存（可用于后台管理）
        /// </summary>
        public void RefreshCache()
        {
            _cache.Remove(CacheKey); 
        }
    }
}
