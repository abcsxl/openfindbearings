using Demand.Models;
using Demand.Models.DTOs;

namespace Demand.Services
{
    public interface IDemandMatchingService
    {
        Task<MatchResultResponse> MatchDemandToSuppliersAsync(long demandId, MatchStrategy strategy);
        Task<double> CalculateMatchScoreAsync(Models.Demand demand, long supplierId);
        Task<List<long>> FindPotentialSuppliersAsync(Models.Demand demand);
        Task<Dictionary<MatchReason, double>> AnalyzeMatchReasonsAsync(Models.Demand demand, long supplierId);
    }
}
