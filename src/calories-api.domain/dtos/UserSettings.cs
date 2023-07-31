using System.ComponentModel.DataAnnotations;

namespace calories_api.domain;

public class UserSettings
{
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Expected number of calories per day is required")]
    [Range(10, 5000, ErrorMessage = "Must be between 10 and 5000")]
    public double ExpectedNumberOfCaloriesPerDay { get; set; }
}
