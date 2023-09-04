using Microsoft.AspNetCore.Identity;

namespace calories_tracking.domain;

public class UserProfile
{
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public double DailyCalorieLimit { get; set; }
    public bool HasExceededDailyCalorieLimit { get; set; }
    public List<IdentityError> Errors { get; set; } = new();
}
