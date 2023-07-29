using calories_api.domain;

namespace calories_api.persistence;

public interface ICaloriesRepository
{
    Task<CalorieEntry?> Create(CalorieEntry entry);
    Task<CalorieEntry?> Retrieve(Guid id);
    Task<IEnumerable<CalorieEntry>> RetrieveAll(QueryParameters query);
    Task<CalorieEntry?> Update(CalorieEntry entry);
    Task<bool?> Delete(Guid id);
}
