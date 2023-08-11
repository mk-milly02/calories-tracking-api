﻿using System.ComponentModel.DataAnnotations;

namespace calories_api.domain;

public class UpdateMealRequest
{
    [Required(ErrorMessage = "Text is required"), MaxLength(100, ErrorMessage = "Maximum number of characters exceeded")]
    public string? Text { get; set; }

    [Range(0, 5000, ErrorMessage = "Must be between 0 and 5000")]
    public double NumberOfCalories { get; set; }

    public Meal ToMeal()
    {
        return new()
        {
            Text = Text,
            NumberOfCalories = NumberOfCalories
        };
    }
}
