using FleetManagementSystem.Data;
using FleetManagementSystem.Services;
using FleetManagementSystem.WPF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Application = System.Windows.Application;

namespace FleetManagementSystem.WPF;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();

            var connectionStore = new ConnectionSettingsStore();
            var fallbackProfile = ClientConnectionProfile.FromConnectionString(configuration.GetConnectionString("DefaultConnection"));
            var connectionProfile = await ResolveConnectionProfileAsync(connectionStore, fallbackProfile);
            if (connectionProfile is null)
            {
                Shutdown(0);
                return;
            }

            var services = new ServiceCollection();
            ConfigureServices(services, configuration, connectionStore, connectionProfile);
            _serviceProvider = services.BuildServiceProvider();

            using var scope = _serviceProvider.CreateScope();
            var bootstrapService = scope.ServiceProvider.GetRequiredService<IDataBootstrapService>();
            await bootstrapService.InitializeAsync();

            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            MainWindow = loginWindow;
            loginWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"تعذر بدء التطبيق:\n{ex.Message}",
                "خطأ في التشغيل",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(-1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    private static async Task<ClientConnectionProfile?> ResolveConnectionProfileAsync(ConnectionSettingsStore connectionStore, ClientConnectionProfile fallbackProfile)
    {
        var savedProfile = connectionStore.Load();
        if (savedProfile is not null)
        {
            var savedResult = await connectionStore.TestAsync(savedProfile);
            if (savedResult.IsSuccess)
            {
                return savedProfile;
            }
        }

        var setupWindow = new ConnectionSettingsWindow(connectionStore, savedProfile ?? fallbackProfile);
        return setupWindow.ShowDialog() == true ? setupWindow.ConnectionProfile : null;
    }

    private static void ConfigureServices(
        IServiceCollection services,
        IConfiguration configuration,
        ConnectionSettingsStore connectionStore,
        ClientConnectionProfile connectionProfile)
    {
        var connectionString = connectionProfile.BuildConnectionString();

        services.AddSingleton(configuration);
        services.AddSingleton(connectionStore);
        services.AddSingleton(connectionProfile);

        services.AddDbContext<FleetDbContext>(options =>
        {
            if (connectionProfile.IsSqlite)
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            }
        });

        services.AddScoped<IDataBootstrapService, DataBootstrapService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<IContractService, ContractService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        services.AddScoped<IDriverService, DriverService>();
        services.AddScoped<IDriverAttendanceService, DriverAttendanceService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ITripService, TripService>();
        services.AddScoped<IFuelService, FuelService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IOilChangeService, OilChangeService>();
        services.AddScoped<ITreasuryService, TreasuryService>();
        services.AddScoped<ILicenseService, LicenseService>();
        services.AddScoped<IInsuranceService, InsuranceService>();
        services.AddScoped<ICustodyService, CustodyService>();
        services.AddScoped<IMasterDataService, MasterDataService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddScoped<ISettingsService, SettingsService>();

        services.AddTransient<LoginWindow>();
        services.AddTransient<ConnectionSettingsWindow>();
        services.AddTransient<MainWindow>();
    }
}
