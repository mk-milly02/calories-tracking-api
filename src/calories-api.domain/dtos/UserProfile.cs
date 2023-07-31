namespace calories_api.domain;

public class UserProfile
{
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? UserName { get; set;}
    public string? Email { get; set;}
    public string? Role { get; set; }
    public double ExpectedNumberOfCaloriesPerDay { get; set; }
    public bool IsCalorieDeficient { get; set; }
}
