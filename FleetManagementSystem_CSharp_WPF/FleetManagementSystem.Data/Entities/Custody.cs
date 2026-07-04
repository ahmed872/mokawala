using System;

namespace FleetManagementSystem.Data.Entities
{
    /// <summary>
    /// Financial custody (عهدة مالية) entrusted to a human custodian — either a driver or an
    /// employee, never a vehicle. Exactly one of <see cref="DriverId"/> or <see cref="EmployeeId"/>
    /// must be set; the invariant is enforced at the database level by the
    /// CK_Custody_ExactlyOneCustodian check constraint (see CustodyConfiguration) and revalidated
    /// in the application service layer.
    /// </summary>
    public class Custody
    {
        public int Id { get; set; }

        /// <summary>Custodian when the custody is held by a driver. Mutually exclusive with <see cref="EmployeeId"/>.</summary>
        public int? DriverId { get; set; }

        /// <summary>Custodian when the custody is held by an employee. Mutually exclusive with <see cref="DriverId"/>.</summary>
        public int? EmployeeId { get; set; }

        public string CustodyNumber { get; set; } = string.Empty;

        /// <summary>Disbursed custody amount. Non-negative; precision configured as decimal(18,2).</summary>
        public decimal Amount { get; set; }

        public DateTime HandoverDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = "Active";

        /// <summary>True when the disbursement was funded from the treasury within the same unit of work.</summary>
        public bool PaidFromTreasury { get; set; }

        /// <summary>Treasury movement that funded this custody, when <see cref="PaidFromTreasury"/> is true.</summary>
        public int? TreasuryTransactionId { get; set; }

        public string Notes { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Driver? Driver { get; set; }
        public Employee? Employee { get; set; }
        public TreasuryTransaction? TreasuryTransaction { get; set; }
    }
}
