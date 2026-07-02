using FleetManagementSystem.Core.DTOs;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

namespace FleetManagementSystem.WPF;

public partial class TripEntryWindow : Window
{
    private readonly IReadOnlyList<VehicleDto> _vehicles;
    private readonly IReadOnlyList<EmployeeDto> _supervisors;
    private int _destinationCounter = 1;

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

        var driverOptions = drivers
            .Where(driver => !string.IsNullOrWhiteSpace(driver.FullName))
            .OrderBy(driver => driver.FullName)
            .ToList();

        var activeEmployees = employees
            .Where(IsEmployeeActive)
            .OrderBy(employee => employee.FullName)
            .ToList();

        _supervisors = activeEmployees;

        VehicleComboBox.ItemsSource = _vehicles;
        DriverComboBox.ItemsSource = driverOptions;
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
        var vehicle = GetSelectedVehicle();
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

        var destinations = GetDestinationValues();
        if (destinations.Count == 0)
        {
            ShowValidation("اكتب منطقة وصول واحدة على الأقل.");
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
            EndLocation = BuildDestinationRoute(destinations),
            Purpose = PurposeTextBox.Text.Trim(),
            StartMileage = vehicle.CurrentMileage,
            Status = "Open"
        };

        DialogResult = true;
    }

    private void AddDestinationButton_Click(object sender, RoutedEventArgs e) => AddDestinationRow();

    private void AddDestinationRow(string value = "")
    {
        _destinationCounter++;

        var row = new Grid
        {
            Margin = new Thickness(0, 8, 0, 0)
        };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var destinationTextBox = new TextBox
        {
            Text = value,
            Style = (Style)FindResource("AppTextBoxStyle"),
            ToolTip = "اكتب منطقة وصول إضافية"
        };
        AutomationProperties.SetAutomationId(destinationTextBox, $"TripDestinationTextBox{_destinationCounter}");
        AutomationProperties.SetName(destinationTextBox, $"منطقة وصول {_destinationCounter}");
        Grid.SetColumn(destinationTextBox, 0);

        var removeButton = new Button
        {
            Content = "حذف",
            Height = 36,
            MinWidth = 70,
            Padding = new Thickness(12, 4, 12, 4),
            Margin = new Thickness(8, 0, 0, 0),
            Style = (Style)FindResource("TripPopupSecondaryButtonStyle")
        };
        AutomationProperties.SetAutomationId(removeButton, $"RemoveTripDestinationButton{_destinationCounter}");
        AutomationProperties.SetName(removeButton, $"حذف منطقة وصول {_destinationCounter}");
        removeButton.Click += (_, _) => DestinationsPanel.Children.Remove(row);
        Grid.SetColumn(removeButton, 1);

        row.Children.Add(destinationTextBox);
        row.Children.Add(removeButton);
        DestinationsPanel.Children.Add(row);
        destinationTextBox.Focus();
    }

    private IReadOnlyList<string> GetDestinationValues()
    {
        var values = new List<string>();
        foreach (var child in DestinationsPanel.Children)
        {
            var textBox = child switch
            {
                TextBox directTextBox => directTextBox,
                Grid grid => grid.Children.OfType<TextBox>().FirstOrDefault(),
                _ => null
            };

            var value = textBox?.Text.Trim();
            if (!string.IsNullOrWhiteSpace(value))
            {
                values.Add(value);
            }
        }

        return values;
    }

    private static string BuildDestinationRoute(IReadOnlyList<string> destinations) =>
        string.Join(" ثم ", destinations);

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void UpdateVehicleSummary()
    {
        var vehicle = GetSelectedVehicle();

        VehiclePlateTextBlock.Text = vehicle?.PlateNumber ?? "اختر سيارة";
        VehicleYearTextBlock.Text = vehicle is null || vehicle.Year <= 0
            ? "—"
            : vehicle.Year.ToString(CultureInfo.InvariantCulture);
        VehicleModelTextBlock.Text = vehicle?.Model ?? "—";
        VehicleChassisTextBlock.Text = string.IsNullOrWhiteSpace(vehicle?.ChassisNumber) ? "—" : vehicle.ChassisNumber;
        VehicleEngineTextBlock.Text = string.IsNullOrWhiteSpace(vehicle?.EngineNumber) ? "—" : vehicle.EngineNumber;
        VehicleRegistrationStartTextBlock.Text = FormatDate(vehicle?.RegistrationStartDate);
        VehicleRegistrationExpiryTextBlock.Text = FormatDate(vehicle?.RegistrationExpiryDate);
        VehicleCurrentMileageTextBlock.Text = vehicle is null ? "—" : $"{vehicle.CurrentMileage:0.##} كم";
        VehicleStatusTextBlock.Text = string.IsNullOrWhiteSpace(vehicle?.Status) ? "—" : vehicle.Status;

        if (vehicle is null)
        {
            VehicleInsuranceTextBlock.Text = "—";
            return;
        }

        VehicleInsuranceTextBlock.Text = string.IsNullOrWhiteSpace(vehicle.AccidentInsuranceDetails)
            ? "تأمين حوادث: غير مسجل"
            : $"تأمين حوادث: {vehicle.AccidentInsuranceDetails}";
    }

    private VehicleDto? GetSelectedVehicle()
    {
        if (VehicleComboBox.SelectedItem is VehicleDto selectedVehicle)
        {
            return selectedVehicle;
        }

        if (VehicleComboBox.SelectedValue is int vehicleId)
        {
            return _vehicles.FirstOrDefault(vehicle => vehicle.Id == vehicleId);
        }

        return null;
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
