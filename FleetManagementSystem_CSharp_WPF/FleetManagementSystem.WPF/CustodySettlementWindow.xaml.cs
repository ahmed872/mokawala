using FleetManagementSystem.Core.DTOs;
using System.Globalization;
using System.Windows;

namespace FleetManagementSystem.WPF;

public partial class CustodySettlementWindow : Window
{
    private readonly CustodyDto _custody;

    public CustodySettlementFormDto? SettlementForm { get; private set; }

    public CustodySettlementWindow(CustodyDto custody)
    {
        InitializeComponent();

        _custody = custody;
        TitleTextBlock.Text = $"تصفية العهدة {custody.CustodyNumber}";
        CustodianTextBlock.Text = $"المستلم: {custody.CustodianName} - العربية: {custody.VehiclePlateNumber}";
        AmountTextBlock.Text = custody.Amount.ToString("N2", CultureInfo.InvariantCulture);
        SettledTextBlock.Text = custody.SettledAmount.ToString("N2", CultureInfo.InvariantCulture);
        RemainingTextBlock.Text = custody.RemainingAmount.ToString("N2", CultureInfo.InvariantCulture);
        SettleAmountTextBox.Text = custody.RemainingAmount.ToString("0.##", CultureInfo.InvariantCulture);
        SettlementDatePicker.SelectedDate = DateTime.Today;

        Loaded += (_, _) =>
        {
            SettleAmountTextBox.Focus();
            SettleAmountTextBox.SelectAll();
        };
    }

    private void SettleButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorTextBlock.Text = string.Empty;

        if (!decimal.TryParse(SettleAmountTextBox.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ||
            amount <= 0)
        {
            ErrorTextBlock.Text = "اكتب مبلغ تصفية صحيح أكبر من صفر.";
            return;
        }

        if (amount > _custody.RemainingAmount)
        {
            ErrorTextBlock.Text = $"مبلغ التصفية أكبر من المتبقي على العهدة ({_custody.RemainingAmount:0.##}).";
            return;
        }

        SettlementForm = new CustodySettlementFormDto
        {
            CustodyId = _custody.Id,
            Amount = amount,
            SettlementDate = SettlementDatePicker.SelectedDate ?? DateTime.Today,
            Notes = NotesTextBox.Text.Trim()
        };

        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
