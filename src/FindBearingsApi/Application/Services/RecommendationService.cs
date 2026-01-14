using FindBearingsApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FindBearingsApi.Application.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly AppDbContext _context;

        public RecommendationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<long>> GetInterestedUserIdsAsync(string bearingModel)
        {
            // 找出所有对 bearingModel 感兴趣的用户ID
            return await _context.Interests
                .Where(i =>
                    // 关联的消息未被删除
                    !i.Message!.IsDeleted &&
                    // 轴承型号匹配（不区分大小写）
                    EF.Functions.ILike(i.Message.BearingModel, bearingModel))
                .Select(i => i.UserId)
                .Distinct()
                .ToListAsync();
        }
    }
}
