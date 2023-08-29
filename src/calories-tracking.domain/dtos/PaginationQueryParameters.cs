using System.ComponentModel.DataAnnotations;

namespace calories_tracking.domain;

public class PaginationQueryParameters
{
    [Required, GreaterThan(0, ErrorMessage = "Page number must be greater than 1")]
    public int Page { get; set; } = 1;

    [Required, Range(5, 30, ErrorMessage = "Returns a minimum of 5 items and a maximum of 30 items per request")]
    public int Size { get; set; } = 10;
}
