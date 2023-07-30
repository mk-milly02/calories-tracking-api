using System.ComponentModel.DataAnnotations;

namespace calories_api.domain;

public class AuthenticationRequest
{
    [Required(ErrorMessage = "Email is required")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string? Password { get; set; }
}
