using System.ComponentModel.DataAnnotations;

namespace calories_tracking.domain;

public class UpdateUserRequest
{
    [Required(ErrorMessage = "FirstName is required")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "LastName is required")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Username is required")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Expected number of calories per day is required")]
    [Range(10, 5000, ErrorMessage = "Must be between 10 and 5000")]
    public double ExpectedNumberOfCaloriesPerDay { get; set; }

    [Required]
    public bool IsCaloriesDeficient { get; set; }
}
