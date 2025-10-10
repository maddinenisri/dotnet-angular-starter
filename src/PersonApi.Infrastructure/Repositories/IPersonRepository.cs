using PersonApi.Domain.Entities;

namespace PersonApi.Infrastructure.Repositories;

/// <summary>
/// Person-specific repository interface with custom queries
/// </summary>
public interface IPersonRepository : IRepository<Person>
{
    Task<IEnumerable<Person>> GetByNameAsync(string name);
    Task<IEnumerable<Person>> GetByAgeRangeAsync(int minAge, int maxAge);
    Task<IEnumerable<Person>> GetPagedAsync(int pageNumber, int pageSize);
}
