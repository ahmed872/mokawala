using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace FleetManagementSystem.WPF;

/// <summary>
/// Self-service dashboard ("عهدتي") for custodian accounts: shows only the logged-in
/// driver's/employee's custodies and lets them record receipt-backed expense settlements.
/// When a custody's remaining balance reaches zero it automatically moves to the treasury
/// supervisor's approval queue.
/// </summary>
public partial class MyCustodyWindow : Window
{
    private readonly ICustodyService _custodyService;
    private readonly UserDto _user;
    private readonly ObservableCollection<CustodyDto> _custodies = new();
    private string _pickedReceiptPath = string.Empty;

    public MyCustodyWindow(ICustodyService custodyService, UserDto user)
    {
        InitializeComponent();

        _custodyService = custodyService;
        _user = user;

        Title = $"عهدتي - {user.FullName}";
        WelcomeTextBlock.Text =
            $"أهلًا {user.FullName}. سجل مصاريفك من العهدة بفاتورة ممسوحة، وعند تصفير الرصيد تتحول العهدة تلقائيًا لمشرف الخزينة للاعتماد.";

        MyCustodiesGrid.ItemsSource = _custodies;
        SettlementDatePicker.SelectedDate = DateTime.Today;

        Loaded += async (_, _) => await LoadCustodiesAsync();
    }

    private CustodyDto? SelectedCustody => MyCustodiesGrid.SelectedItem as CustodyDto;

    private async Task LoadCustodiesAsync()
    {
        try
        {
            if (!_user.DriverId.HasValue && !_user.EmployeeId.HasValue)
            {
                MessageBox.Show(
                    "حسابك غير مرتبط بسائق أو موظف؛ اطلب من مدير النظام ربط الحساب حتى تظهر عهدتك.",
                    "الحساب غير مرتبط",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var selectedId = SelectedCustody?.Id;
            var items = await _custodyService.GetActiveForCustodianAsync(_user.DriverId, _user.EmployeeId);

            _custodies.Clear();
            foreach (var custody in items)
            {
                _custodies.Add(custody);
            }

            if (_custodies.Count > 0)
            {
                var toSelect = selectedId.HasValue
                    ? _custodies.FirstOrDefault(c => c.Id == selectedId.Value) ?? _custodies[0]
                    : _custodies[0];
                MyCustodiesGrid.SelectedItem = toSelect;
            }
            else
            {
                SettlementsGrid.ItemsSource = null;
                SelectedCustodyTextBlock.Text = "لا توجد عهد نشطة عليك حاليًا.";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void MyCustodiesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var custody = SelectedCustody;
        if (custody is null)
        {
            SettlementsGrid.ItemsSource = null;
            SelectedCustodyTextBlock.Text = "اختر عهدة من القائمة أولًا";
            SubmitSettlementButton.IsEnabled = false;
            return;
        }

        SettlementsGrid.ItemsSource = custody.Settlements;

        var isActive = string.Equals(custody.Status, "نشط", StringComparison.OrdinalIgnoreCase);
        SubmitSettlementButton.IsEnabled = isActive;
        SelectedCustodyTextBlock.Text = isActive
            ? $"العهدة {custody.CustodyNumber} — الرصيد المتبقي: {custody.RemainingBalance:N2}"
            : $"العهدة {custody.CustodyNumber} بحالة \"{custody.Status}\" ولا يمكن التسوية عليها الآن.";
    }

    private void PickReceiptButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "اختيار الفاتورة الممسوحة",
            Filter = "فواتير وإيصالات (PNG, JPEG, PDF)|*.png;*.jpg;*.jpeg;*.pdf",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == true)
        {
            _pickedReceiptPath = dialog.FileName;
            ReceiptPathTextBlock.Text = dialog.FileName;
        }
    }

    private async void SubmitSettlementButton_Click(object sender, RoutedEventArgs e)
    {
        var custody = SelectedCustody;
        if (custody is null)
        {
            ShowValidation("اختر عهدة من القائمة أولًا.");
            return;
        }

        if (!decimal.TryParse(SettlementAmountTextBox.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        {
            ShowValidation("قيمة المصروف يجب أن تكون رقمًا أكبر من صفر.");
            return;
        }

        if (amount > custody.RemainingBalance)
        {
            ShowValidation($"قيمة المصروف ({amount:N2}) أكبر من الرصيد المتبقي ({custody.RemainingBalance:N2}).");
            return;
        }

        if (string.IsNullOrWhiteSpace(_pickedReceiptPath))
        {
            ShowValidation("إرفاق الفاتورة الممسوحة إلزامي لتسجيل التسوية.");
            return;
        }

        try
        {
            SubmitSettlementButton.IsEnabled = false;

            var updated = await _custodyService.SettleAsync(new CustodySettlementFormDto
            {
                CustodyId = custody.Id,
                Amount = amount,
                SettlementDate = SettlementDatePicker.SelectedDate ?? DateTime.Today,
                Description = SettlementDescriptionTextBox.Text.Trim(),
                ReceiptSourceFilePath = _pickedReceiptPath,
                Notes = SettlementNotesTextBox.Text.Trim()
            });

            SettlementAmountTextBox.Text = string.Empty;
            SettlementDescriptionTextBox.Text = string.Empty;
            SettlementNotesTextBox.Text = string.Empty;
            SettlementDatePicker.SelectedDate = DateTime.Today;
            _pickedReceiptPath = string.Empty;
            ReceiptPathTextBlock.Text = "لم يتم اختيار ملف";

            await LoadCustodiesAsync();

            if (string.Equals(updated.Status, "بانتظار الاعتماد", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    $"تمت تسوية كامل العهدة {updated.CustodyNumber}. تحولت العهدة تلقائيًا إلى مشرف الخزينة للاعتماد، وبعد موافقته تُسجل مصاريفها في الخزينة باسمك.",
                    "العهدة بانتظار الاعتماد",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(
                    $"تم تسجيل التسوية. الرصيد المتبقي من العهدة: {updated.RemainingBalance:N2}",
                    "تمت التسوية",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "تعذر تسجيل التسوية", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SubmitSettlementButton.IsEnabled = SelectedCustody is not null &&
                string.Equals(SelectedCustody.Status, "نشط", StringComparison.OrdinalIgnoreCase);
        }
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e) => await LoadCustodiesAsync();

    private void LogoutButton_Click(object sender, RoutedEventArgs e) => Close();

    private void ShowValidation(string message) =>
        MessageBox.Show(message, "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
}
