using FleetManagementSystem.Core.Enums;

namespace FleetManagementSystem.Core.Entities;

public class License : BaseEntity
{
    public int DriverId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string? LicenseClass { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public LicenseStatus Status { get; set; } = LicenseStatus.Valid;
    public string? IssuingAuthority { get; set; }
    public string? Notes { get; set; }

    // Navigation property
    public Driver? Driver { get; set; }
}
