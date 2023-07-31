using System.Net.Http.Json;
using AutoMapper;
using calories_api.domain;
using calories_api.persistence;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace calories_api.services;

public class MealService : IMealService
{
    private readonly IMealRepository _repository;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public MealService(IMealRepository repository, HttpClient httpClient, IConfiguration configuration, IMapper mapper)
    {
        _repository = repository;
        _httpClient = httpClient;
        _configuration = configuration;
        _mapper = mapper;
    }
    
    public async Task<MealResponse?> AddMeal(CreateMealRequest request)
    {
        if(request.NumberOfCalories is 0) { request.NumberOfCalories = await RetrieveNumberOfCalories(request.Text!); }

        Meal meal = _mapper.Map<Meal>(request);
        Meal? addedMeal = await _repository.Create(meal);
        return addedMeal is null ? null : _mapper.Map<MealResponse>(addedMeal);
    }

    public async Task<MealResponse?> EditMeal(Guid id, UpdateMealRequest request)
    {
        if(request.NumberOfCalories is 0) { request.NumberOfCalories = await RetrieveNumberOfCalories(request.Text!); }
        
        Meal meal = _mapper.Map<Meal>(request);
        meal.Id = id;
        Meal? updatedMeal = await _repository.Update(meal);
        return updatedMeal is null ? null : _mapper.Map<MealResponse>(updatedMeal);
    }

    public async Task<IEnumerable<MealResponse>> GetAllMeals(QueryParameters query)
    {
        List<MealResponse> output = new();
        IEnumerable<Meal> meals = await _repository.RetrieveAll();

        if(!string.IsNullOrEmpty(query.SeachString)) 
        { 
            meals = meals.Where(meal => meal.Text!.Contains(query.SeachString)); 
        }

        foreach (Meal meal in meals)
        {
            MealResponse mealResponse = _mapper.Map<MealResponse>(meal);
            output.Add(mealResponse);
        }

        return output.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize);
    }

    public async Task<IEnumerable<MealResponse>> GetAllUserMeals(Guid userId, QueryParameters query)
    {
        List<MealResponse> output = new();
        IEnumerable<Meal> meals = await _repository.RetrieveAllByUser(userId);

        if(!string.IsNullOrEmpty(query.SeachString)) 
        { 
            meals = meals.Where(meal => meal.Text!.Contains(query.SeachString)); 
        }

        foreach (Meal meal in meals)
        {
            MealResponse mealResponse = _mapper.Map<MealResponse>(meal);
            output.Add(mealResponse);
        }
        return output.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize);
    }

    public async Task<MealResponse?> GetMeal(Guid id)
    {
        Meal? meal = await _repository.Retrieve(id);
        return meal is null ? null : _mapper.Map<MealResponse>(meal);
    }

    public Task<double> GetTotalUserCaloriesForToday(Guid userId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool?> RemoveMeal(Guid id)
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
