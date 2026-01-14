namespace FindBearingsApi.Application.Common
{
    /// <summary>
    /// 分页响应包装
    /// </summary>
    public class PagedResponse<T>
    {
        /// <summary>
        /// 数据列表
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// 当前页码（从1开始）
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总记录数
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool HasPrevious => PageNumber > 1;

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNext => PageNumber < TotalPages;
    }
}
