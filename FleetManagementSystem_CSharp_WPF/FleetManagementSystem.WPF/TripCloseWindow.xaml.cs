using FleetManagementSystem.Core.DTOs;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace FleetManagementSystem.WPF;

public partial class TripCloseWindow : Window
{
    private readonly TripDto _trip;
    private readonly VehicleDto? _vehicle;

    public TripFormDto? TripForm { get; private set; }

    public TripCloseWindow(TripDto trip, VehicleDto? vehicle = null)
    {
        InitializeComponent();
        _trip = trip;
        _vehicle = vehicle;

        TripSummaryTextBlock.Text = $"العربية: {ValueOrDash(trip.VehiclePlateNumber)} - السائق: {ValueOrDash(trip.DriverName)} - المسار: {ValueOrDash(trip.StartLocation)} إلى {ValueOrDash(trip.EndLocation)}";
        StartDateTextBox.Text = trip.StartDate.ToString("yyyy-MM-dd HH:mm");
        StartMileageTextBox.Text = trip.StartMileage.ToString("0.##");
        VehicleCurrentMileageTextBox.Text = (_vehicle?.CurrentMileage ?? trip.EndMileage ?? trip.StartMileage).ToString("0.##");

        var endDate = trip.EndDate ?? DateTime.Now;
        EndDatePicker.SelectedDate = endDate.Date;
        EndTimeTextBox.Text = endDate.ToString("HH:mm");
        EndMileageTextBox.Text = trip.EndMileage?.ToString("0.##") ?? trip.StartMileage.ToString("0.##");
        UpdateDistanceFromMileage();
        NotesTextBox.Text = trip.Notes;

        StatusComboBox.SelectedIndex = 0;
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

        if (!TryParseDecimal(StartMileageTextBox.Text, out var startMileage))
        {
            ShowValidation("عداد البداية يجب أن يكون رقمًا صحيحًا.");
            return;
        }

        if (!TryParseDecimal(EndMileageTextBox.Text, out var endMileage))
        {
            ShowValidation("عداد النهاية يجب أن يكون رقمًا صحيحًا.");
            return;
        }

        if (startMileage < 0 || endMileage < 0)
        {
            ShowValidation("قراءات العداد لا يمكن أن تكون سالبة.");
            return;
        }

        if (endMileage < startMileage)
        {
            ShowValidation($"عداد النهاية ({endMileage:0.##}) أقل من عداد البداية ({startMileage:0.##}). اكتب قراءة عداد النهاية كما هي عند رجوع العربية، ولا يمكن أن تكون أقل من بداية التشغيلة.");
            return;
        }

        var status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Closed";
        var endDate = status == "Closed" ? selectedDate.Date.Add(selectedTime) : (DateTime?)null;
        var distance = status == "Closed" ? endMileage - startMileage : 0;
        if (endDate.HasValue && endDate.Value < _trip.StartDate)
        {
            ShowValidation($"تاريخ الإغلاق ({endDate:yyyy-MM-dd HH:mm}) قبل بداية التشغيلة ({_trip.StartDate:yyyy-MM-dd HH:mm}). اختر تاريخ ووقت إغلاق بعد بداية التشغيلة.");
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
            StartMileage = startMileage,
            EndMileage = status == "Closed" ? endMileage : null,
            Distance = distance,
            Purpose = _trip.Purpose,
            Status = status,
            FuelConsumed = _trip.FuelConsumed,
            TripCost = _trip.TripCost,
            Notes = NotesTextBox.Text.Trim()
        };

        DialogResult = true;
    }

    private void MileageTextBox_TextChanged(object sender, TextChangedEventArgs e) => UpdateDistanceFromMileage();

    private void UpdateDistanceFromMileage()
    {
        if (DistanceTextBox is null)
        {
            return;
        }

        DistanceTextBox.Text =
            TryParseDecimal(StartMileageTextBox.Text, out var startMileage) &&
            TryParseDecimal(EndMileageTextBox.Text, out var endMileage) &&
            startMileage >= 0 &&
            endMileage >= startMileage
            ? (endMileage - startMileage).ToString("0.##")
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
