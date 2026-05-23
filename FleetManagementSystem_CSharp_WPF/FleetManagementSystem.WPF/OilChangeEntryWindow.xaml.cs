using FleetManagementSystem.Core.DTOs;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace FleetManagementSystem.WPF;

public partial class OilChangeEntryWindow : Window
{
    private readonly IReadOnlyList<VehicleDto> _vehicles;
    private readonly int _sourceId;
    private readonly decimal _sourceNextOdometer;

    public OilChangeFormDto? OilChangeForm { get; private set; }

    public OilChangeEntryWindow(IEnumerable<VehicleDto> vehicles, OilChangeDto source)
    {
        InitializeComponent();

        _sourceId = source.Id;
        _sourceNextOdometer = source.NextOilChangeOdometer;
        _vehicles = vehicles
            .OrderBy(vehicle => vehicle.PlateNumber)
            .ToList();

        VehicleComboBox.ItemsSource = _vehicles;
        VehicleComboBox.SelectedValue = source.VehicleId > 0
            ? source.VehicleId
            : _vehicles.FirstOrDefault()?.Id;

        var selectedVehicleId = VehicleComboBox.SelectedValue is int value ? value : 0;
        var selectedVehicle = _vehicles.FirstOrDefault(vehicle => vehicle.Id == selectedVehicleId);
        ChangeDatePicker.SelectedDate = source.ChangeDate == default ? DateTime.Today : source.ChangeDate.Date;
        OdometerTextBox.Text = source.OdometerAtChange > 0
            ? source.OdometerAtChange.ToString("0.##")
            : (selectedVehicle?.CurrentMileage ?? 0).ToString("0.##");
        OilTypeTextBox.Text = source.OilType;
        QuantityTextBox.Text = source.Quantity.ToString("0.##");
        CostTextBox.Text = source.Cost.ToString("0.##");
        NotesTextBox.Text = source.Notes;
        SelectStatus(source.Status);
        UpdateVehicleSummary();
    }

    private void VehicleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateVehicleSummary();

        if (VehicleComboBox.SelectedItem is VehicleDto vehicle && (_sourceId == 0 || IsZeroOrEmpty(OdometerTextBox.Text)))
        {
            OdometerTextBox.Text = vehicle.CurrentMileage.ToString("0.##");
        }

        UpdateNextOilChangePreview();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ValidationTextBlock.Text = string.Empty;

        if (VehicleComboBox.SelectedItem is not VehicleDto vehicle)
        {
            ShowValidation("اختر العربية أولًا.");
            return;
        }

        if (!TryParseDecimal(OdometerTextBox.Text, out var odometer) || odometer < 0)
        {
            ShowValidation("عداد العربية وقت تغيير الزيت يجب أن يكون رقمًا لا يقل عن صفر.");
            return;
        }

        if (string.IsNullOrWhiteSpace(OilTypeTextBox.Text))
        {
            ShowValidation("نوع الزيت مطلوب. اكتب نوع الزيت المستخدم قبل الحفظ.");
            return;
        }

        if (!TryParseDecimal(QuantityTextBox.Text, out var quantity) || quantity <= 0)
        {
            ShowValidation("كمية الزيت لازم تكون أكبر من صفر لتر.");
            return;
        }

        if (!TryParseDecimal(CostTextBox.Text, out var cost) || cost < 0)
        {
            ShowValidation("تكلفة الزيت يجب أن تكون رقمًا لا يقل عن صفر.");
            return;
        }

        var status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Completed";

        OilChangeForm = new OilChangeFormDto
        {
            Id = _sourceId,
            VehicleId = vehicle.Id,
            ChangeDate = ChangeDatePicker.SelectedDate ?? DateTime.Today,
            OdometerAtChange = odometer,
            OilType = OilTypeTextBox.Text.Trim(),
            Quantity = quantity,
            Cost = cost,
            NextOilChangeOdometer = _sourceNextOdometer,
            Status = status,
            Notes = NotesTextBox.Text.Trim()
        };

        DialogResult = true;
    }

    private void UpdateVehicleSummary()
    {
        if (VehicleComboBox.SelectedItem is not VehicleDto vehicle)
        {
            VehicleMileageTextBlock.Text = "—";
            VehicleOilIntervalTextBlock.Text = "—";
            NextOilChangePreviewTextBlock.Text = "—";
            return;
        }

        VehicleMileageTextBlock.Text = $"{vehicle.CurrentMileage:0.##} كم";
        VehicleOilIntervalTextBlock.Text = vehicle.OilChangeIntervalKm > 0
            ? $"{vehicle.OilChangeIntervalKm:0.##} كم"
            : "غير مسجلة";
        UpdateNextOilChangePreview();
    }

    private void OilOdometerTextBox_TextChanged(object sender, TextChangedEventArgs e) => UpdateNextOilChangePreview();

    private void UpdateNextOilChangePreview()
    {
        if (NextOilChangePreviewTextBlock is null)
        {
            return;
        }

        if (VehicleComboBox.SelectedItem is not VehicleDto vehicle || vehicle.OilChangeIntervalKm <= 0)
        {
            NextOilChangePreviewTextBlock.Text = "—";
            return;
        }

        NextOilChangePreviewTextBlock.Text = TryParseDecimal(OdometerTextBox.Text, out var odometer) && odometer >= 0
            ? $"{odometer + vehicle.OilChangeIntervalKm:0.##} كم"
            : "—";
    }

    private void SelectStatus(string? status)
    {
        var normalized = string.IsNullOrWhiteSpace(status) ? "Completed" : status.Trim();
        foreach (ComboBoxItem item in StatusComboBox.Items)
        {
            if (string.Equals(item.Tag?.ToString(), normalized, StringComparison.OrdinalIgnoreCase))
            {
                StatusComboBox.SelectedItem = item;
                return;
            }
        }

        StatusComboBox.SelectedIndex = 0;
    }

    private static bool IsZeroOrEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ||
        (TryParseDecimal(value, out var parsed) && parsed == 0);

    private static bool TryParseDecimal(string value, out decimal result) =>
        decimal.TryParse(value.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out result) ||
        decimal.TryParse(value.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out result);

    private void ShowValidation(string message) => ValidationTextBlock.Text = message;

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
