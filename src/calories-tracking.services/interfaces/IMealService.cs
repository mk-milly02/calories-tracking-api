using calories_tracking.domain;

namespace calories_tracking.services;

public interface IMealService
{
    Task<MealResponse?> AddMealAsync(CreateMealRequest request);
    Task<MealResponse?> GetMealByIdAsync(Guid id);
    Task<IEnumerable<MealResponse>> GetMealsAsync(FiltrationQueryParameters query);
    Task<IEnumerable<MealResponse>> GetMealsByUserAsync(Guid userId, FiltrationQueryParameters query);
    Task<double> GetTotalUserCaloriesForTodayAsync(Guid userId);
    Task<MealResponse?> UpdateMealAsync(Guid id, UpdateMealRequest request);
    Task<bool?> RemoveMealAsync(Guid id);
}