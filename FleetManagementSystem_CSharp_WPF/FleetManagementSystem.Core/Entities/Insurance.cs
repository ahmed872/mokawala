using FleetManagementSystem.Core.Enums;

namespace FleetManagementSystem.Core.Entities;

public class Insurance : BaseEntity
{
    public int VehicleId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string? Provider { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal PremiumAmount { get; set; }
    public InsuranceStatus Status { get; set; } = InsuranceStatus.Active;
    public string? CoverageType { get; set; }
    public decimal? CoverageAmount { get; set; }
    public string? Notes { get; set; }

    // Navigation property
    public Vehicle? Vehicle { get; set; }
}
