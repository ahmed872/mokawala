using System;
using System.Collections.Generic;

namespace FleetManagementSystem.Data.Entities
{
    public class VehicleType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
