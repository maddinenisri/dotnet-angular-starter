using System.Text.Json;
using PersonApi.Application.DTOs;
using PersonApi.Application.Interfaces;
using PersonApi.Domain.Entities;
using PersonApi.Infrastructure.Repositories;

namespace PersonApi.Application.Services;

/// <summary>
/// Service implementation for Person business logic
/// </summary>
public class PersonService : IPersonService
{
    private readonly IPersonRepository _personRepository;

    public PersonService(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    public async Task<PersonDto?> GetByIdAsync(int id)
    {
        var person = await _personRepository.GetByIdAsync(id);
        return person != null ? MapToDto(person) : null;
    }

    public async Task<IEnumerable<PersonDto>> GetAllAsync()
    {
        var persons = await _personRepository.GetAllAsync();
        return persons.Select(MapToDto);
    }

    public async Task<IEnumerable<PersonDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var persons = await _personRepository.GetPagedAsync(pageNumber, pageSize);
        return persons.Select(MapToDto);
    }

    public async Task<PersonDto> CreateAsync(CreatePersonDto createDto)
    {
        var person = new Person
        {
            Name = createDto.Name,
            Age = createDto.Age,
            DateOfBirth = createDto.DateOfBirth,
            Skills = JsonSerializer.Serialize(createDto.Skills),
            CreatedAt = DateTime.UtcNow
        };

        var created = await _personRepository.AddAsync(person);
        return MapToDto(created);
    }

    public async Task<PersonDto?> UpdateAsync(int id, UpdatePersonDto updateDto)
    {
        var person = await _personRepository.GetByIdAsync(id);
        if (person == null)
            return null;

        person.Name = updateDto.Name;
        person.Age = updateDto.Age;
        person.DateOfBirth = updateDto.DateOfBirth;
        person.Skills = JsonSerializer.Serialize(updateDto.Skills);
        person.UpdatedAt = DateTime.UtcNow;

        await _personRepository.UpdateAsync(person);
        return MapToDto(person);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var exists = await _personRepository.ExistsAsync(id);
        if (!exists)
            return false;

        await _personRepository.DeleteAsync(id);
        return true;
    }

    public async Task<IEnumerable<PersonDto>> SearchByNameAsync(string name)
    {
        var persons = await _personRepository.GetByNameAsync(name);
        return persons.Select(MapToDto);
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _personRepository.CountAsync();
    }

    private static PersonDto MapToDto(Person person)
    {
        return new PersonDto
        {
            Id = person.Id,
            Name = person.Name,
            Age = person.Age,
            DateOfBirth = person.DateOfBirth,
            Skills = string.IsNullOrWhiteSpace(person.Skills)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(person.Skills) ?? new List<string>(),
            CreatedAt = person.CreatedAt,
            UpdatedAt = person.UpdatedAt
        };
    }
}
