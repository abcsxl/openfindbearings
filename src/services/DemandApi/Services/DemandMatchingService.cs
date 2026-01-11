using Dapr.Client;
using DemandApi.Data;
using DemandApi.Models;
using DemandApi.Models.DTOs;

namespace DemandApi.Services
{
    public class DemandMatchingService : IDemandMatchingService
    {
        private readonly ILogger<DemandMatchingService> _logger;
        private readonly DaprClient _daprClient;
        private readonly DemandDbContext _context;

        public DemandMatchingService(
            ILogger<DemandMatchingService> logger,
            DaprClient daprClient,
            DemandDbContext context)
        {
            _logger = logger;
            _daprClient = daprClient;
            _context = context;
        }

        public async Task<MatchResultResponse> MatchDemandToSuppliersAsync(long demandId, MatchStrategy strategy)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("开始匹配需求: {DemandId}, 策略: {Strategy}", demandId, strategy);

            var demand = await _context.Demands.FindAsync(demandId);
            if (demand == null)
            {
                throw new KeyNotFoundException($"需求 {demandId} 不存在");
            }

            // 查找潜在供应商
            var potentialSupplierIds = await FindPotentialSuppliersAsync(demand);
            _logger.LogInformation("找到 {Count} 个潜在供应商", potentialSupplierIds.Count);

            var matches = new List<DemandMatch>();
            var supplierMatches = new List<SupplierMatch>();

            foreach (var supplierId in potentialSupplierIds)
            {
                try
                {
                    var matchScore = await CalculateMatchScoreAsync(demand, supplierId);
                    var matchReasons = await AnalyzeMatchReasonsAsync(demand, supplierId);

                    if (matchScore > 0) // 只添加匹配分数大于0的供应商
                    {
                        // 获取供应商信息
                        var supplierInfo = await GetSupplierInfoAsync(supplierId);

                        var demandMatch = new DemandMatch
                        {
                            DemandId = demandId,
                            SupplierId = supplierId,
                            SupplierName = supplierInfo?.Name ?? "未知供应商",
                            MatchScore = matchScore,
                            MatchReason = GetPrimaryMatchReason(matchReasons),
                            MatchDetails = System.Text.Json.JsonSerializer.Serialize(matchReasons),
                            IsNotified = false,
                            HasResponded = false,
                            IsInterested = false
                        };

                        matches.Add(demandMatch);

                        supplierMatches.Add(new SupplierMatch
                        {
                            SupplierId = supplierId,
                            SupplierName = supplierInfo?.Name ?? "未知供应商",
                            MatchScore = matchScore,
                            PrimaryReason = GetPrimaryMatchReason(matchReasons),
                            Strengths = GetMatchStrengths(matchReasons),
                            IsNotified = false
                        });

                        _logger.LogDebug("供应商 {SupplierId} 匹配分数: {Score}", supplierId, matchScore);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "计算供应商 {SupplierId} 匹配分数时出错", supplierId);
                }
            }

            // 按匹配分数排序
            matches = matches.OrderByDescending(m => m.MatchScore).ToList();
            supplierMatches = supplierMatches.OrderByDescending(m => m.MatchScore).ToList();

            // 保存匹配结果到数据库
            await SaveMatchesAsync(matches);

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            _logger.LogInformation("需求匹配完成: {DemandId}, 匹配供应商数: {Count}, 耗时: {Duration}ms",
                demandId, matches.Count, duration.TotalMilliseconds);

            return new MatchResultResponse
            {
                DemandId = demandId,
                TotalSuppliersMatched = matches.Count,
                TotalSuppliersNotified = 0, // 初始化为0，通知是异步过程
                Matches = supplierMatches,
                MatchingDuration = duration,
                MatchedAt = endTime
            };
        }

        public async Task<double> CalculateMatchScoreAsync(Demand demand, long supplierId)
        {
            double totalScore = 0;
            int factorCount = 0;

            // 1. 产品匹配度 (40%)
            var productMatchScore = await CalculateProductMatchScoreAsync(demand, supplierId);
            totalScore += productMatchScore * 0.4;
            factorCount++;

            // 2. 价格竞争力 (30%)
            var priceMatchScore = await CalculatePriceMatchScoreAsync(demand, supplierId);
            totalScore += priceMatchScore * 0.3;
            factorCount++;

            // 3. 地理位置 (15%)
            var locationScore = await CalculateLocationScoreAsync(demand, supplierId);
            totalScore += locationScore * 0.15;
            factorCount++;

            // 4. 供应商信誉 (15%)
            var reputationScore = await CalculateReputationScoreAsync(supplierId);
            totalScore += reputationScore * 0.15;
            factorCount++;

            // 计算平均分
            return factorCount > 0 ? totalScore : 0;
        }

        public async Task<List<long>> FindPotentialSuppliersAsync(Demand demand)
        {
            // 这里应该调用Supplier服务来查找有相关产品的供应商
            // 简化版本：返回示例供应商ID列表
            try
            {
                // 通过Dapr调用Supplier服务
                var supplierIds = await _daprClient.InvokeMethodAsync<List<long>>(
                    HttpMethod.Get,
                    "supplier",
                    $"api/suppliers/search?bearingNumber={demand.BearingNumber}&brand={demand.Brand}");

                return supplierIds ?? new List<long>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用Supplier服务失败，返回示例供应商列表");
                // 开发环境：返回示例数据
                return new List<long> { 1, 2, 3, 4, 5 };
            }
        }

        public async Task<Dictionary<MatchReason, double>> AnalyzeMatchReasonsAsync(Demand demand, long supplierId)
        {
            var reasons = new Dictionary<MatchReason, double>();

            // 分析各种匹配原因及其权重
            var productMatch = await CalculateProductMatchScoreAsync(demand, supplierId);
            if (productMatch > 0.7) reasons[MatchReason.ExactMatch] = productMatch;
            else if (productMatch > 0.4) reasons[MatchReason.SimilarProduct] = productMatch;

            var priceMatch = await CalculatePriceMatchScoreAsync(demand, supplierId);
            if (priceMatch > 0.8) reasons[MatchReason.PriceAdvantage] = priceMatch;

            var locationScore = await CalculateLocationScoreAsync(demand, supplierId);
            if (locationScore > 0.7) reasons[MatchReason.LocationProximity] = locationScore;

            var reputationScore = await CalculateReputationScoreAsync(supplierId);
            if (reputationScore > 0.6) reasons[MatchReason.HistoricalCooperation] = reputationScore;

            return reasons;
        }

        #region 私有方法
        private async Task<double> CalculateProductMatchScoreAsync(Demand demand, long supplierId)
        {
            try
            {
                // 调用Supplier服务检查是否有匹配的产品
                var hasProduct = await _daprClient.InvokeMethodAsync<bool>(
                    HttpMethod.Get,
                    "supplier",
                    $"api/suppliers/{supplierId}/products/exists?bearingNumber={demand.BearingNumber}");

                return hasProduct ? 0.9 : 0.1; // 简化实现
            }
            catch
            {
                return 0.5; // 默认值
            }
        }

        private async Task<double> CalculatePriceMatchScoreAsync(Demand demand, long supplierId)
        {
            try
            {
                // 获取供应商产品价格
                var price = await _daprClient.InvokeMethodAsync<decimal?>(
                    HttpMethod.Get,
                    "supplier",
                    $"api/suppliers/{supplierId}/products/price?bearingNumber={demand.BearingNumber}");

                if (price == null) return 0.3;

                // 计算价格匹配度
                if (demand.MaxPrice.HasValue && price <= demand.MaxPrice.Value)
                {
                    if (demand.MinPrice.HasValue)
                    {
                        // 价格在范围内
                        if (price >= demand.MinPrice.Value)
                            return 0.9;
                        else
                            return 0.7; // 价格低于最低价，可能质量有问题
                    }
                    return 0.8; // 价格在最高价以下
                }
                return 0.3; // 价格超出范围
            }
            catch
            {
                return 0.5;
            }
        }

        private async Task<double> CalculateLocationScoreAsync(Demand demand, long supplierId)
        {
            try
            {
                // 获取供应商位置
                var supplierInfo = await GetSupplierInfoAsync(supplierId);
                if (supplierInfo == null || string.IsNullOrEmpty(supplierInfo.City))
                    return 0.5;

                // 简化实现：同城市得高分
                if (!string.IsNullOrEmpty(demand.DeliveryAddress) &&
                    demand.DeliveryAddress.Contains(supplierInfo.City, StringComparison.OrdinalIgnoreCase))
                    return 0.9;

                return 0.5;
            }
            catch
            {
                return 0.5;
            }
        }

        private async Task<double> CalculateReputationScoreAsync(long supplierId)
        {
            try
            {
                // 获取供应商评分
                var rating = await _daprClient.InvokeMethodAsync<double?>(
                    HttpMethod.Get,
                    "supplier",
                    $"api/suppliers/{supplierId}/rating");

                if (rating == null) return 0.5;
                return Math.Min(rating.Value / 5.0, 1.0); // 转换为0-1的分数
            }
            catch
            {
                return 0.5;
            }
        }

        private async Task<SupplierInfo?> GetSupplierInfoAsync(long supplierId)
        {
            try
            {
                return await _daprClient.InvokeMethodAsync<SupplierInfo>(
                    HttpMethod.Get,
                    "supplier",
                    $"api/suppliers/{supplierId}/basic-info");
            }
            catch
            {
                return null;
            }
        }

        private async Task SaveMatchesAsync(List<DemandMatch> matches)
        {
            if (matches.Count == 0) return;

            foreach (var match in matches)
            {
                _context.DemandMatches.Add(match);
            }

            await _context.SaveChangesAsync();
        }

        private MatchReason GetPrimaryMatchReason(Dictionary<MatchReason, double> reasons)
        {
            if (reasons.Count == 0) return MatchReason.ExactMatch;

            return reasons.OrderByDescending(r => r.Value)
                         .First().Key;
        }

        private List<string> GetMatchStrengths(Dictionary<MatchReason, double> reasons)
        {
            var strengths = new List<string>();
            foreach (var reason in reasons.Where(r => r.Value > 0.6))
            {
                strengths.Add(reason.Key.ToString());
            }
            return strengths;
        }
        #endregion
    }

    // 辅助类
    public class SupplierInfo
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Country { get; set; }
        public double? Rating { get; set; }
    }
}
