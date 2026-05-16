using System;

namespace FleetManagementSystem.Data.Entities
{
    public class MaintenanceRequest
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int MaintenanceTypeId { get; set; }
        public int? ServiceProviderId { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string Status { get; set; } = "Open";
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public decimal ActualCost { get; set; }
        public string WorkPerformed { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public Vehicle? Vehicle { get; set; }
        public MaintenanceType? MaintenanceType { get; set; }
        public ServiceProvider? ServiceProvider { get; set; }
    }
}
