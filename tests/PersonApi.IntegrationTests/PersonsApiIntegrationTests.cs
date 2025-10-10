using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PersonApi.Application.DTOs;

namespace PersonApi.IntegrationTests;

/// <summary>
/// Integration tests for Persons API using Testcontainers with real SQL Server
/// </summary>
public class PersonsApiIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;

    public PersonsApiIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    #region GET Tests

    [Fact]
    public async Task GetAllPersons_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/persons");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllPersons_WithPagination_ReturnsPersonsAndHeaders()
    {
        // Arrange - Create some test data first
        var createDto = new CreatePersonDto
        {
            Name = "Integration Test Person",
            Age = 30,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = new List<string> { "Testing" }
        };
        await _client.PostAsJsonAsync("/api/persons", createDto);

        // Act
        var response = await _client.GetAsync("/api/persons?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("X-Total-Count");
        response.Headers.Should().ContainKey("X-Page-Number");
        response.Headers.Should().ContainKey("X-Page-Size");

        var persons = await response.Content.ReadFromJsonAsync<List<PersonDto>>();
        persons.Should().NotBeNull();
        persons.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetPersonById_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/persons/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST Tests

    [Fact]
    public async Task CreatePerson_WithValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreatePersonDto
        {
            Name = "John Doe",
            Age = 30,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = new List<string> { "C#", ".NET", "Docker" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/persons", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var createdPerson = await response.Content.ReadFromJsonAsync<PersonDto>();
        createdPerson.Should().NotBeNull();
        createdPerson!.Name.Should().Be("John Doe");
        createdPerson.Age.Should().Be(30);
        createdPerson.Skills.Should().HaveCount(3);
    }

    [Fact]
    public async Task CreatePerson_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange - Missing required Name field
        var invalidDto = new
        {
            Age = 30,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = new List<string>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/persons", invalidDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT Tests

    [Fact]
    public async Task UpdatePerson_WithValidData_ReturnsNoContent()
    {
        // Arrange - Create a person first
        var createDto = new CreatePersonDto
        {
            Name = "Original Name",
            Age = 30,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = new List<string> { "C#" }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/persons", createDto);
        var createdPerson = await createResponse.Content.ReadFromJsonAsync<PersonDto>();

        var updateDto = new UpdatePersonDto
        {
            Name = "Updated Name",
            Age = 31,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = new List<string> { "C#", ".NET", "Docker" }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/persons/{createdPerson!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update
        var getResponse = await _client.GetAsync($"/api/persons/{createdPerson.Id}");
        var updatedPerson = await getResponse.Content.ReadFromJsonAsync<PersonDto>();
        updatedPerson!.Name.Should().Be("Updated Name");
        updatedPerson.Age.Should().Be(31);
        updatedPerson.Skills.Should().HaveCount(3);
    }

    [Fact]
    public async Task UpdatePerson_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdatePersonDto
        {
            Name = "Updated Name",
            Age = 31,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = new List<string> { "C#" }
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/persons/99999", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public async Task DeletePerson_WithValidId_ReturnsNoContent()
    {
        // Arrange - Create a person first
        var createDto = new CreatePersonDto
        {
            Name = "To Be Deleted",
            Age = 30,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = new List<string> { "C#" }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/persons", createDto);
        var createdPerson = await createResponse.Content.ReadFromJsonAsync<PersonDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/persons/{createdPerson!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/persons/{createdPerson.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePerson_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/persons/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchPersons_WithValidName_ReturnsMatchingPersons()
    {
        // Arrange - Create test persons
        var person1 = new CreatePersonDto
        {
            Name = "John Doe",
            Age = 30,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = new List<string> { "C#" }
        };
        var person2 = new CreatePersonDto
        {
            Name = "Jane Smith",
            Age = 25,
            DateOfBirth = new DateTime(1999, 5, 20),
            Skills = new List<string> { "Angular" }
        };

        await _client.PostAsJsonAsync("/api/persons", person1);
        await _client.PostAsJsonAsync("/api/persons", person2);

        // Act
        var response = await _client.GetAsync("/api/persons/search?name=John");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var persons = await response.Content.ReadFromJsonAsync<List<PersonDto>>();
        persons.Should().NotBeNull();
        persons.Should().Contain(p => p.Name == "John Doe");
        persons.Should().NotContain(p => p.Name == "Jane Smith");
    }

    [Fact]
    public async Task SearchPersons_WithEmptyName_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/persons/search?name=");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Full CRUD Workflow Test

    [Fact]
    public async Task FullCrudWorkflow_CreatesUpdatesAndDeletesPerson()
    {
        // 1. CREATE
        var createDto = new CreatePersonDto
        {
            Name = "Workflow Test Person",
            Age = 28,
            DateOfBirth = new DateTime(1996, 3, 10),
            Skills = new List<string> { "Testing", "Integration" }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/persons", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdPerson = await createResponse.Content.ReadFromJsonAsync<PersonDto>();
        createdPerson.Should().NotBeNull();

        // 2. READ
        var getResponse = await _client.GetAsync($"/api/persons/{createdPerson!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetchedPerson = await getResponse.Content.ReadFromJsonAsync<PersonDto>();
        fetchedPerson!.Name.Should().Be("Workflow Test Person");

        // 3. UPDATE
        var updateDto = new UpdatePersonDto
        {
            Name = "Updated Workflow Person",
            Age = 29,
            DateOfBirth = new DateTime(1996, 3, 10),
            Skills = new List<string> { "Testing", "Integration", "CRUD" }
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/persons/{createdPerson.Id}", updateDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getUpdatedResponse = await _client.GetAsync($"/api/persons/{createdPerson.Id}");
        var updatedPerson = await getUpdatedResponse.Content.ReadFromJsonAsync<PersonDto>();
        updatedPerson!.Name.Should().Be("Updated Workflow Person");
        updatedPerson.Age.Should().Be(29);
        updatedPerson.Skills.Should().HaveCount(3);

        // 4. DELETE
        var deleteResponse = await _client.DeleteAsync($"/api/persons/{createdPerson.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getDeletedResponse = await _client.GetAsync($"/api/persons/{createdPerson.Id}");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    public void Dispose()
    {
        _client.Dispose();
    }
}
