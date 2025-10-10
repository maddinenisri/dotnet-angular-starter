using System.ComponentModel.DataAnnotations;

namespace PersonApi.Application.DTOs;

/// <summary>
/// Data transfer object for creating a new Person
/// </summary>
public class CreatePersonDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0, 150, ErrorMessage = "Age must be between 0 and 150")]
    public int Age { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; set; }

    [MaxLength(20, ErrorMessage = "Maximum 20 skills allowed")]
    public List<string> Skills { get; set; } = new();
}
