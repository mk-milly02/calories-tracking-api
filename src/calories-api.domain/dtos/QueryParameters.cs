namespace calories_api.domain;

public class QueryParameters
{
    const int MAXPAGESIZE = 30;

    public int PageNumber { get; set; } = 1;
    public string? SeachString { get; set; }

    private int pageSize;
    public int PageSize
    {
        get { return pageSize; }
        set { pageSize = (value > MAXPAGESIZE) ? MAXPAGESIZE : value; }
    }
    
}
