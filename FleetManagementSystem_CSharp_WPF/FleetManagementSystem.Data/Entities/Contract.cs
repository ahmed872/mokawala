using System;

namespace FleetManagementSystem.Data.Entities
{
    public class Contract
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public int ContractStatusId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientPhoneNumber { get; set; } = string.Empty;
        public string ClientAddress { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal ContractValue { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentTerms { get; set; } = string.Empty;
        public string ContractTerms { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public Vehicle? Vehicle { get; set; }
        public ContractStatus? ContractStatus { get; set; }
    }
}
