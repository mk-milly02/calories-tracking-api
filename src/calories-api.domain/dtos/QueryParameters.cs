namespace calories_api.domain;

public class QueryParameters
{
    const int MAXPAGESIZE = 30;

    public int Page { get; set; } = 1;

    public string? S { get; set; }

    private int size;

    public int Size
    {
        get { return size; }
        set { size = (value > MAXPAGESIZE) ? MAXPAGESIZE : value; }
    }
    
}
