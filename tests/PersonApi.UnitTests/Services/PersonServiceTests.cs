using FluentAssertions;
using Moq;
using PersonApi.Application.DTOs;
using PersonApi.Application.Services;
using PersonApi.Domain.Entities;
using PersonApi.Infrastructure.Repositories;
using System.Text.Json;

namespace PersonApi.UnitTests.Services;

public class PersonServiceTests
{
    private readonly Mock<IPersonRepository> _mockRepository;
    private readonly PersonService _service;

    public PersonServiceTests()
    {
        _mockRepository = new Mock<IPersonRepository>();
        _service = new PersonService(_mockRepository.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenPersonExists_ReturnsPersonDto()
    {
        // Arrange
        var personId = 1;
        var person = new Person
        {
            Id = personId,
            Name = "John Doe",
            Age = 30,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = JsonSerializer.Serialize(new List<string> { "C#", ".NET" }),
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.GetByIdAsync(personId))
            .ReturnsAsync(person);

        // Act
        var result = await _service.GetByIdAsync(personId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(personId);
        result.Name.Should().Be("John Doe");
        result.Age.Should().Be(30);
        result.Skills.Should().HaveCount(2);
        result.Skills.Should().Contain("C#");
        _mockRepository.Verify(r => r.GetByIdAsync(personId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenPersonDoesNotExist_ReturnsNull()
    {
        // Arrange
        var personId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(personId))
            .ReturnsAsync((Person?)null);

        // Act
        var result = await _service.GetByIdAsync(personId);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(personId), Times.Once);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllPersons()
    {
        // Arrange
        var persons = new List<Person>
        {
            new() { Id = 1, Name = "John Doe", Age = 30, DateOfBirth = new DateTime(1994, 1, 15), Skills = "[]", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Jane Smith", Age = 25, DateOfBirth = new DateTime(1999, 5, 20), Skills = "[]", CreatedAt = DateTime.UtcNow }
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(persons);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("John Doe");
        result.Last().Name.Should().Be("Jane Smith");
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoPersons_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Person>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedPerson()
    {
        // Arrange
        var createDto = new CreatePersonDto
        {
            Name = "John Doe",
            Age = 30,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = new List<string> { "C#", ".NET", "Docker" }
        };

        var createdPerson = new Person
        {
            Id = 1,
            Name = createDto.Name,
            Age = createDto.Age,
            DateOfBirth = createDto.DateOfBirth,
            Skills = JsonSerializer.Serialize(createDto.Skills),
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Person>()))
            .ReturnsAsync(createdPerson);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("John Doe");
        result.Age.Should().Be(30);
        result.Skills.Should().HaveCount(3);
        result.Skills.Should().Contain("Docker");
        _mockRepository.Verify(r => r.AddAsync(It.Is<Person>(p =>
            p.Name == createDto.Name &&
            p.Age == createDto.Age &&
            p.DateOfBirth == createDto.DateOfBirth
        )), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithEmptySkills_CreatesPersonWithEmptySkillsList()
    {
        // Arrange
        var createDto = new CreatePersonDto
        {
            Name = "John Doe",
            Age = 30,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = new List<string>()
        };

        var createdPerson = new Person
        {
            Id = 1,
            Name = createDto.Name,
            Age = createDto.Age,
            DateOfBirth = createDto.DateOfBirth,
            Skills = "[]",
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Person>()))
            .ReturnsAsync(createdPerson);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Skills.Should().BeEmpty();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidData_ReturnsUpdatedPerson()
    {
        // Arrange
        var personId = 1;
        var existingPerson = new Person
        {
            Id = personId,
            Name = "John Doe",
            Age = 30,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = "[]",
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        var updateDto = new UpdatePersonDto
        {
            Name = "John Doe Updated",
            Age = 31,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = new List<string> { "C#", ".NET", "Docker" }
        };

        _mockRepository.Setup(r => r.GetByIdAsync(personId))
            .ReturnsAsync(existingPerson);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Person>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(personId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(personId);
        result.Name.Should().Be("John Doe Updated");
        result.Age.Should().Be(31);
        result.Skills.Should().HaveCount(3);
        result.UpdatedAt.Should().NotBeNull();
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Person>(p =>
            p.Id == personId &&
            p.Name == updateDto.Name &&
            p.UpdatedAt != null
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenPersonDoesNotExist_ReturnsNull()
    {
        // Arrange
        var personId = 999;
        var updateDto = new UpdatePersonDto
        {
            Name = "John Doe",
            Age = 30,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = new List<string> { "C#" }
        };

        _mockRepository.Setup(r => r.GetByIdAsync(personId))
            .ReturnsAsync((Person?)null);

        // Act
        var result = await _service.UpdateAsync(personId, updateDto);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Person>()), Times.Never);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenPersonExists_ReturnsTrue()
    {
        // Arrange
        var personId = 1;
        _mockRepository.Setup(r => r.ExistsAsync(personId))
            .ReturnsAsync(true);
        _mockRepository.Setup(r => r.DeleteAsync(personId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync(personId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(personId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenPersonDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var personId = 999;
        _mockRepository.Setup(r => r.ExistsAsync(personId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteAsync(personId);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region SearchByNameAsync Tests

    [Fact]
    public async Task SearchByNameAsync_ReturnsMatchingPersons()
    {
        // Arrange
        var searchTerm = "John";
        var persons = new List<Person>
        {
            new() { Id = 1, Name = "John Doe", Age = 30, DateOfBirth = new DateTime(1994, 1, 15), Skills = "[]", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Johnny Cash", Age = 45, DateOfBirth = new DateTime(1979, 2, 10), Skills = "[]", CreatedAt = DateTime.UtcNow }
        };

        _mockRepository.Setup(r => r.GetByNameAsync(searchTerm))
            .ReturnsAsync(persons);

        // Act
        var result = await _service.SearchByNameAsync(searchTerm);

        // Assert
        result.Should().HaveCount(2);
        result.All(p => p.Name.Contains("John", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }

    #endregion

    #region GetTotalCountAsync Tests

    [Fact]
    public async Task GetTotalCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var expectedCount = 42;
        _mockRepository.Setup(r => r.CountAsync())
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _service.GetTotalCountAsync();

        // Assert
        result.Should().Be(expectedCount);
        _mockRepository.Verify(r => r.CountAsync(), Times.Once);
    }

    #endregion
}
