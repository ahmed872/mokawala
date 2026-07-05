using System;
using System.Collections.Generic;
using FleetManagementSystem.Core.Enums;

namespace FleetManagementSystem.Data.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Staff;

        /// <summary>Set when this account belongs to a driver custodian (Role = Custodian).</summary>
        public int? DriverId { get; set; }

        /// <summary>Set when this account belongs to an employee custodian (Role = Custodian).</summary>
        public int? EmployeeId { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
