# .NET Application Template - Preferred Standards & Architecture

**Version:** 1.0
**Last Updated:** 2025-10-10
**Target Framework:** .NET 9.0
**Purpose:** Reusable template for building production-ready .NET applications with Clean Architecture

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Solution Structure](#solution-structure)
3. [Technology Stack](#technology-stack)
4. [Coding Standards](#coding-standards)
5. [Testing Strategy](#testing-strategy)
6. [Step-by-Step Implementation Guide](#step-by-step-implementation-guide)
7. [Docker & DevOps](#docker--devops)
8. [Best Practices Checklist](#best-practices-checklist)

---

## Architecture Overview

### Clean Architecture - 4 Layer Design

```
┌─────────────────────────────────────────────────┐
│                  API Layer                       │
│  (Controllers, Middleware, Program.cs)          │
│  - HTTP endpoints                                │
│  - Global exception handling                     │
│  - Swagger/OpenAPI documentation                 │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┐
│            Application Layer                     │
│  (DTOs, Services, Interfaces)                   │
│  - Business logic                                │
│  - Data transformation                           │
│  - Service orchestration                         │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┐
│          Infrastructure Layer                    │
│  (DbContext, Repositories, EF Core)             │
│  - Data access                                   │
│  - Repository pattern                            │
│  - Database migrations                           │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┐
│              Domain Layer                        │
│  (Entities, Domain Models)                      │
│  - Core business entities                        │
│  - No external dependencies                      │
│  - Pure POCO classes                             │
└─────────────────────────────────────────────────┘
```

### Dependency Flow

- **API** → Application → Infrastructure → Domain
- **Application** → Infrastructure → Domain
- **Infrastructure** → Domain
- **Domain** → No dependencies (pure)

**Rule:** Dependencies flow inward only. Inner layers have no knowledge of outer layers.

---

## Solution Structure

### Directory Layout

```
solution-name/
├── src/
│   ├── ProjectName.API/              # Web API layer
│   ├── ProjectName.Application/       # Business logic layer
│   ├── ProjectName.Infrastructure/    # Data access layer
│   └── ProjectName.Domain/            # Domain entities
├── tests/
│   ├── ProjectName.UnitTests/         # Unit tests (Moq, InMemory DB)
│   └── ProjectName.IntegrationTests/  # Integration tests (Testcontainers)
├── docker/
│   └── sql/                           # SQL initialization scripts
├── docker-compose.yml                 # Container orchestration
├── Dockerfile                         # Multi-stage build
├── Makefile                           # Build automation
├── .dockerignore                      # Docker exclusions
├── .env                               # Environment variables
├── README.md                          # Project documentation
└── ProjectName.sln                    # Solution file
```

### Project Creation Commands

```bash
# Create solution
dotnet new sln -n ProjectName

# Create projects
dotnet new webapi -n ProjectName.API -o src/ProjectName.API
dotnet new classlib -n ProjectName.Application -o src/ProjectName.Application
dotnet new classlib -n ProjectName.Infrastructure -o src/ProjectName.Infrastructure
dotnet new classlib -n ProjectName.Domain -o src/ProjectName.Domain

# Create test projects
dotnet new xunit -n ProjectName.UnitTests -o tests/ProjectName.UnitTests
dotnet new xunit -n ProjectName.IntegrationTests -o tests/ProjectName.IntegrationTests

# Add projects to solution
dotnet sln add src/ProjectName.API/ProjectName.API.csproj
dotnet sln add src/ProjectName.Application/ProjectName.Application.csproj
dotnet sln add src/ProjectName.Infrastructure/ProjectName.Infrastructure.csproj
dotnet sln add src/ProjectName.Domain/ProjectName.Domain.csproj
dotnet sln add tests/ProjectName.UnitTests/ProjectName.UnitTests.csproj
dotnet sln add tests/ProjectName.IntegrationTests/ProjectName.IntegrationTests.csproj

# Setup project references
dotnet add src/ProjectName.API/ProjectName.API.csproj reference src/ProjectName.Application/ProjectName.Application.csproj
dotnet add src/ProjectName.API/ProjectName.API.csproj reference src/ProjectName.Infrastructure/ProjectName.Infrastructure.csproj
dotnet add src/ProjectName.Application/ProjectName.Application.csproj reference src/ProjectName.Domain/ProjectName.Domain.csproj
dotnet add src/ProjectName.Application/ProjectName.Application.csproj reference src/ProjectName.Infrastructure/ProjectName.Infrastructure.csproj
dotnet add src/ProjectName.Infrastructure/ProjectName.Infrastructure.csproj reference src/ProjectName.Domain/ProjectName.Domain.csproj

# Test project references
dotnet add tests/ProjectName.UnitTests/ProjectName.UnitTests.csproj reference src/ProjectName.API/ProjectName.API.csproj
dotnet add tests/ProjectName.UnitTests/ProjectName.UnitTests.csproj reference src/ProjectName.Application/ProjectName.Application.csproj
dotnet add tests/ProjectName.UnitTests/ProjectName.UnitTests.csproj reference src/ProjectName.Infrastructure/ProjectName.Infrastructure.csproj
dotnet add tests/ProjectName.UnitTests/ProjectName.UnitTests.csproj reference src/ProjectName.Domain/ProjectName.Domain.csproj
dotnet add tests/ProjectName.IntegrationTests/ProjectName.IntegrationTests.csproj reference src/ProjectName.API/ProjectName.API.csproj
```

---

## Technology Stack

### Core Frameworks

| Component | Technology | Version |
|-----------|------------|---------|
| Runtime | .NET | 9.0 |
| Web Framework | ASP.NET Core | 9.0 |
| ORM | Entity Framework Core | 9.0 |
| Database | SQL Server | 2022 |
| Container Platform | Docker | Latest |

### NuGet Packages

#### API Project
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.9" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.9" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.2.0" />
```

#### Infrastructure Project
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.9" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.9" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.9" />
```

#### Application Project
```xml
<PackageReference Include="System.Text.Json" Version="9.0.9" />
```

#### Unit Tests Project
```xml
<PackageReference Include="coverlet.collector" Version="6.0.2" />
<PackageReference Include="FluentAssertions" Version="8.7.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.9" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
<PackageReference Include="Moq" Version="4.20.72" />
<PackageReference Include="xunit" Version="2.9.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
```

#### Integration Tests Project
```xml
<PackageReference Include="coverlet.collector" Version="6.0.2" />
<PackageReference Include="FluentAssertions" Version="8.7.1" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.9" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
<PackageReference Include="Testcontainers.MsSql" Version="4.7.0" />
<PackageReference Include="xunit" Version="2.9.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
```

---

## Coding Standards

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Classes | PascalCase | `PersonService` |
| Interfaces | I + PascalCase | `IPersonRepository` |
| Methods | PascalCase | `GetByIdAsync` |
| Properties | PascalCase | `FirstName` |
| Private Fields | _camelCase | `_personRepository` |
| Parameters | camelCase | `personId` |
| Local Variables | camelCase | `totalCount` |
| Constants | PascalCase | `MaxPageSize` |
| Async Methods | Suffix with Async | `CreateAsync` |

### File Organization

**One class per file** - File name matches class name

```
Infrastructure/
├── Data/
│   ├── AppDbContext.cs
│   └── Migrations/
├── Repositories/
│   ├── IRepository.cs
│   ├── Repository.cs
│   ├── IPersonRepository.cs
│   └── PersonRepository.cs
```

### Domain Layer Pattern

```csharp
namespace ProjectName.Domain.Entities;

/// <summary>
/// Represents a domain entity
/// </summary>
public class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Skills { get; set; } = string.Empty; // JSON serialized
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
```

**Rules:**
- Pure POCO classes
- No external dependencies
- No business logic in entities (anemic domain model acceptable for CRUD apps)
- Use `DateTime.UtcNow` for timestamps
- Nullable reference types enabled (`<Nullable>enable</Nullable>`)

### Infrastructure Layer Pattern

#### DbContext

```csharp
using Microsoft.EntityFrameworkCore;
using ProjectName.Domain.Entities;

namespace ProjectName.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Person> Persons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Skills).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(e => e.Name);
        });
    }
}
```

#### Generic Repository Pattern

```csharp
namespace ProjectName.Infrastructure.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<int> CountAsync();
    Task<bool> ExistsAsync(int id);
}

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public virtual async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.AnyAsync(e => EF.Property<int>(e, "Id") == id);
    }
}
```

#### Specific Repository

```csharp
public interface IPersonRepository : IRepository<Person>
{
    Task<IEnumerable<Person>> GetByNameAsync(string name);
    Task<IEnumerable<Person>> GetByAgeRangeAsync(int minAge, int maxAge);
    Task<IEnumerable<Person>> GetPagedAsync(int pageNumber, int pageSize);
}

public class PersonRepository : Repository<Person>, IPersonRepository
{
    public PersonRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Person>> GetByNameAsync(string name)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.Name.Contains(name))
            .ToListAsync();
    }

    public async Task<IEnumerable<Person>> GetByAgeRangeAsync(int minAge, int maxAge)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.Age >= minAge && p.Age <= maxAge)
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
```

### Application Layer Pattern

#### DTOs

```csharp
using System.ComponentModel.DataAnnotations;

namespace ProjectName.Application.DTOs;

// Response DTO
public class PersonDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime DateOfBirth { get; set; }
    public List<string> Skills { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// Create DTO
public class CreatePersonDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Range(0, 150, ErrorMessage = "Age must be between 0 and 150")]
    public int Age { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; set; }

    public List<string> Skills { get; set; } = new();
}

// Update DTO
public class UpdatePersonDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Range(0, 150, ErrorMessage = "Age must be between 0 and 150")]
    public int Age { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; set; }

    public List<string> Skills { get; set; } = new();
}
```

#### Service Interface & Implementation

```csharp
namespace ProjectName.Application.Interfaces;

public interface IPersonService
{
    Task<PersonDto?> GetByIdAsync(int id);
    Task<IEnumerable<PersonDto>> GetAllAsync();
    Task<PersonDto> CreateAsync(CreatePersonDto createDto);
    Task<PersonDto?> UpdateAsync(int id, UpdatePersonDto updateDto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<PersonDto>> SearchByNameAsync(string name);
    Task<IEnumerable<PersonDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<int> GetTotalCountAsync();
}
```

```csharp
using System.Text.Json;
using ProjectName.Application.DTOs;
using ProjectName.Application.Interfaces;
using ProjectName.Domain.Entities;
using ProjectName.Infrastructure.Repositories;

namespace ProjectName.Application.Services;

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
        return person == null ? null : MapToDto(person);
    }

    public async Task<IEnumerable<PersonDto>> GetAllAsync()
    {
        var persons = await _personRepository.GetAllAsync();
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
        if (person == null) return null;

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
        if (!await _personRepository.ExistsAsync(id))
            return false;

        await _personRepository.DeleteAsync(id);
        return true;
    }

    public async Task<IEnumerable<PersonDto>> SearchByNameAsync(string name)
    {
        var persons = await _personRepository.GetByNameAsync(name);
        return persons.Select(MapToDto);
    }

    public async Task<IEnumerable<PersonDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var persons = await _personRepository.GetPagedAsync(pageNumber, pageSize);
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
            Skills = JsonSerializer.Deserialize<List<string>>(person.Skills) ?? new List<string>(),
            CreatedAt = person.CreatedAt,
            UpdatedAt = person.UpdatedAt
        };
    }
}
```

### API Layer Pattern

#### Controller

```csharp
using Microsoft.AspNetCore.Mvc;
using ProjectName.Application.DTOs;
using ProjectName.Application.Interfaces;

namespace ProjectName.API.Controllers;

/// <summary>
/// Controller for managing entities
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PersonsController : ControllerBase
{
    private readonly IPersonService _personService;
    private readonly ILogger<PersonsController> _logger;

    public PersonsController(IPersonService personService, ILogger<PersonsController> logger)
    {
        _personService = personService;
        _logger = logger;
    }

    /// <summary>
    /// Get all persons with optional pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PersonDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Getting persons - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

        pageSize = Math.Min(pageSize, 100); // Limit page size

        if (pageNumber <= 0 || pageSize <= 0)
            return BadRequest(new { message = "Page number and page size must be greater than 0" });

        var persons = await _personService.GetPagedAsync(pageNumber, pageSize);
        var total = await _personService.GetTotalCountAsync();

        Response.Headers.Append("X-Total-Count", total.ToString());
        Response.Headers.Append("X-Page-Number", pageNumber.ToString());
        Response.Headers.Append("X-Page-Size", pageSize.ToString());

        return Ok(persons);
    }

    /// <summary>
    /// Get person by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PersonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("Getting person with ID: {Id}", id);

        var person = await _personService.GetByIdAsync(id);

        if (person == null)
        {
            _logger.LogWarning("Person with ID {Id} not found", id);
            return NotFound(new { message = $"Person with ID {id} not found" });
        }

        return Ok(person);
    }

    /// <summary>
    /// Create a new person
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PersonDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePersonDto createDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _logger.LogInformation("Creating new person: {Name}", createDto.Name);

        var person = await _personService.CreateAsync(createDto);

        return CreatedAtAction(nameof(GetById), new { id = person.Id }, person);
    }

    /// <summary>
    /// Update an existing person
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePersonDto updateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _logger.LogInformation("Updating person with ID: {Id}", id);

        var updated = await _personService.UpdateAsync(id, updateDto);

        if (updated == null)
        {
            _logger.LogWarning("Person with ID {Id} not found for update", id);
            return NotFound(new { message = $"Person with ID {id} not found" });
        }

        return NoContent();
    }

    /// <summary>
    /// Delete a person
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Deleting person with ID: {Id}", id);

        var result = await _personService.DeleteAsync(id);

        if (!result)
        {
            _logger.LogWarning("Person with ID {Id} not found for deletion", id);
            return NotFound(new { message = $"Person with ID {id} not found" });
        }

        return NoContent();
    }

    /// <summary>
    /// Search persons by name
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<PersonDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { message = "Search term cannot be empty" });

        _logger.LogInformation("Searching persons by name: {Name}", name);

        var persons = await _personService.SearchByNameAsync(name);
        return Ok(persons);
    }
}
```

#### Global Exception Handling Middleware

```csharp
using System.Net;
using System.Text.Json;

namespace ProjectName.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            ArgumentNullException => HttpStatusCode.BadRequest,
            ArgumentException => HttpStatusCode.BadRequest,
            KeyNotFoundException => HttpStatusCode.NotFound,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            _ => HttpStatusCode.InternalServerError
        };

        var response = new
        {
            error = exception.Message,
            statusCode = (int)statusCode
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
```

#### Program.cs

```csharp
using Microsoft.EntityFrameworkCore;
using ProjectName.API.Middleware;
using ProjectName.Application.Interfaces;
using ProjectName.Application.Services;
using ProjectName.Infrastructure.Data;
using ProjectName.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IPersonRepository, PersonRepository>();

// Register Services
builder.Services.AddScoped<IPersonService, PersonService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("X-Total-Count", "X-Page-Number", "X-Page-Size");
    });
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Project API",
        Version = "v1",
        Description = "API Description",
        Contact = new() { Name = "Support" }
    });

    // Enable XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Project API V1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

// Global Exception Handler Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Logger.LogInformation("Application started successfully");
app.Logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

app.Run();

// Make the implicit Program class public for testing
public partial class Program { }
```

---

## Testing Strategy

### Testing Pyramid

```
        ┌─────────────┐
        │     E2E     │  ← Few (Manual/Automated UI)
        └─────────────┘
       ┌───────────────┐
       │ Integration   │  ← Some (Testcontainers)
       └───────────────┘
     ┌───────────────────┐
     │   Unit Tests      │  ← Many (Fast, Isolated)
     └───────────────────┘
```

### Unit Tests (Fast, Isolated)

**Framework:** xUnit + Moq + FluentAssertions + InMemory Database

**Coverage Targets:**
- Controllers: Test HTTP logic, validation, response codes
- Services: Test business logic with mocked repositories
- Repositories: Test data access with InMemory database

#### Controller Unit Test Pattern

```csharp
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectName.API.Controllers;
using ProjectName.Application.DTOs;
using ProjectName.Application.Interfaces;

namespace ProjectName.UnitTests.Controllers;

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
            Skills = createDto.Skills
        };

        _mockPersonService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(createdPerson);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(PersonsController.GetById));
        var returnedPerson = createdResult.Value.Should().BeOfType<PersonDto>().Subject;
        returnedPerson.Id.Should().Be(1);
    }
}
```

#### Service Unit Test Pattern

```csharp
using FluentAssertions;
using Moq;
using ProjectName.Application.DTOs;
using ProjectName.Application.Services;
using ProjectName.Domain.Entities;
using ProjectName.Infrastructure.Repositories;
using System.Text.Json;

namespace ProjectName.UnitTests.Services;

public class PersonServiceTests
{
    private readonly Mock<IPersonRepository> _mockRepository;
    private readonly PersonService _service;

    public PersonServiceTests()
    {
        _mockRepository = new Mock<IPersonRepository>();
        _service = new PersonService(_mockRepository.Object);
    }

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
            Skills = JsonSerializer.Serialize(new List<string> { "C#", ".NET" })
        };

        _mockRepository.Setup(r => r.GetByIdAsync(personId))
            .ReturnsAsync(person);

        // Act
        var result = await _service.GetByIdAsync(personId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(personId);
        result.Name.Should().Be("John Doe");
        result.Skills.Should().HaveCount(2);
        _mockRepository.Verify(r => r.GetByIdAsync(personId), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedPerson()
    {
        // Arrange
        var createDto = new CreatePersonDto
        {
            Name = "John Doe",
            Age = 30,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = new List<string> { "C#", ".NET" }
        };

        var createdPerson = new Person
        {
            Id = 1,
            Name = createDto.Name,
            Age = createDto.Age,
            Skills = JsonSerializer.Serialize(createDto.Skills)
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Person>()))
            .ReturnsAsync(createdPerson);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Skills.Should().HaveCount(2);
        _mockRepository.Verify(r => r.AddAsync(It.Is<Person>(p =>
            p.Name == createDto.Name &&
            p.Age == createDto.Age
        )), Times.Once);
    }
}
```

#### Repository Unit Test Pattern

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectName.Domain.Entities;
using ProjectName.Infrastructure.Data;
using ProjectName.Infrastructure.Repositories;

namespace ProjectName.UnitTests.Repositories;

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

    [Fact]
    public async Task GetByIdAsync_WhenPersonExists_ReturnsPerson()
    {
        // Arrange
        var person = new Person
        {
            Name = "John Doe",
            Age = 30,
            DateOfBirth = new DateTime(1994, 1, 15),
            Skills = "[]"
        };
        await _context.Persons.AddAsync(person);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(person.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task AddAsync_AddsPersonToDatabase()
    {
        // Arrange
        var person = new Person
        {
            Name = "John Doe",
            Age = 30,
            Skills = "[]"
        };

        // Act
        var result = await _repository.AddAsync(person);

        // Assert
        result.Id.Should().BeGreaterThan(0);
        var savedPerson = await _context.Persons.FindAsync(result.Id);
        savedPerson.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

### Integration Tests (Testcontainers)

**Framework:** xUnit + Testcontainers + WebApplicationFactory + FluentAssertions

**Purpose:** Test against real SQL Server with full HTTP request/response cycle

#### Custom WebApplicationFactory with Testcontainers

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectName.Infrastructure.Data;
using Testcontainers.MsSql;

namespace ProjectName.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory that uses Testcontainers for SQL Server
/// </summary>
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer;

    public IntegrationTestWebAppFactory()
    {
        // Create SQL Server container
        // Uses Rosetta emulation on Mac M-series for SQL Server 2022
        _msSqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("YourStrong@Password123")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext with Testcontainers connection string
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(_msSqlContainer.GetConnectionString());
            });

            // Run migrations
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        // Start SQL Server container before tests
        await _msSqlContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        // Cleanup container after tests
        await _msSqlContainer.DisposeAsync();
    }
}
```

#### Integration Test Pattern

```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectName.Application.DTOs;

namespace ProjectName.IntegrationTests;

public class PersonsApiIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;

    public PersonsApiIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

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
    }

    [Fact]
    public async Task FullCrudWorkflow_CreatesUpdatesAndDeletesPerson()
    {
        // CREATE
        var createDto = new CreatePersonDto
        {
            Name = "Workflow Test",
            Age = 28,
            DateOfBirth = new DateTime(1996, 3, 10),
            Skills = new List<string> { "Testing" }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/persons", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdPerson = await createResponse.Content.ReadFromJsonAsync<PersonDto>();

        // READ
        var getResponse = await _client.GetAsync($"/api/persons/{createdPerson!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // UPDATE
        var updateDto = new UpdatePersonDto
        {
            Name = "Updated Name",
            Age = 29,
            DateOfBirth = new DateTime(1996, 3, 10),
            Skills = new List<string> { "Testing", "Integration" }
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/persons/{createdPerson.Id}", updateDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // DELETE
        var deleteResponse = await _client.DeleteAsync($"/api/persons/{createdPerson.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getDeletedResponse = await _client.GetAsync($"/api/persons/{createdPerson.Id}");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
```

### Test Execution

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test --filter "FullyQualifiedName~UnitTests"

# Run integration tests only
dotnet test --filter "FullyQualifiedName~IntegrationTests"

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

---

## Step-by-Step Implementation Guide

### Phase 1: Project Setup

1. **Create solution structure**
```bash
# Run all project creation commands from "Solution Structure" section
```

2. **Add NuGet packages**
```bash
# Add packages as per "Technology Stack" section
```

3. **Enable nullable reference types** in all projects
```xml
<PropertyGroup>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

4. **Enable XML documentation** in API project
```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

### Phase 2: Domain Layer

1. Create entity classes in `Domain/Entities/`
2. Use POCO pattern with no dependencies
3. Add XML documentation comments

### Phase 3: Infrastructure Layer

1. **Create DbContext** with fluent configuration
2. **Implement generic repository** pattern
3. **Create specific repositories** with custom queries
4. **Add connection strings** to appsettings.json:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=ProjectDb;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True;"
  }
}
```

5. **Create initial migration**:
```bash
dotnet ef migrations add InitialCreate --project src/ProjectName.Infrastructure --startup-project src/ProjectName.API
```

### Phase 4: Application Layer

1. Create DTOs (Response, Create, Update)
2. Create service interfaces
3. Implement services with mapping logic
4. Use `System.Text.Json` for JSON serialization

### Phase 5: API Layer

1. **Create controllers** with full XML documentation
2. **Create global exception middleware**
3. **Configure Program.cs**:
   - DI registration
   - CORS policy
   - Swagger/OpenAPI
   - Health checks
4. **Add logging** throughout

### Phase 6: Testing

1. **Setup unit tests**:
   - Controller tests with Moq
   - Service tests with Moq
   - Repository tests with InMemory database

2. **Setup integration tests**:
   - Create IntegrationTestWebAppFactory with Testcontainers
   - Write end-to-end API tests
   - Test full CRUD workflows

### Phase 7: Docker

1. Create Dockerfile (multi-stage build)
2. Create docker-compose.yml
3. Create .dockerignore
4. Test containerized application

---

## Docker & DevOps

### Dockerfile (Multi-stage Build)

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["src/ProjectName.API/ProjectName.API.csproj", "src/ProjectName.API/"]
COPY ["src/ProjectName.Application/ProjectName.Application.csproj", "src/ProjectName.Application/"]
COPY ["src/ProjectName.Infrastructure/ProjectName.Infrastructure.csproj", "src/ProjectName.Infrastructure/"]
COPY ["src/ProjectName.Domain/ProjectName.Domain.csproj", "src/ProjectName.Domain/"]

# Restore dependencies
RUN dotnet restore "src/ProjectName.API/ProjectName.API.csproj"

# Copy remaining source code
COPY . .

# Build and publish
WORKDIR "/src/src/ProjectName.API"
RUN dotnet build "ProjectName.API.csproj" -c Release -o /app/build
RUN dotnet publish "ProjectName.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 5000

# Create non-root user for security
RUN addgroup --system --gid 1000 appuser && \
    adduser --system --uid 1000 --ingroup appuser --shell /bin/sh appuser

# Copy published files
COPY --from=build /app/publish .

# Switch to non-root user
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "ProjectName.API.dll"]
```

### docker-compose.yml

```yaml
version: '3.8'

services:
  db:
    image: mcr.microsoft.com/azure-sql-edge:1.0.7
    container_name: projectname-sqlserver
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${SA_PASSWORD:-YourStrong@Password123}
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    healthcheck:
      test: /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$$SA_PASSWORD" -Q "SELECT 1" || exit 1
      interval: 10s
      timeout: 3s
      retries: 10
      start_period: 10s
    networks:
      - app-network

  api:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: projectname-api
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:5000
      - ConnectionStrings__DefaultConnection=Server=db;Database=ProjectDb;User Id=sa;Password=${SA_PASSWORD:-YourStrong@Password123};TrustServerCertificate=True;
    ports:
      - "5000:5000"
    depends_on:
      db:
        condition: service_healthy
    command: >
      sh -c "
      dotnet ef database update --no-build &&
      dotnet ProjectName.API.dll
      "
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
      interval: 30s
      timeout: 3s
      retries: 3
      start_period: 10s
    networks:
      - app-network

networks:
  app-network:
    driver: bridge

volumes:
  sqlserver_data:
```

### Makefile

```makefile
.PHONY: help setup build test clean docker-up docker-down migrate run-api db-reset

help:
	@echo "Available commands:"
	@echo "  make setup       - Install dependencies"
	@echo "  make build       - Build .NET solution"
	@echo "  make test        - Run all tests"
	@echo "  make docker-up   - Start all Docker services"
	@echo "  make docker-down - Stop all Docker services"
	@echo "  make migrate     - Run EF Core migrations"
	@echo "  make run-api     - Run API locally"
	@echo "  make clean       - Clean build artifacts"
	@echo "  make db-reset    - Reset database"

setup:
	dotnet restore
	dotnet tool install --global dotnet-ef

build:
	dotnet build

test:
	dotnet test --verbosity normal

clean:
	dotnet clean
	rm -rf */bin */obj

docker-up:
	docker-compose up --build -d
	@echo "Services started. API: http://localhost:5000"

docker-down:
	docker-compose down -v

migrate:
	dotnet ef database update --project src/ProjectName.Infrastructure --startup-project src/ProjectName.API

run-api:
	dotnet run --project src/ProjectName.API

db-reset:
	dotnet ef database drop --project src/ProjectName.Infrastructure --startup-project src/ProjectName.API --force
	dotnet ef database update --project src/ProjectName.Infrastructure --startup-project src/ProjectName.API
```

---

## Best Practices Checklist

### ✅ Architecture

- [ ] Clean Architecture with 4 layers
- [ ] Dependency Injection throughout
- [ ] Repository pattern for data access
- [ ] DTOs for data transfer
- [ ] Async/await for all I/O operations

### ✅ Code Quality

- [ ] Nullable reference types enabled
- [ ] XML documentation on public APIs
- [ ] Consistent naming conventions
- [ ] One class per file
- [ ] Meaningful variable/method names

### ✅ API Design

- [ ] RESTful endpoints
- [ ] Proper HTTP status codes
- [ ] Pagination for list endpoints
- [ ] API versioning ready
- [ ] Swagger/OpenAPI documentation

### ✅ Error Handling

- [ ] Global exception middleware
- [ ] Structured error responses
- [ ] Logging with ILogger
- [ ] Validation with Data Annotations

### ✅ Security

- [ ] CORS configured properly
- [ ] No secrets in source code
- [ ] Non-root user in Docker
- [ ] TrustServerCertificate only in dev

### ✅ Testing

- [ ] Unit tests for all layers
- [ ] Integration tests with Testcontainers
- [ ] FluentAssertions for readable tests
- [ ] 70%+ code coverage target

### ✅ DevOps

- [ ] Multi-stage Docker build
- [ ] Health check endpoints
- [ ] Docker Compose for local dev
- [ ] Makefile for automation
- [ ] .dockerignore configured

### ✅ Database

- [ ] EF Core migrations
- [ ] Seeding strategy
- [ ] Connection string externalized
- [ ] Database health checks

---

## Appendix: Common Commands Reference

### EF Core Migrations

```bash
# Add migration
dotnet ef migrations add MigrationName --project src/ProjectName.Infrastructure --startup-project src/ProjectName.API

# Update database
dotnet ef database update --project src/ProjectName.Infrastructure --startup-project src/ProjectName.API

# List migrations
dotnet ef migrations list --project src/ProjectName.Infrastructure --startup-project src/ProjectName.API

# Remove last migration
dotnet ef migrations remove --project src/ProjectName.Infrastructure --startup-project src/ProjectName.API

# Drop database
dotnet ef database drop --project src/ProjectName.Infrastructure --startup-project src/ProjectName.API --force
```

### Testing

```bash
# Run all tests
dotnet test

# Run with verbosity
dotnet test --verbosity detailed

# Run specific test project
dotnet test tests/ProjectName.UnitTests

# Run with filter
dotnet test --filter "FullyQualifiedName~ControllerTests"

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Docker

```bash
# Build image
docker build -t projectname-api .

# Run container
docker run -p 5000:5000 projectname-api

# Docker Compose
docker-compose up -d
docker-compose down -v
docker-compose logs -f api

# Check Rosetta emulation (Mac M-series)
docker run --rm mcr.microsoft.com/mssql/server:2022-latest /opt/mssql-tools18/bin/sqlcmd -?
```

---

## Template Usage Instructions

### For LLM Context

When starting a new .NET project, provide this template to the LLM with:

1. **Project name**: Replace `ProjectName` with actual name
2. **Entity details**: Specify domain entities and their properties
3. **Features**: List required CRUD operations and custom queries
4. **Database**: Confirm SQL Server or specify alternative
5. **Frontend**: Specify if Angular/React/Vue needed

### Example Prompt

```
Using the .NET Application Template (DOTNET_APPLICATION_TEMPLATE.md),
create a new project with the following specifications:

Project Name: OrderManagement
Entities:
  - Order (Id, CustomerId, OrderDate, TotalAmount, Status)
  - OrderItem (Id, OrderId, ProductId, Quantity, Price)
Features:
  - Full CRUD for Orders
  - Search orders by customer
  - Filter by date range
  - Calculate order totals
Database: SQL Server 2022
Frontend: Angular 20

Follow all coding standards and testing approaches from the template.
```

---

**End of Template Document**

*This template represents production-ready standards for .NET 9 applications with Clean Architecture, comprehensive testing, and Docker support.*
