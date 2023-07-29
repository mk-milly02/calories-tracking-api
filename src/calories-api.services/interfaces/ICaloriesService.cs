using calories_api.domain;

namespace calories_api.services;

public interface ICaloriesService
{
    Task<CalorieEntryResponse?> AddCalorieEntry(CreateCalorieEntryRequest request);
    Task<CalorieEntryResponse?> GetCalorieEntry(Guid id);
    Task<IEnumerable<CalorieEntryResponse>> GetAllCalorieEntries(QueryParameters query);
    Task<CalorieEntryResponse?> EditCalorieEntry(Guid id, UpdateCalorieEntryRequest request);
    Task<bool?> RemoveCalorieEntry(Guid id);
}
