namespace FleetManagementSystem.Core.Entities;

public class ContractStatus : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
