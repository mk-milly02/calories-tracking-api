using calories_tracking.domain;

namespace calories_tracking.services;

public interface IMealService
{
    Task<MealResponse?> GetMealByIdAsync(Guid id);
    Task<MealResponse?> AddMealAsync(CreateMealRequest request);
    PageList<MealResponse> GetMealsAsync(QueryParameters query);
    PageList<MealResponse> GetMealsByUserAsync(Guid userId, QueryParameters query);
    Task<bool?> UpdateMealAsync(Guid id, UpdateMealRequest request);
    Task<bool> RemoveMealAsync(Guid id);
    double GetTotalUserCaloriesForTodayAsync(Guid userId);
}