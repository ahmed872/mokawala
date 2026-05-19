// ============================================================================
// FILE: App.xaml
// LOCATION: FleetManagementSystem.WPF/App.xaml
// ============================================================================

/*
<Application x:Class="FleetManagementSystem.WPF.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:FleetManagementSystem.WPF"
             StartupUri="LoginWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- Fonts -->
                <ResourceDictionary Source="Resources/Fonts.xaml" />
                <!-- Colors -->
                <ResourceDictionary Source="Resources/Colors.xaml" />
                <!-- Brushes -->
                <ResourceDictionary Source="Resources/Brushes.xaml" />
                <!-- Styles -->
                <ResourceDictionary Source="Resources/Styles.xaml" />
                <!-- Converters -->
                <ResourceDictionary Source="Resources/Converters.xaml" />
                <!-- Data Templates -->
                <ResourceDictionary Source="Resources/DataTemplates.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
*/

// ============================================================================
// FILE: App.xaml.cs
// LOCATION: FleetManagementSystem.WPF/App.xaml.cs
// ============================================================================

using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FleetManagementSystem.Data;
using FleetManagementSystem.Services;
using FleetManagementSystem.WPF.Views;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// Complete dependency injection setup and application startup configuration.
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Build configuration
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                // Build service collection
                var services = new ServiceCollection();

                // Register configuration
                services.AddSingleton(configuration);

                // Register database context
                services.AddDbContext<FleetDbContext>(options =>
                    options.UseMySql(
                        configuration.GetConnectionString("DefaultConnection"),
                        ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))
                    )
                );

                // Register data layer services
                RegisterDataServices(services);

                // Register business services
                RegisterBusinessServices(services);

                // Register WPF services
                RegisterWpfServices(services);

                // Register ViewModels
                RegisterViewModels(services);

                // Register Views
                RegisterViews(services);

                // Build service provider
                _serviceProvider = services.BuildServiceProvider();

                // Show login window
                var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
                loginWindow.Show();

                // Hide default window
                this.MainWindow = loginWindow;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"خطأ في بدء التطبيق: {ex.Message}\n\n{ex.StackTrace}",
                    "خطأ في البدء",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                this.Shutdown(1);
            }
        }

        private void RegisterDataServices(IServiceCollection services)
        {
            // Data access services
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        }

        private void RegisterBusinessServices(IServiceCollection services)
        {
            // Authentication and authorization
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();

            // Business services
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

            // Export/Report services
            services.AddScoped<IExcelExportService, ExcelExportService>();
            services.AddScoped<IPdfExportService, PdfExportService>();
        }

        private void RegisterWpfServices(IServiceCollection services)
        {
            // Navigation and UI services
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<IProgressService, ProgressService>();

            // Session management
            services.AddSingleton<ISessionManager, SessionManager>();
        }

        private void RegisterViewModels(IServiceCollection services)
        {
            // Core ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<DashboardViewModel>();

            // Module ViewModels
            services.AddTransient<VehicleViewModel>();
            services.AddTransient<VehicleFormViewModel>();
            services.AddTransient<ContractViewModel>();
            services.AddTransient<ContractFormViewModel>();
            services.AddTransient<MaintenanceViewModel>();
            services.AddTransient<MaintenanceFormViewModel>();
            services.AddTransient<DriverViewModel>();
            services.AddTransient<DriverFormViewModel>();
            services.AddTransient<EmployeeViewModel>();
            services.AddTransient<EmployeeFormViewModel>();
            services.AddTransient<TripViewModel>();
            services.AddTransient<TripFormViewModel>();
            services.AddTransient<FuelViewModel>();
            services.AddTransient<FuelFormViewModel>();
            services.AddTransient<ExpenseViewModel>();
            services.AddTransient<ExpenseFormViewModel>();
            services.AddTransient<LicenseViewModel>();
            services.AddTransient<LicenseFormViewModel>();
            services.AddTransient<InsuranceViewModel>();
            services.AddTransient<InsuranceFormViewModel>();
            services.AddTransient<CustodyViewModel>();
            services.AddTransient<CustodyFormViewModel>();
            services.AddTransient<MasterDataViewModel>();
            services.AddTransient<ReportsViewModel>();
            services.AddTransient<SettingsViewModel>();
        }

        private void RegisterViews(IServiceCollection services)
        {
            // Windows
            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();

            // Views
            services.AddTransient<DashboardView>();
            services.AddTransient<VehicleListView>();
            services.AddTransient<VehicleFormDialog>();
            services.AddTransient<ContractListView>();
            services.AddTransient<ContractFormDialog>();
            services.AddTransient<MaintenanceListView>();
            services.AddTransient<MaintenanceFormDialog>();
            services.AddTransient<DriverListView>();
            services.AddTransient<DriverFormDialog>();
            services.AddTransient<EmployeeListView>();
            services.AddTransient<EmployeeFormDialog>();
            services.AddTransient<TripListView>();
            services.AddTransient<TripFormDialog>();
            services.AddTransient<FuelListView>();
            services.AddTransient<FuelFormDialog>();
            services.AddTransient<ExpenseListView>();
            services.AddTransient<ExpenseFormDialog>();
            services.AddTransient<LicenseListView>();
            services.AddTransient<LicenseFormDialog>();
            services.AddTransient<InsuranceListView>();
            services.AddTransient<InsuranceFormDialog>();
            services.AddTransient<CustodyListView>();
            services.AddTransient<CustodyFormDialog>();
            services.AddTransient<MasterDataView>();
            services.AddTransient<ServiceProviderFormDialog>();
            services.AddTransient<ReportsView>();
            services.AddTransient<SettingsView>();
            services.AddTransient<UserFormDialog>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            (_serviceProvider as IDisposable)?.Dispose();
        }

        public static IServiceProvider GetServiceProvider()
        {
            return ((App)Current)._serviceProvider;
        }
    }
}
