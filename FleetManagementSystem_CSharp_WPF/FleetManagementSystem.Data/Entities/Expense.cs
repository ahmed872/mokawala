using System;

namespace FleetManagementSystem.Data.Entities
{
    public class Expense
    {
        public int Id { get; set; }
        public int? VehicleId { get; set; }
        public int? CustodyId { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string ReceiptUrl { get; set; } = string.Empty;
        public bool PaidFromTreasury { get; set; }
        public int? TreasuryTransactionId { get; set; }
        public string Status { get; set; } = "Pending";
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public Vehicle? Vehicle { get; set; }
        public Custody? Custody { get; set; }
        public TreasuryTransaction? TreasuryTransaction { get; set; }
    }
}
