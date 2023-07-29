using System.Net.Http.Json;
using AutoMapper;
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
    private readonly IMapper _mapper;

    public CaloriesService(ICaloriesRepository repository, HttpClient httpClient, IConfiguration configuration, IMapper mapper)
    {
        _repository = repository;
        _httpClient = httpClient;
        _configuration = configuration;
        _mapper = mapper;
    }
    
    public async Task<CalorieEntryResponse?> AddCalorieEntry(CreateCalorieEntryRequest request)
    {
        if(request.NumberOfCalories is 0) { request.NumberOfCalories = await RetrieveNumberOfCalories(request.Text!); }

        CalorieEntry entry = _mapper.Map<CalorieEntry>(request);
        CalorieEntry? addedEntry = await _repository.Create(entry);
        return addedEntry is null ? null : _mapper.Map<CalorieEntryResponse>(addedEntry);
    }

    public async Task<CalorieEntryResponse?> EditCalorieEntry(Guid id, UpdateCalorieEntryRequest request)
    {
        CalorieEntry entry = _mapper.Map<CalorieEntry>(request);
        entry.EntryId = id;
        CalorieEntry? updatedEntry = await _repository.Update(entry);
        return updatedEntry is null ? null : _mapper.Map<CalorieEntryResponse>(updatedEntry);
    }

    public async Task<IEnumerable<CalorieEntryResponse>> GetAllCalorieEntries(QueryParameters query)
    {
        List<CalorieEntryResponse> calorieEntries = new();
        IEnumerable<CalorieEntry> entries = await _repository.RetrieveAll(query);

        foreach (CalorieEntry entry in entries)
        {
            CalorieEntryResponse calorieEntry = _mapper.Map<CalorieEntryResponse>(entry);
            calorieEntries.Add(calorieEntry);
        }
        return calorieEntries;
    }

    public async Task<CalorieEntryResponse?> GetCalorieEntry(Guid id)
    {
        CalorieEntry? entry = await _repository.Retrieve(id);
        return entry is null ? null : _mapper.Map<CalorieEntryResponse>(entry);
    }

    public async Task<bool?> RemoveCalorieEntry(Guid id)
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
