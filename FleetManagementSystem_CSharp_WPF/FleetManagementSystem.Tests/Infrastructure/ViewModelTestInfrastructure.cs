// ============================================================================
// FILE: ViewModelTestBase.cs
// LOCATION: FleetManagementSystem.Tests/ViewModels/ViewModelTestBase.cs
// ============================================================================

using System;
using System.Collections.Generic;
using Moq;
using Xunit;

namespace FleetManagementSystem.Tests.ViewModels
{
    /// <summary>
    /// Base class for all ViewModel unit tests providing common setup and mock services.
    /// </summary>
    public abstract class ViewModelTestBase
    {
        protected Mock<IVehicleService> MockVehicleService { get; set; }
        protected Mock<IContractService> MockContractService { get; set; }
        protected Mock<IMaintenanceService> MockMaintenanceService { get; set; }
        protected Mock<IDriverService> MockDriverService { get; set; }
        protected Mock<IEmployeeService> MockEmployeeService { get; set; }
        protected Mock<ITripService> MockTripService { get; set; }
        protected Mock<IFuelService> MockFuelService { get; set; }
        protected Mock<IExpenseService> MockExpenseService { get; set; }
        protected Mock<ILicenseService> MockLicenseService { get; set; }
        protected Mock<IInsuranceService> MockInsuranceService { get; set; }
        protected Mock<ICustodyService> MockCustodyService { get; set; }
        protected Mock<IMasterDataService> MockMasterDataService { get; set; }
        protected Mock<IReportingService> MockReportingService { get; set; }
        protected Mock<IAuditService> MockAuditService { get; set; }
        protected Mock<INotificationService> MockNotificationService { get; set; }
        protected Mock<INavigationService> MockNavigationService { get; set; }

        public ViewModelTestBase()
        {
            SetupMocks();
        }

        protected virtual void SetupMocks()
        {
            MockVehicleService = new Mock<IVehicleService>();
            MockContractService = new Mock<IContractService>();
            MockMaintenanceService = new Mock<IMaintenanceService>();
            MockDriverService = new Mock<IDriverService>();
            MockEmployeeService = new Mock<IEmployeeService>();
            MockTripService = new Mock<ITripService>();
            MockFuelService = new Mock<IFuelService>();
            MockExpenseService = new Mock<IExpenseService>();
            MockLicenseService = new Mock<ILicenseService>();
            MockInsuranceService = new Mock<IInsuranceService>();
            MockCustodyService = new Mock<ICustodyService>();
            MockMasterDataService = new Mock<IMasterDataService>();
            MockReportingService = new Mock<IReportingService>();
            MockAuditService = new Mock<IAuditService>();
            MockNotificationService = new Mock<INotificationService>();
            MockNavigationService = new Mock<INavigationService>();
        }

        protected void VerifyPropertyChangedNotification(
            object viewModel,
            string propertyName,
            Action<object> action)
        {
            bool notificationRaised = false;
            var notifyPropertyChanged = viewModel as INotifyPropertyChanged;

            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == propertyName)
                        notificationRaised = true;
                };
            }

            action(viewModel);
            Assert.True(notificationRaised, $"PropertyChanged not raised for {propertyName}");
        }
    }
}

// ============================================================================
// FILE: MockDataFactory.cs
// LOCATION: FleetManagementSystem.Tests/ViewModels/MockDataFactory.cs
// ============================================================================

using System;
using System.Collections.Generic;

namespace FleetManagementSystem.Tests.ViewModels
{
    /// <summary>
    /// Factory for creating mock data for ViewModel unit tests.
    /// </summary>
    public static class MockDataFactory
    {
        public static VehicleDto CreateMockVehicleDto(
            int id = 1,
            string plateNumber = "TEST-001",
            string model = "Toyota Corolla",
            int year = 2023,
            string status = "متاح")
        {
            return new VehicleDto
            {
                Id = id,
                PlateNumber = plateNumber,
                Model = model,
                Year = year,
                Status = status,
                Mileage = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static ContractDto CreateMockContractDto(
            int id = 1,
            int vehicleId = 1,
            string contractNumber = "CNT-2024-001",
            string clientName = "عميل اختبار",
            int statusId = 1)
        {
            return new ContractDto
            {
                Id = id,
                VehicleId = vehicleId,
                ContractNumber = contractNumber,
                ClientName = clientName,
                ContractStatusId = statusId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                ContractValue = 10000,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static MaintenanceRequestDto CreateMockMaintenanceDto(
            int id = 1,
            int vehicleId = 1,
            string status = "مفتوح")
        {
            return new MaintenanceRequestDto
            {
                Id = id,
                VehicleId = vehicleId,
                MaintenanceTypeId = 1,
                Status = status,
                RequestDate = DateTime.UtcNow,
                Cost = 500,
                ServiceProviderId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static DriverDto CreateMockDriverDto(
            int id = 1,
            string fullName = "علي محمد",
            string licenseNumber = "DL-2024-001")
        {
            return new DriverDto
            {
                Id = id,
                FullName = fullName,
                LicenseNumber = licenseNumber,
                LicenseExpiryDate = DateTime.UtcNow.AddYears(2),
                Phone = "0501234567",
                Status = "نشط",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static EmployeeDto CreateMockEmployeeDto(
            int id = 1,
            string fullName = "أحمد محمد",
            string position = "مدير",
            string department = "الإدارة")
        {
            return new EmployeeDto
            {
                Id = id,
                FullName = fullName,
                Position = position,
                Department = department,
                Email = "test@example.com",
                Phone = "0509876543",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static TripDto CreateMockTripDto(
            int id = 1,
            int vehicleId = 1,
            int driverId = 1,
            string destination = "جدة")
        {
            return new TripDto
            {
                Id = id,
                VehicleId = vehicleId,
                DriverId = driverId,
                Destination = destination,
                Status = "مخطط",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static FuelTransactionDto CreateMockFuelDto(
            int id = 1,
            int vehicleId = 1,
            decimal quantity = 50,
            decimal costPerUnit = 2.5m)
        {
            return new FuelTransactionDto
            {
                Id = id,
                VehicleId = vehicleId,
                Quantity = quantity,
                CostPerUnit = costPerUnit,
                TotalCost = quantity * costPerUnit,
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static ExpenseDto CreateMockExpenseDto(
            int id = 1,
            int vehicleId = 1,
            string category = "إصلاح",
            decimal amount = 1500)
        {
            return new ExpenseDto
            {
                Id = id,
                VehicleId = vehicleId,
                Category = category,
                Amount = amount,
                Description = "وصف النفقة",
                ExpenseDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static List<VehicleTypeDto> CreateMockVehicleTypes()
        {
            return new List<VehicleTypeDto>
            {
                new VehicleTypeDto { Id = 1, Name = "سيارة", Description = "سيارة عادية" },
                new VehicleTypeDto { Id = 2, Name = "شاحنة", Description = "شاحنة نقل" },
                new VehicleTypeDto { Id = 3, Name = "حافلة", Description = "حافلة ركاب" }
            };
        }

        public static List<ContractStatusDto> CreateMockContractStatuses()
        {
            return new List<ContractStatusDto>
            {
                new ContractStatusDto { Id = 1, Name = "مسودة" },
                new ContractStatusDto { Id = 2, Name = "نشط" },
                new ContractStatusDto { Id = 3, Name = "منتهي" },
                new ContractStatusDto { Id = 4, Name = "ملغى" }
            };
        }

        public static List<MaintenanceTypeDto> CreateMockMaintenanceTypes()
        {
            return new List<MaintenanceTypeDto>
            {
                new MaintenanceTypeDto { Id = 1, Name = "صيانة دورية" },
                new MaintenanceTypeDto { Id = 2, Name = "إصلاح عطل" },
                new MaintenanceTypeDto { Id = 3, Name = "فحص شامل" }
            };
        }

        public static List<ServiceProviderDto> CreateMockServiceProviders()
        {
            return new List<ServiceProviderDto>
            {
                new ServiceProviderDto { Id = 1, Name = "مركز الصيانة الأول", Phone = "0501111111" },
                new ServiceProviderDto { Id = 2, Name = "مركز الصيانة الثاني", Phone = "0502222222" }
            };
        }
    }
}

// ============================================================================
// FILE: FleetManagementSystem.Tests.csproj (ViewModel Tests)
// LOCATION: FleetManagementSystem.Tests/FleetManagementSystem.Tests.csproj
// ============================================================================

/*
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <IsPackable>false</IsPackable>
    <UseWPF>true</UseWPF>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="xunit" Version="2.6.6" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.6" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
    <PackageReference Include="Moq" Version="4.20.70" />
    <PackageReference Include="Testcontainers" Version="3.7.0" />
    <PackageReference Include="Testcontainers.MySql" Version="3.7.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.MySql" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\FleetManagementSystem.Core\FleetManagementSystem.Core.csproj" />
    <ProjectReference Include="..\FleetManagementSystem.Data\FleetManagementSystem.Data.csproj" />
    <ProjectReference Include="..\FleetManagementSystem.Services\FleetManagementSystem.Services.csproj" />
    <ProjectReference Include="..\FleetManagementSystem.WPF\FleetManagementSystem.WPF.csproj" />
  </ItemGroup>

</Project>
*/
