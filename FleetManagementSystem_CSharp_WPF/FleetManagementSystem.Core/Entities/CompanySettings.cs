namespace FleetManagementSystem.Core.Entities;

public class CompanySettings : BaseEntity
{
    public string CompanyName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? TaxId { get; set; }
    public string? RegistrationNumber { get; set; }
}
