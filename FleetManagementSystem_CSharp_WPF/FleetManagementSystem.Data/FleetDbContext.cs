using Microsoft.EntityFrameworkCore;
using FleetManagementSystem.Data.Configurations;
using FleetManagementSystem.Data.Entities;

namespace FleetManagementSystem.Data
{
    public class FleetDbContext : DbContext
    {
        public FleetDbContext(DbContextOptions<FleetDbContext> options) : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<User> Users { get; set; }
        public DbSet<CompanySettings> CompanySettings { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<ContractStatus> ContractStatuses { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<MaintenanceType> MaintenanceTypes { get; set; }
        public DbSet<ServiceProvider> ServiceProviders { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<DriverAttendance> DriverAttendances { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<FuelTransaction> FuelTransactions { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<OilChange> OilChanges { get; set; }
        public DbSet<TreasuryTransaction> TreasuryTransactions { get; set; }
        public DbSet<License> Licenses { get; set; }
        public DbSet<Insurance> Insurance { get; set; }
        public DbSet<Insurance> Insurances => Set<Insurance>();
        public DbSet<Custody> Custody { get; set; }
        public DbSet<Custody> Custodies => Set<Custody>();
        public DbSet<CustodySettlement> CustodySettlements { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all entity configurations
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new CompanySettingsConfiguration());
            modelBuilder.ApplyConfiguration(new VehicleTypeConfiguration());
            modelBuilder.ApplyConfiguration(new VehicleConfiguration());
            modelBuilder.ApplyConfiguration(new ContractStatusConfiguration());
            modelBuilder.ApplyConfiguration(new ContractConfiguration());
            modelBuilder.ApplyConfiguration(new MaintenanceTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ServiceProviderConfiguration());
            modelBuilder.ApplyConfiguration(new MaintenanceRequestConfiguration());
            modelBuilder.ApplyConfiguration(new DriverConfiguration());
            modelBuilder.ApplyConfiguration(new DriverAttendanceConfiguration());
            modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
            modelBuilder.ApplyConfiguration(new TripConfiguration());
            modelBuilder.ApplyConfiguration(new FuelTransactionConfiguration());
            modelBuilder.ApplyConfiguration(new ExpenseConfiguration());
            modelBuilder.ApplyConfiguration(new OilChangeConfiguration());
            modelBuilder.ApplyConfiguration(new TreasuryTransactionConfiguration());
            modelBuilder.ApplyConfiguration(new LicenseConfiguration());
            modelBuilder.ApplyConfiguration(new InsuranceConfiguration());
            modelBuilder.ApplyConfiguration(new CustodyConfiguration());
            modelBuilder.ApplyConfiguration(new CustodySettlementConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationConfiguration());
            modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        }
    }
}
