using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonApi.Domain.Entities;
using PersonApi.Infrastructure.Data;
using PersonApi.Infrastructure.Repositories;

namespace PersonApi.UnitTests.Repositories;

public class PersonRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PersonRepository _repository;

    public PersonRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new PersonRepository(_context);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenPersonExists_ReturnsPerson()
    {
        // Arrange
        var person = new Person
        {
            Name = "John Doe",
            Age = 30,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = "[]",
            CreatedAt = DateTime.UtcNow
        };
        await _context.Persons.AddAsync(person);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(person.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("John Doe");
        result.Age.Should().Be(30);
    }

    [Fact]
    public async Task GetByIdAsync_WhenPersonDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllPersons()
    {
        // Arrange
        var persons = new List<Person>
        {
            new() { Name = "John Doe", Age = 30, DateOfBirth = new DateTime(1994, 1, 15), Skills = "[]", CreatedAt = DateTime.UtcNow },
            new() { Name = "Jane Smith", Age = 25, DateOfBirth = new DateTime(1999, 5, 20), Skills = "[]", CreatedAt = DateTime.UtcNow }
        };
        await _context.Persons.AddRangeAsync(persons);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "John Doe");
        result.Should().Contain(p => p.Name == "Jane Smith");
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_AddsPersonToDatabase()
    {
        // Arrange
        var person = new Person
        {
            Name = "John Doe",
            Age = 30,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = "[]",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.AddAsync(person);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        var savedPerson = await _context.Persons.FindAsync(result.Id);
        savedPerson.Should().NotBeNull();
        savedPerson!.Name.Should().Be("John Doe");
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_UpdatesPersonInDatabase()
    {
        // Arrange
        var person = new Person
        {
            Name = "John Doe",
            Age = 30,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = "[]",
            CreatedAt = DateTime.UtcNow
        };
        await _context.Persons.AddAsync(person);
        await _context.SaveChangesAsync();

        // Act
        person.Name = "John Doe Updated";
        person.Age = 31;
        await _repository.UpdateAsync(person);

        // Assert
        var updatedPerson = await _context.Persons.FindAsync(person.Id);
        updatedPerson.Should().NotBeNull();
        updatedPerson!.Name.Should().Be("John Doe Updated");
        updatedPerson.Age.Should().Be(31);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_RemovesPersonFromDatabase()
    {
        // Arrange
        var person = new Person
        {
            Name = "John Doe",
            Age = 30,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = "[]",
            CreatedAt = DateTime.UtcNow
        };
        await _context.Persons.AddAsync(person);
        await _context.SaveChangesAsync();
        var personId = person.Id;

        // Act
        await _repository.DeleteAsync(personId);

        // Assert
        var deletedPerson = await _context.Persons.FindAsync(personId);
        deletedPerson.Should().BeNull();
    }

    #endregion

    #region GetByNameAsync Tests

    [Fact]
    public async Task GetByNameAsync_ReturnsMatchingPersons()
    {
        // Arrange
        var persons = new List<Person>
        {
            new() { Name = "John Doe", Age = 30, DateOfBirth = new DateTime(1994, 1, 15), Skills = "[]", CreatedAt = DateTime.UtcNow },
            new() { Name = "Jane Smith", Age = 25, DateOfBirth = new DateTime(1999, 5, 20), Skills = "[]", CreatedAt = DateTime.UtcNow },
            new() { Name = "Johnny Cash", Age = 45, DateOfBirth = new DateTime(1979, 2, 10), Skills = "[]", CreatedAt = DateTime.UtcNow }
        };
        await _context.Persons.AddRangeAsync(persons);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("John");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "John Doe");
        result.Should().Contain(p => p.Name == "Johnny Cash");
    }

    #endregion

    #region GetByAgeRangeAsync Tests

    [Fact]
    public async Task GetByAgeRangeAsync_ReturnsPersonsInRange()
    {
        // Arrange
        var persons = new List<Person>
        {
            new() { Name = "Young Person", Age = 20, DateOfBirth = new DateTime(2004, 1, 1), Skills = "[]", CreatedAt = DateTime.UtcNow },
            new() { Name = "Middle Person", Age = 30, DateOfBirth = new DateTime(1994, 1, 1), Skills = "[]", CreatedAt = DateTime.UtcNow },
            new() { Name = "Old Person", Age = 50, DateOfBirth = new DateTime(1974, 1, 1), Skills = "[]", CreatedAt = DateTime.UtcNow }
        };
        await _context.Persons.AddRangeAsync(persons);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByAgeRangeAsync(25, 35);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Middle Person");
    }

    #endregion

    #region GetPagedAsync Tests

    [Fact]
    public async Task GetPagedAsync_ReturnsCorrectPage()
    {
        // Arrange
        for (int i = 1; i <= 15; i++)
        {
            await _context.Persons.AddAsync(new Person
            {
                Name = $"Person {i}",
                Age = 20 + i,
                DateOfBirth = new DateTime(2000 - i, 1, 1),
                Skills = "[]",
                CreatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var page1 = await _repository.GetPagedAsync(1, 5);
        var page2 = await _repository.GetPagedAsync(2, 5);

        // Assert
        page1.Should().HaveCount(5);
        page2.Should().HaveCount(5);
        page1.Should().NotIntersectWith(page2);
    }

    #endregion

    #region CountAsync Tests

    [Fact]
    public async Task CountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var persons = new List<Person>
        {
            new() { Name = "Person 1", Age = 30, DateOfBirth = new DateTime(1994, 1, 15), Skills = "[]", CreatedAt = DateTime.UtcNow },
            new() { Name = "Person 2", Age = 25, DateOfBirth = new DateTime(1999, 5, 20), Skills = "[]", CreatedAt = DateTime.UtcNow },
            new() { Name = "Person 3", Age = 35, DateOfBirth = new DateTime(1989, 8, 10), Skills = "[]", CreatedAt = DateTime.UtcNow }
        };
        await _context.Persons.AddRangeAsync(persons);
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.CountAsync();

        // Assert
        count.Should().Be(3);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WhenPersonExists_ReturnsTrue()
    {
        // Arrange
        var person = new Person
        {
            Name = "John Doe",
            Age = 30,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = "[]",
            CreatedAt = DateTime.UtcNow
        };
        await _context.Persons.AddAsync(person);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsAsync(person.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenPersonDoesNotExist_ReturnsFalse()
    {
        // Act
        var exists = await _repository.ExistsAsync(999);

        // Assert
        exists.Should().BeFalse();
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
