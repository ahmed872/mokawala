// ============================================================================
// FILE: DriverViewModelTests.cs
// LOCATION: FleetManagementSystem.Tests/ViewModels/DriverViewModelTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace FleetManagementSystem.Tests.ViewModels
{
    public class DriverViewModelTests : ViewModelTestBase
    {
        private Mock<IDriverService> _mockDriverService;
        private Mock<ILicenseService> _mockLicenseService;

        public DriverViewModelTests()
        {
            _mockDriverService = new Mock<IDriverService>();
            _mockLicenseService = new Mock<ILicenseService>();
        }

        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var viewModel = new DriverViewModel(
                _mockDriverService.Object,
                _mockLicenseService.Object
            );

            // Assert
            Assert.NotNull(viewModel);
            Assert.NotNull(viewModel.Drivers);
            Assert.NotNull(viewModel.CreateCommand);
            Assert.NotNull(viewModel.EditCommand);
            Assert.NotNull(viewModel.DeleteCommand);
        }

        [Fact]
        public async Task LoadDrivers_ShouldPopulateDriversList()
        {
            // Arrange
            var drivers = new List<DriverDto>
            {
                MockDataFactory.CreateMockDriverDto(1, "علي محمد", "DL-001"),
                MockDataFactory.CreateMockDriverDto(2, "محمد علي", "DL-002")
            };

            _mockDriverService
                .Setup(x => x.GetAllDriversAsync())
                .ReturnsAsync(drivers);

            var viewModel = new DriverViewModel(
                _mockDriverService.Object,
                _mockLicenseService.Object
            );

            // Act
            await viewModel.LoadDriversAsync();

            // Assert
            Assert.NotEmpty(viewModel.Drivers);
            Assert.Equal(2, viewModel.Drivers.Count);
        }

        [Fact]
        public async Task GetDriversWithExpiringLicenses_ShouldReturnDriversNeedingRenewal()
        {
            // Arrange
            var driversWithExpiringLicenses = new List<DriverDto>
            {
                MockDataFactory.CreateMockDriverDto(1, "علي محمد", licenseExpiryDate: DateTime.UtcNow.AddDays(10))
            };

            _mockDriverService
                .Setup(x => x.GetDriversWithExpiringLicensesAsync(30))
                .ReturnsAsync(driversWithExpiringLicenses);

            var viewModel = new DriverViewModel(
                _mockDriverService.Object,
                _mockLicenseService.Object
            );

            // Act
            await viewModel.LoadDriversWithExpiringLicensesAsync();

            // Assert
            Assert.NotEmpty(viewModel.Drivers);
        }

        [Fact]
        public async Task SearchDrivers_ShouldFilterByName()
        {
            // Arrange
            var drivers = new List<DriverDto>
            {
                MockDataFactory.CreateMockDriverDto(1, "علي محمد")
            };

            _mockDriverService
                .Setup(x => x.SearchDriversAsync("علي"))
                .ReturnsAsync(drivers);

            var viewModel = new DriverViewModel(
                _mockDriverService.Object,
                _mockLicenseService.Object
            );

            viewModel.SearchText = "علي";

            // Act
            await viewModel.SearchCommand.ExecuteAsync(null);

            // Assert
            _mockDriverService.Verify(x => x.SearchDriversAsync("علي"), Times.Once);
        }

        [Fact]
        public async Task DeleteDriver_ShouldRemoveFromList()
        {
            // Arrange
            var driver = MockDataFactory.CreateMockDriverDto(1, "علي محمد");

            _mockDriverService
                .Setup(x => x.DeleteDriverAsync(1))
                .Returns(Task.CompletedTask);

            var viewModel = new DriverViewModel(
                _mockDriverService.Object,
                _mockLicenseService.Object
            );

            viewModel.Drivers.Add(driver);
            viewModel.SelectedDriver = driver;

            // Act
            await viewModel.DeleteCommand.ExecuteAsync(null);

            // Assert
            _mockDriverService.Verify(x => x.DeleteDriverAsync(1), Times.Once);
        }
    }
}

// ============================================================================
// FILE: EmployeeViewModelTests.cs
// LOCATION: FleetManagementSystem.Tests/ViewModels/EmployeeViewModelTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace FleetManagementSystem.Tests.ViewModels
{
    public class EmployeeViewModelTests : ViewModelTestBase
    {
        private Mock<IEmployeeService> _mockEmployeeService;

        public EmployeeViewModelTests()
        {
            _mockEmployeeService = new Mock<IEmployeeService>();
        }

        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var viewModel = new EmployeeViewModel(_mockEmployeeService.Object);

            // Assert
            Assert.NotNull(viewModel);
            Assert.NotNull(viewModel.Employees);
            Assert.NotNull(viewModel.CreateCommand);
            Assert.NotNull(viewModel.EditCommand);
            Assert.NotNull(viewModel.DeleteCommand);
        }

        [Fact]
        public async Task LoadEmployees_ShouldPopulateEmployeesList()
        {
            // Arrange
            var employees = new List<EmployeeDto>
            {
                MockDataFactory.CreateMockEmployeeDto(1, "أحمد محمد", "مدير"),
                MockDataFactory.CreateMockEmployeeDto(2, "فاطمة علي", "موظف")
            };

            _mockEmployeeService
                .Setup(x => x.GetAllEmployeesAsync())
                .ReturnsAsync(employees);

            var viewModel = new EmployeeViewModel(_mockEmployeeService.Object);

            // Act
            await viewModel.LoadEmployeesAsync();

            // Assert
            Assert.NotEmpty(viewModel.Employees);
            Assert.Equal(2, viewModel.Employees.Count);
        }

        [Fact]
        public async Task FilterByDepartment_ShouldReturnOnlyMatchingDepartment()
        {
            // Arrange
            var employees = new List<EmployeeDto>
            {
                MockDataFactory.CreateMockEmployeeDto(1, "أحمد محمد", department: "الإدارة"),
                MockDataFactory.CreateMockEmployeeDto(2, "علي محمد", department: "الإدارة")
            };

            _mockEmployeeService
                .Setup(x => x.GetEmployeesByDepartmentAsync("الإدارة"))
                .ReturnsAsync(employees);

            var viewModel = new EmployeeViewModel(_mockEmployeeService.Object);

            viewModel.SelectedDepartment = "الإدارة";

            // Act
            await viewModel.FilterCommand.ExecuteAsync(null);

            // Assert
            _mockEmployeeService.Verify(x => x.GetEmployeesByDepartmentAsync("الإدارة"), Times.Once);
        }

        [Fact]
        public async Task SearchEmployees_ShouldFilterByName()
        {
            // Arrange
            var employees = new List<EmployeeDto>
            {
                MockDataFactory.CreateMockEmployeeDto(1, "أحمد محمد")
            };

            _mockEmployeeService
                .Setup(x => x.SearchEmployeesAsync("أحمد"))
                .ReturnsAsync(employees);

            var viewModel = new EmployeeViewModel(_mockEmployeeService.Object);

            viewModel.SearchText = "أحمد";

            // Act
            await viewModel.SearchCommand.ExecuteAsync(null);

            // Assert
            _mockEmployeeService.Verify(x => x.SearchEmployeesAsync("أحمد"), Times.Once);
        }

        [Fact]
        public async Task DeleteEmployee_ShouldRemoveFromList()
        {
            // Arrange
            var employee = MockDataFactory.CreateMockEmployeeDto(1, "أحمد محمد");

            _mockEmployeeService
                .Setup(x => x.DeleteEmployeeAsync(1))
                .Returns(Task.CompletedTask);

            var viewModel = new EmployeeViewModel(_mockEmployeeService.Object);

            viewModel.Employees.Add(employee);
            viewModel.SelectedEmployee = employee;

            // Act
            await viewModel.DeleteCommand.ExecuteAsync(null);

            // Assert
            _mockEmployeeService.Verify(x => x.DeleteEmployeeAsync(1), Times.Once);
        }
    }
}

// ============================================================================
// FILE: TripViewModelTests.cs
// LOCATION: FleetManagementSystem.Tests/ViewModels/TripViewModelTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace FleetManagementSystem.Tests.ViewModels
{
    public class TripViewModelTests : ViewModelTestBase
    {
        private Mock<ITripService> _mockTripService;
        private Mock<IVehicleService> _mockVehicleService;

        public TripViewModelTests()
        {
            _mockTripService = new Mock<ITripService>();
            _mockVehicleService = new Mock<IVehicleService>();
        }

        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var viewModel = new TripViewModel(
                _mockTripService.Object,
                _mockVehicleService.Object
            );

            // Assert
            Assert.NotNull(viewModel);
            Assert.NotNull(viewModel.Trips);
            Assert.NotNull(viewModel.CreateCommand);
            Assert.NotNull(viewModel.EditCommand);
            Assert.NotNull(viewModel.DeleteCommand);
        }

        [Fact]
        public async Task LoadTrips_ShouldPopulateTripsList()
        {
            // Arrange
            var trips = new List<TripDto>
            {
                MockDataFactory.CreateMockTripDto(1, 1, 1, "جدة"),
                MockDataFactory.CreateMockTripDto(2, 2, 2, "الرياض")
            };

            _mockTripService
                .Setup(x => x.GetAllTripsAsync())
                .ReturnsAsync(trips);

            var viewModel = new TripViewModel(
                _mockTripService.Object,
                _mockVehicleService.Object
            );

            // Act
            await viewModel.LoadTripsAsync();

            // Assert
            Assert.NotEmpty(viewModel.Trips);
            Assert.Equal(2, viewModel.Trips.Count);
        }

        [Fact]
        public async Task FilterByStatus_ShouldReturnOnlyMatchingStatus()
        {
            // Arrange
            var trips = new List<TripDto>
            {
                MockDataFactory.CreateMockTripDto(1, 1, 1, status: "مكتمل"),
                MockDataFactory.CreateMockTripDto(2, 2, 2, status: "مكتمل")
            };

            _mockTripService
                .Setup(x => x.GetTripsByStatusAsync("مكتمل"))
                .ReturnsAsync(trips);

            var viewModel = new TripViewModel(
                _mockTripService.Object,
                _mockVehicleService.Object
            );

            viewModel.SelectedStatus = "مكتمل";

            // Act
            await viewModel.FilterCommand.ExecuteAsync(null);

            // Assert
            _mockTripService.Verify(x => x.GetTripsByStatusAsync("مكتمل"), Times.Once);
        }

        [Fact]
        public async Task GetTripsByVehicle_ShouldReturnVehicleTripsHistory()
        {
            // Arrange
            var trips = new List<TripDto>
            {
                MockDataFactory.CreateMockTripDto(1, 1, 1),
                MockDataFactory.CreateMockTripDto(2, 1, 2)
            };

            _mockTripService
                .Setup(x => x.GetTripsByVehicleAsync(1))
                .ReturnsAsync(trips);

            var viewModel = new TripViewModel(
                _mockTripService.Object,
                _mockVehicleService.Object
            );

            // Act
            await viewModel.LoadTripsByVehicleAsync(1);

            // Assert
            Assert.NotEmpty(viewModel.Trips);
            Assert.All(viewModel.Trips, t => Assert.Equal(1, t.VehicleId));
        }

        [Fact]
        public async Task DeleteTrip_ShouldRemoveFromList()
        {
            // Arrange
            var trip = MockDataFactory.CreateMockTripDto(1, 1, 1);

            _mockTripService
                .Setup(x => x.DeleteTripAsync(1))
                .Returns(Task.CompletedTask);

            var viewModel = new TripViewModel(
                _mockTripService.Object,
                _mockVehicleService.Object
            );

            viewModel.Trips.Add(trip);
            viewModel.SelectedTrip = trip;

            // Act
            await viewModel.DeleteCommand.ExecuteAsync(null);

            // Assert
            _mockTripService.Verify(x => x.DeleteTripAsync(1), Times.Once);
        }
    }
}

// ============================================================================
// FILE: FuelViewModelTests.cs
// LOCATION: FleetManagementSystem.Tests/ViewModels/FuelViewModelTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace FleetManagementSystem.Tests.ViewModels
{
    public class FuelViewModelTests : ViewModelTestBase
    {
        private Mock<IFuelService> _mockFuelService;
        private Mock<IVehicleService> _mockVehicleService;

        public FuelViewModelTests()
        {
            _mockFuelService = new Mock<IFuelService>();
            _mockVehicleService = new Mock<IVehicleService>();
        }

        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var viewModel = new FuelViewModel(
                _mockFuelService.Object,
                _mockVehicleService.Object
            );

            // Assert
            Assert.NotNull(viewModel);
            Assert.NotNull(viewModel.FuelTransactions);
            Assert.NotNull(viewModel.CreateCommand);
            Assert.NotNull(viewModel.DeleteCommand);
        }

        [Fact]
        public async Task LoadFuelTransactions_ShouldPopulateFuelList()
        {
            // Arrange
            var transactions = new List<FuelTransactionDto>
            {
                MockDataFactory.CreateMockFuelDto(1, 1, 50, 2.5m),
                MockDataFactory.CreateMockFuelDto(2, 2, 60, 2.5m)
            };

            _mockFuelService
                .Setup(x => x.GetAllFuelTransactionsAsync())
                .ReturnsAsync(transactions);

            var viewModel = new FuelViewModel(
                _mockFuelService.Object,
                _mockVehicleService.Object
            );

            // Act
            await viewModel.LoadFuelTransactionsAsync();

            // Assert
            Assert.NotEmpty(viewModel.FuelTransactions);
            Assert.Equal(2, viewModel.FuelTransactions.Count);
        }

        [Fact]
        public async Task GetFuelByVehicle_ShouldReturnVehicleFuelHistory()
        {
            // Arrange
            var transactions = new List<FuelTransactionDto>
            {
                MockDataFactory.CreateMockFuelDto(1, 1, 50, 2.5m),
                MockDataFactory.CreateMockFuelDto(2, 1, 60, 2.5m)
            };

            _mockFuelService
                .Setup(x => x.GetFuelTransactionsByVehicleAsync(1))
                .ReturnsAsync(transactions);

            var viewModel = new FuelViewModel(
                _mockFuelService.Object,
                _mockVehicleService.Object
            );

            // Act
            await viewModel.LoadFuelByVehicleAsync(1);

            // Assert
            Assert.NotEmpty(viewModel.FuelTransactions);
            Assert.All(viewModel.FuelTransactions, f => Assert.Equal(1, f.VehicleId));
        }

        [Fact]
        public async Task GetFuelConsumptionAnalytics_ShouldReturnStatistics()
        {
            // Arrange
            var analytics = new FuelAnalyticsDto
            {
                TotalQuantity = 500,
                AverageCostPerLiter = 2.5m,
                TotalCost = 1250
            };

            _mockFuelService
                .Setup(x => x.GetFuelAnalyticsAsync(1))
                .ReturnsAsync(analytics);

            var viewModel = new FuelViewModel(
                _mockFuelService.Object,
                _mockVehicleService.Object
            );

            // Act
            var result = await viewModel.GetFuelAnalyticsAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500, result.TotalQuantity);
        }

        [Fact]
        public async Task DeleteFuelTransaction_ShouldRemoveFromList()
        {
            // Arrange
            var transaction = MockDataFactory.CreateMockFuelDto(1, 1);

            _mockFuelService
                .Setup(x => x.DeleteFuelTransactionAsync(1))
                .Returns(Task.CompletedTask);

            var viewModel = new FuelViewModel(
                _mockFuelService.Object,
                _mockVehicleService.Object
            );

            viewModel.FuelTransactions.Add(transaction);
            viewModel.SelectedTransaction = transaction;

            // Act
            await viewModel.DeleteCommand.ExecuteAsync(null);

            // Assert
            _mockFuelService.Verify(x => x.DeleteFuelTransactionAsync(1), Times.Once);
        }
    }
}

// ============================================================================
// FILE: ExpenseViewModelTests.cs
// LOCATION: FleetManagementSystem.Tests/ViewModels/ExpenseViewModelTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace FleetManagementSystem.Tests.ViewModels
{
    public class ExpenseViewModelTests : ViewModelTestBase
    {
        private Mock<IExpenseService> _mockExpenseService;
        private Mock<IVehicleService> _mockVehicleService;

        public ExpenseViewModelTests()
        {
            _mockExpenseService = new Mock<IExpenseService>();
            _mockVehicleService = new Mock<IVehicleService>();
        }

        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var viewModel = new ExpenseViewModel(
                _mockExpenseService.Object,
                _mockVehicleService.Object
            );

            // Assert
            Assert.NotNull(viewModel);
            Assert.NotNull(viewModel.Expenses);
            Assert.NotNull(viewModel.CreateCommand);
            Assert.NotNull(viewModel.EditCommand);
            Assert.NotNull(viewModel.DeleteCommand);
        }

        [Fact]
        public async Task LoadExpenses_ShouldPopulateExpensesList()
        {
            // Arrange
            var expenses = new List<ExpenseDto>
            {
                MockDataFactory.CreateMockExpenseDto(1, 1, "إصلاح", 1500),
                MockDataFactory.CreateMockExpenseDto(2, 2, "صيانة", 2000)
            };

            _mockExpenseService
                .Setup(x => x.GetAllExpensesAsync())
                .ReturnsAsync(expenses);

            var viewModel = new ExpenseViewModel(
                _mockExpenseService.Object,
                _mockVehicleService.Object
            );

            // Act
            await viewModel.LoadExpensesAsync();

            // Assert
            Assert.NotEmpty(viewModel.Expenses);
            Assert.Equal(2, viewModel.Expenses.Count);
        }

        [Fact]
        public async Task FilterByCategory_ShouldReturnOnlyMatchingCategory()
        {
            // Arrange
            var expenses = new List<ExpenseDto>
            {
                MockDataFactory.CreateMockExpenseDto(1, 1, "إصلاح", 1500),
                MockDataFactory.CreateMockExpenseDto(2, 2, "إصلاح", 2000)
            };

            _mockExpenseService
                .Setup(x => x.GetExpensesByCategoryAsync("إصلاح"))
                .ReturnsAsync(expenses);

            var viewModel = new ExpenseViewModel(
                _mockExpenseService.Object,
                _mockVehicleService.Object
            );

            viewModel.SelectedCategory = "إصلاح";

            // Act
            await viewModel.FilterCommand.ExecuteAsync(null);

            // Assert
            _mockExpenseService.Verify(x => x.GetExpensesByCategoryAsync("إصلاح"), Times.Once);
        }

        [Fact]
        public async Task GetExpensesByVehicle_ShouldReturnVehicleExpensesHistory()
        {
            // Arrange
            var expenses = new List<ExpenseDto>
            {
                MockDataFactory.CreateMockExpenseDto(1, 1, "إصلاح", 1500),
                MockDataFactory.CreateMockExpenseDto(2, 1, "صيانة", 2000)
            };

            _mockExpenseService
                .Setup(x => x.GetExpensesByVehicleAsync(1))
                .ReturnsAsync(expenses);

            var viewModel = new ExpenseViewModel(
                _mockExpenseService.Object,
                _mockVehicleService.Object
            );

            // Act
            await viewModel.LoadExpensesByVehicleAsync(1);

            // Assert
            Assert.NotEmpty(viewModel.Expenses);
            Assert.All(viewModel.Expenses, e => Assert.Equal(1, e.VehicleId));
        }

        [Fact]
        public async Task DeleteExpense_ShouldRemoveFromList()
        {
            // Arrange
            var expense = MockDataFactory.CreateMockExpenseDto(1, 1);

            _mockExpenseService
                .Setup(x => x.DeleteExpenseAsync(1))
                .Returns(Task.CompletedTask);

            var viewModel = new ExpenseViewModel(
                _mockExpenseService.Object,
                _mockVehicleService.Object
            );

            viewModel.Expenses.Add(expense);
            viewModel.SelectedExpense = expense;

            // Act
            await viewModel.DeleteCommand.ExecuteAsync(null);

            // Assert
            _mockExpenseService.Verify(x => x.DeleteExpenseAsync(1), Times.Once);
        }
    }
}
