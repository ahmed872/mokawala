using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Services;
using FleetManagementSystem.WPF.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Windows;

namespace FleetManagementSystem.WPF;

public partial class LoginWindow : Window
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ISettingsService _settingsService;
    private readonly ConnectionSettingsStore _connectionSettingsStore;
    private readonly ClientConnectionProfile _connectionProfile;
    private readonly IServiceProvider _serviceProvider;

    public LoginWindow(
        IAuthenticationService authenticationService,
        ISettingsService settingsService,
        ConnectionSettingsStore connectionSettingsStore,
        ClientConnectionProfile connectionProfile,
        IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _authenticationService = authenticationService;
        _settingsService = settingsService;
        _connectionSettingsStore = connectionSettingsStore;
        _connectionProfile = connectionProfile;
        _serviceProvider = serviceProvider;
        CompanyNameTextBlock.Text = "شركة جوميكس للحركة والمعدات";

        Loaded += LoginWindow_Loaded;
    }

    private async void LoginWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            if (!string.IsNullOrWhiteSpace(settings.CompanyName))
            {
                var companyName = NormalizeCompanyName(settings.CompanyName);
                CompanyNameTextBlock.Text = companyName;
                Title = $"تسجيل الدخول - {companyName}";
                SystemNameTextBlock.Text = "تسجيل الدخول إلى النظام";
            }
        }
        catch
        {
            // Keep the login window usable even if settings loading fails.
        }

        UsernameTextBox.Focus();
        UsernameTextBox.SelectAll();
    }

    private static string NormalizeCompanyName(string companyName)
    {
        if (companyName.Contains("شركه جوميكس", StringComparison.OrdinalIgnoreCase) ||
            companyName.Contains("للحركه", StringComparison.OrdinalIgnoreCase))
        {
            return "شركة جوميكس للحركة والمعدات";
        }

        return companyName;
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ErrorTextBlock.Text = string.Empty;

            var user = await _authenticationService.AuthenticateAsync(new UserLoginDto
            {
                Username = UsernameTextBox.Text,
                Password = PasswordBox.Password
            });

            if (user is null)
            {
                ErrorTextBlock.Text = "بيانات الدخول غير صحيحة أو الحساب غير مفعل.";
                return;
            }

            // Custodian accounts (drivers/employees holding custodies) get the self-service
            // "عهدتي" dashboard only — never the operational main window.
            if (string.Equals(user.Role, "Custodian", StringComparison.OrdinalIgnoreCase))
            {
                var custodyWindow = new MyCustodyWindow(
                    _serviceProvider.GetRequiredService<ICustodyService>(),
                    user);
                Application.Current.MainWindow = custodyWindow;
                custodyWindow.Show();
                Close();
                return;
            }

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.SetCurrentUser(user);
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
            Close();
        }
        catch (Exception ex)
        {
            ErrorTextBlock.Text = ex.Message;
        }
    }

    private void ConnectionSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var window = new ConnectionSettingsWindow(_connectionSettingsStore, _connectionProfile)
        {
            Owner = this
        };

        if (window.ShowDialog() != true)
        {
            return;
        }

        MessageBox.Show(
            "تم حفظ إعدادات الاتصال. سيُعاد تشغيل التطبيق لتطبيق الاتصال الجديد.",
            "تم الحفظ",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        var executablePath = Environment.ProcessPath;
        if (!string.IsNullOrWhiteSpace(executablePath))
        {
            Process.Start(new ProcessStartInfo(executablePath) { UseShellExecute = true });
        }

        Application.Current.Shutdown();
    }
}
