using System.ComponentModel.DataAnnotations;

namespace calories_tracking.domain;

public class QueryParameters
{
    [Required, GreaterThan(0, ErrorMessage = "Page number must be greater than 0")]
    public int Page { get; set; }

    public string? S { get; set; } // search term

    [Required, Range(5, 30, ErrorMessage = "Returns a minimum of 5 items and a maximum of 30 items per request")]
    public int Size { get; set; }

}
