using Microsoft.AspNetCore.Identity;

namespace calories_api.domain;

public class User : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set;}
    public string? PasswordSalt { get; set; }
    public double ExpectedNumberOfCaloriesPerDay { get; set; }
    public bool IsCaloriesDeficient { get; set; }
    public ICollection<Meal> Meals { get; set; } = new List<Meal>();

    public UserRegistrationResponse ToUserRegistrationResponse()
    {
        return new()
        {
            Id = Id,
            FirstName = FirstName,
            LastName = LastName,
            Username = UserName,
            Email = Email
        };
    }

    public UserProfile ToUserProfile(string role)
    {
        return new()
        {
            UserId = Id,
            FirstName = FirstName,
            LastName = LastName,
            Username = UserName,
            Email = Email,
            ExpectedNumberOfCaloriesPerDay = ExpectedNumberOfCaloriesPerDay,
            IsCalorieDeficient = IsCaloriesDeficient,
            Role = role
        };
    }
}
