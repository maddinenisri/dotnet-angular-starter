using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PersonApi.API.Controllers;
using PersonApi.Application.DTOs;
using PersonApi.Application.Interfaces;

namespace PersonApi.UnitTests.Controllers;

public class PersonsControllerTests
{
    private readonly Mock<IPersonService> _mockPersonService;
    private readonly Mock<ILogger<PersonsController>> _mockLogger;
    private readonly PersonsController _controller;

    public PersonsControllerTests()
    {
        _mockPersonService = new Mock<IPersonService>();
        _mockLogger = new Mock<ILogger<PersonsController>>();
        _controller = new PersonsController(_mockPersonService.Object, _mockLogger.Object);

        // Setup HttpContext for Response.Headers access
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithValidPagination_ReturnsOkResultWithPersons()
    {
        // Arrange
        var persons = new List<PersonDto>
        {
            new() { Id = 1, Name = "John Doe", Age = 30, DateOfBirth = new DateTime(1994, 1, 15), Skills = new List<string> { "C#", ".NET" } },
            new() { Id = 2, Name = "Jane Smith", Age = 25, DateOfBirth = new DateTime(1999, 5, 20), Skills = new List<string> { "Angular", "TypeScript" } }
        };

        _mockPersonService.Setup(s => s.GetPagedAsync(1, 10))
            .ReturnsAsync(persons);
        _mockPersonService.Setup(s => s.GetTotalCountAsync())
            .ReturnsAsync(2);

        // Act
        var result = await _controller.GetAll(1, 10);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPersons = okResult.Value.Should().BeAssignableTo<IEnumerable<PersonDto>>().Subject;
        returnedPersons.Should().HaveCount(2);
        returnedPersons.First().Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetAll_WithInvalidPageNumber_ReturnsBadRequest()
    {
        // Arrange
        var pageNumber = 0;
        var pageSize = 10;

        // Act
        var result = await _controller.GetAll(pageNumber, pageSize);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetAll_WithPageSizeGreaterThan100_LimitsTo100()
    {
        // Arrange
        _mockPersonService.Setup(s => s.GetPagedAsync(1, 100))
            .ReturnsAsync(new List<PersonDto>());
        _mockPersonService.Setup(s => s.GetTotalCountAsync())
            .ReturnsAsync(0);

        // Act
        var result = await _controller.GetAll(1, 500);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockPersonService.Verify(s => s.GetPagedAsync(1, 100), Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkResultWithPerson()
    {
        // Arrange
        var personId = 1;
        var person = new PersonDto
        {
            Id = personId,
            Name = "John Doe",
            Age = 30,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = new List<string> { "C#", ".NET" }
        };

        _mockPersonService.Setup(s => s.GetByIdAsync(personId))
            .ReturnsAsync(person);

        // Act
        var result = await _controller.GetById(personId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPerson = okResult.Value.Should().BeOfType<PersonDto>().Subject;
        returnedPerson.Id.Should().Be(personId);
        returnedPerson.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var personId = 999;
        _mockPersonService.Setup(s => s.GetByIdAsync(personId))
            .ReturnsAsync((PersonDto?)null);

        // Act
        var result = await _controller.GetById(personId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreatePersonDto
        {
            Name = "John Doe",
            Age = 30,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = new List<string> { "C#", ".NET" }
        };

        var createdPerson = new PersonDto
        {
            Id = 1,
            Name = createDto.Name,
            Age = createDto.Age,
            DateOfBirth = createDto.DateOfBirth,
            Skills = createDto.Skills,
            CreatedAt = DateTime.UtcNow
        };

        _mockPersonService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(createdPerson);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(PersonsController.GetById));
        var returnedPerson = createdResult.Value.Should().BeOfType<PersonDto>().Subject;
        returnedPerson.Name.Should().Be(createDto.Name);
        returnedPerson.Id.Should().Be(1);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var personId = 1;
        var updateDto = new UpdatePersonDto
        {
            Name = "John Doe Updated",
            Age = 31,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = new List<string> { "C#", ".NET", "Docker" }
        };

        var updatedPerson = new PersonDto
        {
            Id = personId,
            Name = updateDto.Name,
            Age = updateDto.Age,
            DateOfBirth = updateDto.DateOfBirth,
            Skills = updateDto.Skills
        };

        _mockPersonService.Setup(s => s.UpdateAsync(personId, updateDto))
            .ReturnsAsync(updatedPerson);

        // Act
        var result = await _controller.Update(personId, updateDto);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockPersonService.Verify(s => s.UpdateAsync(personId, updateDto), Times.Once);
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsNotFound()
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

        _mockPersonService.Setup(s => s.UpdateAsync(personId, updateDto))
            .ReturnsAsync((PersonDto?)null);

        // Act
        var result = await _controller.Update(personId, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var personId = 1;
        _mockPersonService.Setup(s => s.DeleteAsync(personId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(personId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockPersonService.Verify(s => s.DeleteAsync(personId), Times.Once);
    }

    [Fact]
    public async Task Delete_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var personId = 999;
        _mockPersonService.Setup(s => s.DeleteAsync(personId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(personId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task Search_WithValidName_ReturnsOkResultWithMatchingPersons()
    {
        // Arrange
        var searchTerm = "John";
        var persons = new List<PersonDto>
        {
            new() { Id = 1, Name = "John Doe", Age = 30, DateOfBirth = new DateTime(1994, 1, 15), Skills = new List<string> { "C#" } },
            new() { Id = 2, Name = "Johnny Cash", Age = 45, DateOfBirth = new DateTime(1979, 2, 10), Skills = new List<string> { "Music" } }
        };

        _mockPersonService.Setup(s => s.SearchByNameAsync(searchTerm))
            .ReturnsAsync(persons);

        // Act
        var result = await _controller.Search(searchTerm);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPersons = okResult.Value.Should().BeAssignableTo<IEnumerable<PersonDto>>().Subject;
        returnedPersons.Should().HaveCount(2);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Search_WithEmptyName_ReturnsBadRequest(string? searchTerm)
    {
        // Act
        var result = await _controller.Search(searchTerm);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion
}
