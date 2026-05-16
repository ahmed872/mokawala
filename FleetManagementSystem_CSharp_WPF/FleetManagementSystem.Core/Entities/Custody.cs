using FleetManagementSystem.Core.Enums;

namespace FleetManagementSystem.Core.Entities;

public class Custody : BaseEntity
{
    public int VehicleId { get; set; }
    public string ItemDescription { get; set; } = string.Empty;
    public DateTime AssignmentDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string? AssignedTo { get; set; }
    public CustodyStatus Status { get; set; } = CustodyStatus.Assigned;
    public string? Notes { get; set; }

    // Navigation property
    public Vehicle? Vehicle { get; set; }
}
