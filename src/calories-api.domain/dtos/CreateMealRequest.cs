using System.ComponentModel.DataAnnotations;

namespace calories_api.domain;

public class CreateMealRequest
{
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Text is required"), MaxLength(100)]
    public string? Text { get; set; }

    [Range(0, 5000)]
    public double NumberOfCalories { get; set; }
    
    public DateTime DateTime { get; set; } = DateTime.UtcNow;
}