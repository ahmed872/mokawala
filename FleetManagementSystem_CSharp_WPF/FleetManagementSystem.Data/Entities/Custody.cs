using System;
using System.Collections.Generic;

namespace FleetManagementSystem.Data.Entities
{
    public class Custody
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CustodyNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal ReturnedAmount { get; set; }
        public DateTime HandoverDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = "Active";
        public string Notes { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public User? User { get; set; }
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
