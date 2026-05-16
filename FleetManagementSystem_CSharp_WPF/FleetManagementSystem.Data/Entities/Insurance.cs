using System;

namespace FleetManagementSystem.Data.Entities
{
    public class Insurance
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string InsuranceCompany { get; set; } = string.Empty;
        public string PolicyType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal CoverageAmount { get; set; }
        public string CoverageDetails { get; set; } = string.Empty;
        public string AgentName { get; set; } = string.Empty;
        public string AgentPhoneNumber { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public string DocumentUrl { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public Vehicle? Vehicle { get; set; }
    }
}
