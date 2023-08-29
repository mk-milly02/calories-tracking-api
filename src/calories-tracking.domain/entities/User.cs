using Microsoft.AspNetCore.Identity;

namespace calories_tracking.domain;

public class User : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set;}
    public string? PasswordSalt { get; set; }
    public double DailyCalorieLimit { get; set; }
    public bool HasExceededDailyCalorieLimit { get; set; }
    public ICollection<Meal> Meals { get; set; } = new List<Meal>();

    public UserProfile ToUserProfile(string role)
    {
        return new()
        {
            UserId = Id,
            FirstName = FirstName,
            LastName = LastName,
            Username = UserName,
            Email = Email,
            DailyCalorieLimit = DailyCalorieLimit,
            HasExceededDailyCalorieLimit = HasExceededDailyCalorieLimit,
            Role = role
        };
    }
}
