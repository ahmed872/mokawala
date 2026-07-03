using FleetManagementSystem.Core.Enums;

namespace FleetManagementSystem.Core.Entities;

public class Trip : BaseEntity
{
    public int VehicleId { get; set; }
    public int? DriverId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? StartLocation { get; set; }
    public string? EndLocation { get; set; }
    public decimal? Distance { get; set; }
    public TripStatus Status { get; set; } = TripStatus.Scheduled;
    public string? Purpose { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Vehicle? Vehicle { get; set; }
    public Driver? Driver { get; set; }
}
