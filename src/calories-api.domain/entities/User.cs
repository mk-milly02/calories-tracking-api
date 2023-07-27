using Microsoft.AspNetCore.Identity;

namespace calories_api.domain;

public class User : IdentityUser<Guid>
{
    public string? PasswordSalt { get; set; }
    public double ExpectedNumberOfCaloriesPerDay { get; set; }
    public bool IsCaloriesDeficient { get; set; }
    public ICollection<CalorieEntry> Calories { get; set; } = new List<CalorieEntry>();
}
