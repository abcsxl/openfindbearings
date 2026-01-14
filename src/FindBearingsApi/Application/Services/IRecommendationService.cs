namespace FindBearingsApi.Application.Services
{
    public interface IRecommendationService
    {
        Task<List<long>> GetInterestedUserIdsAsync(string bearingModel);
    }
}
