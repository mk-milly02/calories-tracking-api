using System.ComponentModel.DataAnnotations;

namespace calories_api.domain;

public class PagingFilter
{
    const int MAXPAGESIZE = 30;

    [Required, Range(1, MAXPAGESIZE)]
    public int Page { get; set; } = 1;

    private int size;

    [Required, Range(1, double.MaxValue)]
    public int Size
    {
        get { return size; }
        set { size = (value > MAXPAGESIZE) ? MAXPAGESIZE : value; }
    }
}
