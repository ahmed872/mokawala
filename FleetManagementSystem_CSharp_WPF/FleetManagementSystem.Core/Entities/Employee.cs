namespace FleetManagementSystem.Core.Entities;

public class Employee : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? EmployeeId { get; set; }
    public bool IsActive { get; set; } = true;
}
