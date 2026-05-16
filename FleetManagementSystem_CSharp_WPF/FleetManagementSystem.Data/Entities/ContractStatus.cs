using System;
using System.Collections.Generic;

namespace FleetManagementSystem.Data.Entities
{
    public class ContractStatus
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = "#1F2937";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}
