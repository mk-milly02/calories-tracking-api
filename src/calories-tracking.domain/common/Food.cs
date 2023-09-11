using System.Text.Json.Serialization;

namespace calories_tracking.domain;

public class Food
{
    [JsonPropertyName("nf_calories")]
    public string? Calories { get; set; }
}
