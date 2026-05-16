// ============================================================================
// FILE: INavigationService.cs
// LOCATION: FleetManagementSystem.WPF/Services/Navigation/INavigationService.cs
// ============================================================================

using System;
using System.Windows.Controls;

namespace FleetManagementSystem.WPF.Services.Navigation
{
    public interface INavigationService
    {
        void NavigateTo(string viewName);
        void NavigateToDialog(string dialogName);
        void CloseDialog();
        event EventHandler<NavigationEventArgs> NavigationRequested;
        event EventHandler<DialogEventArgs> DialogRequested;
    }

    public class NavigationEventArgs : EventArgs
    {
        public string ViewName { get; set; }
    }

    public class DialogEventArgs : EventArgs
    {
        public string DialogName { get; set; }
        public object DataContext { get; set; }
    }
}

// ============================================================================
// FILE: NavigationService.cs
// LOCATION: FleetManagementSystem.WPF/Services/Navigation/NavigationService.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using FleetManagementSystem.WPF.Views;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Services.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private Dictionary<string, Type> _viewRegistry;
        private Dictionary<string, Type> _dialogRegistry;

        public event EventHandler<NavigationEventArgs> NavigationRequested;
        public event EventHandler<DialogEventArgs> DialogRequested;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeViewRegistry();
            InitializeDialogRegistry();
        }

        private void InitializeViewRegistry()
        {
            _viewRegistry = new Dictionary<string, Type>
            {
                { "Dashboard", typeof(DashboardView) },
                { "Vehicles", typeof(VehicleListView) },
                { "Contracts", typeof(ContractListView) },
                { "Maintenance", typeof(MaintenanceListView) },
                { "Drivers", typeof(DriverListView) },
                { "Employees", typeof(EmployeeListView) },
                { "Trips", typeof(TripListView) },
                { "Fuel", typeof(FuelListView) },
                { "Expenses", typeof(ExpenseListView) },
                { "Licenses", typeof(LicenseListView) },
                { "Insurance", typeof(InsuranceListView) },
                { "Custody", typeof(CustodyListView) },
                { "MasterData", typeof(MasterDataView) },
                { "Reports", typeof(ReportsView) },
                { "Settings", typeof(SettingsView) }
            };
        }

        private void InitializeDialogRegistry()
        {
            _dialogRegistry = new Dictionary<string, Type>
            {
                { "VehicleForm", typeof(VehicleFormDialog) },
                { "ContractForm", typeof(ContractFormDialog) },
                { "MaintenanceForm", typeof(MaintenanceFormDialog) },
                { "DriverForm", typeof(DriverFormDialog) },
                { "EmployeeForm", typeof(EmployeeFormDialog) },
                { "TripForm", typeof(TripFormDialog) },
                { "FuelForm", typeof(FuelFormDialog) },
                { "ExpenseForm", typeof(ExpenseFormDialog) },
                { "LicenseForm", typeof(LicenseFormDialog) },
                { "InsuranceForm", typeof(InsuranceFormDialog) },
                { "CustodyForm", typeof(CustodyFormDialog) }
            };
        }

        public void NavigateTo(string viewName)
        {
            if (_viewRegistry.ContainsKey(viewName))
            {
                NavigationRequested?.Invoke(this, new NavigationEventArgs { ViewName = viewName });
            }
            else
            {
                throw new InvalidOperationException($"View '{viewName}' not found in registry");
            }
        }

        public void NavigateToDialog(string dialogName)
        {
            if (_dialogRegistry.ContainsKey(dialogName))
            {
                DialogRequested?.Invoke(this, new DialogEventArgs { DialogName = dialogName });
            }
            else
            {
                throw new InvalidOperationException($"Dialog '{dialogName}' not found in registry");
            }
        }

        public void CloseDialog()
        {
            DialogRequested?.Invoke(this, new DialogEventArgs { DialogName = null });
        }

        public UserControl GetView(string viewName)
        {
            if (_viewRegistry.ContainsKey(viewName))
            {
                var viewType = _viewRegistry[viewName];
                return (UserControl)_serviceProvider.GetService(viewType);
            }
            return null;
        }

        public Window GetDialog(string dialogName)
        {
            if (_dialogRegistry.ContainsKey(dialogName))
            {
                var dialogType = _dialogRegistry[dialogName];
                return (Window)_serviceProvider.GetService(dialogType);
            }
            return null;
        }
    }
}

// ============================================================================
// FILE: MainWindow.xaml.cs (UPDATED WITH NAVIGATION WIRING)
// LOCATION: FleetManagementSystem.WPF/MainWindow.xaml.cs
// ============================================================================

using System;
using System.Windows;
using System.Windows.Controls;
using FleetManagementSystem.WPF.Services.Navigation;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF
{
    public partial class MainWindow : Window
    {
        private readonly INavigationService _navigationService;
        private readonly MainWindowViewModel _viewModel;

        public MainWindow(INavigationService navigationService, MainWindowViewModel viewModel)
        {
            InitializeComponent();
            _navigationService = navigationService;
            _viewModel = viewModel;
            DataContext = _viewModel;

            // Subscribe to navigation events
            _navigationService.NavigationRequested += OnNavigationRequested;
            _navigationService.DialogRequested += OnDialogRequested;

            // Load dashboard on startup
            _navigationService.NavigateTo("Dashboard");
        }

        private void OnNavigationRequested(object sender, NavigationEventArgs e)
        {
            try
            {
                var view = _navigationService.GetView(e.ViewName);
                if (view != null)
                {
                    MainContentArea.Content = view;
                    _viewModel.CurrentViewName = e.ViewName;

                    // Update sidebar selection
                    UpdateSidebarSelection(e.ViewName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في الملاحة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnDialogRequested(object sender, DialogEventArgs e)
        {
            if (e.DialogName == null)
            {
                // Close dialog
                return;
            }

            try
            {
                var dialog = _navigationService.GetDialog(e.DialogName);
                if (dialog != null)
                {
                    dialog.Owner = this;
                    dialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح النافذة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSidebarSelection(string viewName)
        {
            // Update sidebar menu item selection based on current view
            foreach (var item in SidebarMenu.Items)
            {
                if (item is MenuItem menuItem)
                {
                    menuItem.IsChecked = (string)menuItem.Tag == viewName;
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is string viewName)
            {
                _navigationService.NavigateTo(viewName);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "هل تريد تسجيل الخروج؟",
                "تأكيد",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Clear user session
                Application.Current.Properties["CurrentUser"] = null;

                // Navigate to login window
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load company settings and display branding
            _viewModel.LoadCompanySettings();
        }
    }
}

// ============================================================================
// FILE: MainWindowViewModel.cs (UPDATED WITH NAVIGATION LOGIC)
// LOCATION: FleetManagementSystem.WPF/ViewModels/MainWindowViewModel.cs
// ============================================================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Services.Interfaces;
using FleetManagementSystem.WPF.Commands;
using FleetManagementSystem.WPF.Services.Navigation;

namespace FleetManagementSystem.WPF.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly ISettingsService _settingsService;
        private string _currentViewName;
        private CompanySettingsDto _companySettings;
        private ObservableCollection<DashboardItemDto> _dashboardItems;
        private int _unreadNotificationCount;

        public string CurrentViewName
        {
            get => _currentViewName;
            set => SetProperty(ref _currentViewName, value);
        }

        public CompanySettingsDto CompanySettings
        {
            get => _companySettings;
            set => SetProperty(ref _companySettings, value);
        }

        public ObservableCollection<DashboardItemDto> DashboardItems
        {
            get => _dashboardItems;
            set => SetProperty(ref _dashboardItems, value);
        }

        public int UnreadNotificationCount
        {
            get => _unreadNotificationCount;
            set => SetProperty(ref _unreadNotificationCount, value);
        }

        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToVehiclesCommand { get; }
        public ICommand NavigateToContractsCommand { get; }
        public ICommand NavigateToMaintenanceCommand { get; }
        public ICommand NavigateToDriversCommand { get; }
        public ICommand NavigateToEmployeesCommand { get; }
        public ICommand NavigateToTripsCommand { get; }
        public ICommand NavigateToFuelCommand { get; }
        public ICommand NavigateToExpensesCommand { get; }
        public ICommand NavigateToLicensesCommand { get; }
        public ICommand NavigateToInsuranceCommand { get; }
        public ICommand NavigateToCustodyCommand { get; }
        public ICommand NavigateToMasterDataCommand { get; }
        public ICommand NavigateToReportsCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }

        public MainWindowViewModel(INavigationService navigationService, ISettingsService settingsService)
        {
            _navigationService = navigationService;
            _settingsService = settingsService;

            // Initialize commands
            NavigateToDashboardCommand = new RelayCommand(() => _navigationService.NavigateTo("Dashboard"));
            NavigateToVehiclesCommand = new RelayCommand(() => _navigationService.NavigateTo("Vehicles"));
            NavigateToContractsCommand = new RelayCommand(() => _navigationService.NavigateTo("Contracts"));
            NavigateToMaintenanceCommand = new RelayCommand(() => _navigationService.NavigateTo("Maintenance"));
            NavigateToDriversCommand = new RelayCommand(() => _navigationService.NavigateTo("Drivers"));
            NavigateToEmployeesCommand = new RelayCommand(() => _navigationService.NavigateTo("Employees"));
            NavigateToTripsCommand = new RelayCommand(() => _navigationService.NavigateTo("Trips"));
            NavigateToFuelCommand = new RelayCommand(() => _navigationService.NavigateTo("Fuel"));
            NavigateToExpensesCommand = new RelayCommand(() => _navigationService.NavigateTo("Expenses"));
            NavigateToLicensesCommand = new RelayCommand(() => _navigationService.NavigateTo("Licenses"));
            NavigateToInsuranceCommand = new RelayCommand(() => _navigationService.NavigateTo("Insurance"));
            NavigateToCustodyCommand = new RelayCommand(() => _navigationService.NavigateTo("Custody"));
            NavigateToMasterDataCommand = new RelayCommand(() => _navigationService.NavigateTo("MasterData"));
            NavigateToReportsCommand = new RelayCommand(() => _navigationService.NavigateTo("Reports"));
            NavigateToSettingsCommand = new RelayCommand(() => _navigationService.NavigateTo("Settings"));

            InitializeDashboardItems();
        }

        private void InitializeDashboardItems()
        {
            DashboardItems = new ObservableCollection<DashboardItemDto>
            {
                new DashboardItemDto { Title = "المركبات", Icon = "🚗", ViewName = "Vehicles", Description = "إدارة المركبات" },
                new DashboardItemDto { Title = "العقود", Icon = "📋", ViewName = "Contracts", Description = "إدارة العقود" },
                new DashboardItemDto { Title = "الصيانة", Icon = "🔧", ViewName = "Maintenance", Description = "طلبات الصيانة" },
                new DashboardItemDto { Title = "السائقون", Icon = "👨‍✈️", ViewName = "Drivers", Description = "إدارة السائقين" },
                new DashboardItemDto { Title = "الموظفون", Icon = "👥", ViewName = "Employees", Description = "إدارة الموظفين" },
                new DashboardItemDto { Title = "الرحلات", Icon = "🛣️", ViewName = "Trips", Description = "تتبع الرحلات" },
                new DashboardItemDto { Title = "الوقود", Icon = "⛽", ViewName = "Fuel", Description = "معاملات الوقود" },
                new DashboardItemDto { Title = "المصروفات", Icon = "💰", ViewName = "Expenses", Description = "تتبع المصروفات" },
                new DashboardItemDto { Title = "الرخص", Icon = "📜", ViewName = "Licenses", Description = "إدارة الرخص" },
                new DashboardItemDto { Title = "التأمين", Icon = "🛡️", ViewName = "Insurance", Description = "بوليصات التأمين" },
                new DashboardItemDto { Title = "العهد", Icon = "📦", ViewName = "Custody", Description = "تتبع العهد" },
                new DashboardItemDto { Title = "البيانات الأساسية", Icon = "⚙️", ViewName = "MasterData", Description = "إعدادات البيانات" },
                new DashboardItemDto { Title = "التقارير", Icon = "📊", ViewName = "Reports", Description = "التقارير والإحصائيات" },
                new DashboardItemDto { Title = "الإعدادات", Icon = "🔧", ViewName = "Settings", Description = "إعدادات التطبيق" }
            };
        }

        public async void LoadCompanySettings()
        {
            try
            {
                CompanySettings = await _settingsService.GetCompanySettingsAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"خطأ في تحميل الإعدادات: {ex.Message}");
            }
        }
    }
}

// ============================================================================
// FILE: MainWindow.xaml (UPDATED WITH NAVIGATION UI)
// LOCATION: FleetManagementSystem.WPF/MainWindow.xaml
// ============================================================================

/*
<Window x:Class="FleetManagementSystem.WPF.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="نظام إدارة الأسطول" Height="768" Width="1024"
        FlowDirection="RightToLeft" Background="#F5F5F5"
        WindowStartupLocation="CenterScreen" WindowState="Maximized">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="250"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>

        <!-- Sidebar Navigation -->
        <StackPanel Grid.Column="0" Background="#2C3E50" Orientation="Vertical">
            <!-- Company Logo and Name -->
            <Border Background="#34495E" Padding="20">
                <StackPanel Orientation="Vertical">
                    <TextBlock Text="{Binding CompanySettings.CompanyName}" 
                               Foreground="White" FontSize="18" FontWeight="Bold" TextAlignment="Center"/>
                    <TextBlock Text="نظام إدارة الأسطول" 
                               Foreground="#BDC3C7" FontSize="12" TextAlignment="Center" Margin="0,5,0,0"/>
                </StackPanel>
            </Border>

            <!-- Menu Items -->
            <ScrollViewer VerticalScrollBarVisibility="Auto">
                <StackPanel Orientation="Vertical" Margin="0,10,0,0">
                    <!-- Dashboard -->
                    <Button Command="{Binding NavigateToDashboardCommand}"
                            Style="{StaticResource MenuButtonStyle}"
                            Content="لوحة التحكم" Tag="Dashboard"/>

                    <!-- Vehicles -->
                    <Button Command="{Binding NavigateToVehiclesCommand}"
                            Style="{StaticResource MenuButtonStyle}"
                            Content="المركبات" Tag="Vehicles"/>

                    <!-- Contracts -->
                    <Button Command="{Binding NavigateToContractsCommand}"
                            Style="{StaticResource MenuButtonStyle}"
                            Content="العقود" Tag="Contracts"/>

                    <!-- Maintenance -->
                    <Button Command="{Binding NavigateToMaintenanceCommand}"
                            Style="{StaticResource MenuButtonStyle}"
                            Content="الصيانة" Tag="Maintenance"/>

                    <!-- Drivers -->
                    <Button Command="{Binding NavigateToDriversCommand}"
                            Style="{StaticResource MenuButtonStyle}"
                            Content="السائقون" Tag="Drivers"/>

                    <!-- Employees -->
                    <Button Command="{Binding NavigateToEmployeesCommand}"
                            Style="{StaticResource MenuButtonStyle}"
                            Content="الموظفون" Tag="Employees"/>

                    <!-- Trips -->
                    <Button Command="{Binding NavigateToTripsCommand}"
                            Style="{StaticResource MenuButtonStyle}"
                            Content="الرحلات" Tag="Trips"/>

                    <!-- Fuel -->
                    <Button Command="{Binding NavigateToFuelCommand}"
                            Style="{StaticResource MenuButtonStyle}"
                            Content="الوقود" Tag="Fuel"/>

                    <!-- Expenses -->
                    <Button Command="{Binding NavigateToExpensesCommand}"
                            Style="{StaticResource MenuButtonStyle}"
                            Content="المصروفات" Tag="Expenses"/>

                    <!-- Licenses -->
                    <Button Command="{Binding NavigateToLicensesCommand}"
                            Style="{StaticResource MenuButtonStyle}"
                            Content="الرخص" Tag="Licenses"/>

                    <!-- Insurance -->
                    <Button Command="{Binding NavigateToInsuranceCommand}"
                            Style="{StaticResource MenuButtonStyle}"
                            Content="التأمين" Tag="Insurance"/>

                    <!-- Custody -->
                    <Button Command="{Binding NavigateToCustodyCommand}"
                            Style="{StaticResource MenuButtonStyle}"
                            Content="العهد" Tag="Custody"/>

                    <!-- Master Data -->
                    <Button Command="{Binding NavigateToMasterDataCommand}"
                            Style="{StaticResource MenuButtonStyle}"
                            Content="البيانات الأساسية" Tag="MasterData"/>

                    <!-- Reports -->
                    <Button Command="{Binding NavigateToReportsCommand}"
                            Style="{StaticResource MenuButtonStyle}"
                            Content="التقارير" Tag="Reports"/>

                    <!-- Settings -->
                    <Button Command="{Binding NavigateToSettingsCommand}"
                            Style="{StaticResource MenuButtonStyle}"
                            Content="الإعدادات" Tag="Settings"/>
                </StackPanel>
            </ScrollViewer>

            <!-- Logout Button -->
            <Button Click="LogoutButton_Click"
                    Background="#E74C3C" Foreground="White"
                    Padding="20,15" Margin="10" FontSize="14" FontWeight="Bold"
                    VerticalAlignment="Bottom"
                    Content="تسجيل الخروج"/>
        </StackPanel>

        <!-- Main Content Area -->
        <Grid Grid.Column="1">
            <Grid.RowDefinitions>
                <RowDefinition Height="60"/>
                <RowDefinition Height="*"/>
            </Grid.RowDefinitions>

            <!-- Header -->
            <Border Grid.Row="0" Background="White" BorderBrush="#E0E0E0" BorderThickness="0,0,0,1">
                <Grid Padding="20">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="Auto"/>
                    </Grid.ColumnDefinitions>

                    <!-- Title -->
                    <TextBlock Grid.Column="0" Text="{Binding CurrentViewName}" 
                               FontSize="20" FontWeight="Bold" VerticalAlignment="Center"/>

                    <!-- Notifications -->
                    <StackPanel Grid.Column="1" Orientation="Horizontal" VerticalAlignment="Center">
                        <Button Content="{Binding UnreadNotificationCount}" 
                                Background="#3498DB" Foreground="White"
                                Padding="10,5" BorderRadius="50" FontWeight="Bold"/>
                    </StackPanel>
                </Grid>
            </Border>

            <!-- Content Area -->
            <ContentControl Grid.Row="1" x:Name="MainContentArea" 
                           Margin="20" Content="{Binding CurrentView}"/>
        </Grid>
    </Grid>
</Window>
*/

// ============================================================================
// FILE: App.xaml.cs (UPDATED WITH NAVIGATION AND VIEW DI REGISTRATION)
// LOCATION: FleetManagementSystem.WPF/App.xaml.cs
// ============================================================================

using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using FleetManagementSystem.Data;
using FleetManagementSystem.Services.Interfaces;
using FleetManagementSystem.Services.Implementations;
using FleetManagementSystem.WPF.Services.Navigation;
using FleetManagementSystem.WPF.ViewModels;
using FleetManagementSystem.WPF.Views;

namespace FleetManagementSystem.WPF
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            // Register DbContext
            services.AddScoped<FleetDbContext>();

            // Register Services
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<IMaintenanceService, MaintenanceService>();
            services.AddScoped<IDriverService, DriverService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<ITripService, TripService>();
            services.AddScoped<IFuelService, FuelService>();
            services.AddScoped<IExpenseService, ExpenseService>();
            services.AddScoped<ILicenseService, LicenseService>();
            services.AddScoped<IInsuranceService, InsuranceService>();
            services.AddScoped<ICustodyService, CustodyService>();
            services.AddScoped<IMasterDataService, MasterDataService>();
            services.AddScoped<IReportingService, ReportingService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            // Register Navigation Service
            services.AddSingleton<INavigationService, NavigationService>();

            // Register ViewModels
            services.AddScoped<LoginViewModel>();
            services.AddScoped<MainWindowViewModel>();
            services.AddScoped<DashboardViewModel>();
            services.AddScoped<VehicleViewModel>();
            services.AddScoped<VehicleFormViewModel>();
            services.AddScoped<ContractViewModel>();
            services.AddScoped<ContractFormViewModel>();
            services.AddScoped<MaintenanceViewModel>();
            services.AddScoped<MaintenanceFormViewModel>();
            services.AddScoped<DriverViewModel>();
            services.AddScoped<DriverFormViewModel>();
            services.AddScoped<EmployeeViewModel>();
            services.AddScoped<EmployeeFormViewModel>();
            services.AddScoped<TripViewModel>();
            services.AddScoped<TripFormViewModel>();
            services.AddScoped<FuelViewModel>();
            services.AddScoped<FuelFormViewModel>();
            services.AddScoped<ExpenseViewModel>();
            services.AddScoped<ExpenseFormViewModel>();
            services.AddScoped<LicenseViewModel>();
            services.AddScoped<LicenseFormViewModel>();
            services.AddScoped<InsuranceViewModel>();
            services.AddScoped<InsuranceFormViewModel>();
            services.AddScoped<CustodyViewModel>();
            services.AddScoped<CustodyFormViewModel>();
            services.AddScoped<MasterDataViewModel>();
            services.AddScoped<ReportsViewModel>();
            services.AddScoped<SettingsViewModel>();

            // Register Views
            services.AddScoped<LoginWindow>();
            services.AddScoped<MainWindow>();
            services.AddScoped<DashboardView>();
            services.AddScoped<VehicleListView>();
            services.AddScoped<VehicleFormDialog>();
            services.AddScoped<ContractListView>();
            services.AddScoped<ContractFormDialog>();
            services.AddScoped<MaintenanceListView>();
            services.AddScoped<MaintenanceFormDialog>();
            services.AddScoped<DriverListView>();
            services.AddScoped<DriverFormDialog>();
            services.AddScoped<EmployeeListView>();
            services.AddScoped<EmployeeFormDialog>();
            services.AddScoped<TripListView>();
            services.AddScoped<TripFormDialog>();
            services.AddScoped<FuelListView>();
            services.AddScoped<FuelFormDialog>();
            services.AddScoped<ExpenseListView>();
            services.AddScoped<ExpenseFormDialog>();
            services.AddScoped<LicenseListView>();
            services.AddScoped<LicenseFormDialog>();
            services.AddScoped<InsuranceListView>();
            services.AddScoped<InsuranceFormDialog>();
            services.AddScoped<CustodyListView>();
            services.AddScoped<CustodyFormDialog>();
            services.AddScoped<MasterDataView>();
            services.AddScoped<ReportsView>();
            services.AddScoped<SettingsView>();

            _serviceProvider = services.BuildServiceProvider();

            // Show login window
            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();

            // Close splash screen if exists
            this.MainWindow = null;
        }

        public IServiceProvider ServiceProvider => _serviceProvider;
    }
}

// ============================================================================
// FILE: LoginWindow.xaml.cs (UPDATED WITH NAVIGATION TO MAINWINDOW)
// LOCATION: FleetManagementSystem.WPF/LoginWindow.xaml.cs
// ============================================================================

using System.Windows;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;
        private readonly IServiceProvider _serviceProvider;

        public LoginWindow(LoginViewModel viewModel, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _serviceProvider = serviceProvider;
            DataContext = _viewModel;

            // Subscribe to login success event
            _viewModel.LoginSucceeded += ViewModel_LoginSucceeded;
        }

        private void ViewModel_LoginSucceeded(object sender, System.EventArgs e)
        {
            // Navigate to main window
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            this.Close();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.Password = PasswordBox.Password;
        }
    }
}

// ============================================================================
// FILE: DashboardItemDto.cs
// LOCATION: FleetManagementSystem.Core/DTOs/DashboardItemDto.cs
// ============================================================================

using System;

namespace FleetManagementSystem.Core.DTOs
{
    public class DashboardItemDto
    {
        public string Title { get; set; }
        public string Icon { get; set; }
        public string ViewName { get; set; }
        public string Description { get; set; }
    }
}

// ============================================================================
// FILE: ViewModelLocator.cs (OPTIONAL - FOR XAML-BASED BINDING)
// LOCATION: FleetManagementSystem.WPF/ViewModels/ViewModelLocator.cs
// ============================================================================

using System;
using Microsoft.Extensions.DependencyInjection;

namespace FleetManagementSystem.WPF.ViewModels
{
    public class ViewModelLocator
    {
        private readonly IServiceProvider _serviceProvider;

        public ViewModelLocator()
        {
            _serviceProvider = ((App)App.Current).ServiceProvider;
        }

        public MainWindowViewModel MainWindowViewModel => _serviceProvider.GetRequiredService<MainWindowViewModel>();
        public DashboardViewModel DashboardViewModel => _serviceProvider.GetRequiredService<DashboardViewModel>();
        public VehicleViewModel VehicleViewModel => _serviceProvider.GetRequiredService<VehicleViewModel>();
        public ContractViewModel ContractViewModel => _serviceProvider.GetRequiredService<ContractViewModel>();
        public MaintenanceViewModel MaintenanceViewModel => _serviceProvider.GetRequiredService<MaintenanceViewModel>();
        public DriverViewModel DriverViewModel => _serviceProvider.GetRequiredService<DriverViewModel>();
        public EmployeeViewModel EmployeeViewModel => _serviceProvider.GetRequiredService<EmployeeViewModel>();
        public TripViewModel TripViewModel => _serviceProvider.GetRequiredService<TripViewModel>();
        public FuelViewModel FuelViewModel => _serviceProvider.GetRequiredService<FuelViewModel>();
        public ExpenseViewModel ExpenseViewModel => _serviceProvider.GetRequiredService<ExpenseViewModel>();
        public LicenseViewModel LicenseViewModel => _serviceProvider.GetRequiredService<LicenseViewModel>();
        public InsuranceViewModel InsuranceViewModel => _serviceProvider.GetRequiredService<InsuranceViewModel>();
        public CustodyViewModel CustodyViewModel => _serviceProvider.GetRequiredService<CustodyViewModel>();
        public MasterDataViewModel MasterDataViewModel => _serviceProvider.GetRequiredService<MasterDataViewModel>();
        public ReportsViewModel ReportsViewModel => _serviceProvider.GetRequiredService<ReportsViewModel>();
        public SettingsViewModel SettingsViewModel => _serviceProvider.GetRequiredService<SettingsViewModel>();
    }
}
