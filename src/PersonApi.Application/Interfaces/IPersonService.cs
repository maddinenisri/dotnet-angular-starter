using PersonApi.Application.DTOs;

namespace PersonApi.Application.Interfaces;

/// <summary>
/// Service interface for Person business logic
/// </summary>
public interface IPersonService
{
    Task<PersonDto?> GetByIdAsync(int id);
    Task<IEnumerable<PersonDto>> GetAllAsync();
    Task<IEnumerable<PersonDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<PersonDto> CreateAsync(CreatePersonDto createDto);
    Task<PersonDto?> UpdateAsync(int id, UpdatePersonDto updateDto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<PersonDto>> SearchByNameAsync(string name);
    Task<int> GetTotalCountAsync();
}
