namespace FleetManagementSystem.Core.Entities;

public class ServiceProvider : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Specialization { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property
    public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
}
