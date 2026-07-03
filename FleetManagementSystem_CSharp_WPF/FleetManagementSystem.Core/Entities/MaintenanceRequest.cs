using FleetManagementSystem.Core.Enums;

namespace FleetManagementSystem.Core.Entities;

public class MaintenanceRequest : BaseEntity
{
    public int VehicleId { get; set; }
    public int MaintenanceTypeId { get; set; }
    public int? ServiceProviderId { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public decimal Cost { get; set; }
    public MaintenanceStatusEnum Status { get; set; } = MaintenanceStatusEnum.Open;
    public string? Description { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Vehicle? Vehicle { get; set; }
    public MaintenanceType? MaintenanceType { get; set; }
    public ServiceProvider? ServiceProvider { get; set; }
}
