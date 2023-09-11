using System.Linq.Expressions;
using calories_tracking.domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace calories_tracking.infrastructure;

public class MealRepository : IMealRepository
{
    private readonly ApplicationDbContext _context;

    public MealRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Meal> CreateAsync(Meal entity)
    {
        EntityEntry<Meal> added = await _context.Meals.AddAsync(entity);
        await _context.SaveChangesAsync();
        return added.Entity;
    }
    
    public async Task DeleteAsync(Guid id)
    {
        _context.Remove(new Meal { Id = id });
        await _context.SaveChangesAsync();
    }

    public IQueryable<Meal> RetrieveAll()
    {
        return _context.Meals.AsNoTracking();
    }

    public IQueryable<Meal> RetrieveAllByCondition(Expression<Func<Meal, bool>> condition)
    {
        return _context.Meals.AsNoTracking().Where(condition);
    }

    public async Task<IEnumerable<Meal>> RetrieveAllByUser(Guid userId)
    {
        return await _context.Meals.AsNoTracking().Where(meal => meal.UserId.Equals(userId)).ToListAsync();
    }

    public async Task<Meal?> RetrieveAsync(Guid id)
    {
        return await _context.Meals.SingleOrDefaultAsync(meal => meal.Id.Equals(id));
    }

    public async Task UpdateAsync(Meal entity)
    {
        _context.Meals.Update(entity);
        await _context.SaveChangesAsync();
    }
}
