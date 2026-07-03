using FleetManagementSystem.Core.DTOs;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace FleetManagementSystem.WPF;

public partial class FuelEntryWindow : Window
{
    private readonly IReadOnlyList<VehicleDto> _vehicles;
    private readonly IReadOnlyList<TripDto> _trips;
    private readonly int _sourceId;
    private bool _updatingCost;
    private bool _refreshingTrips;

    public FuelTransactionFormDto? FuelForm { get; private set; }

    public FuelEntryWindow(
        IEnumerable<VehicleDto> vehicles,
        IEnumerable<TripDto> trips,
        FuelTransactionDto source)
    {
        InitializeComponent();
        _sourceId = source.Id;

        _vehicles = vehicles
            .OrderBy(vehicle => vehicle.PlateNumber)
            .ToList();
        _trips = trips
            .OrderByDescending(trip => trip.StartDate)
            .ToList();

        VehicleComboBox.ItemsSource = _vehicles;

        var sourceTripVehicleId = source.TripId.HasValue
            ? _trips.FirstOrDefault(trip => trip.Id == source.TripId.Value)?.VehicleId
            : null;
        var initialVehicleId = source.VehicleId > 0
            ? source.VehicleId
            : sourceTripVehicleId ?? _vehicles.FirstOrDefault()?.Id;

        VehicleComboBox.SelectedValue = initialVehicleId;
        RefreshTripOptions(source.TripId);

        TransactionDatePicker.SelectedDate = source.TransactionDate == default ? DateTime.Today : source.TransactionDate.Date;
        FuelTypeTextBox.Text = string.IsNullOrWhiteSpace(source.FuelType) ? "بنزين" : source.FuelType;
        _updatingCost = true;
        QuantityTextBox.Text = source.Quantity.ToString("0.##");
        UnitPriceTextBox.Text = source.UnitPrice.ToString("0.##");
        TotalCostTextBox.Text = source.TotalCost.ToString("0.##");
        _updatingCost = false;
        FuelStationTextBox.Text = source.FuelStation;
        OdometerTextBox.Text = source.Odometer > 0
            ? source.Odometer.ToString("0.##")
            : (_vehicles.FirstOrDefault(vehicle => vehicle.Id == initialVehicleId)?.CurrentMileage ?? 0).ToString("0.##");
        PaidFromTreasuryCheckBox.IsChecked = source.PaidFromTreasury;
        NotesTextBox.Text = source.Notes;
        UpdateVehicleMileageSummary();
    }

    private static IReadOnlyList<TripOption> BuildTripOptions(IEnumerable<TripDto> trips)
    {
        var options = new List<TripOption>
        {
            new(null, 0, "بدون تشغيلة")
        };

        options.AddRange(trips.Select(trip => new TripOption(
            trip.Id,
            trip.VehicleId,
            $"{trip.Id} - {trip.VehiclePlateNumber} - {trip.StartDate:yyyy-MM-dd HH:mm}")));

        return options;
    }

    private void VehicleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_refreshingTrips)
        {
            return;
        }

        var selectedTripId = (TripComboBox.SelectedItem as TripOption)?.Id;
        RefreshTripOptions(selectedTripId);

        UpdateVehicleMileageSummary();

        if (VehicleComboBox.SelectedItem is VehicleDto vehicle && (_sourceId == 0 || IsZeroOrEmpty(OdometerTextBox.Text)))
        {
            OdometerTextBox.Text = vehicle.CurrentMileage.ToString("0.##");
        }
    }

    private void RefreshTripOptions(int? preferredTripId = null)
    {
        var vehicleId = VehicleComboBox.SelectedItem is VehicleDto vehicle ? vehicle.Id : 0;
        var options = BuildTripOptions(_trips.Where(trip => vehicleId == 0 || trip.VehicleId == vehicleId));

        _refreshingTrips = true;
        TripComboBox.ItemsSource = options;
        TripComboBox.SelectedItem = preferredTripId.HasValue
            ? options.FirstOrDefault(option => option.Id == preferredTripId.Value) ?? options[0]
            : options[0];
        _refreshingTrips = false;
    }

    private void TripComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_refreshingTrips)
        {
            return;
        }

        if (TripComboBox.SelectedItem is not TripOption { Id: not null } option)
        {
            return;
        }

        if (VehicleComboBox.SelectedValue is int selectedVehicleId && selectedVehicleId != option.VehicleId)
        {
            VehicleComboBox.SelectedValue = option.VehicleId;
        }

        var trip = _trips.FirstOrDefault(item => item.Id == option.Id.Value);
        if (trip is null)
        {
            return;
        }

        TransactionDatePicker.SelectedDate = (trip.EndDate ?? trip.StartDate).Date;

        if (_sourceId == 0 || IsZeroOrEmpty(OdometerTextBox.Text))
        {
            OdometerTextBox.Text = (trip.EndMileage ?? trip.StartMileage).ToString("0.##");
        }
    }

    private void FuelCostInput_Changed(object sender, TextChangedEventArgs e)
    {
        if (_updatingCost || TotalCostTextBox is null)
        {
            return;
        }

        var hasQuantity = TryParseDecimal(QuantityTextBox.Text, out var quantity);
        var hasUnitPrice = TryParseDecimal(UnitPriceTextBox.Text, out var unitPrice);
        var hasTotalCost = TryParseDecimal(TotalCostTextBox.Text, out var totalCost);

        _updatingCost = true;
        try
        {
            if (ReferenceEquals(sender, TotalCostTextBox))
            {
                if (hasTotalCost && totalCost > 0 && hasUnitPrice && unitPrice > 0)
                {
                    SetTextIfChanged(QuantityTextBox, totalCost / unitPrice);
                }

                return;
            }

            if (hasQuantity && quantity > 0 && hasUnitPrice && unitPrice > 0)
            {
                SetTextIfChanged(TotalCostTextBox, quantity * unitPrice);
                return;
            }

            if (ReferenceEquals(sender, UnitPriceTextBox) &&
                IsZeroOrEmpty(QuantityTextBox.Text) &&
                hasTotalCost &&
                totalCost > 0 &&
                hasUnitPrice &&
                unitPrice > 0)
            {
                SetTextIfChanged(QuantityTextBox, totalCost / unitPrice);
            }
        }
        finally
        {
            _updatingCost = false;
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ValidationTextBlock.Text = string.Empty;

        if (VehicleComboBox.SelectedItem is not VehicleDto vehicle)
        {
            ShowValidation("اختر العربية أولًا.");
            return;
        }

        var tripOption = TripComboBox.SelectedItem as TripOption;
        if (tripOption?.Id is int tripId && tripOption.VehicleId != vehicle.Id)
        {
            ShowValidation("التشغيلة المختارة مرتبطة بعربية مختلفة. اختر نفس عربية التشغيلة.");
            return;
        }

        if (!TryParseDecimal(QuantityTextBox.Text, out var quantity) || quantity <= 0)
        {
            ShowValidation("عدد اللترات لازم يكون رقم أكبر من صفر. مثال: 50 لتر.");
            return;
        }

        if (!TryParseDecimal(UnitPriceTextBox.Text, out var unitPrice) || unitPrice < 0)
        {
            ShowValidation("سعر اللتر لا يمكن أن يكون سالبًا. اكتب سعر اللتر الفعلي أو صفر لو غير معروف.");
            return;
        }

        if (!TryParseDecimal(TotalCostTextBox.Text, out var totalCost) || totalCost < 0)
        {
            ShowValidation("إجمالي التكلفة لا يمكن أن يكون سالبًا. اتركه صفر أو اكتب عدد اللترات وسعر اللتر ليتم حسابه تلقائيًا.");
            return;
        }

        if (!TryParseDecimal(OdometerTextBox.Text, out var odometer) || odometer < 0)
        {
            ShowValidation("عداد العربية وقت التموين يجب أن يكون رقمًا لا يقل عن صفر.");
            return;
        }

        FuelForm = new FuelTransactionFormDto
        {
            Id = _sourceId,
            VehicleId = vehicle.Id,
            TripId = tripOption?.Id,
            TransactionDate = TransactionDatePicker.SelectedDate ?? DateTime.Today,
            FuelType = string.IsNullOrWhiteSpace(FuelTypeTextBox.Text) ? "بنزين" : FuelTypeTextBox.Text.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice,
            TotalCost = totalCost > 0 ? totalCost : quantity * unitPrice,
            FuelStation = FuelStationTextBox.Text.Trim(),
            Odometer = odometer,
            PaidFromTreasury = PaidFromTreasuryCheckBox.IsChecked == true,
            Notes = NotesTextBox.Text.Trim()
        };

        DialogResult = true;
    }

    private static void SetTextIfChanged(TextBox textBox, decimal value)
    {
        var formatted = value.ToString("0.##");
        if (!string.Equals(textBox.Text, formatted, StringComparison.Ordinal))
        {
            textBox.Text = formatted;
        }
    }

    private static bool IsZeroOrEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ||
        (TryParseDecimal(value, out var parsed) && parsed == 0);

    private void UpdateVehicleMileageSummary()
    {
        if (FuelVehicleMileageTextBlock is null)
        {
            return;
        }

        FuelVehicleMileageTextBlock.Text = VehicleComboBox.SelectedItem is VehicleDto vehicle
            ? $"عداد العربية الحالي: {vehicle.CurrentMileage:0.##} كم"
            : "عداد العربية الحالي: —";
    }

    private static bool TryParseDecimal(string value, out decimal result) =>
        decimal.TryParse(value.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out result) ||
        decimal.TryParse(value.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out result);

    private void ShowValidation(string message) => ValidationTextBlock.Text = message;

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private sealed record TripOption(int? Id, int VehicleId, string Label);
}
