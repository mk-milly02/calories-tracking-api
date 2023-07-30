using System.ComponentModel.DataAnnotations;

namespace calories_api.domain;

public class UserRegistrationRequest
{
    [Required(ErrorMessage = "Username is required")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Email is required")]
    public string? Email { get; set; }
}