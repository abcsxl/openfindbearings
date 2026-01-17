using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FindBearingsApi.Infrastructure.Services
{
    public class WeChatTokenOptions
    {
        public string AppId { get; set; } = string.Empty;
        public string AppSecret { get; set; } = string.Empty;
    }

    public class WeChatTokenService : IWeChatTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly WeChatTokenOptions _options;
        private readonly IMemoryCache _cache;
        private const string TokenCacheKey = "WeChatAccessToken";

        // 微信 access_token 有效期 2 小时，我们缓存 1.5 小时
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(90);

        public WeChatTokenService(
            HttpClient httpClient,
            IOptions<WeChatTokenOptions> options,
            IMemoryCache cache)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _cache = cache;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (_cache.TryGetValue(TokenCacheKey, out string? token))
                return token!;

            // 调用微信接口获取新 token
            var url = $"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={_options.AppId}&secret={_options.AppSecret}";
            using var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var accessToken = doc.RootElement.GetProperty("access_token").GetString()!;

            _cache.Set(TokenCacheKey, accessToken, CacheDuration);
            return accessToken;
        }
    }
}
