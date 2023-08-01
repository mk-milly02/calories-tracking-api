using System.ComponentModel.DataAnnotations;

namespace calories_api.domain;

public class CreatePasswordRequest
{
    [Required(ErrorMessage = "Password is required")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match")]
    public string? ConfirmPassword { get; set; }
}
