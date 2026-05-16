// ============================================================================
// FILE: VehicleViewModelTests.cs
// LOCATION: FleetManagementSystem.Tests/ViewModels/VehicleViewModelTests.cs
// ============================================================================

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace FleetManagementSystem.Tests.ViewModels
{
    public class VehicleViewModelTests : ViewModelTestBase
    {
        private Mock<IVehicleService> _mockVehicleService;
        private Mock<IMasterDataService> _mockMasterDataService;

        public VehicleViewModelTests()
        {
            _mockVehicleService = new Mock<IVehicleService>();
            _mockMasterDataService = new Mock<IMasterDataService>();
        }

        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var viewModel = new VehicleViewModel(
                _mockVehicleService.Object,
                _mockMasterDataService.Object
            );

            // Assert
            Assert.NotNull(viewModel);
            Assert.NotNull(viewModel.Vehicles);
            Assert.NotNull(viewModel.CreateCommand);
            Assert.NotNull(viewModel.EditCommand);
            Assert.NotNull(viewModel.DeleteCommand);
            Assert.NotNull(viewModel.SearchCommand);
            Assert.NotNull(viewModel.RefreshCommand);
        }

        [Fact]
        public async Task LoadVehicles_ShouldPopulateVehiclesList()
        {
            // Arrange
            var vehicles = new List<VehicleDto>
            {
                MockDataFactory.CreateMockVehicleDto(1, "ABC-001"),
                MockDataFactory.CreateMockVehicleDto(2, "ABC-002"),
                MockDataFactory.CreateMockVehicleDto(3, "ABC-003")
            };

            _mockVehicleService
                .Setup(x => x.GetAllVehiclesAsync())
                .ReturnsAsync(vehicles);

            var viewModel = new VehicleViewModel(
                _mockVehicleService.Object,
                _mockMasterDataService.Object
            );

            // Act
            await viewModel.LoadVehiclesAsync();

            // Assert
            Assert.NotEmpty(viewModel.Vehicles);
            Assert.Equal(3, viewModel.Vehicles.Count);
        }

        [Fact]
        public async Task SearchVehicles_ShouldFilterByPlateNumber()
        {
            // Arrange
            var vehicles = new List<VehicleDto>
            {
                MockDataFactory.CreateMockVehicleDto(1, "ABC-001"),
                MockDataFactory.CreateMockVehicleDto(2, "ABC-002")
            };

            _mockVehicleService
                .Setup(x => x.SearchVehiclesAsync("ABC"))
                .ReturnsAsync(vehicles);

            var viewModel = new VehicleViewModel(
                _mockVehicleService.Object,
                _mockMasterDataService.Object
            );

            viewModel.SearchText = "ABC";

            // Act
            await viewModel.SearchCommand.ExecuteAsync(null);

            // Assert
            _mockVehicleService.Verify(x => x.SearchVehiclesAsync("ABC"), Times.Once);
        }

        [Fact]
        public async Task FilterByStatus_ShouldReturnOnlyMatchingStatus()
        {
            // Arrange
            var vehicles = new List<VehicleDto>
            {
                MockDataFactory.CreateMockVehicleDto(1, "ABC-001", status: "متاح"),
                MockDataFactory.CreateMockVehicleDto(2, "ABC-002", status: "متاح")
            };

            _mockVehicleService
                .Setup(x => x.FilterVehiclesByStatusAsync("متاح"))
                .ReturnsAsync(vehicles);

            var viewModel = new VehicleViewModel(
                _mockVehicleService.Object,
                _mockMasterDataService.Object
            );

            viewModel.SelectedStatus = "متاح";

            // Act
            await viewModel.FilterCommand.ExecuteAsync(null);

            // Assert
            _mockVehicleService.Verify(x => x.FilterVehiclesByStatusAsync("متاح"), Times.Once);
        }

        [Fact]
        public async Task DeleteVehicle_ShouldRemoveFromList()
        {
            // Arrange
            var vehicle = MockDataFactory.CreateMockVehicleDto(1, "ABC-001");

            _mockVehicleService
                .Setup(x => x.DeleteVehicleAsync(1))
                .Returns(Task.CompletedTask);

            var viewModel = new VehicleViewModel(
                _mockVehicleService.Object,
                _mockMasterDataService.Object
            );

            viewModel.Vehicles.Add(vehicle);
            viewModel.SelectedVehicle = vehicle;

            // Act
            await viewModel.DeleteCommand.ExecuteAsync(null);

            // Assert
            _mockVehicleService.Verify(x => x.DeleteVehicleAsync(1), Times.Once);
        }

        [Fact]
        public void SearchText_PropertyChanged_ShouldRaiseNotification()
        {
            // Arrange
            var viewModel = new VehicleViewModel(
                _mockVehicleService.Object,
                _mockMasterDataService.Object
            );

            bool notificationRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(VehicleViewModel.SearchText))
                    notificationRaised = true;
            };

            // Act
            viewModel.SearchText = "ABC-001";

            // Assert
            Assert.True(notificationRaised);
        }

        [Fact]
        public async Task RefreshCommand_ShouldReloadVehicles()
        {
            // Arrange
            _mockVehicleService
                .Setup(x => x.GetAllVehiclesAsync())
                .ReturnsAsync(new List<VehicleDto>());

            var viewModel = new VehicleViewModel(
                _mockVehicleService.Object,
                _mockMasterDataService.Object
            );

            // Act
            await viewModel.RefreshCommand.ExecuteAsync(null);

            // Assert
            _mockVehicleService.Verify(x => x.GetAllVehiclesAsync(), Times.Once);
        }
    }
}

// ============================================================================
// FILE: VehicleFormViewModelTests.cs
// LOCATION: FleetManagementSystem.Tests/ViewModels/VehicleFormViewModelTests.cs
// ============================================================================

using System;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace FleetManagementSystem.Tests.ViewModels
{
    public class VehicleFormViewModelTests : ViewModelTestBase
    {
        private Mock<IVehicleService> _mockVehicleService;
        private Mock<IMasterDataService> _mockMasterDataService;

        public VehicleFormViewModelTests()
        {
            _mockVehicleService = new Mock<IVehicleService>();
            _mockMasterDataService = new Mock<IMasterDataService>();
        }

        [Fact]
        public void Constructor_ShouldInitializeForCreate()
        {
            // Arrange & Act
            var viewModel = new VehicleFormViewModel(
                _mockVehicleService.Object,
                _mockMasterDataService.Object,
                isEditMode: false
            );

            // Assert
            Assert.NotNull(viewModel);
            Assert.Null(viewModel.Vehicle.Id);
            Assert.NotNull(viewModel.SaveCommand);
            Assert.NotNull(viewModel.CancelCommand);
        }

        [Fact]
        public void Constructor_ShouldInitializeForEdit()
        {
            // Arrange
            var vehicle = MockDataFactory.CreateMockVehicleDto(1, "ABC-001");

            // Act
            var viewModel = new VehicleFormViewModel(
                _mockVehicleService.Object,
                _mockMasterDataService.Object,
                isEditMode: true,
                vehicle: vehicle
            );

            // Assert
            Assert.Equal(1, viewModel.Vehicle.Id);
            Assert.Equal("ABC-001", viewModel.Vehicle.PlateNumber);
        }

        [Fact]
        public async Task SaveCommand_WithValidData_ShouldCreateVehicle()
        {
            // Arrange
            var newVehicle = new VehicleDto
            {
                PlateNumber = "NEW-001",
                Model = "Toyota Corolla",
                Year = 2024,
                Status = "متاح"
            };

            _mockVehicleService
                .Setup(x => x.CreateVehicleAsync(It.IsAny<VehicleDto>()))
                .ReturnsAsync(newVehicle);

            var viewModel = new VehicleFormViewModel(
                _mockVehicleService.Object,
                _mockMasterDataService.Object,
                isEditMode: false
            );

            viewModel.Vehicle = newVehicle;

            // Act
            await viewModel.SaveCommand.ExecuteAsync(null);

            // Assert
            _mockVehicleService.Verify(x => x.CreateVehicleAsync(It.IsAny<VehicleDto>()), Times.Once);
        }

        [Fact]
        public async Task SaveCommand_WithInvalidPlateNumber_ShouldShowError()
        {
            // Arrange
            var viewModel = new VehicleFormViewModel(
                _mockVehicleService.Object,
                _mockMasterDataService.Object,
                isEditMode: false
            );

            viewModel.Vehicle.PlateNumber = ""; // Invalid

            // Act
            await viewModel.SaveCommand.ExecuteAsync(null);

            // Assert
            Assert.NotEmpty(viewModel.ErrorMessage);
        }

        [Fact]
        public async Task SaveCommand_WithValidData_ShouldUpdateVehicle()
        {
            // Arrange
            var vehicle = MockDataFactory.CreateMockVehicleDto(1, "ABC-001");
            vehicle.Status = "تحت الصيانة";

            _mockVehicleService
                .Setup(x => x.UpdateVehicleAsync(It.IsAny<VehicleDto>()))
                .ReturnsAsync(vehicle);

            var viewModel = new VehicleFormViewModel(
                _mockVehicleService.Object,
                _mockMasterDataService.Object,
                isEditMode: true,
                vehicle: vehicle
            );

            // Act
            await viewModel.SaveCommand.ExecuteAsync(null);

            // Assert
            _mockVehicleService.Verify(x => x.UpdateVehicleAsync(It.IsAny<VehicleDto>()), Times.Once);
        }

        [Fact]
        public void PlateNumber_PropertyChanged_ShouldRaiseNotification()
        {
            // Arrange
            var viewModel = new VehicleFormViewModel(
                _mockVehicleService.Object,
                _mockMasterDataService.Object,
                isEditMode: false
            );

            bool notificationRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(VehicleFormViewModel.Vehicle))
                    notificationRaised = true;
            };

            // Act
            viewModel.Vehicle.PlateNumber = "NEW-001";

            // Assert
            Assert.True(notificationRaised);
        }
    }
}

// ============================================================================
// FILE: ContractViewModelTests.cs
// LOCATION: FleetManagementSystem.Tests/ViewModels/ContractViewModelTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace FleetManagementSystem.Tests.ViewModels
{
    public class ContractViewModelTests : ViewModelTestBase
    {
        private Mock<IContractService> _mockContractService;
        private Mock<IVehicleService> _mockVehicleService;

        public ContractViewModelTests()
        {
            _mockContractService = new Mock<IContractService>();
            _mockVehicleService = new Mock<IVehicleService>();
        }

        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var viewModel = new ContractViewModel(
                _mockContractService.Object,
                _mockVehicleService.Object
            );

            // Assert
            Assert.NotNull(viewModel);
            Assert.NotNull(viewModel.Contracts);
            Assert.NotNull(viewModel.CreateCommand);
            Assert.NotNull(viewModel.EditCommand);
            Assert.NotNull(viewModel.DeleteCommand);
        }

        [Fact]
        public async Task LoadContracts_ShouldPopulateContractsList()
        {
            // Arrange
            var contracts = new List<ContractDto>
            {
                MockDataFactory.CreateMockContractDto(1, 1, "CNT-001"),
                MockDataFactory.CreateMockContractDto(2, 2, "CNT-002")
            };

            _mockContractService
                .Setup(x => x.GetAllContractsAsync())
                .ReturnsAsync(contracts);

            var viewModel = new ContractViewModel(
                _mockContractService.Object,
                _mockVehicleService.Object
            );

            // Act
            await viewModel.LoadContractsAsync();

            // Assert
            Assert.NotEmpty(viewModel.Contracts);
            Assert.Equal(2, viewModel.Contracts.Count);
        }

        [Fact]
        public async Task FilterByStatus_ShouldReturnOnlyMatchingStatus()
        {
            // Arrange
            var contracts = new List<ContractDto>
            {
                MockDataFactory.CreateMockContractDto(1, 1, statusId: 2), // Active
                MockDataFactory.CreateMockContractDto(2, 2, statusId: 2)
            };

            _mockContractService
                .Setup(x => x.GetContractsByStatusAsync(2))
                .ReturnsAsync(contracts);

            var viewModel = new ContractViewModel(
                _mockContractService.Object,
                _mockVehicleService.Object
            );

            viewModel.SelectedStatus = 2;

            // Act
            await viewModel.FilterCommand.ExecuteAsync(null);

            // Assert
            _mockContractService.Verify(x => x.GetContractsByStatusAsync(2), Times.Once);
        }

        [Fact]
        public async Task GetExpiringContracts_ShouldReturnContractsExpiringWithinDays()
        {
            // Arrange
            var expiringContracts = new List<ContractDto>
            {
                MockDataFactory.CreateMockContractDto(1, 1, endDate: DateTime.UtcNow.AddDays(5))
            };

            _mockContractService
                .Setup(x => x.GetExpiringContractsAsync(30))
                .ReturnsAsync(expiringContracts);

            var viewModel = new ContractViewModel(
                _mockContractService.Object,
                _mockVehicleService.Object
            );

            // Act
            await viewModel.LoadExpiringContractsAsync();

            // Assert
            Assert.NotEmpty(viewModel.Contracts);
        }

        [Fact]
        public async Task DeleteContract_ShouldRemoveFromList()
        {
            // Arrange
            var contract = MockDataFactory.CreateMockContractDto(1, 1);

            _mockContractService
                .Setup(x => x.DeleteContractAsync(1))
                .Returns(Task.CompletedTask);

            var viewModel = new ContractViewModel(
                _mockContractService.Object,
                _mockVehicleService.Object
            );

            viewModel.Contracts.Add(contract);
            viewModel.SelectedContract = contract;

            // Act
            await viewModel.DeleteCommand.ExecuteAsync(null);

            // Assert
            _mockContractService.Verify(x => x.DeleteContractAsync(1), Times.Once);
        }
    }
}

// ============================================================================
// FILE: MaintenanceViewModelTests.cs
// LOCATION: FleetManagementSystem.Tests/ViewModels/MaintenanceViewModelTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace FleetManagementSystem.Tests.ViewModels
{
    public class MaintenanceViewModelTests : ViewModelTestBase
    {
        private Mock<IMaintenanceService> _mockMaintenanceService;
        private Mock<IVehicleService> _mockVehicleService;

        public MaintenanceViewModelTests()
        {
            _mockMaintenanceService = new Mock<IMaintenanceService>();
            _mockVehicleService = new Mock<IVehicleService>();
        }

        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var viewModel = new MaintenanceViewModel(
                _mockMaintenanceService.Object,
                _mockVehicleService.Object
            );

            // Assert
            Assert.NotNull(viewModel);
            Assert.NotNull(viewModel.MaintenanceRequests);
            Assert.NotNull(viewModel.CreateCommand);
            Assert.NotNull(viewModel.CompleteCommand);
            Assert.NotNull(viewModel.DeleteCommand);
        }

        [Fact]
        public async Task LoadOpenRequests_ShouldPopulateOpenMaintenanceList()
        {
            // Arrange
            var openRequests = new List<MaintenanceRequestDto>
            {
                MockDataFactory.CreateMockMaintenanceDto(1, 1, "مفتوح"),
                MockDataFactory.CreateMockMaintenanceDto(2, 2, "مفتوح")
            };

            _mockMaintenanceService
                .Setup(x => x.GetOpenMaintenanceRequestsAsync())
                .ReturnsAsync(openRequests);

            var viewModel = new MaintenanceViewModel(
                _mockMaintenanceService.Object,
                _mockVehicleService.Object
            );

            // Act
            await viewModel.LoadOpenRequestsAsync();

            // Assert
            Assert.NotEmpty(viewModel.MaintenanceRequests);
            Assert.Equal(2, viewModel.MaintenanceRequests.Count);
        }

        [Fact]
        public async Task CompleteCommand_ShouldMarkMaintenanceAsComplete()
        {
            // Arrange
            var maintenance = MockDataFactory.CreateMockMaintenanceDto(1, 1, "مفتوح");
            maintenance.Status = "مكتمل";

            _mockMaintenanceService
                .Setup(x => x.MarkMaintenanceAsCompleteAsync(1))
                .ReturnsAsync(maintenance);

            var viewModel = new MaintenanceViewModel(
                _mockMaintenanceService.Object,
                _mockVehicleService.Object
            );

            viewModel.SelectedMaintenance = MockDataFactory.CreateMockMaintenanceDto(1, 1);

            // Act
            await viewModel.CompleteCommand.ExecuteAsync(null);

            // Assert
            _mockMaintenanceService.Verify(x => x.MarkMaintenanceAsCompleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetMaintenanceByVehicle_ShouldReturnVehicleMaintenanceHistory()
        {
            // Arrange
            var maintenanceHistory = new List<MaintenanceRequestDto>
            {
                MockDataFactory.CreateMockMaintenanceDto(1, 1),
                MockDataFactory.CreateMockMaintenanceDto(2, 1)
            };

            _mockMaintenanceService
                .Setup(x => x.GetMaintenanceByVehicleAsync(1))
                .ReturnsAsync(maintenanceHistory);

            var viewModel = new MaintenanceViewModel(
                _mockMaintenanceService.Object,
                _mockVehicleService.Object
            );

            // Act
            await viewModel.LoadMaintenanceByVehicleAsync(1);

            // Assert
            Assert.NotEmpty(viewModel.MaintenanceRequests);
            Assert.All(viewModel.MaintenanceRequests, m => Assert.Equal(1, m.VehicleId));
        }

        [Fact]
        public async Task DeleteCommand_ShouldRemoveMaintenanceRequest()
        {
            // Arrange
            var maintenance = MockDataFactory.CreateMockMaintenanceDto(1, 1);

            _mockMaintenanceService
                .Setup(x => x.DeleteMaintenanceRequestAsync(1))
                .Returns(Task.CompletedTask);

            var viewModel = new MaintenanceViewModel(
                _mockMaintenanceService.Object,
                _mockVehicleService.Object
            );

            viewModel.MaintenanceRequests.Add(maintenance);
            viewModel.SelectedMaintenance = maintenance;

            // Act
            await viewModel.DeleteCommand.ExecuteAsync(null);

            // Assert
            _mockMaintenanceService.Verify(x => x.DeleteMaintenanceRequestAsync(1), Times.Once);
        }
    }
}
