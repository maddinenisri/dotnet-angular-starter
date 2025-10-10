using Microsoft.AspNetCore.Mvc;
using PersonApi.Application.DTOs;
using PersonApi.Application.Interfaces;

namespace PersonApi.API.Controllers;

/// <summary>
/// Controller for managing Person entities
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
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <returns>List of persons</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PersonDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Getting persons - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

        pageSize = Math.Min(pageSize, 100);

        if (pageNumber <= 0 || pageSize <= 0)
        {
            return BadRequest(new { message = "Page number and page size must be greater than 0" });
        }

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
    /// <param name="id">Person ID</param>
    /// <returns>Person details</returns>
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
    /// <param name="createDto">Person creation data</param>
    /// <returns>Created person</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PersonDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePersonDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Creating new person: {Name}", createDto.Name);

        var person = await _personService.CreateAsync(createDto);

        return CreatedAtAction(nameof(GetById), new { id = person.Id }, person);
    }

    /// <summary>
    /// Update an existing person
    /// </summary>
    /// <param name="id">Person ID</param>
    /// <param name="updateDto">Updated person data</param>
    /// <returns>No content on success</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePersonDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

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
    /// <param name="id">Person ID</param>
    /// <returns>No content on success</returns>
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
    /// <param name="name">Search term</param>
    /// <returns>List of matching persons</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<PersonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search([FromQuery] string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new { message = "Search term cannot be empty" });
        }

        _logger.LogInformation("Searching persons by name: {Name}", name);

        var persons = await _personService.SearchByNameAsync(name);
        return Ok(persons);
    }
}
