using FleetManagementSystem.Core.Enums;

namespace FleetManagementSystem.Core.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Staff;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
}
