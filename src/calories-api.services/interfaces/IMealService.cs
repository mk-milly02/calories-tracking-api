using calories_api.domain;

namespace calories_api.services;

public interface IMealService
{
    Task<MealResponse?> AddMealAsync(CreateMealRequest request);
    Task<MealResponse?> GetMealByIdAsync(Guid id);
    Task<IEnumerable<MealResponse>> GetMealsAsync(QueryParameters query);
    Task<IEnumerable<MealResponse>> GetMealsByUserAsync(Guid userId, QueryParameters query);
    Task<double> GetTotalUserCaloriesForToday(Guid userId);
    Task<MealResponse?> UpdateMealAsync(Guid id, UpdateMealRequest request);
    Task<bool?> RemoveMealAsync(Guid id);
}