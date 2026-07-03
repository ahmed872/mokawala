using FleetManagementSystem.Core.Enums;

namespace FleetManagementSystem.Core.Entities;

public class Expense : BaseEntity
{
    public int VehicleId { get; set; }
    public DateTime ExpenseDate { get; set; }
    public ExpenseCategory Category { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? Receipt { get; set; }
    public string? Notes { get; set; }

    // Navigation property
    public Vehicle? Vehicle { get; set; }
}
