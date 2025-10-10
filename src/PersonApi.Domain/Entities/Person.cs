namespace PersonApi.Domain.Entities;

/// <summary>
/// Represents a Person entity in the system
/// </summary>
public class Person
{
    /// <summary>
    /// Unique identifier for the person
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Full name of the person
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Age of the person
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// JSON-serialized array of skills
    /// </summary>
    public string Skills { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the record was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
