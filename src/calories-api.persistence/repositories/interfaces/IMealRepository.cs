using calories_api.domain;

namespace calories_api.persistence;

public interface IMealRepository
{
    Task<Meal?> Create(Meal entry);
    Task<Meal?> Retrieve(Guid id);
    Task<IEnumerable<Meal>> RetrieveAll();
    Task<IEnumerable<Meal>> RetrieveAllByUser(Guid userId);
    Task<Meal?> Update(Meal entry);
    Task<bool?> Delete(Guid id);
}