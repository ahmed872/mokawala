using System;
using System.Collections.Generic;

namespace FleetManagementSystem.Data.Entities
{
    public class Employee
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateTime? HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string Status { get; set; } = "Active";
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Trip> RequesterTrips { get; set; } = new List<Trip>();
        public ICollection<Trip> SupervisorTrips { get; set; } = new List<Trip>();
    }
}
