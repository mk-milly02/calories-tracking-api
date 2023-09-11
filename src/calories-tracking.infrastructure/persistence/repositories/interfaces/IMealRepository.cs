using System.Linq.Expressions;
using calories_tracking.domain;

namespace calories_tracking.infrastructure;

public interface IMealRepository
{
    Task DeleteAsync(Guid id);
    Task<Meal> CreateAsync(Meal entity);
    Task<Meal?> RetrieveAsync(Guid id);
    Task UpdateAsync(Meal entity);
    IQueryable<Meal> RetrieveAll();
    IQueryable<Meal> RetrieveAllByCondition(Expression<Func<Meal, bool>> condition);
}
