using System;
using System.Collections.Generic;

namespace FleetManagementSystem.Data.Entities
{
    public class TreasuryTransaction
    {
        public int Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } = "Out";
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string RelatedEntityType { get; set; } = string.Empty;
        public int? RelatedEntityId { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<FuelTransaction> FuelTransactions { get; set; } = new List<FuelTransaction>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public ICollection<Custody> Custodies { get; set; } = new List<Custody>();
    }
}
