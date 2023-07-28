using System.Net.Http.Json;
using calories_api.domain;
using calories_api.persistence;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace calories_api.services;

public class CaloriesService : ICaloriesService
{
    private readonly ICaloriesRepository _repository;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public CaloriesService(ICaloriesRepository repository, HttpClient httpClient, IConfiguration configuration)
    {
        _repository = repository;
        _httpClient = httpClient;
        _configuration = configuration;
    }
    
    public Task<CalorieEntryResponse?> AddCalorieEntry(CreateCalorieEntryRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<CalorieEntryResponse?> EditCalorieEntry(Guid id, UpdateCalorieEntryRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<CalorieEntryResponse>> GetAllCalorieEntries()
    {
        throw new NotImplementedException();
    }

    public Task<CalorieEntryResponse?> GetCalorieEntry(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<bool?> RemoveCalorieEntry(Guid id)
    {
        throw new NotImplementedException();
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
