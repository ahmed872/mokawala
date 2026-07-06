using FleetManagementSystem.Core.DTOs;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace FleetManagementSystem.WPF;

public partial class CustodyEntryWindow : Window
{
    /// <summary>مستخدم من مستخدمي النظام يمكن تسليمه عهدة.</summary>
    public sealed record CustodianOption(int UserId, string Display, string Username);

    private readonly IReadOnlyList<VehicleDto> _vehicles;
    private readonly CustodyDto? _source;

    public CustodyFormDto? CustodyForm { get; private set; }

    public CustodyEntryWindow(
        IEnumerable<VehicleDto> vehicles,
        IEnumerable<CustodianOption> custodianOptions,
        int? preselectedVehicleId = null,
        CustodyDto? source = null)
    {
        InitializeComponent();

        _source = source;
        _vehicles = vehicles
            .OrderBy(vehicle => vehicle.PlateNumber)
            .ToList();

        VehicleComboBox.ItemsSource = _vehicles;
        CustodianNameComboBox.ItemsSource = custodianOptions
            .OrderBy(option => option.Display)
            .ToList();

        if (source is not null)
        {
            Title = "تعديل عهدة";
            CustodyNumberTextBox.Text = source.CustodyNumber;
            CustodianNameComboBox.Text = source.CustodianName;
            CustodianPositionTextBox.Text = source.CustodianPosition;
            HandoverDatePicker.SelectedDate = source.HandoverDate == default ? DateTime.Today : source.HandoverDate;
            ReturnDatePicker.SelectedDate = source.ReturnDate;
            ConditionRatingTextBox.Text = source.VehicleConditionRating <= 0
                ? "5"
                : source.VehicleConditionRating.ToString("0.##", CultureInfo.InvariantCulture);
            AmountTextBox.Text = source.Amount <= 0 ? "0" : source.Amount.ToString("0.##", CultureInfo.InvariantCulture);
            NotesTextBox.Text = source.Notes;
            SelectStatus(source.Status);
            if (source.VehicleId > 0)
            {
                VehicleComboBox.SelectedValue = source.VehicleId;
            }
        }
        else
        {
            CustodyNumberTextBox.Text = $"CU-{DateTime.Now:yyyyMMdd-HHmm}";
            HandoverDatePicker.SelectedDate = DateTime.Today;
            ConditionRatingTextBox.Text = "5";
            AmountTextBox.Text = "0";

            if (preselectedVehicleId.HasValue)
            {
                VehicleComboBox.SelectedValue = preselectedVehicleId.Value;
            }
        }

        UpdateVehicleSummary();
        Loaded += (_, _) => CustodianNameComboBox.Focus();
    }

    private void VehicleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateVehicleSummary();

    private void ClearVehicleButton_Click(object sender, RoutedEventArgs e)
    {
        VehicleComboBox.SelectedItem = null;
        UpdateVehicleSummary();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedUser = CustodianNameComboBox.SelectedItem as CustodianOption;
        var custodianName = selectedUser?.Display ?? CustodianNameComboBox.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(custodianName))
        {
            ShowValidation("اختر المستلم من مستخدمي النظام أو اكتب اسمه أولًا.");
            return;
        }

        if (string.IsNullOrWhiteSpace(CustodyNumberTextBox.Text))
        {
            ShowValidation("اكتب رقم العهدة أولًا.");
            return;
        }

        if (!decimal.TryParse(ConditionRatingTextBox.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var rating))
        {
            rating = 5;
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

        decimal amount = 0;
        if (!string.IsNullOrWhiteSpace(AmountTextBox.Text) &&
            !decimal.TryParse(AmountTextBox.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out amount))
        {
            ShowValidation("اكتب مبلغ العهدة كرقم صحيح مثل 5000.");
            return;
        }

        if (amount < 0)
        {
            ShowValidation("مبلغ العهدة لا يمكن أن يكون سالبًا.");
            return;
        }

        var vehicle = VehicleComboBox.SelectedItem as VehicleDto;

        CustodyForm = new CustodyFormDto
        {
            Id = _source?.Id ?? 0,
            VehicleId = vehicle?.Id ?? 0,
            UserId = selectedUser?.UserId ?? 0,
            CustodyNumber = CustodyNumberTextBox.Text.Trim(),
            CustodianName = custodianName,
            CustodianPosition = CustodianPositionTextBox.Text.Trim(),
            HandoverDate = handoverDate,
            ReturnDate = returnDate,
            Status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Active",
            VehicleConditionRating = rating,
            Amount = amount,
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
        VehicleSummaryBorder.Visibility = vehicle is null ? Visibility.Collapsed : Visibility.Visible;
        VehiclePlateTextBlock.Text = vehicle?.PlateNumber ?? "بدون عربية";
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
