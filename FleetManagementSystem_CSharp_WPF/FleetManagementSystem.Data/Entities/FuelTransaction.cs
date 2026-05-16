using System;

namespace FleetManagementSystem.Data.Entities
{
    public class FuelTransaction
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalCost { get; set; }
        public string FuelStation { get; set; } = string.Empty;
        public decimal? Odometer { get; set; }
        public bool PaidFromTreasury { get; set; }
        public int? TreasuryTransactionId { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public Vehicle? Vehicle { get; set; }
        public TreasuryTransaction? TreasuryTransaction { get; set; }
    }
}
