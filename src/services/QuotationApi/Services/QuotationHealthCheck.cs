using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using QuotationApi.Data;

namespace QuotationApi.Services
{
    public class QuotationHealthCheck : IHealthCheck
    {
        private readonly QuotationDbContext _context;
        private readonly ILogger<QuotationHealthCheck> _logger;

        public QuotationHealthCheck(QuotationDbContext context, ILogger<QuotationHealthCheck> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // 检查数据库连接
                var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
                if (!canConnect)
                {
                    _logger.LogWarning("数据库连接失败");
                    return HealthCheckResult.Unhealthy("数据库连接失败");
                }

                // 检查基本查询
                var quotationCount = await _context.Quotations.CountAsync(cancellationToken);
                _logger.LogInformation("健康检查通过，当前报价单数量: {Count}", quotationCount);

                return HealthCheckResult.Healthy("服务运行正常",
                    new Dictionary<string, object>
                    {
                        ["database"] = "connected",
                        ["quotations_count"] = quotationCount,
                        ["timestamp"] = DateTime.UtcNow
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "健康检查失败");
                return HealthCheckResult.Unhealthy("健康检查失败", ex);
            }
        }
    }
}
