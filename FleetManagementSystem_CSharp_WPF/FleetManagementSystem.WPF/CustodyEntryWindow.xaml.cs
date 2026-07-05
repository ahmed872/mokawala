using FleetManagementSystem.Core.DTOs;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace FleetManagementSystem.WPF;

public partial class CustodyEntryWindow : Window
{
    private readonly IReadOnlyList<DriverDto> _drivers;
    private readonly IReadOnlyList<EmployeeDto> _employees;
    private readonly CustodyDto? _source;

    public CustodyFormDto? CustodyForm { get; private set; }

    public CustodyEntryWindow(IEnumerable<DriverDto> drivers, IEnumerable<EmployeeDto> employees, CustodyDto? source = null)
    {
        InitializeComponent();

        _source = source;
        _drivers = drivers
            .OrderBy(driver => driver.FullName)
            .ToList();
        _employees = employees
            .OrderBy(employee => employee.FullName)
            .ToList();

        if (source is not null)
        {
            Title = "تعديل عهدة";
            SelectCustodianType(source.DriverId.HasValue ? "Driver" : "Employee");
            RefreshCustodianItems();
            CustodianComboBox.SelectedValue = source.DriverId ?? source.EmployeeId;
            CustodyNumberTextBox.Text = source.CustodyNumber;
            AmountTextBox.Text = source.Amount <= 0 ? "0" : source.Amount.ToString("0.##", CultureInfo.InvariantCulture);
            HandoverDatePicker.SelectedDate = source.HandoverDate == default ? DateTime.Today : source.HandoverDate;
            ReturnDatePicker.SelectedDate = source.ReturnDate;
            PaidFromTreasuryCheckBox.IsChecked = source.PaidFromTreasury;
            NotesTextBox.Text = source.Notes;
            SelectStatus(source.Status);

            SettledSummaryTextBlock.Text = source.SettledAmount.ToString("N2", CultureInfo.InvariantCulture);
            RemainingSummaryTextBlock.Text = source.RemainingBalance.ToString("N2", CultureInfo.InvariantCulture);
            EditBalancePanel.Visibility = Visibility.Visible;
        }
        else
        {
            RefreshCustodianItems();
            CustodyNumberTextBox.Text = $"CU-{DateTime.Now:yyyyMMdd-HHmm}";
            HandoverDatePicker.SelectedDate = DateTime.Today;
            AmountTextBox.Text = "0";
            if (CustodianComboBox.Items.Count > 0)
            {
                CustodianComboBox.SelectedIndex = 0;
            }
        }

        UpdateCustodianSummary();
    }

    private bool IsDriverSelected =>
        string.Equals(
            (CustodianTypeComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString(),
            "Driver",
            StringComparison.OrdinalIgnoreCase);

    private void CustodianTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded && CustodianComboBox is null)
        {
            return;
        }

        RefreshCustodianItems();
        if (CustodianComboBox.Items.Count > 0)
        {
            CustodianComboBox.SelectedIndex = 0;
        }

        UpdateCustodianSummary();
    }

    private void CustodianComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateCustodianSummary();

    private void RefreshCustodianItems()
    {
        if (CustodianComboBox is null)
        {
            return;
        }

        CustodianComboBox.ItemsSource = IsDriverSelected ? _drivers : _employees;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        int? driverId = null;
        int? employeeId = null;

        if (IsDriverSelected)
        {
            driverId = (CustodianComboBox.SelectedItem as DriverDto)?.Id;
        }
        else
        {
            employeeId = (CustodianComboBox.SelectedItem as EmployeeDto)?.Id;
        }

        if (driverId is null && employeeId is null)
        {
            ShowValidation("اختر أمين العهدة (سائق أو موظف) أولًا.");
            return;
        }

        if (string.IsNullOrWhiteSpace(CustodyNumberTextBox.Text))
        {
            ShowValidation("اكتب رقم العهدة أولًا.");
            return;
        }

        if (!decimal.TryParse(AmountTextBox.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        {
            ShowValidation("مبلغ العهدة يجب أن يكون رقمًا أكبر من صفر.");
            return;
        }

        if (_source is not null && amount < _source.SettledAmount)
        {
            ShowValidation($"لا يمكن تخفيض مبلغ العهدة عن إجمالي التسويات المسجلة عليها ({_source.SettledAmount:N2}).");
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
            DriverId = driverId,
            EmployeeId = employeeId,
            CustodyNumber = CustodyNumberTextBox.Text.Trim(),
            Amount = amount,
            HandoverDate = handoverDate,
            ReturnDate = returnDate,
            Status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Active",
            PaidFromTreasury = PaidFromTreasuryCheckBox.IsChecked == true,
            Notes = NotesTextBox.Text.Trim(),
            DocumentUrl = _source?.DocumentUrl ?? string.Empty
        };

        DialogResult = true;
    }

    private void SelectCustodianType(string tag)
    {
        foreach (var item in CustodianTypeComboBox.Items.OfType<ComboBoxItem>())
        {
            if (string.Equals(item.Tag?.ToString(), tag, StringComparison.OrdinalIgnoreCase))
            {
                CustodianTypeComboBox.SelectedItem = item;
                return;
            }
        }

        CustodianTypeComboBox.SelectedIndex = 0;
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

    private void UpdateCustodianSummary()
    {
        if (CustodianSummaryTextBlock is null)
        {
            return;
        }

        var name = IsDriverSelected
            ? (CustodianComboBox.SelectedItem as DriverDto)?.FullName
            : (CustodianComboBox.SelectedItem as EmployeeDto)?.FullName;

        var type = IsDriverSelected ? "سائق" : "موظف";
        CustodianSummaryTextBlock.Text = string.IsNullOrWhiteSpace(name) ? "-" : $"{name} ({type})";
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
