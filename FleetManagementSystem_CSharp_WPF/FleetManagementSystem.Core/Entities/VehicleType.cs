namespace FleetManagementSystem.Core.Entities;

public class VehicleType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
