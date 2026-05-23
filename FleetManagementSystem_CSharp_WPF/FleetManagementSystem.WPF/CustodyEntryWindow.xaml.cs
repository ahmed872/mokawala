using FleetManagementSystem.Core.DTOs;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace FleetManagementSystem.WPF;

public partial class CustodyEntryWindow : Window
{
    private readonly IReadOnlyList<VehicleDto> _vehicles;
    private readonly CustodyDto? _source;

    public CustodyFormDto? CustodyForm { get; private set; }

    public CustodyEntryWindow(IEnumerable<VehicleDto> vehicles, int? preselectedVehicleId = null, CustodyDto? source = null)
    {
        InitializeComponent();

        _source = source;
        _vehicles = vehicles
            .OrderBy(vehicle => vehicle.PlateNumber)
            .ToList();

        VehicleComboBox.ItemsSource = _vehicles;

        if (source is not null)
        {
            Title = "تعديل عهدة";
            CustodyNumberTextBox.Text = source.CustodyNumber;
            CustodianNameTextBox.Text = source.CustodianName;
            CustodianPositionTextBox.Text = source.CustodianPosition;
            HandoverDatePicker.SelectedDate = source.HandoverDate == default ? DateTime.Today : source.HandoverDate;
            ReturnDatePicker.SelectedDate = source.ReturnDate;
            ConditionRatingTextBox.Text = source.VehicleConditionRating <= 0
                ? "5"
                : source.VehicleConditionRating.ToString("0.##", CultureInfo.InvariantCulture);
            NotesTextBox.Text = source.Notes;
            SelectStatus(source.Status);
            VehicleComboBox.SelectedValue = source.VehicleId;
        }
        else
        {
            CustodyNumberTextBox.Text = $"CU-{DateTime.Now:yyyyMMdd-HHmm}";
            HandoverDatePicker.SelectedDate = DateTime.Today;
            ConditionRatingTextBox.Text = "5";
        }

        if (source is null && preselectedVehicleId.HasValue)
        {
            VehicleComboBox.SelectedValue = preselectedVehicleId.Value;
        }
        else if (source is null && _vehicles.Count > 0)
        {
            VehicleComboBox.SelectedItem = _vehicles[0];
        }

        UpdateVehicleSummary();
    }

    private void VehicleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateVehicleSummary();

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var vehicle = VehicleComboBox.SelectedItem as VehicleDto;
        if (vehicle is null)
        {
            ShowValidation("اختر رقم السيارة أولًا.");
            return;
        }

        if (string.IsNullOrWhiteSpace(CustodyNumberTextBox.Text))
        {
            ShowValidation("اكتب رقم العهدة أولًا.");
            return;
        }

        if (string.IsNullOrWhiteSpace(CustodianNameTextBox.Text))
        {
            ShowValidation("اكتب اسم المستلم أولًا.");
            return;
        }

        if (!decimal.TryParse(ConditionRatingTextBox.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var rating))
        {
            ShowValidation("تقييم الحالة يجب أن يكون رقمًا صحيحًا مثل 5.");
            return;
        }

        if (rating is < 1 or > 10)
        {
            ShowValidation("تقييم الحالة يجب أن يكون من 1 إلى 10.");
            return;
        }

        var handoverDate = HandoverDatePicker.SelectedDate ?? DateTime.Today;
        var returnDate = ReturnDatePicker.SelectedDate;
        if (returnDate.HasValue && returnDate.Value.Date < handoverDate.Date)
        {
            ShowValidation("تاريخ الإرجاع يجب أن يكون بعد تاريخ التسليم.");
            return;
        }

        CustodyForm = new CustodyFormDto
        {
            Id = _source?.Id ?? 0,
            VehicleId = vehicle.Id,
            CustodyNumber = CustodyNumberTextBox.Text.Trim(),
            CustodianName = CustodianNameTextBox.Text.Trim(),
            CustodianPosition = CustodianPositionTextBox.Text.Trim(),
            HandoverDate = handoverDate,
            ReturnDate = returnDate,
            Status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Active",
            VehicleConditionRating = rating,
            Notes = NotesTextBox.Text.Trim()
        };

        DialogResult = true;
    }

    private void SelectStatus(string? status)
    {
        foreach (var item in StatusComboBox.Items.OfType<ComboBoxItem>())
        {
            if (string.Equals(item.Tag?.ToString(), status, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(item.Content?.ToString(), status, StringComparison.OrdinalIgnoreCase))
            {
                StatusComboBox.SelectedItem = item;
                return;
            }
        }

        StatusComboBox.SelectedIndex = 0;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void UpdateVehicleSummary()
    {
        var vehicle = VehicleComboBox.SelectedItem as VehicleDto;
        VehiclePlateTextBlock.Text = vehicle?.PlateNumber ?? "اختر سيارة";
        VehicleTypeTextBlock.Text = string.IsNullOrWhiteSpace(vehicle?.VehicleType) ? "-" : vehicle.VehicleType;
        VehicleModelTextBlock.Text = string.IsNullOrWhiteSpace(vehicle?.Model) ? "-" : vehicle.Model;
        VehicleYearTextBlock.Text = vehicle is null || vehicle.Year <= 0 ? "-" : vehicle.Year.ToString(CultureInfo.InvariantCulture);
        VehicleChassisTextBlock.Text = string.IsNullOrWhiteSpace(vehicle?.ChassisNumber) ? "-" : vehicle.ChassisNumber;
        VehicleEngineTextBlock.Text = string.IsNullOrWhiteSpace(vehicle?.EngineNumber) ? "-" : vehicle.EngineNumber;
        VehicleStatusTextBlock.Text = string.IsNullOrWhiteSpace(vehicle?.Status) ? "-" : vehicle.Status;
    }

    private void ShowValidation(string message)
    {
        MessageBox.Show(
            message,
            "تنبيه",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
