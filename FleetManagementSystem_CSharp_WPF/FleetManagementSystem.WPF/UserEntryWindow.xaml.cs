using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Core.Enums;
using System.Windows;

namespace FleetManagementSystem.WPF;

public partial class UserEntryWindow : Window
{
    private static readonly IReadOnlyList<UserRoleOption> RoleOptions = new List<UserRoleOption>
    {
        new("Admin", "مدير النظام"),
        new("OperationsManager", "مدير التشغيل"),
        new("OperationsDataEntry", "إدخال بيانات التشغيل"),
        new("MaintenanceOfficer", "مسؤول الصيانة"),
        new("TreasuryOfficer", "مسؤول الخزينة"),
        new("TripsLicensesOfficer", "مسؤول التشغيلات والتراخيص"),
        new("InsuranceOfficer", "مسؤول التأمينات"),
        new("Viewer", "عرض فقط"),
        new("Staff", "موظف")
    };

    private static readonly Dictionary<string, string> ModuleDisplayNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Dashboard"] = "الرئيسية",
        ["Vehicles"] = "المركبات",
        ["Licenses"] = "تراخيص المركبات",
        ["Contracts"] = "العقود",
        ["Maintenance"] = "الصيانة",
        ["Drivers"] = "السائقين",
        ["Employees"] = "الموظفين",
        ["Trips"] = "التشغيلات",
        ["Fuel"] = "الوقود",
        ["Expenses"] = "المصروفات",
        ["OilChanges"] = "تغيير الزيت",
        ["Treasury"] = "الخزينة",
        ["Insurance"] = "التأمين",
        ["Custody"] = "العهد",
        ["MasterData"] = "البيانات الأساسية",
        ["Reports"] = "التقارير",
        ["Settings"] = "الإعدادات",
        ["Users"] = "المستخدمين",
        ["Notifications"] = "التنبيهات"
    };

    // Mirrors ServiceHelpers.AllowedModulesForRole (internal to the Services project, not reachable here).
    private static readonly Dictionary<UserRole, string[]> ModulesByRole = new()
    {
        [UserRole.Admin] = new[]
        {
            "Dashboard", "Vehicles", "Drivers", "Employees", "Trips", "Fuel", "Expenses", "OilChanges",
            "Maintenance", "Licenses", "Insurance", "Custody", "Treasury", "Reports", "MasterData", "Settings", "Users"
        },
        [UserRole.OperationsManager] = new[]
        {
            "Dashboard", "Vehicles", "Contracts", "Drivers", "Employees", "Trips", "Fuel", "Expenses",
            "Licenses", "Insurance", "Reports", "Notifications"
        },
        [UserRole.OperationsDataEntry] = new[]
        {
            "Dashboard", "Vehicles", "Drivers", "Employees", "Trips", "Fuel", "Expenses",
            "Licenses", "Insurance", "Notifications"
        },
        [UserRole.MaintenanceOfficer] = new[]
        {
            "Dashboard", "Vehicles", "Maintenance", "OilChanges", "Expenses", "Reports", "Notifications", "Insurance"
        },
        [UserRole.TreasuryOfficer] = new[] { "Dashboard", "Treasury" },
        [UserRole.TripsLicensesOfficer] = new[] { "Dashboard", "Trips", "Licenses" },
        [UserRole.InsuranceOfficer] = new[] { "Dashboard", "Insurance" },
        [UserRole.Viewer] = new[] { "Dashboard", "Reports", "Notifications" },
        [UserRole.Staff] = new[] { "Dashboard", "Trips", "Vehicles" }
    };

    private readonly UserFormDto? _source;

    public UserFormDto? UserForm { get; private set; }

    public UserEntryWindow(UserFormDto? source = null)
    {
        InitializeComponent();

        _source = source;
        RoleComboBox.ItemsSource = RoleOptions;

        if (source is not null && source.Id != 0)
        {
            Title = "تعديل بيانات مستخدم";
            HeaderTitleTextBlock.Text = "تعديل بيانات مستخدم";
            FullNameTextBox.Text = source.FullName;
            UsernameTextBox.Text = source.Username;
            EmailTextBox.Text = source.Email;
            PhoneTextBox.Text = source.PhoneNumber;
            IsActiveCheckBox.IsChecked = source.IsActive;
            RoleComboBox.SelectedValue = source.Role;
            PasswordHintTextBlock.Visibility = Visibility.Visible;
        }
        else
        {
            IsActiveCheckBox.IsChecked = true;
            RoleComboBox.SelectedIndex = 2;
        }

        if (RoleComboBox.SelectedItem is null)
        {
            RoleComboBox.SelectedIndex = 2;
        }

        UpdateRoleModulesPreview();
    }

    private void RoleComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) => UpdateRoleModulesPreview();

    private void UpdateRoleModulesPreview()
    {
        if (RoleComboBox.SelectedItem is not UserRoleOption option || !Enum.TryParse<UserRole>(option.Value, true, out var role))
        {
            RoleModulesTextBlock.Text = "-";
            return;
        }

        var modules = ModulesByRole.TryGetValue(role, out var moduleTags)
            ? moduleTags
            : Array.Empty<string>();

        RoleModulesTextBlock.Text = string.Join("، ", modules.Select(module => ModuleDisplayNames.TryGetValue(module, out var display) ? display : module));
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var fullName = FullNameTextBox.Text.Trim();
        var username = UsernameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(fullName))
        {
            ShowValidation("اكتب الاسم الكامل أولًا.");
            return;
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            ShowValidation("اكتب اسم الدخول (Username) أولًا.");
            return;
        }

        if (RoleComboBox.SelectedItem is not UserRoleOption role)
        {
            ShowValidation("اختر دور المستخدم أولًا.");
            return;
        }

        var isNewUser = _source is null || _source.Id == 0;
        var password = PasswordBox.Password;
        var confirmPassword = ConfirmPasswordBox.Password;

        if (isNewUser && string.IsNullOrWhiteSpace(password))
        {
            ShowValidation("كلمة المرور مطلوبة عند إنشاء مستخدم جديد.");
            return;
        }

        if (!string.IsNullOrEmpty(password) && !string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            ShowValidation("تأكيد كلمة المرور غير مطابق.");
            return;
        }

        UserForm = new UserFormDto
        {
            Id = _source?.Id ?? 0,
            FullName = fullName,
            Username = username,
            Email = EmailTextBox.Text.Trim(),
            PhoneNumber = PhoneTextBox.Text.Trim(),
            Role = role.Value,
            IsActive = IsActiveCheckBox.IsChecked == true,
            Password = password,
            ConfirmPassword = string.IsNullOrEmpty(password) ? password : confirmPassword
        };

        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void ShowValidation(string message)
    {
        MessageBox.Show(
            message,
            "تنبيه",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private sealed record UserRoleOption(string Value, string DisplayName);
}
