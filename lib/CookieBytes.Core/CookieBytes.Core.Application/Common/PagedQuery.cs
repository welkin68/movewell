namespace CookieBytes.Core.Application.Common;

/// <summary>
/// Standard paging parameters for read-side queries.
/// </summary>
public record PagedQuery(int PageNumber = 1, int PageSize = 20)
{
    public int Skip => Math.Max(0, (PageNumber - 1) * PageSize);

    public int Take => PageSize <= 0 ? 20 : PageSize;
}
