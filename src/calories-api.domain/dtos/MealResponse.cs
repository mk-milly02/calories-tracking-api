namespace calories_api.domain;

public class MealResponse
{
    public Guid EntryId { get; set; }
    public Guid UserId { get; set; }
    public string? Text { get; set; }
    public double NumberOfCalories { get; set; }
    public DateOnly CreatedOn { get; set; }
    public TimeOnly CreatedAt { get; set; }
}
