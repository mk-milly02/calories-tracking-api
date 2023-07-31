using System.ComponentModel.DataAnnotations;

namespace calories_api.domain;

public class PagingFilter
{
    const int MAXPAGESIZE = 30;

    public int PageNumber { get; set; } = 1;

    private int pageSize;
    public int PageSize
    {
        get { return pageSize; }
        set { pageSize = (value > MAXPAGESIZE) ? MAXPAGESIZE : value; }
    }
}
