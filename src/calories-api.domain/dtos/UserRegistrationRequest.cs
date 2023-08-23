using System.ComponentModel.DataAnnotations;

namespace calories_api.domain;

public class UserRegistrationRequest
{
    [Required(ErrorMessage = "Firstname is required")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Lastname is required")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Username is required")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Password is required"), MinLength(8, ErrorMessage = "Password must have a minimum of 8 characters")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Email is required"), EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }

    public User ToUser()
    {
        return new()
        {
            FirstName = FirstName,
            LastName = LastName,
            UserName = Username,
            Email = Email
        };
    }
}