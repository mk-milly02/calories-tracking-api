using calories_api.domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace calories_api.persistence;

public class CaloriesRepository : ICaloriesRepository
{
    private readonly ApplicationDbContext _context;

    public CaloriesRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CalorieEntry?> Create(CalorieEntry entry)
    {
        EntityEntry<CalorieEntry> added = await _context.Calories.AddAsync(entry);
        return await _context.SaveChangesAsync() > 0 ? added.Entity : null;
    }

    public async Task<bool?> Delete(Guid id)
    {
        CalorieEntry? existingEntry = await _context.Calories.FindAsync(id);

        if (existingEntry is null) { return null;}
        _context.Calories.Remove(existingEntry);

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<CalorieEntry?> Retrieve(Guid id)
    {
        return await _context.Calories.AsNoTracking().SingleOrDefaultAsync(entry => entry.EntryId.Equals(id));
    }

    public async Task<IEnumerable<CalorieEntry>> RetrieveAll(QueryParameters query)
    {
        IEnumerable<CalorieEntry> entries = await _context.Calories.AsNoTracking().ToListAsync();

        if(!string.IsNullOrEmpty(query.SeachString)) 
        { 
            entries = entries.Where(entry => entry.Text!.Contains(query.SeachString)); 
        }

        return entries.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize);
    }

    public async Task<CalorieEntry?> Update(CalorieEntry entry)
    {
        EntityEntry<CalorieEntry> updated = _context.Calories.Update(entry);
        return await _context.SaveChangesAsync() > 0 ? updated.Entity : null;
    }
}
