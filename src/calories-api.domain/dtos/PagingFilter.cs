namespace calories_api.domain;

public class PagingFilter
{
    const int MAXPAGESIZE = 30;

    public int Page { get; set; } = 1;

    private int size;

    public int Size
    {
        get { return size; }
        set { size = (value > MAXPAGESIZE) ? MAXPAGESIZE : value; }
    }
}
