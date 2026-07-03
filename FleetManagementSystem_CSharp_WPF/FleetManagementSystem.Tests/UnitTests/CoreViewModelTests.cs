// ============================================================================
// FILE: LoginViewModelTests.cs
// LOCATION: FleetManagementSystem.Tests/ViewModels/LoginViewModelTests.cs
// ============================================================================

using System;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace FleetManagementSystem.Tests.ViewModels
{
    public class LoginViewModelTests : ViewModelTestBase
    {
        private Mock<IAuthenticationService> _mockAuthService;
        private Mock<INavigationService> _mockNavigationService;

        public LoginViewModelTests()
        {
            _mockAuthService = new Mock<IAuthenticationService>();
            _mockNavigationService = new Mock<INavigationService>();
        }

        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var viewModel = new LoginViewModel(_mockAuthService.Object, _mockNavigationService.Object);

            // Assert
            Assert.NotNull(viewModel);
            Assert.Empty(viewModel.Username);
            Assert.Empty(viewModel.ErrorMessage);
            Assert.False(viewModel.IsLoading);
            Assert.NotNull(viewModel.LoginCommand);
        }

        [Fact]
        public void LoginCommand_ShouldBeInitialized()
        {
            // Arrange
            var viewModel = new LoginViewModel(_mockAuthService.Object, _mockNavigationService.Object);

            // Act & Assert
            Assert.NotNull(viewModel.LoginCommand);
            Assert.True(viewModel.LoginCommand.CanExecute(null));
        }

        [Fact]
        public async Task LoginCommand_WithValidCredentials_ShouldSucceed()
        {
            // Arrange
            var viewModel = new LoginViewModel(_mockAuthService.Object, _mockNavigationService.Object);
            viewModel.Username = "admin";
            viewModel.Password = "Admin@123";

            _mockAuthService
                .Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new UserDto { Id = 1, Name = "Admin", Role = "Admin" });

            // Act
            await viewModel.LoginCommand.ExecuteAsync(null);

            // Assert
            _mockAuthService.Verify(x => x.LoginAsync("admin", "Admin@123"), Times.Once);
            _mockNavigationService.Verify(x => x.NavigateTo("MainWindow"), Times.Once);
        }

        [Fact]
        public async Task LoginCommand_WithInvalidCredentials_ShouldShowError()
        {
            // Arrange
            var viewModel = new LoginViewModel(_mockAuthService.Object, _mockNavigationService.Object);
            viewModel.Username = "admin";
            viewModel.Password = "WrongPassword";

            _mockAuthService
                .Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new UnauthorizedAccessException("بيانات الدخول غير صحيحة"));

            // Act
            await viewModel.LoginCommand.ExecuteAsync(null);

            // Assert
            Assert.NotEmpty(viewModel.ErrorMessage);
            Assert.Contains("بيانات الدخول", viewModel.ErrorMessage);
            _mockNavigationService.Verify(x => x.NavigateTo(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task LoginCommand_ShouldSetLoadingState()
        {
            // Arrange
            var viewModel = new LoginViewModel(_mockAuthService.Object, _mockNavigationService.Object);
            viewModel.Username = "admin";
            viewModel.Password = "Admin@123";

            _mockAuthService
                .Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new UserDto { Id = 1, Name = "Admin" });

            // Act
            var task = viewModel.LoginCommand.ExecuteAsync(null);
            Assert.True(viewModel.IsLoading);

            await task;

            // Assert
            Assert.False(viewModel.IsLoading);
        }

        [Fact]
        public void Username_PropertyChanged_ShouldRaiseNotification()
        {
            // Arrange
            var viewModel = new LoginViewModel(_mockAuthService.Object, _mockNavigationService.Object);
            bool notificationRaised = false;

            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(LoginViewModel.Username))
                    notificationRaised = true;
            };

            // Act
            viewModel.Username = "newuser";

            // Assert
            Assert.True(notificationRaised);
        }

        [Fact]
        public void Password_PropertyChanged_ShouldRaiseNotification()
        {
            // Arrange
            var viewModel = new LoginViewModel(_mockAuthService.Object, _mockNavigationService.Object);
            bool notificationRaised = false;

            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(LoginViewModel.Password))
                    notificationRaised = true;
            };

            // Act
            viewModel.Password = "newpassword";

            // Assert
            Assert.True(notificationRaised);
        }
    }
}

// ============================================================================
// FILE: MainWindowViewModelTests.cs
// LOCATION: FleetManagementSystem.Tests/ViewModels/MainWindowViewModelTests.cs
// ============================================================================

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace FleetManagementSystem.Tests.ViewModels
{
    public class MainWindowViewModelTests : ViewModelTestBase
    {
        private Mock<INavigationService> _mockNavigationService;
        private Mock<ISettingsService> _mockSettingsService;

        public MainWindowViewModelTests()
        {
            _mockNavigationService = new Mock<INavigationService>();
            _mockSettingsService = new Mock<ISettingsService>();
        }

        [Fact]
        public void Constructor_ShouldInitializeDashboardItems()
        {
            // Arrange & Act
            var viewModel = new MainWindowViewModel(
                _mockNavigationService.Object,
                _mockSettingsService.Object
            );

            // Assert
            Assert.NotNull(viewModel.DashboardItems);
            Assert.True(viewModel.DashboardItems.Count >= 7); // 7 main modules
        }

        [Fact]
        public void Constructor_ShouldInitializeNavigationCommands()
        {
            // Arrange & Act
            var viewModel = new MainWindowViewModel(
                _mockNavigationService.Object,
                _mockSettingsService.Object
            );

            // Assert
            Assert.NotNull(viewModel.NavigateToDashboardCommand);
            Assert.NotNull(viewModel.NavigateToVehiclesCommand);
            Assert.NotNull(viewModel.NavigateToContractsCommand);
            Assert.NotNull(viewModel.NavigateToMaintenanceCommand);
            Assert.NotNull(viewModel.NavigateToReportsCommand);
            Assert.NotNull(viewModel.NavigateToSettingsCommand);
        }

        [Fact]
        public async Task NavigateToDashboardCommand_ShouldNavigateToDashboard()
        {
            // Arrange
            var viewModel = new MainWindowViewModel(
                _mockNavigationService.Object,
                _mockSettingsService.Object
            );

            // Act
            await viewModel.NavigateToDashboardCommand.ExecuteAsync(null);

            // Assert
            _mockNavigationService.Verify(x => x.NavigateTo("Dashboard"), Times.Once);
        }

        [Fact]
        public async Task NavigateToVehiclesCommand_ShouldNavigateToVehicles()
        {
            // Arrange
            var viewModel = new MainWindowViewModel(
                _mockNavigationService.Object,
                _mockSettingsService.Object
            );

            // Act
            await viewModel.NavigateToVehiclesCommand.ExecuteAsync(null);

            // Assert
            _mockNavigationService.Verify(x => x.NavigateTo("Vehicles"), Times.Once);
        }

        [Fact]
        public async Task LogoutCommand_ShouldClearSessionAndNavigateToLogin()
        {
            // Arrange
            var viewModel = new MainWindowViewModel(
                _mockNavigationService.Object,
                _mockSettingsService.Object
            );

            // Act
            await viewModel.LogoutCommand.ExecuteAsync(null);

            // Assert
            _mockNavigationService.Verify(x => x.NavigateTo("Login"), Times.Once);
        }

        [Fact]
        public void DashboardItems_ShouldHaveCorrectStructure()
        {
            // Arrange & Act
            var viewModel = new MainWindowViewModel(
                _mockNavigationService.Object,
                _mockSettingsService.Object
            );

            // Assert
            var vehicleItem = viewModel.DashboardItems.FirstOrDefault(x => x.Title == "المركبات");
            Assert.NotNull(vehicleItem);
            Assert.NotEmpty(vehicleItem.Description);
            Assert.NotNull(vehicleItem.Icon);
        }

        [Fact]
        public void CurrentViewName_PropertyChanged_ShouldRaiseNotification()
        {
            // Arrange
            var viewModel = new MainWindowViewModel(
                _mockNavigationService.Object,
                _mockSettingsService.Object
            );

            bool notificationRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainWindowViewModel.CurrentViewName))
                    notificationRaised = true;
            };

            // Act
            viewModel.CurrentViewName = "Vehicles";

            // Assert
            Assert.True(notificationRaised);
        }
    }
}

// ============================================================================
// FILE: DashboardViewModelTests.cs
// LOCATION: FleetManagementSystem.Tests/ViewModels/DashboardViewModelTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace FleetManagementSystem.Tests.ViewModels
{
    public class DashboardViewModelTests : ViewModelTestBase
    {
        private Mock<IReportingService> _mockReportingService;
        private Mock<INotificationService> _mockNotificationService;

        public DashboardViewModelTests()
        {
            _mockReportingService = new Mock<IReportingService>();
            _mockNotificationService = new Mock<INotificationService>();
        }

        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var viewModel = new DashboardViewModel(
                _mockReportingService.Object,
                _mockNotificationService.Object
            );

            // Assert
            Assert.NotNull(viewModel);
            Assert.NotNull(viewModel.RefreshCommand);
            Assert.NotNull(viewModel.Alerts);
            Assert.NotNull(viewModel.RecentActivity);
        }

        [Fact]
        public async Task LoadDashboard_ShouldLoadKPIs()
        {
            // Arrange
            var stats = new FleetStatisticsDto
            {
                TotalVehicles = 50,
                AvailableVehicles = 40,
                MaintenanceVehicles = 10,
                ActiveContracts = 45,
                MaintenanceRequests = 5
            };

            _mockReportingService
                .Setup(x => x.GetFleetStatisticsAsync())
                .ReturnsAsync(stats);

            var viewModel = new DashboardViewModel(
                _mockReportingService.Object,
                _mockNotificationService.Object
            );

            // Act
            await viewModel.LoadDashboardAsync();

            // Assert
            Assert.Equal(50, viewModel.TotalVehicles);
            Assert.Equal(40, viewModel.AvailableVehicles);
            Assert.Equal(10, viewModel.MaintenanceVehicles);
        }

        [Fact]
        public async Task LoadAlerts_ShouldLoadExpiringContractsAndLicenses()
        {
            // Arrange
            var alerts = new List<AlertDto>
            {
                new AlertDto { Id = 1, Title = "عقد ينتهي قريباً", Type = "Warning" },
                new AlertDto { Id = 2, Title = "رخصة تنتهي قريباً", Type = "Warning" }
            };

            _mockReportingService
                .Setup(x => x.GetAlertsAsync())
                .ReturnsAsync(alerts);

            var viewModel = new DashboardViewModel(
                _mockReportingService.Object,
                _mockNotificationService.Object
            );

            // Act
            await viewModel.LoadAlertsAsync();

            // Assert
            Assert.NotEmpty(viewModel.Alerts);
            Assert.Equal(2, viewModel.Alerts.Count);
        }

        [Fact]
        public async Task RefreshCommand_ShouldReloadAllData()
        {
            // Arrange
            _mockReportingService
                .Setup(x => x.GetFleetStatisticsAsync())
                .ReturnsAsync(new FleetStatisticsDto { TotalVehicles = 50 });

            _mockReportingService
                .Setup(x => x.GetAlertsAsync())
                .ReturnsAsync(new List<AlertDto>());

            var viewModel = new DashboardViewModel(
                _mockReportingService.Object,
                _mockNotificationService.Object
            );

            // Act
            await viewModel.RefreshCommand.ExecuteAsync(null);

            // Assert
            _mockReportingService.Verify(x => x.GetFleetStatisticsAsync(), Times.Once);
            _mockReportingService.Verify(x => x.GetAlertsAsync(), Times.Once);
        }

        [Fact]
        public async Task LoadDashboard_WithError_ShouldSetErrorMessage()
        {
            // Arrange
            _mockReportingService
                .Setup(x => x.GetFleetStatisticsAsync())
                .ThrowsAsync(new Exception("خطأ في تحميل البيانات"));

            var viewModel = new DashboardViewModel(
                _mockReportingService.Object,
                _mockNotificationService.Object
            );

            // Act
            await viewModel.LoadDashboardAsync();

            // Assert
            Assert.NotEmpty(viewModel.ErrorMessage);
            Assert.Contains("خطأ", viewModel.ErrorMessage);
        }

        [Fact]
        public void IsLoading_PropertyChanged_ShouldRaiseNotification()
        {
            // Arrange
            var viewModel = new DashboardViewModel(
                _mockReportingService.Object,
                _mockNotificationService.Object
            );

            bool notificationRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DashboardViewModel.IsLoading))
                    notificationRaised = true;
            };

            // Act
            viewModel.IsLoading = true;

            // Assert
            Assert.True(notificationRaised);
        }
    }
}
