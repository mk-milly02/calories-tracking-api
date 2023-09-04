namespace calories_tracking.domain;

public class Meal
{
    public Guid Id { get; set; }
    public string? Text { get; set; }
    public double NumberOfCalories { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public Guid UserId { get; set; }
    public User? User { get; set; }
    
    public MealResponse ToMealResponse()
    {
        return new()
        {
            Id = Id,
            UserId = UserId,
            Text = Text,
            NumberOfCalories = NumberOfCalories,
            CreatedOn = DateOnly.FromDateTime(Created),
            CreatedAt = TimeOnly.FromDateTime(Created)
        };
    }
}
