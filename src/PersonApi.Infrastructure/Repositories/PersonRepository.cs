using Microsoft.EntityFrameworkCore;
using PersonApi.Domain.Entities;
using PersonApi.Infrastructure.Data;

namespace PersonApi.Infrastructure.Repositories;

/// <summary>
/// Person repository implementation with custom queries
/// </summary>
public class PersonRepository : Repository<Person>, IPersonRepository
{
    public PersonRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Person>> GetByNameAsync(string name)
    {
        return await _dbSet
            .Where(p => p.Name.Contains(name))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Person>> GetByAgeRangeAsync(int minAge, int maxAge)
    {
        return await _dbSet
            .Where(p => p.Age >= minAge && p.Age <= maxAge)
            .AsNoTracking()
            .OrderBy(p => p.Age)
            .ToListAsync();
    }

    public async Task<IEnumerable<Person>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await _dbSet
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
