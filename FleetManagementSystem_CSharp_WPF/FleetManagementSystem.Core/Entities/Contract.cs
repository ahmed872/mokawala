namespace FleetManagementSystem.Core.Entities;

public class Contract : BaseEntity
{
    public string ContractNumber { get; set; } = string.Empty;
    public int VehicleId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? ClientPhone { get; set; }
    public string? ClientEmail { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal ContractValue { get; set; }
    public int ContractStatusId { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Vehicle? Vehicle { get; set; }
    public ContractStatus? Status { get; set; }
}
