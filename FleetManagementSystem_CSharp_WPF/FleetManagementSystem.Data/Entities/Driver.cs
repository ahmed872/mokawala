using System;
using System.Collections.Generic;

namespace FleetManagementSystem.Data.Entities
{
    public class Driver
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime? LicenseStartDate { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }
        public string LicenseType { get; set; } = string.Empty;
        public bool IsCompanyInsured { get; set; }
        public string Governorate { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public string TrafficUnit { get; set; } = string.Empty;
        public string WorkLocation { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public bool IsActive { get; set; } = true;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
        public ICollection<License> Licenses { get; set; } = new List<License>();
        public ICollection<DriverAttendance> AttendanceRecords { get; set; } = new List<DriverAttendance>();
    }
}
