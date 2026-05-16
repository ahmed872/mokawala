namespace FleetManagementSystem.Core.Entities;

/// <summary>
/// Base entity class for all domain entities with audit timestamps
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
