namespace FleetManagementSystem.Core.Entities;

public class AuditLog : BaseEntity
{
    public int UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IPAddress { get; set; }
}
