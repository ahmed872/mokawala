using FleetManagementSystem.Core.DTOs;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace FleetManagementSystem.WPF;

public partial class TripCloseWindow : Window
{
    private readonly TripDto _trip;

    public TripFormDto? TripForm { get; private set; }

    public TripCloseWindow(TripDto trip)
    {
        InitializeComponent();
        _trip = trip;

        TripSummaryTextBlock.Text = $"العربية: {ValueOrDash(trip.VehiclePlateNumber)} - السائق: {ValueOrDash(trip.DriverName)} - المسار: {ValueOrDash(trip.StartLocation)} إلى {ValueOrDash(trip.EndLocation)}";
        StartDateTextBox.Text = trip.StartDate.ToString("yyyy-MM-dd HH:mm");
        StartMileageTextBox.Text = trip.StartMileage.ToString("0.##");

        var endDate = trip.EndDate ?? DateTime.Now;
        EndDatePicker.SelectedDate = endDate.Date;
        EndTimeTextBox.Text = endDate.ToString("HH:mm");
        EndMileageTextBox.Text = trip.EndMileage?.ToString("0.##") ?? trip.StartMileage.ToString("0.##");
        UpdateDistanceFromMileage();
        TripCostTextBox.Text = trip.TripCost.ToString("0.##");
        FuelConsumedTextBox.Text = trip.FuelConsumed.ToString("0.##");
        NotesTextBox.Text = trip.Notes;

        StatusComboBox.SelectedIndex = IsClosed(trip) ? 0 : 1;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ValidationTextBlock.Text = string.Empty;

        var selectedDate = EndDatePicker.SelectedDate ?? DateTime.Today;
        if (!TimeSpan.TryParseExact(
                EndTimeTextBox.Text.Trim(),
                new[] { @"hh\:mm", @"h\:mm" },
                CultureInfo.InvariantCulture,
                out var selectedTime))
        {
            ShowValidation("اكتب وقت الإغلاق بصيغة صحيحة مثل 15:30.");
            return;
        }

        if (!TryParseDecimal(EndMileageTextBox.Text, out var endMileage))
        {
            ShowValidation("عداد النهاية يجب أن يكون رقمًا صحيحًا.");
            return;
        }

        if (endMileage < _trip.StartMileage)
        {
            ShowValidation("عداد النهاية لا يمكن أن يكون أقل من عداد البداية.");
            return;
        }

        if (!TryParseDecimal(TripCostTextBox.Text, out var tripCost))
        {
            ShowValidation("تكلفة التشغيلة يجب أن تكون رقمًا.");
            return;
        }

        if (!TryParseDecimal(FuelConsumedTextBox.Text, out var fuelConsumed))
        {
            ShowValidation("الوقود المستهلك يجب أن يكون رقمًا.");
            return;
        }

        if (tripCost < 0 || fuelConsumed < 0)
        {
            ShowValidation("التكلفة والوقود لا يمكن أن تكون قيمًا سالبة.");
            return;
        }

        var status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Closed";
        var endDate = status == "Closed" ? selectedDate.Date.Add(selectedTime) : (DateTime?)null;
        var distance = status == "Closed" ? endMileage - _trip.StartMileage : 0;
        if (endDate.HasValue && endDate.Value < _trip.StartDate)
        {
            ShowValidation("تاريخ الإغلاق لا يمكن أن يكون قبل بداية التشغيلة.");
            return;
        }

        TripForm = new TripFormDto
        {
            Id = _trip.Id,
            VehicleId = _trip.VehicleId,
            DriverId = _trip.DriverId,
            RequesterEmployeeId = _trip.RequesterEmployeeId,
            RequesterNameText = string.IsNullOrWhiteSpace(_trip.RequesterNameText) ? _trip.RequesterName : _trip.RequesterNameText,
            SupervisorEmployeeId = _trip.SupervisorEmployeeId,
            StartDate = _trip.StartDate,
            EndDate = endDate,
            StartLocation = _trip.StartLocation,
            EndLocation = _trip.EndLocation,
            StartMileage = _trip.StartMileage,
            EndMileage = status == "Closed" ? endMileage : null,
            Distance = distance,
            Purpose = _trip.Purpose,
            Status = status,
            FuelConsumed = fuelConsumed,
            TripCost = tripCost,
            Notes = NotesTextBox.Text.Trim()
        };

        DialogResult = true;
    }

    private void EndMileageTextBox_TextChanged(object sender, TextChangedEventArgs e) => UpdateDistanceFromMileage();

    private void UpdateDistanceFromMileage()
    {
        if (DistanceTextBox is null)
        {
            return;
        }

        DistanceTextBox.Text = TryParseDecimal(EndMileageTextBox.Text, out var endMileage) && endMileage >= _trip.StartMileage
            ? (endMileage - _trip.StartMileage).ToString("0.##")
            : "0";
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private static bool IsClosed(TripDto trip) =>
        trip.EndDate.HasValue ||
        string.Equals(trip.Status, "Closed", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(trip.Status, "مغلقة", StringComparison.OrdinalIgnoreCase);

    private static bool TryParseDecimal(string value, out decimal result) =>
        decimal.TryParse(value.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out result) ||
        decimal.TryParse(value.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out result);

    private void ShowValidation(string message) => ValidationTextBlock.Text = message;

    private static string ValueOrDash(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
}
