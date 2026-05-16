using System;
using System.Collections.Generic;

namespace FleetManagementSystem.Data.Entities
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public int VehicleTypeId { get; set; }
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Manufacturer { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string ChassisNumber { get; set; } = string.Empty;
        public string EngineNumber { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public string AssignedTo { get; set; } = string.Empty;
        public decimal Mileage { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime? RegistrationStartDate { get; set; }
        public DateTime? RegistrationExpiryDate { get; set; }
        public string AccidentInsuranceDetails { get; set; } = string.Empty;
        public string SocialInsuranceDetails { get; set; } = string.Empty;
        public decimal OilChangeIntervalKm { get; set; } = 10000;
        public decimal MaintenanceIntervalKm { get; set; } = 15000;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public VehicleType? VehicleType { get; set; }

        // Navigation properties
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
        public ICollection<FuelTransaction> FuelTransactions { get; set; } = new List<FuelTransaction>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public ICollection<OilChange> OilChanges { get; set; } = new List<OilChange>();
        public ICollection<Insurance> InsurancePolicies { get; set; } = new List<Insurance>();
    }
}
