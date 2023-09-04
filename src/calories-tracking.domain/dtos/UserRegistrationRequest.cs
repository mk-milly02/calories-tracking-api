using System.ComponentModel.DataAnnotations;

namespace calories_tracking.domain;

public class UserRegistrationRequest
{
    [Required(ErrorMessage = "Firstname is required")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Lastname is required")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Username is required")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [RegularExpression("^(?=.*[A-Z])(?=.*[a-z])(?=.*[\\d])(?=.*[\\W_)]).{8,}$",
    ErrorMessage = "The password must contain at least eight characters, an uppercase letter, a lowercase letter, a number and a special character"),]
    public string? Password { get; set; }

    [Required(ErrorMessage = "ConfirmPassword is required")]
    [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match")]
    public string? ConfirmPassword { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
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