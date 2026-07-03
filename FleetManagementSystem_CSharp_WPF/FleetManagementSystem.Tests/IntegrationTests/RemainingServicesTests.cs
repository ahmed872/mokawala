// ============================================================================
// FILE: LicenseServiceTests.cs
// LOCATION: FleetManagementSystem.Tests/Services/LicenseServiceTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetManagementSystem.Tests.Infrastructure;
using Xunit;

namespace FleetManagementSystem.Tests.Services
{
    public class LicenseServiceTests : IntegrationTestBase
    {
        public LicenseServiceTests(IntegrationTestFixture fixture) : base(fixture) { }

        [Fact]
        public async Task CreateLicense_WithValidData_ShouldSucceed()
        {
            // Arrange
            var driver = TestDataBuilder.CreateTestDriver();
            var createdDriver = await DriverService.CreateDriverAsync(driver);

            var license = TestDataBuilder.CreateTestLicense(
                driverId: createdDriver.Id,
                licenseType: "عام"
            );

            // Act
            var result = await LicenseService.CreateLicenseAsync(license);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("عام", result.LicenseType);
        }

        [Fact]
        public async Task GetLicensesByDriver_ShouldReturnDriverLicenses()
        {
            // Arrange
            var driver = TestDataBuilder.CreateTestDriver();
            var createdDriver = await DriverService.CreateDriverAsync(driver);

            var license1 = TestDataBuilder.CreateTestLicense(driverId: createdDriver.Id);
            var license2 = TestDataBuilder.CreateTestLicense(driverId: createdDriver.Id);

            await LicenseService.CreateLicenseAsync(license1);
            await LicenseService.CreateLicenseAsync(license2);

            // Act
            var result = await LicenseService.GetLicensesByDriverAsync(createdDriver.Id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
            Assert.All(result, l => Assert.Equal(createdDriver.Id, l.DriverId));
        }

        [Fact]
        public async Task GetExpiringLicenses_ShouldReturnLicensesExpiringWithinDays()
        {
            // Arrange
            var driver = TestDataBuilder.CreateTestDriver();
            var createdDriver = await DriverService.CreateDriverAsync(driver);

            var expiringLicense = TestDataBuilder.CreateTestLicense(
                driverId: createdDriver.Id,
                expiryDate: DateTime.UtcNow.AddDays(5)
            );

            var validLicense = TestDataBuilder.CreateTestLicense(
                driverId: createdDriver.Id,
                expiryDate: DateTime.UtcNow.AddYears(2)
            );

            await LicenseService.CreateLicenseAsync(expiringLicense);
            await LicenseService.CreateLicenseAsync(validLicense);

            // Act
            var result = await LicenseService.GetExpiringLicensesAsync(30);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.All(result, l => Assert.True((l.ExpiryDate - DateTime.UtcNow).TotalDays <= 30));
        }

        [Fact]
        public async Task DeleteLicense_ShouldSucceed()
        {
            // Arrange
            var driver = TestDataBuilder.CreateTestDriver();
            var createdDriver = await DriverService.CreateDriverAsync(driver);

            var license = TestDataBuilder.CreateTestLicense(driverId: createdDriver.Id);
            var created = await LicenseService.CreateLicenseAsync(license);

            // Act
            await LicenseService.DeleteLicenseAsync(created.Id);

            // Assert
            var result = await LicenseService.GetLicenseByIdAsync(created.Id);
            Assert.Null(result);
        }
    }
}

// ============================================================================
// FILE: InsuranceServiceTests.cs
// LOCATION: FleetManagementSystem.Tests/Services/InsuranceServiceTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetManagementSystem.Tests.Infrastructure;
using Xunit;

namespace FleetManagementSystem.Tests.Services
{
    public class InsuranceServiceTests : IntegrationTestBase
    {
        public InsuranceServiceTests(IntegrationTestFixture fixture) : base(fixture) { }

        [Fact]
        public async Task CreateInsurance_WithValidData_ShouldSucceed()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var insurance = TestDataBuilder.CreateTestInsurance(
                vehicleId: createdVehicle.Id,
                policyNumber: "POL-2024-001"
            );

            // Act
            var result = await InsuranceService.CreateInsuranceAsync(insurance);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("POL-2024-001", result.PolicyNumber);
        }

        [Fact]
        public async Task GetInsuranceByVehicle_ShouldReturnVehicleInsurance()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var insurance = TestDataBuilder.CreateTestInsurance(vehicleId: createdVehicle.Id);
            await InsuranceService.CreateInsuranceAsync(insurance);

            // Act
            var result = await InsuranceService.GetInsuranceByVehicleAsync(createdVehicle.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createdVehicle.Id, result.VehicleId);
        }

        [Fact]
        public async Task GetExpiringInsurances_ShouldReturnPoliciesExpiringWithinDays()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var expiringInsurance = TestDataBuilder.CreateTestInsurance(
                vehicleId: createdVehicle.Id,
                endDate: DateTime.UtcNow.AddDays(5)
            );

            var validInsurance = TestDataBuilder.CreateTestInsurance(
                vehicleId: createdVehicle.Id,
                endDate: DateTime.UtcNow.AddYears(2)
            );

            await InsuranceService.CreateInsuranceAsync(expiringInsurance);
            await InsuranceService.CreateInsuranceAsync(validInsurance);

            // Act
            var result = await InsuranceService.GetExpiringInsurancesAsync(30);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.All(result, i => Assert.True((i.EndDate - DateTime.UtcNow).TotalDays <= 30));
        }

        [Fact]
        public async Task DeleteInsurance_ShouldSucceed()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var insurance = TestDataBuilder.CreateTestInsurance(vehicleId: createdVehicle.Id);
            var created = await InsuranceService.CreateInsuranceAsync(insurance);

            // Act
            await InsuranceService.DeleteInsuranceAsync(created.Id);

            // Assert
            var result = await InsuranceService.GetInsuranceByIdAsync(created.Id);
            Assert.Null(result);
        }
    }
}

// ============================================================================
// FILE: CustodyServiceTests.cs
// LOCATION: FleetManagementSystem.Tests/Services/CustodyServiceTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetManagementSystem.Tests.Infrastructure;
using Xunit;

namespace FleetManagementSystem.Tests.Services
{
    public class CustodyServiceTests : IntegrationTestBase
    {
        public CustodyServiceTests(IntegrationTestFixture fixture) : base(fixture) { }

        [Fact]
        public async Task CreateCustody_WithValidData_ShouldSucceed()
        {
            // Arrange
            var employee = TestDataBuilder.CreateTestEmployee();
            var createdEmployee = await EmployeeService.CreateEmployeeAsync(employee);

            var custody = TestDataBuilder.CreateTestCustody(
                assignedToEmployeeId: createdEmployee.Id,
                itemDescription: "جهاز GPS"
            );

            // Act
            var result = await CustodyService.CreateCustodyAsync(custody);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("جهاز GPS", result.ItemDescription);
            Assert.Equal("مُسَلَّم", result.Status);
        }

        [Fact]
        public async Task MarkCustodyAsReturned_ShouldUpdateStatus()
        {
            // Arrange
            var employee = TestDataBuilder.CreateTestEmployee();
            var createdEmployee = await EmployeeService.CreateEmployeeAsync(employee);

            var custody = TestDataBuilder.CreateTestCustody(assignedToEmployeeId: createdEmployee.Id);
            var created = await CustodyService.CreateCustodyAsync(custody);

            // Act
            var result = await CustodyService.MarkCustodyAsReturnedAsync(created.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("مُرْجَع", result.Status);
        }

        [Fact]
        public async Task GetCustodyByEmployee_ShouldReturnEmployeeCustody()
        {
            // Arrange
            var employee = TestDataBuilder.CreateTestEmployee();
            var createdEmployee = await EmployeeService.CreateEmployeeAsync(employee);

            var custody1 = TestDataBuilder.CreateTestCustody(assignedToEmployeeId: createdEmployee.Id);
            var custody2 = TestDataBuilder.CreateTestCustody(assignedToEmployeeId: createdEmployee.Id);

            await CustodyService.CreateCustodyAsync(custody1);
            await CustodyService.CreateCustodyAsync(custody2);

            // Act
            var result = await CustodyService.GetCustodyByEmployeeAsync(createdEmployee.Id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
            Assert.All(result, c => Assert.Equal(createdEmployee.Id, c.AssignedToEmployeeId));
        }

        [Fact]
        public async Task GetPendingCustody_ShouldReturnUnreturnedItems()
        {
            // Arrange
            var employee = TestDataBuilder.CreateTestEmployee();
            var createdEmployee = await EmployeeService.CreateEmployeeAsync(employee);

            var pending = TestDataBuilder.CreateTestCustody(
                assignedToEmployeeId: createdEmployee.Id,
                status: "مُسَلَّم"
            );

            var returned = TestDataBuilder.CreateTestCustody(
                assignedToEmployeeId: createdEmployee.Id,
                status: "مُرْجَع"
            );

            await CustodyService.CreateCustodyAsync(pending);
            await CustodyService.CreateCustodyAsync(returned);

            // Act
            var result = await CustodyService.GetPendingCustodyAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.All(result, c => Assert.Equal("مُسَلَّم", c.Status));
        }

        [Fact]
        public async Task DeleteCustody_ShouldSucceed()
        {
            // Arrange
            var employee = TestDataBuilder.CreateTestEmployee();
            var createdEmployee = await EmployeeService.CreateEmployeeAsync(employee);

            var custody = TestDataBuilder.CreateTestCustody(assignedToEmployeeId: createdEmployee.Id);
            var created = await CustodyService.CreateCustodyAsync(custody);

            // Act
            await CustodyService.DeleteCustodyAsync(created.Id);

            // Assert
            var result = await CustodyService.GetCustodyByIdAsync(created.Id);
            Assert.Null(result);
        }
    }
}

// ============================================================================
// FILE: MasterDataServiceTests.cs
// LOCATION: FleetManagementSystem.Tests/Services/MasterDataServiceTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetManagementSystem.Tests.Infrastructure;
using Xunit;

namespace FleetManagementSystem.Tests.Services
{
    public class MasterDataServiceTests : IntegrationTestBase
    {
        public MasterDataServiceTests(IntegrationTestFixture fixture) : base(fixture) { }

        [Fact]
        public async Task GetAllVehicleTypes_ShouldReturnAllTypes()
        {
            // Act
            var result = await MasterDataService.GetAllVehicleTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 3); // From seed data
        }

        [Fact]
        public async Task GetAllContractStatuses_ShouldReturnAllStatuses()
        {
            // Act
            var result = await MasterDataService.GetAllContractStatusesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 4); // From seed data
        }

        [Fact]
        public async Task GetAllMaintenanceTypes_ShouldReturnAllTypes()
        {
            // Act
            var result = await MasterDataService.GetAllMaintenanceTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 3); // From seed data
        }

        [Fact]
        public async Task GetAllServiceProviders_ShouldReturnAllProviders()
        {
            // Act
            var result = await MasterDataService.GetAllServiceProvidersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2); // From seed data
        }

        [Fact]
        public async Task CreateVehicleType_WithValidData_ShouldSucceed()
        {
            // Arrange
            var vehicleType = new VehicleType
            {
                Name = "دراجة نارية",
                Description = "دراجة نارية للتوصيل"
            };

            // Act
            var result = await MasterDataService.CreateVehicleTypeAsync(vehicleType);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("دراجة نارية", result.Name);
        }

        [Fact]
        public async Task UpdateVehicleType_WithValidData_ShouldSucceed()
        {
            // Arrange
            var vehicleType = new VehicleType { Name = "نوع جديد", Description = "وصف جديد" };
            var created = await MasterDataService.CreateVehicleTypeAsync(vehicleType);
            created.Description = "وصف محدث";

            // Act
            var result = await MasterDataService.UpdateVehicleTypeAsync(created);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("وصف محدث", result.Description);
        }

        [Fact]
        public async Task DeleteVehicleType_ShouldSucceed()
        {
            // Arrange
            var vehicleType = new VehicleType { Name = "نوع للحذف", Description = "سيتم حذفه" };
            var created = await MasterDataService.CreateVehicleTypeAsync(vehicleType);

            // Act
            await MasterDataService.DeleteVehicleTypeAsync(created.Id);

            // Assert
            var result = await MasterDataService.GetVehicleTypeByIdAsync(created.Id);
            Assert.Null(result);
        }
    }
}

// ============================================================================
// FILE: ReportingServiceTests.cs
// LOCATION: FleetManagementSystem.Tests/Services/ReportingServiceTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetManagementSystem.Tests.Infrastructure;
using Xunit;

namespace FleetManagementSystem.Tests.Services
{
    public class ReportingServiceTests : IntegrationTestBase
    {
        public ReportingServiceTests(IntegrationTestFixture fixture) : base(fixture) { }

        [Fact]
        public async Task GetVehicleReport_ShouldReturnAllVehicles()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                TestDataBuilder.CreateTestVehicle(),
                TestDataBuilder.CreateTestVehicle(),
                TestDataBuilder.CreateTestVehicle()
            };

            foreach (var vehicle in vehicles)
            {
                await VehicleService.CreateVehicleAsync(vehicle);
            }

            // Act
            var result = await ReportingService.GetVehicleReportAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 3);
        }

        [Fact]
        public async Task GetContractReport_ShouldReturnAllContracts()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var contracts = new List<Contract>
            {
                TestDataBuilder.CreateTestContract(vehicleId: createdVehicle.Id),
                TestDataBuilder.CreateTestContract(vehicleId: createdVehicle.Id)
            };

            foreach (var contract in contracts)
            {
                await ContractService.CreateContractAsync(contract);
            }

            // Act
            var result = await ReportingService.GetContractReportAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
        }

        [Fact]
        public async Task GetMaintenanceReport_ShouldReturnAllMaintenanceRequests()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var maintenances = new List<MaintenanceRequest>
            {
                TestDataBuilder.CreateTestMaintenanceRequest(vehicleId: createdVehicle.Id),
                TestDataBuilder.CreateTestMaintenanceRequest(vehicleId: createdVehicle.Id)
            };

            foreach (var maintenance in maintenances)
            {
                await MaintenanceService.CreateMaintenanceRequestAsync(maintenance);
            }

            // Act
            var result = await ReportingService.GetMaintenanceReportAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
        }

        [Fact]
        public async Task GetFleetStatistics_ShouldReturnAccurateStats()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                TestDataBuilder.CreateTestVehicle(status: "متاح"),
                TestDataBuilder.CreateTestVehicle(status: "متاح"),
                TestDataBuilder.CreateTestVehicle(status: "تحت الصيانة")
            };

            foreach (var vehicle in vehicles)
            {
                await VehicleService.CreateVehicleAsync(vehicle);
            }

            // Act
            var result = await ReportingService.GetFleetStatisticsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.TotalVehicles >= 3);
            Assert.True(result.AvailableVehicles >= 2);
            Assert.True(result.MaintenanceVehicles >= 1);
        }
    }
}

// ============================================================================
// FILE: AuditServiceTests.cs
// LOCATION: FleetManagementSystem.Tests/Services/AuditServiceTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetManagementSystem.Tests.Infrastructure;
using Xunit;

namespace FleetManagementSystem.Tests.Services
{
    public class AuditServiceTests : IntegrationTestBase
    {
        public AuditServiceTests(IntegrationTestFixture fixture) : base(fixture) { }

        [Fact]
        public async Task LogAuditEntry_ShouldSucceed()
        {
            // Arrange
            var auditLog = new AuditLog
            {
                EntityType = "Vehicle",
                EntityId = 1,
                Action = "Create",
                Changes = "PlateNumber: ABC-123",
                UserId = 1,
                Timestamp = DateTime.UtcNow
            };

            // Act
            await AuditService.LogAuditEntryAsync(auditLog);

            // Assert
            var result = await DbContext.AuditLogs
                .FirstOrDefaultAsync(al => al.EntityType == "Vehicle" && al.EntityId == 1);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetAuditLogsByEntity_ShouldReturnEntityAuditLogs()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var created = await VehicleService.CreateVehicleAsync(vehicle);

            // Act
            var result = await AuditService.GetAuditLogsByEntityAsync("Vehicle", created.Id);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.All(result, al => Assert.Equal(created.Id, al.EntityId));
        }

        [Fact]
        public async Task GetAuditLogsByUser_ShouldReturnUserAuditLogs()
        {
            // Arrange - Create audit logs for user 1
            var vehicle = TestDataBuilder.CreateTestVehicle();
            await VehicleService.CreateVehicleAsync(vehicle);

            // Act
            var result = await AuditService.GetAuditLogsByUserAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetAuditLogsByDateRange_ShouldReturnLogsWithinRange()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            await VehicleService.CreateVehicleAsync(vehicle);

            var startDate = DateTime.UtcNow.AddDays(-1);
            var endDate = DateTime.UtcNow.AddDays(1);

            // Act
            var result = await AuditService.GetAuditLogsByDateRangeAsync(startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.All(result, al => Assert.True(al.Timestamp >= startDate && al.Timestamp <= endDate));
        }
    }
}

// ============================================================================
// FILE: NotificationServiceTests.cs
// LOCATION: FleetManagementSystem.Tests/Services/NotificationServiceTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetManagementSystem.Tests.Infrastructure;
using Xunit;

namespace FleetManagementSystem.Tests.Services
{
    public class NotificationServiceTests : IntegrationTestBase
    {
        public NotificationServiceTests(IntegrationTestFixture fixture) : base(fixture) { }

        [Fact]
        public async Task CreateNotification_WithValidData_ShouldSucceed()
        {
            // Arrange
            var notification = new Notification
            {
                Title = "تنبيه اختبار",
                Message = "هذا تنبيه اختبار",
                Type = "Info",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await NotificationService.CreateNotificationAsync(notification);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("تنبيه اختبار", result.Title);
        }

        [Fact]
        public async Task GetUnreadNotifications_ShouldReturnOnlyUnreadNotifications()
        {
            // Arrange
            var unread = new Notification
            {
                Title = "غير مقروء",
                Message = "تنبيه غير مقروء",
                Type = "Warning",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            var read = new Notification
            {
                Title = "مقروء",
                Message = "تنبيه مقروء",
                Type = "Info",
                IsRead = true,
                CreatedAt = DateTime.UtcNow
            };

            await NotificationService.CreateNotificationAsync(unread);
            await NotificationService.CreateNotificationAsync(read);

            // Act
            var result = await NotificationService.GetUnreadNotificationsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.All(result, n => Assert.False(n.IsRead));
        }

        [Fact]
        public async Task MarkNotificationAsRead_ShouldUpdateStatus()
        {
            // Arrange
            var notification = new Notification
            {
                Title = "تنبيه",
                Message = "رسالة",
                Type = "Info",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            var created = await NotificationService.CreateNotificationAsync(notification);

            // Act
            var result = await NotificationService.MarkAsReadAsync(created.Id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsRead);
        }

        [Fact]
        public async Task DeleteNotification_ShouldSucceed()
        {
            // Arrange
            var notification = new Notification
            {
                Title = "تنبيه للحذف",
                Message = "سيتم حذفه",
                Type = "Info",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            var created = await NotificationService.CreateNotificationAsync(notification);

            // Act
            await NotificationService.DeleteNotificationAsync(created.Id);

            // Assert
            var result = await NotificationService.GetNotificationByIdAsync(created.Id);
            Assert.Null(result);
        }
    }
}
