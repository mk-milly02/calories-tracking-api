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

    public async Task<Meal?> Create(Meal entry)
    {
        EntityEntry<Meal> added = await _context.Meals.AddAsync(entry);
        return await _context.SaveChangesAsync() > 0 ? added.Entity : null;
    }

    public async Task<bool?> Delete(Guid id)
    {
        Meal? existingEntry = await _context.Meals.FindAsync(id);

        if (existingEntry is null) { return null;}
        _context.Meals.Remove(existingEntry);

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Meal?> Retrieve(Guid id)
    {
        return await _context.Meals.AsNoTracking().SingleOrDefaultAsync(entry => entry.Id.Equals(id));
    }

    public async Task<IEnumerable<Meal>> RetrieveAll(QueryParameters query)
    {
        IEnumerable<Meal> entries = await _context.Meals.AsNoTracking().ToListAsync();

        if(!string.IsNullOrEmpty(query.SeachString)) 
        { 
            entries = entries.Where(entry => entry.Text!.Contains(query.SeachString)); 
        }

        return entries.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize);
    }

    public async Task<Meal?> Update(Meal entry)
    {
        EntityEntry<Meal> updated = _context.Meals.Update(entry);
        return await _context.SaveChangesAsync() > 0 ? updated.Entity : null;
    }
}
