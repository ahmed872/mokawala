using System;

namespace FleetManagementSystem.Data.Entities
{
    /// <summary>
    /// A settlement (تسوية) posted against a financial custody: the custodian documents spending
    /// part of the custody balance by attaching a scanned receipt. A settlement is invalid without
    /// a stored receipt document — <see cref="ReceiptFilePath"/> is required at the database level
    /// and the service layer refuses to save a settlement whose receipt payload is missing.
    /// </summary>
    public class CustodySettlement
    {
        public int Id { get; set; }
        public int CustodyId { get; set; }

        /// <summary>Settled amount. Positive; precision configured as decimal(18,2).</summary>
        public decimal Amount { get; set; }

        public DateTime SettlementDate { get; set; }
        public string Description { get; set; } = string.Empty;

        /// <summary>Relative path of the scanned receipt (PNG/JPEG/PDF) under Uploads/Receipts.</summary>
        public string ReceiptFilePath { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Custody? Custody { get; set; }
    }
}
