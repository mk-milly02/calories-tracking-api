namespace calories_api.domain;

public class NutritionAPIObject
{
    IEnumerable<Food> Foods { get; set; } = new List<Food>();

    public double ComputeCalories()
    {
        double output = 0;

        foreach (var food in Foods)
        {
            output += Convert.ToDouble(food.Calories);
        }
        
        return output;
    }
}
