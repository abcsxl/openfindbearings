namespace FindBearingsApi.Application.Common
{
    public static class PagingHelper
    {
        public static (int Page, int PageSize) Normalize(int page, int pageSize, int maxPageSize = 50)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, maxPageSize);
            return (page, pageSize);
        }
    }
}
