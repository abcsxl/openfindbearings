using DemandApi.Models;
using DemandApi.Models.DTOs;

namespace DemandApi.Services
{
    public interface IDemandMatchingService
    {
        Task<MatchResultResponse> MatchDemandToSuppliersAsync(long demandId, MatchStrategy strategy);
        Task<double> CalculateMatchScoreAsync(Demand demand, long supplierId);
        Task<List<long>> FindPotentialSuppliersAsync(Demand demand);
        Task<Dictionary<MatchReason, double>> AnalyzeMatchReasonsAsync(Demand demand, long supplierId);
    }
}
