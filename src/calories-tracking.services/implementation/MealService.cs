using calories_tracking.domain;
using calories_tracking.infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace calories_tracking.services;

public class MealService : IMealService
{
    private readonly IMealRepository _repository;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MealService> _logger;

    public MealService(IMealRepository repository, HttpClient httpClient, IConfiguration configuration, ILogger<MealService> logger)
    {
        _repository = repository;
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<MealResponse?> AddMealAsync(CreateMealRequest request)
    {
        if (request.NumberOfCalories is 0) { request.NumberOfCalories = await RetrieveNumberOfCalories(request.Text!); }

        try
        {
            Meal addedMeal = await _repository.CreateAsync(request.ToMeal());
            return addedMeal.ToMealResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured whiles creating meal");
            return null;
        }
    }

    public async Task<bool?> UpdateMealAsync(Guid id, UpdateMealRequest request)
    {
        if (request.NumberOfCalories is 0) { request.NumberOfCalories = await RetrieveNumberOfCalories(request.Text!); }

        Meal? existing = await _repository.RetrieveAsync(id);
        if (existing is null) { return null; }

        existing.Text = request.Text;
        existing.NumberOfCalories = request.NumberOfCalories;

        try
        {
            await _repository.UpdateAsync(existing);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured whiles updating meal with id:{id}", id);
            return false;
        }
    }

    public PageList<MealResponse> GetMealsAsync(QueryParameters query)
    {
        List<MealResponse> responses = new();

        IEnumerable<Meal> meals = !string.IsNullOrEmpty(query.S)
            ? _repository.RetrieveAllByCondition(meal => meal.Text!.Contains(query.S, StringComparison.OrdinalIgnoreCase))
            : (IEnumerable<Meal>)_repository.RetrieveAll();

        foreach (Meal meal in meals)
        {
            responses.Add(meal.ToMealResponse());
        }

        return PageList<MealResponse>.Create(responses, query.Page, query.Size);
    }

    public PageList<MealResponse> GetMealsByUserAsync(Guid userId, QueryParameters query)
    {
        List<MealResponse> responses = new();

        IEnumerable<Meal> meals = _repository.RetrieveAllByCondition(meal => meal.UserId.Equals(userId));

        if (!string.IsNullOrEmpty(query.S)) { meals = meals.Where(meal => meal.Text!.Contains(query.S, StringComparison.OrdinalIgnoreCase)); }

        foreach (Meal meal in meals)
        {
            responses.Add(meal.ToMealResponse());
        }

        return PageList<MealResponse>.Create(responses, query.Page, query.Size);
    }

    public async Task<MealResponse?> GetMealByIdAsync(Guid id)
    {
        Meal? meal = await _repository.RetrieveAsync(id);
        return meal?.ToMealResponse();
    }

    public double GetTotalUserCaloriesForTodayAsync(Guid userId)
    {
        IEnumerable<Meal> meals = _repository.RetrieveAllByCondition(meal => meal.UserId.Equals(userId) && meal.Created.Date.Equals(DateTime.Today));

        double totalUserCalories = 0;

        foreach (Meal meal in meals)
        {
            totalUserCalories += meal.NumberOfCalories;
        }
        return totalUserCalories;
    }

    public async Task<bool> RemoveMealAsync(Guid id)
    {
        try
        {
            await _repository.DeleteAsync(id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured whiles deleting meal with id:{id}", id);
            return false;
        }
    }

    private async Task<double> RetrieveNumberOfCalories(string query)
    {
        string? appId = _configuration["nutritionix-api:application-id"];
        string? appKey = _configuration["nutritionix-api:application-key"];

        _httpClient.DefaultRequestHeaders.Add("x-app-id", appId);
        _httpClient.DefaultRequestHeaders.Add("x-app-key", appKey);

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("https://trackapi.nutritionix.com/v2/natural/nutrients", query);

        if (response.IsSuccessStatusCode)
        {
            NutritionAPIObject? result = JsonConvert.DeserializeObject<NutritionAPIObject>(await response.Content.ReadAsStringAsync());
            return result!.ComputeCalories();
        }

        return 0;
    }
}
