using FleetManagementSystem.Core.DTOs;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace FleetManagementSystem.WPF;

public partial class TripEntryWindow : Window
{
    private readonly IReadOnlyList<VehicleDto> _vehicles;
    private readonly IReadOnlyList<EmployeeDto> _supervisors;

    public TripFormDto? TripForm { get; private set; }

    public TripEntryWindow(
        IEnumerable<VehicleDto> vehicles,
        IEnumerable<DriverDto> drivers,
        IEnumerable<EmployeeDto> employees,
        int? preselectedVehicleId = null)
    {
        InitializeComponent();

        _vehicles = vehicles
            .OrderBy(vehicle => vehicle.PlateNumber)
            .ToList();

        var activeDrivers = drivers
            .Where(driver => driver.IsActive)
            .OrderBy(driver => driver.FullName)
            .ToList();

        var activeEmployees = employees
            .Where(IsEmployeeActive)
            .OrderBy(employee => employee.FullName)
            .ToList();

        _supervisors = activeEmployees;

        VehicleComboBox.ItemsSource = _vehicles;
        DriverComboBox.ItemsSource = activeDrivers;
        SupervisorComboBox.ItemsSource = _supervisors;
        TripDatePicker.SelectedDate = DateTime.Today;
        TripTimeTextBox.Text = DateTime.Now.ToString("HH:mm");

        if (preselectedVehicleId.HasValue)
        {
            VehicleComboBox.SelectedValue = preselectedVehicleId.Value;
        }

        UpdateVehicleSummary();
    }

    private void VehicleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateVehicleSummary();

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var vehicle = VehicleComboBox.SelectedItem as VehicleDto;
        var driver = DriverComboBox.SelectedItem as DriverDto;
        var supervisor = SupervisorComboBox.SelectedItem as EmployeeDto;
        var requesterName = RequesterNameTextBox.Text.Trim();

        if (vehicle is null)
        {
            ShowValidation("اختر رقم السيارة أولًا.");
            return;
        }

        if (driver is null)
        {
            ShowValidation("اختر اسم السائق أولًا.");
            return;
        }

        if (string.IsNullOrWhiteSpace(requesterName))
        {
            ShowValidation("اكتب طالب التشغيل أولًا.");
            return;
        }

        if (supervisor is null)
        {
            ShowValidation("اختر المشرف أولًا.");
            return;
        }

        if (string.IsNullOrWhiteSpace(StartLocationTextBox.Text))
        {
            ShowValidation("اكتب منطقة التحرك.");
            return;
        }

        if (string.IsNullOrWhiteSpace(EndLocationTextBox.Text))
        {
            ShowValidation("اكتب منطقة الوصول.");
            return;
        }

        if (string.IsNullOrWhiteSpace(PurposeTextBox.Text))
        {
            ShowValidation("اكتب الغرض من التشغيل.");
            return;
        }

        var selectedDate = TripDatePicker.SelectedDate ?? DateTime.Today;
        if (!TimeSpan.TryParseExact(
                TripTimeTextBox.Text.Trim(),
                new[] { @"hh\:mm", @"h\:mm" },
                CultureInfo.InvariantCulture,
                out var selectedTime))
        {
            ShowValidation("اكتب الوقت بصيغة صحيحة مثل 14:30.");
            return;
        }

        TripForm = new TripFormDto
        {
            VehicleId = vehicle.Id,
            DriverId = driver.Id,
            RequesterNameText = requesterName,
            SupervisorEmployeeId = supervisor.Id,
            StartDate = selectedDate.Date.Add(selectedTime),
            StartLocation = StartLocationTextBox.Text.Trim(),
            EndLocation = EndLocationTextBox.Text.Trim(),
            Purpose = PurposeTextBox.Text.Trim(),
            StartMileage = vehicle.CurrentMileage,
            Status = "Open"
        };

        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void UpdateVehicleSummary()
    {
        var vehicle = VehicleComboBox.SelectedItem as VehicleDto;

        VehiclePlateTextBlock.Text = vehicle?.PlateNumber ?? "اختر سيارة";
        VehicleTypeTextBlock.Text = vehicle?.VehicleType ?? "—";
        VehicleModelTextBlock.Text = vehicle?.Model ?? "—";
        VehicleChassisTextBlock.Text = string.IsNullOrWhiteSpace(vehicle?.ChassisNumber) ? "—" : vehicle.ChassisNumber;
        VehicleEngineTextBlock.Text = string.IsNullOrWhiteSpace(vehicle?.EngineNumber) ? "—" : vehicle.EngineNumber;
        VehicleRegistrationStartTextBlock.Text = FormatDate(vehicle?.RegistrationStartDate);
        VehicleRegistrationExpiryTextBlock.Text = FormatDate(vehicle?.RegistrationExpiryDate);

        if (vehicle is null)
        {
            VehicleInsuranceTextBlock.Text = "—";
            return;
        }

        var accident = string.IsNullOrWhiteSpace(vehicle.AccidentInsuranceDetails) ? "حوادث: غير مسجل" : $"حوادث: {vehicle.AccidentInsuranceDetails}";
        var social = string.IsNullOrWhiteSpace(vehicle.SocialInsuranceDetails) ? "اجتماعي: غير مسجل" : $"اجتماعي: {vehicle.SocialInsuranceDetails}";
        VehicleInsuranceTextBlock.Text = $"{accident}\n{social}";
    }

    private static bool IsEmployeeActive(EmployeeDto employee)
    {
        var status = employee.Status?.Trim();
        return !string.Equals(status, "Inactive", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(status, "غير نشط", StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatDate(DateTime? value) =>
        value.HasValue ? value.Value.ToString("yyyy-MM-dd") : "—";

    private void ShowValidation(string message)
    {
        MessageBox.Show(
            message,
            "تنبيه",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
