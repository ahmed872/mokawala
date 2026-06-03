using System;

namespace FleetManagementSystem.Data.Entities
{
    public class OilChange
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime ChangeDate { get; set; }
        public decimal OdometerAtChange { get; set; }
        public string OilType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Cost { get; set; }
        public decimal NextOilChangeOdometer { get; set; }
        public decimal? CurrentOdometer { get; set; }
        public DateTime? CurrentOdometerDate { get; set; }
        public bool IsOilChanged { get; set; } = true;
        public string ServiceItems { get; set; } = string.Empty;
        public string Status { get; set; } = "Scheduled";
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Vehicle? Vehicle { get; set; }
    }
}
