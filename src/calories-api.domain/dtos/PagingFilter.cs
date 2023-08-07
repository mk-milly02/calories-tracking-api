using System.Text.Json.Serialization;

namespace calories_api.domain;

public class PagingFilter
{
    const int MAXPAGESIZE = 30;

    [JsonPropertyName("page")]
    public int PageNumber { get; set; } = 1;

    private int pageSize;

    [JsonPropertyName("size")]
    public int PageSize
    {
        get { return pageSize; }
        set { pageSize = (value > MAXPAGESIZE) ? MAXPAGESIZE : value; }
    }
}
