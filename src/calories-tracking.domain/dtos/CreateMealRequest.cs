﻿using System.ComponentModel.DataAnnotations;

namespace calories_tracking.domain;

public class CreateMealRequest
{
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Text is required"), MaxLength(100)]
    public string? Text { get; set; }

    [Range(0, 5000, ErrorMessage = "Must range between 0 and 5000")]
    public double NumberOfCalories { get; set; }

    public Meal ToMeal()
    {
        return new()
        {
            UserId = UserId,
            Text = Text,
            NumberOfCalories = NumberOfCalories
        };
    }
}