using System;

namespace FleetManagementSystem.Data.Entities
{
    public class License
    {
        public int Id { get; set; }
        public int DriverId { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public string LicenseType { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string IssuingAuthority { get; set; } = string.Empty;
        public string Status { get; set; } = "Valid";
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public Driver? Driver { get; set; }
    }
}
