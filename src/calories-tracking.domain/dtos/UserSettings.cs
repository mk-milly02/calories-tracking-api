using System.ComponentModel.DataAnnotations;

namespace calories_tracking.domain;

public class UserSettings
{
    [Required(ErrorMessage = "Daily calorie limit is required")]
    [Range(10, 5000, ErrorMessage = "Must be between 10 and 5000")]
    public double DailyCalorieLimit { get; set; }
}
