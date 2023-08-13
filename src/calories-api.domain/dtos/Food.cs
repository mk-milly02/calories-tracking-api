using System.Text.Json.Serialization;

namespace calories_api.domain;

public class Food
{
    [JsonPropertyName("nf_calories")]
    public string? Calories { get; set; }
}
