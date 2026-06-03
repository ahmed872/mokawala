using FleetManagementSystem.Core.DTOs;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace FleetManagementSystem.WPF;

public partial class OilChangeEntryWindow : Window
{
    private readonly IReadOnlyList<VehicleDto> _vehicles;
    private readonly IReadOnlyList<OilChangeDto> _existingOilChanges;
    private readonly int _sourceId;

    public OilChangeFormDto? OilChangeForm { get; private set; }

    public OilChangeEntryWindow(
        IEnumerable<VehicleDto> vehicles,
        OilChangeDto source,
        IEnumerable<OilChangeDto>? existingOilChanges = null)
    {
        InitializeComponent();

        _sourceId = source.Id;
        _vehicles = vehicles
            .OrderBy(vehicle => vehicle.PlateNumber)
            .ToList();
        _existingOilChanges = (existingOilChanges ?? Enumerable.Empty<OilChangeDto>()).ToList();

        VehicleComboBox.ItemsSource = _vehicles;
        VehicleComboBox.SelectedValue = source.VehicleId > 0
            ? source.VehicleId
            : _vehicles.FirstOrDefault()?.Id;

        var selectedVehicleId = VehicleComboBox.SelectedValue is int value ? value : 0;
        var selectedVehicle = _vehicles.FirstOrDefault(vehicle => vehicle.Id == selectedVehicleId);
        var isOilChanged = source.IsOilChanged || source.Id == 0 && !HasLatestOilChange(selectedVehicle?.Id ?? 0);

        ChangeDatePicker.SelectedDate = source.Id == 0 ? DateTime.Today : source.CurrentOdometerDate?.Date ?? source.ChangeDate.Date;
        CurrentOdometerTextBox.Text = source.CurrentOdometer > 0
            ? source.CurrentOdometer.ToString("0.##")
            : (selectedVehicle?.CurrentMileage ?? source.CurrentVehicleMileage).ToString("0.##");
        OdometerTextBox.Text = source.OdometerAtChange > 0
            ? source.OdometerAtChange.ToString("0.##")
            : (selectedVehicle?.CurrentMileage ?? 0).ToString("0.##");
        OilTypeTextBox.Text = source.IsOilChanged ? source.OilType : string.Empty;
        QuantityTextBox.Text = source.IsOilChanged ? source.Quantity.ToString("0.##") : "0";
        CostTextBox.Text = source.IsOilChanged ? source.Cost.ToString("0.##") : "0";
        NotesTextBox.Text = source.Notes;
        SelectServiceItems(source.ServiceItems);
        SelectEntryMode(isOilChanged);

        if (source.Id == 0 && !isOilChanged)
        {
            ApplyLatestOilChangeToChangeFields(selectedVehicle);
        }

        UpdateChangeFieldsEnabled();
        UpdateVehicleSummary();
    }

    private void VehicleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateVehicleSummary();

        if (VehicleComboBox.SelectedItem is not VehicleDto vehicle)
        {
            return;
        }

        if (_sourceId == 0)
        {
            CurrentOdometerTextBox.Text = vehicle.CurrentMileage.ToString("0.##");
            ChangeDatePicker.SelectedDate = DateTime.Today;

            if (HasLatestOilChange(vehicle.Id))
            {
                SelectEntryMode(false);
                ApplyLatestOilChangeToChangeFields(vehicle);
            }
            else
            {
                SelectEntryMode(true);
                OdometerTextBox.Text = vehicle.CurrentMileage.ToString("0.##");
            }
        }

        UpdateChangeFieldsEnabled();
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

        var isOilChanged = IsOilChangeMode();
        var serviceItems = GetSelectedServiceItems();
        if (!isOilChanged && GetLatestOilChange(vehicle.Id) is null)
        {
            ShowValidation("لا يمكن تسجيل متابعة يومية قبل تسجيل أول تغيير زيت فعلي لهذه العربية.");
            return;
        }

        if (!TryParseDecimal(OdometerTextBox.Text, out var odometer) || odometer < 0)
        {
            ShowValidation(isOilChanged
                ? "عداد العربية وقت تغيير الزيت يجب أن يكون رقمًا لا يقل عن صفر."
                : "لا يوجد آخر عداد تغيير زيت فعلي لهذه العربية. اختر نوع التسجيل: تغيير زيت، وسجل أول تغيير.");
            return;
        }

        decimal currentOdometer;
        var recordDate = isOilChanged || _sourceId != 0
            ? ChangeDatePicker.SelectedDate ?? DateTime.Today
            : DateTime.Today;

        decimal quantity = 0;
        decimal cost = 0;
        var oilType = string.Empty;

        if (isOilChanged)
        {
            currentOdometer = odometer;
            oilType = OilTypeTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(oilType))
            {
                ShowValidation("نوع الزيت مطلوب عند تسجيل تغيير زيت فعلي.");
                return;
            }

            if (!TryParseDecimal(QuantityTextBox.Text, out quantity) || quantity <= 0)
            {
                ShowValidation("كمية الزيت لازم تكون أكبر من صفر لتر عند تسجيل تغيير زيت فعلي.");
                return;
            }

            if (!TryParseDecimal(CostTextBox.Text, out cost) || cost < 0)
            {
                ShowValidation("تكلفة الزيت يجب أن تكون رقمًا لا يقل عن صفر.");
                return;
            }
        }
        else
        {
            if (!TryParseDecimal(CurrentOdometerTextBox.Text, out currentOdometer) || currentOdometer < 0)
            {
                ShowValidation("قراءة العداد اليوم يجب أن تكون رقمًا لا يقل عن صفر.");
                return;
            }
        }

        OilChangeForm = new OilChangeFormDto
        {
            Id = _sourceId,
            VehicleId = vehicle.Id,
            ChangeDate = recordDate,
            OdometerAtChange = odometer,
            CurrentOdometer = currentOdometer,
            CurrentOdometerDate = recordDate,
            IsOilChanged = isOilChanged,
            ServiceItems = serviceItems,
            OilType = oilType,
            Quantity = quantity,
            Cost = cost,
            Status = isOilChanged ? "Completed" : "DailyCheck",
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
            UpdateOilStatusPreview();
            return;
        }

        VehicleMileageTextBlock.Text = $"{vehicle.CurrentMileage:0.##} كم";
        VehicleOilIntervalTextBlock.Text = vehicle.OilChangeIntervalKm > 0
            ? $"{vehicle.OilChangeIntervalKm:0.##} كم"
            : "غير مسجلة";
        UpdateNextOilChangePreview();
        UpdateOilStatusPreview();
    }

    private void OilOdometerTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateNextOilChangePreview();
        UpdateOilStatusPreview();
    }

    private void OilEntryModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (VehicleComboBox.SelectedItem is VehicleDto vehicle && !IsOilChangeMode())
        {
            ApplyLatestOilChangeToChangeFields(vehicle);
        }

        UpdateChangeFieldsEnabled();
        UpdateNextOilChangePreview();
        UpdateOilStatusPreview();
    }

    private void UpdateChangeFieldsEnabled()
    {
        var enabled = IsOilChangeMode();
        ChangeDetailsPanel.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
        CurrentOdometerPanel.Visibility = enabled ? Visibility.Collapsed : Visibility.Visible;
        ChangeDatePicker.IsEnabled = enabled || _sourceId != 0;

        if (!enabled && _sourceId == 0)
        {
            ChangeDatePicker.SelectedDate = DateTime.Today;
        }

        if (enabled && TryParseDecimal(OdometerTextBox.Text, out var odometer))
        {
            CurrentOdometerTextBox.Text = odometer.ToString("0.##");
        }

        if (enabled && ServiceItemsComboBox.SelectedItem is null)
        {
            SelectServiceItems("زيت فقط");
        }
    }

    private void SelectEntryMode(bool isOilChanged)
    {
        if (OilEntryModeComboBox is null)
        {
            return;
        }

        var targetTag = isOilChanged ? "OilChange" : "DailyCheck";
        foreach (ComboBoxItem item in OilEntryModeComboBox.Items)
        {
            if (string.Equals(item.Tag?.ToString(), targetTag, StringComparison.OrdinalIgnoreCase))
            {
                OilEntryModeComboBox.SelectedItem = item;
                return;
            }
        }

        OilEntryModeComboBox.SelectedIndex = isOilChanged ? 1 : 0;
    }

    private bool IsOilChangeMode() =>
        (OilEntryModeComboBox?.SelectedItem as ComboBoxItem)?.Tag?.ToString() == "OilChange";

    private void SelectServiceItems(string? value)
    {
        var normalized = NormalizeServiceItems(value);
        foreach (ComboBoxItem item in ServiceItemsComboBox.Items)
        {
            if (string.Equals(item.Tag?.ToString(), normalized, StringComparison.OrdinalIgnoreCase))
            {
                ServiceItemsComboBox.SelectedItem = item;
                return;
            }
        }

        ServiceItemsComboBox.SelectedIndex = 0;
    }

    private string GetSelectedServiceItems() =>
        (ServiceItemsComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "زيت فقط";

    private static string NormalizeServiceItems(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "زيت فقط";
        }

        return value.Contains("فلتر", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("filter", StringComparison.OrdinalIgnoreCase)
            ? "زيت وفلتر"
            : "زيت فقط";
    }

    private void ApplyLatestOilChangeToChangeFields(VehicleDto? vehicle)
    {
        var latest = vehicle is null ? null : GetLatestOilChange(vehicle.Id);
        if (latest is null)
        {
            return;
        }

        OdometerTextBox.Text = latest.OdometerAtChange.ToString("0.##");
        OilTypeTextBox.Text = string.Empty;
        QuantityTextBox.Text = "0";
        CostTextBox.Text = "0";
    }

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

        if (TryParseDecimal(OdometerTextBox.Text, out var odometer) && odometer >= 0)
        {
            NextOilChangePreviewTextBlock.Text = $"{odometer + vehicle.OilChangeIntervalKm:0.##} كم";
            return;
        }

        NextOilChangePreviewTextBlock.Text = "—";
    }

    private void UpdateOilStatusPreview()
    {
        if (OilStatusTextBlock is null)
        {
            return;
        }

        if (VehicleComboBox.SelectedItem is not VehicleDto vehicle)
        {
            OilStatusTextBlock.Text = "اختر العربية لعرض حالة الزيت.";
            return;
        }

        if (vehicle.OilChangeIntervalKm <= 0)
        {
            OilStatusTextBlock.Text = "قيمة تغيير الزيت كل كام كم غير مسجلة لهذه العربية.";
            return;
        }

        var isOilChanged = IsOilChangeMode();
        if (isOilChanged)
        {
            if (!TryParseDecimal(OdometerTextBox.Text, out var changeOdometer) || changeOdometer < 0)
            {
                OilStatusTextBlock.Text = "اكتب عداد تغيير الزيت لعرض الحالة.";
                return;
            }

            var nextOdometer = changeOdometer + vehicle.OilChangeIntervalKm;
            OilStatusTextBlock.Text = $"بعد الحفظ: التغيير القادم عند {nextOdometer:0.##} كم. المتبقي من نقطة التغيير {vehicle.OilChangeIntervalKm:0.##} كم.";
            return;
        }

        var latestOilChange = GetLatestOilChange(vehicle.Id);
        if (latestOilChange is null)
        {
            OilStatusTextBlock.Text = "لا يوجد تغيير زيت فعلي مسجل لهذه العربية. سجل أول تغيير زيت أولًا.";
            return;
        }

        if (!TryParseDecimal(CurrentOdometerTextBox.Text, out var currentOdometer) || currentOdometer < 0)
        {
            OilStatusTextBlock.Text = "اكتب قراءة العداد اليوم لعرض حالة الزيت.";
            return;
        }

        var nextOilOdometer = latestOilChange.OdometerAtChange + vehicle.OilChangeIntervalKm;
        var remainingKm = nextOilOdometer - currentOdometer;
        OilStatusTextBlock.Text = $"{BuildOilAlertText(remainingKm)}. التغيير القادم عند {nextOilOdometer:0.##} كم.";
    }

    private const decimal OilAlertThresholdKm = 500;

    private static string BuildOilAlertText(decimal remainingKm)
    {
        if (remainingKm < 0)
        {
            return $"تغيير الزيت متأخر بـ {Math.Abs(remainingKm):0} كم";
        }

        return remainingKm <= OilAlertThresholdKm
            ? $"متبقي {remainingKm:0} كم على تغيير الزيت"
            : $"حالة الزيت سليمة، متبقي {remainingKm:0} كم";
    }

    private OilChangeDto? GetLatestOilChange(int vehicleId) =>
        _existingOilChanges
            .Where(oil => oil.VehicleId == vehicleId && oil.Id != _sourceId && oil.IsOilChanged)
            .OrderByDescending(oil => oil.OdometerAtChange)
            .ThenByDescending(oil => oil.ChangeDate)
            .FirstOrDefault();

    private bool HasLatestOilChange(int vehicleId) => GetLatestOilChange(vehicleId) is not null;

    private static bool TryParseDecimal(string value, out decimal result) =>
        decimal.TryParse(value.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out result) ||
        decimal.TryParse(value.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out result);

    private void ShowValidation(string message) => ValidationTextBlock.Text = message;

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
