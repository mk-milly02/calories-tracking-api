using calories_api.domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace calories_api.persistence;

public class MealRepository : IMealRepository
{
    private readonly ApplicationDbContext _context;

    public MealRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Meal?> Create(Meal meal)
    {
        EntityEntry<Meal> added = await _context.Meals.AddAsync(meal);
        return await _context.SaveChangesAsync() > 0 ? added.Entity : null;
    }

    public async Task<bool?> Delete(Guid id)
    {
        Meal? existingMeal = await _context.Meals.FindAsync(id);

        if (existingMeal is null) { return null;}
        _context.Meals.Remove(existingMeal);

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Meal?> Retrieve(Guid id)
    {
        return await _context.Meals.AsNoTracking().SingleOrDefaultAsync(meal => meal.Id.Equals(id));
    }

    public async Task<IEnumerable<Meal>> RetrieveAll(QueryParameters query)
    {
        IEnumerable<Meal> meals = await _context.Meals.AsNoTracking().ToListAsync();

        if(!string.IsNullOrEmpty(query.SeachString)) 
        { 
            meals = meals.Where(meal => meal.Text!.Contains(query.SeachString)); 
        }

        return meals.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize);
    }

    public async Task<IEnumerable<Meal>> RetrieveAllByUser(Guid userId, QueryParameters query)
    {
        IEnumerable<Meal> meals = await _context.Meals.AsNoTracking().Where(meal => meal.Equals(userId)).ToListAsync();

        if(!string.IsNullOrEmpty(query.SeachString)) 
        { 
            meals = meals.Where(meal => meal.Text!.Contains(query.SeachString)); 
        }

        return meals.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize);
    }

    public async Task<Meal?> Update(Meal meal)
    {
        EntityEntry<Meal> updated = _context.Meals.Update(meal);
        return await _context.SaveChangesAsync() > 0 ? updated.Entity : null;
    }
}