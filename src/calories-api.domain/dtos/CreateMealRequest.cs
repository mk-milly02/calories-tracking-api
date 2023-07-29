using System.ComponentModel.DataAnnotations;

namespace calories_api.domain;

public class CreateMealRequest
{
    public Guid UserId { get; set; }

    [Required, MaxLength(100)]
    public string? Text { get; set; }
    public double NumberOfCalories { get; set; }
    public DateTime DateTime { get; set; } = DateTime.UtcNow;
}