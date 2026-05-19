namespace FleetManagementSystem.Core.Entities;

public class Driver : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? LicenseNumber { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public string? NationalId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public ICollection<License> Licenses { get; set; } = new List<License>();
}
