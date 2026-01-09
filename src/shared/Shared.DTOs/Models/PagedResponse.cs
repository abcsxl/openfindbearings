namespace OpenFindBearings.Shared.DTOs.Models;

/// <summary>
/// 分页请求模型
/// </summary>
public class PagedRequest
{
    private int _pageIndex = 1;
    private int _pageSize = 20;

    /// <summary>
    /// 页码（从 1 开始）
    /// </summary>
    public int PageIndex
    {
        get => _pageIndex;
        set => _pageIndex = value < 1 ? 1 : value;
    }

    /// <summary>
    /// 每页数量
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 20 : (value > 100 ? 100 : value);
    }

    /// <summary>
    /// 跳过的记录数
    /// </summary>
    public int Skip => (PageIndex - 1) * PageSize;

    /// <summary>
    /// 获取的记录数
    /// </summary>
    public int Take => PageSize;
}

/// <summary>
/// 分页响应模型
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class PagedResponse<T> : ApiResponse<List<T>>
{
    /// <summary>
    /// 当前页码
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// 每页数量
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总记录数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNextPage => PageIndex < TotalPages;

    /// <summary>
    /// 创建分页成功响应
    /// </summary>
    public static PagedResponse<T> SuccessResponse(
        List<T> data,
        int totalCount,
        int pageIndex,
        int pageSize,
        string message = "查询成功")
    {
        return new PagedResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// 创建空分页响应
    /// </summary>
    public static PagedResponse<T> EmptyResponse(int pageIndex = 1, int pageSize = 20, string message = "暂无数据")
    {
        return new PagedResponse<T>
        {
            Success = true,
            Message = message,
            Data = new List<T>(),
            TotalCount = 0,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }
}
