using System.ComponentModel.DataAnnotations;

namespace calories_api.domain;

public class EditUserProfileRequest
{
    [Required(ErrorMessage = "Firstname is required")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Lastname is required")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Username is required")]
    public string? Username { get; set; }
}
