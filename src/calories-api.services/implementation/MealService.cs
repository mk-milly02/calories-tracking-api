using System.Net.Http.Json;
using calories_api.domain;
using calories_api.infrastructure;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace calories_api.services;

public class MealService : IMealService
{
    private readonly IMealRepository _repository;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public MealService(IMealRepository repository, HttpClient httpClient, IConfiguration configuration)
    {
        _repository = repository;
        _httpClient = httpClient;
        _configuration = configuration;
    }
    
    public async Task<MealResponse?> AddMealAsync(CreateMealRequest request)
    {
        if(request.NumberOfCalories is 0) { request.NumberOfCalories = await RetrieveNumberOfCalories(request.Text!); }

        Meal meal = request.ToMeal();
        Meal? addedMeal = await _repository.Create(meal);
        return addedMeal is null ? null : addedMeal.ToMealResponse();
    }

    public async Task<MealResponse?> UpdateMealAsync(Guid id, UpdateMealRequest request)
    {
        if(request.NumberOfCalories is 0) { request.NumberOfCalories = await RetrieveNumberOfCalories(request.Text!); }
        
        Meal meal = request.ToMeal();
        meal.Id = id;
        Meal? updatedMeal = await _repository.Update(meal);
        return updatedMeal is null ? null : updatedMeal.ToMealResponse();
    }

    public async Task<IEnumerable<MealResponse>> GetMealsAsync(QueryParameters query)
    {
        List<MealResponse> output = new();
        IEnumerable<Meal> meals = await _repository.RetrieveAll();

        if(!string.IsNullOrEmpty(query.SeachString)) 
        { 
            meals = meals.Where(meal => meal.Text!.Contains(query.SeachString)); 
        }

        foreach (Meal meal in meals)
        {
            output.Add(meal.ToMealResponse());
        }

        return output.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize);
    }

    public async Task<IEnumerable<MealResponse>> GetMealsByUserAsync(Guid userId, QueryParameters query)
    {
        List<MealResponse> output = new();
        IEnumerable<Meal> meals = await _repository.RetrieveAllByUser(userId);

        if(!string.IsNullOrEmpty(query.SeachString)) 
        { 
            meals = meals.Where(meal => meal.Text!.Contains(query.SeachString)); 
        }

        foreach (Meal meal in meals)
        {
            output.Add(meal.ToMealResponse());
        }
        return output.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize);
    }

    public async Task<MealResponse?> GetMealByIdAsync(Guid id)
    {
        Meal? meal = await _repository.Retrieve(id);
        return meal is null ? null : meal.ToMealResponse();
    }

    public async Task<double> GetTotalUserCaloriesForToday(Guid userId)
    {
        IEnumerable<Meal> meals = await _repository.RetrieveAllByUser(userId);
        IEnumerable<Meal> mealsForToday = meals.Where(meal => meal.Created.Date.Equals(DateTime.Today));

        double totalUserCalories = 0;

        foreach (Meal meal in mealsForToday)
        {
            totalUserCalories += meal.NumberOfCalories;
        }
        return totalUserCalories;
    }

    public async Task<bool?> RemoveMealAsync(Guid id)
    {
        return await _repository.Delete(id);
    }

    private async Task<double> RetrieveNumberOfCalories(string query)
    {
        string? appId = _configuration["nutritionix-api:application-id"];
        string? appKey = _configuration["nutritionix-api:application-key"];

        HttpRequestMessage message = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($" https://trackapi.nutritionix.com/v2/natural/nutrients"),
            Headers =
            {
                { "x-app-id", appId },
                { "x-app-key", appKey },
            },
            Content = JsonContent.Create(query)
        };

        HttpResponseMessage response = await _httpClient.SendAsync(message);
        
        if(response.IsSuccessStatusCode)
        {
            NutritionAPIObject? result = JsonConvert.DeserializeObject<NutritionAPIObject>(await response.Content.ReadAsStringAsync());
            return result!.ComputeCalories();
        }
        
        return 0;
    }
}
