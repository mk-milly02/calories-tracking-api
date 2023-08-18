using System.ComponentModel;
using System.Text.Json.Serialization;

namespace calories_api.domain;

public class QueryParameters
{
    const int MAXPAGESIZE = 30;

    [DisplayName("page")]
    public int PageNumber { get; set; } = 1;

    [DisplayName("s")]
    public string? SeachString { get; set; }

    private int pageSize;

    [DisplayName("size")]
    public int PageSize
    {
        get { return pageSize; }
        set { pageSize = (value > MAXPAGESIZE) ? MAXPAGESIZE : value; }
    }
    
}
