using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderApi.Data;

namespace OrderApi.Services
{
    public class OrderHealthCheck : IHealthCheck
    {
        private readonly OrderDbContext _context;
        private readonly ILogger<OrderHealthCheck> _logger;

        public OrderHealthCheck(OrderDbContext context, ILogger<OrderHealthCheck> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
                if (!canConnect)
                {
                    _logger.LogWarning("数据库连接失败");
                    return HealthCheckResult.Unhealthy("数据库连接失败");
                }

                var orderCount = await _context.Orders.CountAsync(cancellationToken);
                _logger.LogInformation("健康检查通过，当前订单数量: {Count}", orderCount);

                return HealthCheckResult.Healthy("服务运行正常",
                    new Dictionary<string, object>
                    {
                        ["database"] = "connected",
                        ["orders_count"] = orderCount,
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
