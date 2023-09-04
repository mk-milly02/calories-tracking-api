using System.ComponentModel.DataAnnotations;

namespace calories_tracking.domain;

public class CreateUserRequest
{
    [Required(ErrorMessage = "Firstname is required")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Lastname is required")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Email is required"), EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [RegularExpression("^(?=.*[A-Z])(?=.*[a-z])(?=.*[\\d])(?=.*[\\W_)]).{8,}$",
    ErrorMessage = "The password must contain at least eight characters, an uppercase letter, a lowercase letter, a number and a special character"),]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Role is required"), EnumDataType(typeof(Roles), ErrorMessage = "Invalid role")]
    public string? Role { get; set; }

    public User ToUser()
    {
        return new()
        {
            FirstName = FirstName,
            LastName = LastName,
            Email = Email,
            UserName = $"{FirstName!.ToLower()}.{LastName!.ToLower()}"
        };
    }
}
