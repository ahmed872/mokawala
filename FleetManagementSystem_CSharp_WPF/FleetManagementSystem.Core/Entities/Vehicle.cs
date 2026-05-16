using FleetManagementSystem.Core.Enums;

namespace FleetManagementSystem.Core.Entities;

public class Vehicle : BaseEntity
{
    public string PlateNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? ChassisNumber { get; set; }
    public string? EngineNumber { get; set; }
    public int VehicleTypeId { get; set; }
    public VehicleStatus Status { get; set; } = VehicleStatus.Active;
    public decimal Mileage { get; set; }
    public string? CurrentAssignment { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public VehicleType? VehicleType { get; set; }
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public ICollection<FuelTransaction> FuelTransactions { get; set; } = new List<FuelTransaction>();
    public ICollection<Insurance> InsurancePolicies { get; set; } = new List<Insurance>();
}
