using calories_api.domain;

namespace calories_api.services;

public interface IMealService
{
    Task<MealResponse?> AddMeal(CreateMealRequest request);
    Task<MealResponse?> GetMeal(Guid id);
    Task<IEnumerable<MealResponse>> GetAllMeals(QueryParameters query);
    Task<IEnumerable<MealResponse>> GetAllUserMeals(Guid userId, QueryParameters query);
    Task<double> GetTotalUserCaloriesForToday(Guid userId);
    Task<MealResponse?> EditMeal(Guid id, UpdateMealRequest request);
    Task<bool?> RemoveMeal(Guid id);
}