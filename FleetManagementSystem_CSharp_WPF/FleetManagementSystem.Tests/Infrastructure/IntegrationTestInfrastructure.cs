// ============================================================================
// FILE: IntegrationTestFixture.cs
// LOCATION: FleetManagementSystem.Tests/Infrastructure/IntegrationTestFixture.cs
// ============================================================================

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MySql;
using FleetManagementSystem.Data;
using Xunit;

namespace FleetManagementSystem.Tests.Infrastructure
{
    /// <summary>
    /// Base fixture for integration tests using Testcontainers MySQL
    /// </summary>
    public class IntegrationTestFixture : IAsyncLifetime
    {
        private readonly MySqlContainer _container;
        protected FleetDbContext DbContext { get; private set; }

        public IntegrationTestFixture()
        {
            _container = new MySqlBuilder()
                .WithImage("mysql:8.0")
                .WithDatabase("FleetManagementDB_Test")
                .WithUsername("root")
                .WithPassword("testpassword")
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _container.StartAsync();
            
            var connectionString = _container.GetConnectionString();
            var options = new DbContextOptionsBuilder<FleetDbContext>()
                .UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)))
                .Options;

            DbContext = new FleetDbContext(options);
            
            // Create database schema
            await DbContext.Database.MigrateAsync();
            
            // Seed test data
            await SeedTestDataAsync();
        }

        public async Task DisposeAsync()
        {
            if (DbContext != null)
            {
                await DbContext.Database.EnsureDeletedAsync();
                await DbContext.DisposeAsync();
            }

            await _container.StopAsync();
        }

        private async Task SeedTestDataAsync()
        {
            // Seed master data
            var vehicleTypes = new[]
            {
                new VehicleType { Id = 1, Name = "سيارة ركاب", Description = "سيارة ركاب عادية" },
                new VehicleType { Id = 2, Name = "شاحنة", Description = "شاحنة نقل" },
                new VehicleType { Id = 3, Name = "حافلة", Description = "حافلة ركاب" }
            };

            var contractStatuses = new[]
            {
                new ContractStatus { Id = 1, Name = "مسودة", Description = "العقد قيد الإعداد" },
                new ContractStatus { Id = 2, Name = "نشط", Description = "العقد ساري المفعول" },
                new ContractStatus { Id = 3, Name = "منتهي", Description = "انتهى العقد" },
                new ContractStatus { Id = 4, Name = "ملغى", Description = "تم إلغاء العقد" }
            };

            var maintenanceTypes = new[]
            {
                new MaintenanceType { Id = 1, Name = "صيانة دورية", Description = "صيانة دورية منتظمة" },
                new MaintenanceType { Id = 2, Name = "إصلاح", Description = "إصلاح عطل" },
                new MaintenanceType { Id = 3, Name = "تغيير الزيت", Description = "تغيير زيت المحرك" }
            };

            var serviceProviders = new[]
            {
                new ServiceProvider { Id = 1, Name = "مركز الصيانة الأول", Phone = "0501234567", Email = "center1@example.com" },
                new ServiceProvider { Id = 2, Name = "مركز الصيانة الثاني", Phone = "0502345678", Email = "center2@example.com" }
            };

            var companySettings = new CompanySettings
            {
                Id = 1,
                CompanyName = "شركة النقل الوطنية",
                CompanyLogo = "logo.png",
                ContactPhone = "0505555555",
                ContactEmail = "info@company.com",
                Address = "الرياض، المملكة العربية السعودية"
            };

            var adminUser = new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@company.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var staffUser = new User
            {
                Id = 2,
                Username = "staff",
                Email = "staff@company.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
                Role = UserRole.Staff,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            DbContext.VehicleTypes.AddRange(vehicleTypes);
            DbContext.ContractStatuses.AddRange(contractStatuses);
            DbContext.MaintenanceTypes.AddRange(maintenanceTypes);
            DbContext.ServiceProviders.AddRange(serviceProviders);
            DbContext.CompanySettings.Add(companySettings);
            DbContext.Users.AddRange(adminUser, staffUser);

            await DbContext.SaveChangesAsync();
        }

        public void ResetDatabase()
        {
            DbContext.Database.EnsureDeleted();
            DbContext.Database.Migrate();
        }
    }
}

// ============================================================================
// FILE: IntegrationTestBase.cs
// LOCATION: FleetManagementSystem.Tests/Infrastructure/IntegrationTestBase.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetManagementSystem.Data;
using FleetManagementSystem.Services.Interfaces;
using FleetManagementSystem.Services.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FleetManagementSystem.Tests.Infrastructure
{
    /// <summary>
    /// Base class for integration tests with service injection
    /// </summary>
    public abstract class IntegrationTestBase : IClassFixture<IntegrationTestFixture>
    {
        protected IntegrationTestFixture Fixture { get; }
        protected FleetDbContext DbContext { get; }
        protected IServiceProvider ServiceProvider { get; }

        protected IVehicleService VehicleService { get; }
        protected IContractService ContractService { get; }
        protected IMaintenanceService MaintenanceService { get; }
        protected IDriverService DriverService { get; }
        protected IEmployeeService EmployeeService { get; }
        protected ITripService TripService { get; }
        protected IFuelService FuelService { get; }
        protected IExpenseService ExpenseService { get; }
        protected ILicenseService LicenseService { get; }
        protected IInsuranceService InsuranceService { get; }
        protected ICustodyService CustodyService { get; }
        protected IMasterDataService MasterDataService { get; }
        protected IReportingService ReportingService { get; }
        protected ISettingsService SettingsService { get; }
        protected INotificationService NotificationService { get; }
        protected IAuditService AuditService { get; }

        public IntegrationTestBase(IntegrationTestFixture fixture)
        {
            Fixture = fixture;
            DbContext = fixture.DbContext;

            // Setup dependency injection
            var services = new ServiceCollection();
            
            // Register services
            services.AddScoped<IVehicleService>(sp => new VehicleService(DbContext));
            services.AddScoped<IContractService>(sp => new ContractService(DbContext));
            services.AddScoped<IMaintenanceService>(sp => new MaintenanceService(DbContext));
            services.AddScoped<IDriverService>(sp => new DriverService(DbContext));
            services.AddScoped<IEmployeeService>(sp => new EmployeeService(DbContext));
            services.AddScoped<ITripService>(sp => new TripService(DbContext));
            services.AddScoped<IFuelService>(sp => new FuelService(DbContext));
            services.AddScoped<IExpenseService>(sp => new ExpenseService(DbContext));
            services.AddScoped<ILicenseService>(sp => new LicenseService(DbContext));
            services.AddScoped<IInsuranceService>(sp => new InsuranceService(DbContext));
            services.AddScoped<ICustodyService>(sp => new CustodyService(DbContext));
            services.AddScoped<IMasterDataService>(sp => new MasterDataService(DbContext));
            services.AddScoped<IReportingService>(sp => new ReportingService(DbContext));
            services.AddScoped<ISettingsService>(sp => new SettingsService(DbContext));
            services.AddScoped<INotificationService>(sp => new NotificationService(DbContext));
            services.AddScoped<IAuditService>(sp => new AuditService(DbContext));

            ServiceProvider = services.BuildServiceProvider();

            // Resolve services
            VehicleService = ServiceProvider.GetRequiredService<IVehicleService>();
            ContractService = ServiceProvider.GetRequiredService<IContractService>();
            MaintenanceService = ServiceProvider.GetRequiredService<IMaintenanceService>();
            DriverService = ServiceProvider.GetRequiredService<IDriverService>();
            EmployeeService = ServiceProvider.GetRequiredService<IEmployeeService>();
            TripService = ServiceProvider.GetRequiredService<ITripService>();
            FuelService = ServiceProvider.GetRequiredService<IFuelService>();
            ExpenseService = ServiceProvider.GetRequiredService<IExpenseService>();
            LicenseService = ServiceProvider.GetRequiredService<ILicenseService>();
            InsuranceService = ServiceProvider.GetRequiredService<IInsuranceService>();
            CustodyService = ServiceProvider.GetRequiredService<ICustodyService>();
            MasterDataService = ServiceProvider.GetRequiredService<IMasterDataService>();
            ReportingService = ServiceProvider.GetRequiredService<IReportingService>();
            SettingsService = ServiceProvider.GetRequiredService<ISettingsService>();
            NotificationService = ServiceProvider.GetRequiredService<INotificationService>();
            AuditService = ServiceProvider.GetRequiredService<IAuditService>();
        }

        protected async Task CleanupAsync()
        {
            // Clear all tables except master data
            var vehicles = await DbContext.Vehicles.ToListAsync();
            DbContext.Vehicles.RemoveRange(vehicles);

            var contracts = await DbContext.Contracts.ToListAsync();
            DbContext.Contracts.RemoveRange(contracts);

            var maintenanceRequests = await DbContext.MaintenanceRequests.ToListAsync();
            DbContext.MaintenanceRequests.RemoveRange(maintenanceRequests);

            var drivers = await DbContext.Drivers.ToListAsync();
            DbContext.Drivers.RemoveRange(drivers);

            var employees = await DbContext.Employees.ToListAsync();
            DbContext.Employees.RemoveRange(employees);

            var trips = await DbContext.Trips.ToListAsync();
            DbContext.Trips.RemoveRange(trips);

            var fuelTransactions = await DbContext.FuelTransactions.ToListAsync();
            DbContext.FuelTransactions.RemoveRange(fuelTransactions);

            var expenses = await DbContext.Expenses.ToListAsync();
            DbContext.Expenses.RemoveRange(expenses);

            var licenses = await DbContext.Licenses.ToListAsync();
            DbContext.Licenses.RemoveRange(licenses);

            var insurances = await DbContext.Insurances.ToListAsync();
            DbContext.Insurances.RemoveRange(insurances);

            var custodies = await DbContext.Custodies.ToListAsync();
            DbContext.Custodies.RemoveRange(custodies);

            var notifications = await DbContext.Notifications.ToListAsync();
            DbContext.Notifications.RemoveRange(notifications);

            var auditLogs = await DbContext.AuditLogs.ToListAsync();
            DbContext.AuditLogs.RemoveRange(auditLogs);

            await DbContext.SaveChangesAsync();
        }
    }
}

// ============================================================================
// FILE: TestDataBuilder.cs
// LOCATION: FleetManagementSystem.Tests/Infrastructure/TestDataBuilder.cs
// ============================================================================

using System;
using FleetManagementSystem.Data;

namespace FleetManagementSystem.Tests.Infrastructure
{
    /// <summary>
    /// Helper class for building test entities
    /// </summary>
    public static class TestDataBuilder
    {
        public static Vehicle CreateTestVehicle(
            string plateNumber = "ABC-123",
            int vehicleTypeId = 1,
            string model = "Toyota Camry",
            int year = 2023,
            string status = "متاح",
            string assignment = "غير مخصص")
        {
            return new Vehicle
            {
                PlateNumber = plateNumber,
                VehicleTypeId = vehicleTypeId,
                Model = model,
                Year = year,
                Status = status,
                Assignment = assignment,
                Mileage = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Contract CreateTestContract(
            string contractNumber = "CNT-001",
            int vehicleId = 1,
            string clientName = "عميل اختبار",
            DateTime? startDate = null,
            DateTime? endDate = null,
            decimal value = 10000,
            int statusId = 2)
        {
            startDate ??= DateTime.UtcNow;
            endDate ??= DateTime.UtcNow.AddMonths(12);

            return new Contract
            {
                ContractNumber = contractNumber,
                VehicleId = vehicleId,
                ClientName = clientName,
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                ContractValue = value,
                ContractStatusId = statusId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static MaintenanceRequest CreateTestMaintenanceRequest(
            int vehicleId = 1,
            int maintenanceTypeId = 1,
            int serviceProviderId = 1,
            string status = "مفتوح",
            decimal cost = 500)
        {
            return new MaintenanceRequest
            {
                VehicleId = vehicleId,
                MaintenanceTypeId = maintenanceTypeId,
                ServiceProviderId = serviceProviderId,
                RequestDate = DateTime.UtcNow,
                Status = status,
                Cost = cost,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Driver CreateTestDriver(
            string fullName = "محمد أحمد",
            string licenseNumber = "DL-123456",
            DateTime? licenseExpiryDate = null,
            string phone = "0501234567",
            string status = "نشط")
        {
            licenseExpiryDate ??= DateTime.UtcNow.AddYears(2);

            return new Driver
            {
                FullName = fullName,
                LicenseNumber = licenseNumber,
                LicenseExpiryDate = licenseExpiryDate.Value,
                Phone = phone,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Employee CreateTestEmployee(
            string fullName = "فاطمة علي",
            string position = "مشرف",
            string department = "العمليات",
            string phone = "0502345678")
        {
            return new Employee
            {
                FullName = fullName,
                Position = position,
                Department = department,
                Phone = phone,
                Email = $"{fullName.Replace(" ", "")}@company.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Trip CreateTestTrip(
            int vehicleId = 1,
            int driverId = 1,
            string destination = "جدة",
            DateTime? departureTime = null,
            string status = "مخطط")
        {
            departureTime ??= DateTime.UtcNow.AddHours(2);

            return new Trip
            {
                VehicleId = vehicleId,
                DriverId = driverId,
                Destination = destination,
                DepartureTime = departureTime.Value,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static FuelTransaction CreateTestFuelTransaction(
            int vehicleId = 1,
            decimal quantity = 50,
            decimal costPerUnit = 2.5m,
            DateTime? transactionDate = null)
        {
            transactionDate ??= DateTime.UtcNow;

            return new FuelTransaction
            {
                VehicleId = vehicleId,
                Quantity = quantity,
                CostPerUnit = costPerUnit,
                TotalCost = quantity * costPerUnit,
                TransactionDate = transactionDate.Value,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Expense CreateTestExpense(
            int vehicleId = 1,
            string category = "إصلاح",
            decimal amount = 1000,
            DateTime? expenseDate = null,
            string description = "إصلاح المحرك")
        {
            expenseDate ??= DateTime.UtcNow;

            return new Expense
            {
                VehicleId = vehicleId,
                Category = category,
                Amount = amount,
                ExpenseDate = expenseDate.Value,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static License CreateTestLicense(
            int driverId = 1,
            string licenseType = "عام",
            DateTime? issueDate = null,
            DateTime? expiryDate = null)
        {
            issueDate ??= DateTime.UtcNow.AddYears(-2);
            expiryDate ??= DateTime.UtcNow.AddYears(2);

            return new License
            {
                DriverId = driverId,
                LicenseType = licenseType,
                IssueDate = issueDate.Value,
                ExpiryDate = expiryDate.Value,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Insurance CreateTestInsurance(
            int vehicleId = 1,
            string policyNumber = "POL-123456",
            string providerName = "شركة التأمين الوطنية",
            decimal coverageAmount = 100000,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            startDate ??= DateTime.UtcNow;
            endDate ??= DateTime.UtcNow.AddYears(1);

            return new Insurance
            {
                VehicleId = vehicleId,
                PolicyNumber = policyNumber,
                ProviderName = providerName,
                CoverageAmount = coverageAmount,
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Custody CreateTestCustody(
            int assignedToEmployeeId = 1,
            string itemDescription = "جهاز GPS",
            string status = "مُسَلَّم")
        {
            return new Custody
            {
                AssignedToEmployeeId = assignedToEmployeeId,
                ItemDescription = itemDescription,
                AssignmentDate = DateTime.UtcNow,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
