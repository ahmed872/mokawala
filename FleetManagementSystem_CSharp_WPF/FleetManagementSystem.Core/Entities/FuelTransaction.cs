namespace FleetManagementSystem.Core.Entities;

public class FuelTransaction : BaseEntity
{
    public int VehicleId { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalCost { get; set; }
    public decimal? Mileage { get; set; }
    public string? FuelType { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }

    // Navigation property
    public Vehicle? Vehicle { get; set; }
}
