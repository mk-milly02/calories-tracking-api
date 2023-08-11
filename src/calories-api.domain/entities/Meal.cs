namespace calories_api.domain;

public class Meal
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Text { get; set; }
    public double NumberOfCalories { get; set; }
    public DateTime DateTime { get; set; } = DateTime.UtcNow;

    public MealResponse ToMealResponse()
    {
        return new()
        {
            Id = Id,
            UserId = UserId,
            Text = Text,
            NumberOfCalories = NumberOfCalories,
            CreatedOn = DateOnly.FromDateTime(DateTime),
            CreatedAt = TimeOnly.FromDateTime(DateTime)
        };
    }
}
