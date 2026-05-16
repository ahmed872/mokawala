using System;

namespace FleetManagementSystem.Data.Entities
{
    public class Trip
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int? DriverId { get; set; }
        public int? RequesterEmployeeId { get; set; }
        public string RequesterNameText { get; set; } = string.Empty;
        public int? SupervisorEmployeeId { get; set; }
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal StartMileage { get; set; }
        public decimal? EndMileage { get; set; }
        public decimal Distance { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string Status { get; set; } = "Planned";
        public decimal FuelConsumed { get; set; }
        public decimal TripCost { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public Vehicle? Vehicle { get; set; }
        public Driver? Driver { get; set; }
        public Employee? RequesterEmployee { get; set; }
        public Employee? SupervisorEmployee { get; set; }
    }
}
