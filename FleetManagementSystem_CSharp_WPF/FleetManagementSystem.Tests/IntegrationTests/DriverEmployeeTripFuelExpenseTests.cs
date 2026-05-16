// ============================================================================
// FILE: DriverServiceTests.cs
// LOCATION: FleetManagementSystem.Tests/Services/DriverServiceTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetManagementSystem.Tests.Infrastructure;
using Xunit;

namespace FleetManagementSystem.Tests.Services
{
    public class DriverServiceTests : IntegrationTestBase
    {
        public DriverServiceTests(IntegrationTestFixture fixture) : base(fixture) { }

        [Fact]
        public async Task CreateDriver_WithValidData_ShouldSucceed()
        {
            // Arrange
            var driver = TestDataBuilder.CreateTestDriver(
                fullName: "علي محمد",
                licenseNumber: "DL-2024-001"
            );

            // Act
            var result = await DriverService.CreateDriverAsync(driver);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("علي محمد", result.FullName);
            Assert.Equal("DL-2024-001", result.LicenseNumber);
        }

        [Fact]
        public async Task CreateDriver_WithDuplicateLicenseNumber_ShouldFail()
        {
            // Arrange
            var driver1 = TestDataBuilder.CreateTestDriver(licenseNumber: "DL-DUP-001");
            var driver2 = TestDataBuilder.CreateTestDriver(licenseNumber: "DL-DUP-001");

            await DriverService.CreateDriverAsync(driver1);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => DriverService.CreateDriverAsync(driver2)
            );
            Assert.Contains("already exists", ex.Message);
        }

        [Fact]
        public async Task GetDriverById_WithValidId_ShouldReturnDriver()
        {
            // Arrange
            var driver = TestDataBuilder.CreateTestDriver();
            var created = await DriverService.CreateDriverAsync(driver);

            // Act
            var result = await DriverService.GetDriverByIdAsync(created.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(created.Id, result.Id);
        }

        [Fact]
        public async Task GetAllDrivers_ShouldReturnAllDrivers()
        {
            // Arrange
            var drivers = new List<Driver>
            {
                TestDataBuilder.CreateTestDriver(fullName: "محمد أحمد"),
                TestDataBuilder.CreateTestDriver(fullName: "فاطمة علي"),
                TestDataBuilder.CreateTestDriver(fullName: "سارة حسن")
            };

            foreach (var driver in drivers)
            {
                await DriverService.CreateDriverAsync(driver);
            }

            // Act
            var result = await DriverService.GetAllDriversAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 3);
        }

        [Fact]
        public async Task GetDriversWithExpiringLicenses_ShouldReturnDriversExpiringWithinDays()
        {
            // Arrange
            var expiringDriver = TestDataBuilder.CreateTestDriver(
                fullName: "محمد",
                licenseExpiryDate: DateTime.UtcNow.AddDays(5)
            );

            var validDriver = TestDataBuilder.CreateTestDriver(
                fullName: "علي",
                licenseExpiryDate: DateTime.UtcNow.AddYears(2)
            );

            await DriverService.CreateDriverAsync(expiringDriver);
            await DriverService.CreateDriverAsync(validDriver);

            // Act
            var result = await DriverService.GetDriversWithExpiringLicensesAsync(30);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.All(result, d => Assert.True((d.LicenseExpiryDate - DateTime.UtcNow).TotalDays <= 30));
        }

        [Fact]
        public async Task UpdateDriver_WithValidData_ShouldSucceed()
        {
            // Arrange
            var driver = TestDataBuilder.CreateTestDriver();
            var created = await DriverService.CreateDriverAsync(driver);
            created.Status = "معطل";
            created.Phone = "0509999999";

            // Act
            var result = await DriverService.UpdateDriverAsync(created);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("معطل", result.Status);
            Assert.Equal("0509999999", result.Phone);
        }

        [Fact]
        public async Task DeleteDriver_ShouldSucceed()
        {
            // Arrange
            var driver = TestDataBuilder.CreateTestDriver();
            var created = await DriverService.CreateDriverAsync(driver);

            // Act
            await DriverService.DeleteDriverAsync(created.Id);

            // Assert
            var result = await DriverService.GetDriverByIdAsync(created.Id);
            Assert.Null(result);
        }
    }
}

// ============================================================================
// FILE: EmployeeServiceTests.cs
// LOCATION: FleetManagementSystem.Tests/Services/EmployeeServiceTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetManagementSystem.Tests.Infrastructure;
using Xunit;

namespace FleetManagementSystem.Tests.Services
{
    public class EmployeeServiceTests : IntegrationTestBase
    {
        public EmployeeServiceTests(IntegrationTestFixture fixture) : base(fixture) { }

        [Fact]
        public async Task CreateEmployee_WithValidData_ShouldSucceed()
        {
            // Arrange
            var employee = TestDataBuilder.CreateTestEmployee(
                fullName: "أحمد محمد",
                position: "مدير",
                department: "الإدارة"
            );

            // Act
            var result = await EmployeeService.CreateEmployeeAsync(employee);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("أحمد محمد", result.FullName);
            Assert.Equal("مدير", result.Position);
        }

        [Fact]
        public async Task GetEmployeesByDepartment_ShouldReturnOnlyMatchingDepartment()
        {
            // Arrange
            var employee1 = TestDataBuilder.CreateTestEmployee(department: "العمليات");
            var employee2 = TestDataBuilder.CreateTestEmployee(department: "العمليات");
            var employee3 = TestDataBuilder.CreateTestEmployee(department: "الإدارة");

            await EmployeeService.CreateEmployeeAsync(employee1);
            await EmployeeService.CreateEmployeeAsync(employee2);
            await EmployeeService.CreateEmployeeAsync(employee3);

            // Act
            var result = await EmployeeService.GetEmployeesByDepartmentAsync("العمليات");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
            Assert.All(result, e => Assert.Equal("العمليات", e.Department));
        }

        [Fact]
        public async Task UpdateEmployee_WithValidData_ShouldSucceed()
        {
            // Arrange
            var employee = TestDataBuilder.CreateTestEmployee();
            var created = await EmployeeService.CreateEmployeeAsync(employee);
            created.Position = "مشرف أول";
            created.Department = "الصيانة";

            // Act
            var result = await EmployeeService.UpdateEmployeeAsync(created);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("مشرف أول", result.Position);
            Assert.Equal("الصيانة", result.Department);
        }

        [Fact]
        public async Task DeleteEmployee_ShouldSucceed()
        {
            // Arrange
            var employee = TestDataBuilder.CreateTestEmployee();
            var created = await EmployeeService.CreateEmployeeAsync(employee);

            // Act
            await EmployeeService.DeleteEmployeeAsync(created.Id);

            // Assert
            var result = await EmployeeService.GetEmployeeByIdAsync(created.Id);
            Assert.Null(result);
        }
    }
}

// ============================================================================
// FILE: TripServiceTests.cs
// LOCATION: FleetManagementSystem.Tests/Services/TripServiceTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetManagementSystem.Tests.Infrastructure;
using Xunit;

namespace FleetManagementSystem.Tests.Services
{
    public class TripServiceTests : IntegrationTestBase
    {
        public TripServiceTests(IntegrationTestFixture fixture) : base(fixture) { }

        [Fact]
        public async Task CreateTrip_WithValidData_ShouldSucceed()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var driver = TestDataBuilder.CreateTestDriver();
            var createdDriver = await DriverService.CreateDriverAsync(driver);

            var trip = TestDataBuilder.CreateTestTrip(
                vehicleId: createdVehicle.Id,
                driverId: createdDriver.Id,
                destination: "جدة"
            );

            // Act
            var result = await TripService.CreateTripAsync(trip);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("جدة", result.Destination);
            Assert.Equal("مخطط", result.Status);
        }

        [Fact]
        public async Task MarkTripAsComplete_ShouldUpdateStatus()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var driver = TestDataBuilder.CreateTestDriver();
            var createdDriver = await DriverService.CreateDriverAsync(driver);

            var trip = TestDataBuilder.CreateTestTrip(vehicleId: createdVehicle.Id, driverId: createdDriver.Id);
            var created = await TripService.CreateTripAsync(trip);

            // Act
            var result = await TripService.MarkTripAsCompleteAsync(created.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("مكتمل", result.Status);
        }

        [Fact]
        public async Task GetTripsByVehicle_ShouldReturnVehicleTrips()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var driver = TestDataBuilder.CreateTestDriver();
            var createdDriver = await DriverService.CreateDriverAsync(driver);

            var trip1 = TestDataBuilder.CreateTestTrip(vehicleId: createdVehicle.Id, driverId: createdDriver.Id);
            var trip2 = TestDataBuilder.CreateTestTrip(vehicleId: createdVehicle.Id, driverId: createdDriver.Id);

            await TripService.CreateTripAsync(trip1);
            await TripService.CreateTripAsync(trip2);

            // Act
            var result = await TripService.GetTripsByVehicleAsync(createdVehicle.Id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
            Assert.All(result, t => Assert.Equal(createdVehicle.Id, t.VehicleId));
        }

        [Fact]
        public async Task DeleteTrip_ShouldSucceed()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var driver = TestDataBuilder.CreateTestDriver();
            var createdDriver = await DriverService.CreateDriverAsync(driver);

            var trip = TestDataBuilder.CreateTestTrip(vehicleId: createdVehicle.Id, driverId: createdDriver.Id);
            var created = await TripService.CreateTripAsync(trip);

            // Act
            await TripService.DeleteTripAsync(created.Id);

            // Assert
            var result = await TripService.GetTripByIdAsync(created.Id);
            Assert.Null(result);
        }
    }
}

// ============================================================================
// FILE: FuelServiceTests.cs
// LOCATION: FleetManagementSystem.Tests/Services/FuelServiceTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetManagementSystem.Tests.Infrastructure;
using Xunit;

namespace FleetManagementSystem.Tests.Services
{
    public class FuelServiceTests : IntegrationTestBase
    {
        public FuelServiceTests(IntegrationTestFixture fixture) : base(fixture) { }

        [Fact]
        public async Task CreateFuelTransaction_WithValidData_ShouldSucceed()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var fuel = TestDataBuilder.CreateTestFuelTransaction(
                vehicleId: createdVehicle.Id,
                quantity: 50,
                costPerUnit: 2.5m
            );

            // Act
            var result = await FuelService.CreateFuelTransactionAsync(fuel);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal(50, result.Quantity);
            Assert.Equal(125, result.TotalCost); // 50 * 2.5
        }

        [Fact]
        public async Task GetFuelTransactionsByVehicle_ShouldReturnVehicleFuelHistory()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var fuel1 = TestDataBuilder.CreateTestFuelTransaction(vehicleId: createdVehicle.Id);
            var fuel2 = TestDataBuilder.CreateTestFuelTransaction(vehicleId: createdVehicle.Id);

            await FuelService.CreateFuelTransactionAsync(fuel1);
            await FuelService.CreateFuelTransactionAsync(fuel2);

            // Act
            var result = await FuelService.GetFuelTransactionsByVehicleAsync(createdVehicle.Id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
            Assert.All(result, f => Assert.Equal(createdVehicle.Id, f.VehicleId));
        }

        [Fact]
        public async Task GetFuelConsumptionReport_ShouldCalculateTotalConsumption()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var fuel1 = TestDataBuilder.CreateTestFuelTransaction(vehicleId: createdVehicle.Id, quantity: 50);
            var fuel2 = TestDataBuilder.CreateTestFuelTransaction(vehicleId: createdVehicle.Id, quantity: 30);

            await FuelService.CreateFuelTransactionAsync(fuel1);
            await FuelService.CreateFuelTransactionAsync(fuel2);

            // Act
            var result = await FuelService.GetFuelConsumptionReportAsync(createdVehicle.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(80, result.TotalQuantity); // 50 + 30
        }

        [Fact]
        public async Task DeleteFuelTransaction_ShouldSucceed()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var fuel = TestDataBuilder.CreateTestFuelTransaction(vehicleId: createdVehicle.Id);
            var created = await FuelService.CreateFuelTransactionAsync(fuel);

            // Act
            await FuelService.DeleteFuelTransactionAsync(created.Id);

            // Assert
            var result = await FuelService.GetFuelTransactionByIdAsync(created.Id);
            Assert.Null(result);
        }
    }
}

// ============================================================================
// FILE: ExpenseServiceTests.cs
// LOCATION: FleetManagementSystem.Tests/Services/ExpenseServiceTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetManagementSystem.Tests.Infrastructure;
using Xunit;

namespace FleetManagementSystem.Tests.Services
{
    public class ExpenseServiceTests : IntegrationTestBase
    {
        public ExpenseServiceTests(IntegrationTestFixture fixture) : base(fixture) { }

        [Fact]
        public async Task CreateExpense_WithValidData_ShouldSucceed()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var expense = TestDataBuilder.CreateTestExpense(
                vehicleId: createdVehicle.Id,
                category: "إصلاح",
                amount: 1500
            );

            // Act
            var result = await ExpenseService.CreateExpenseAsync(expense);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal(1500, result.Amount);
            Assert.Equal("إصلاح", result.Category);
        }

        [Fact]
        public async Task GetExpensesByVehicle_ShouldReturnVehicleExpenses()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var expense1 = TestDataBuilder.CreateTestExpense(vehicleId: createdVehicle.Id);
            var expense2 = TestDataBuilder.CreateTestExpense(vehicleId: createdVehicle.Id);

            await ExpenseService.CreateExpenseAsync(expense1);
            await ExpenseService.CreateExpenseAsync(expense2);

            // Act
            var result = await ExpenseService.GetExpensesByVehicleAsync(createdVehicle.Id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
            Assert.All(result, e => Assert.Equal(createdVehicle.Id, e.VehicleId));
        }

        [Fact]
        public async Task GetExpensesByCategory_ShouldReturnOnlyMatchingCategory()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var repair = TestDataBuilder.CreateTestExpense(vehicleId: createdVehicle.Id, category: "إصلاح");
            var maintenance = TestDataBuilder.CreateTestExpense(vehicleId: createdVehicle.Id, category: "صيانة");

            await ExpenseService.CreateExpenseAsync(repair);
            await ExpenseService.CreateExpenseAsync(maintenance);

            // Act
            var result = await ExpenseService.GetExpensesByCategoryAsync("إصلاح");

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.All(result, e => Assert.Equal("إصلاح", e.Category));
        }

        [Fact]
        public async Task DeleteExpense_ShouldSucceed()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var expense = TestDataBuilder.CreateTestExpense(vehicleId: createdVehicle.Id);
            var created = await ExpenseService.CreateExpenseAsync(expense);

            // Act
            await ExpenseService.DeleteExpenseAsync(created.Id);

            // Assert
            var result = await ExpenseService.GetExpenseByIdAsync(created.Id);
            Assert.Null(result);
        }
    }
}
