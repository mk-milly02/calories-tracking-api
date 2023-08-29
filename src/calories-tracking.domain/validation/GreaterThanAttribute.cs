using System.ComponentModel.DataAnnotations;

namespace calories_tracking.domain;

public class GreaterThanAttribute : ValidationAttribute
{
    public GreaterThanAttribute(int value)
    {
        Value = value;
    }

    public int Value { get; set; }

    public override bool IsValid(object? value)
    {
        bool x = int.TryParse(value!.ToString(), out int y);
        return x && y > Value;
    }
}
