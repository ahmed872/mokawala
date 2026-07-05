using System;

namespace FleetManagementSystem.Data.Entities
{
    public class Custody
    {
        public int Id { get; set; }
        public int? VehicleId { get; set; }
        public int? EmployeeId { get; set; }
        public int? UserId { get; set; }
        public string CustodyNumber { get; set; } = string.Empty;
        public string CustodianName { get; set; } = string.Empty;
        public string CustodianPosition { get; set; } = string.Empty;
        public DateTime HandoverDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = "Active";
        public decimal VehicleConditionRating { get; set; } = 5m;
        public decimal Amount { get; set; }
        public decimal SettledAmount { get; set; }
        public DateTime? SettlementDate { get; set; }
        public string SettlementNotes { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public Vehicle? Vehicle { get; set; }
        public Employee? Employee { get; set; }
        public User? User { get; set; }
    }
}
