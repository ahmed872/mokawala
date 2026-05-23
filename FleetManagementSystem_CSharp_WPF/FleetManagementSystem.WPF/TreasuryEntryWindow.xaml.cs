using FleetManagementSystem.Core.DTOs;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace FleetManagementSystem.WPF;

public partial class TreasuryEntryWindow : Window
{
    private readonly TreasuryTransactionDto _source;

    public TreasuryTransactionFormDto? TreasuryForm { get; private set; }

    public TreasuryEntryWindow(TreasuryTransactionDto source)
    {
        InitializeComponent();
        _source = source;

        if (source.Id > 0)
        {
            Title = "تعديل حركة خزينة";
            TitleTextBlock.Text = "تعديل حركة خزينة";
        }

        TransactionDatePicker.SelectedDate = source.TransactionDate == default ? DateTime.Today : source.TransactionDate;
        SelectComboItem(TransactionTypeComboBox, string.IsNullOrWhiteSpace(source.TransactionType) ? "إيراد" : source.TransactionType);
        AmountTextBox.Text = source.Amount == 0 ? string.Empty : source.Amount.ToString("0.##", CultureInfo.InvariantCulture);
        DescriptionTextBox.Text = source.Description;
        RelatedEntityTypeTextBox.Text = source.RelatedEntityType;
        RelatedEntityIdTextBox.Text = source.RelatedEntityId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        SelectComboItem(PaymentMethodComboBox, string.IsNullOrWhiteSpace(source.PaymentMethod) ? "نقدي" : source.PaymentMethod);
        NotesTextBox.Text = source.Notes;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!decimal.TryParse(AmountTextBox.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        {
            ShowValidation("المبلغ يجب أن يكون رقمًا صحيحًا.");
            return;
        }

        if (amount <= 0)
        {
            ShowValidation("المبلغ يجب أن يكون أكبر من صفر.");
            return;
        }

        int? relatedEntityId = null;
        if (!string.IsNullOrWhiteSpace(RelatedEntityIdTextBox.Text))
        {
            if (!int.TryParse(RelatedEntityIdTextBox.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedId) || parsedId <= 0)
            {
                ShowValidation("رقم المرتبط يجب أن يكون رقمًا صحيحًا أكبر من صفر.");
                return;
            }

            relatedEntityId = parsedId;
        }

        TreasuryForm = new TreasuryTransactionFormDto
        {
            Id = _source.Id,
            TransactionDate = TransactionDatePicker.SelectedDate ?? DateTime.Today,
            TransactionType = SelectedComboText(TransactionTypeComboBox, "إيراد"),
            Amount = amount,
            Description = DescriptionTextBox.Text.Trim(),
            RelatedEntityType = RelatedEntityTypeTextBox.Text.Trim(),
            RelatedEntityId = relatedEntityId,
            PaymentMethod = SelectedComboText(PaymentMethodComboBox, "نقدي"),
            Notes = NotesTextBox.Text.Trim()
        };

        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private static void SelectComboItem(ComboBox comboBox, string value)
    {
        foreach (var item in comboBox.Items.OfType<ComboBoxItem>())
        {
            if (string.Equals(item.Content?.ToString(), value, StringComparison.OrdinalIgnoreCase))
            {
                comboBox.SelectedItem = item;
                return;
            }
        }

        comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
    }

    private static string SelectedComboText(ComboBox comboBox, string fallback) =>
        (comboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? fallback;

    private static void ShowValidation(string message)
    {
        MessageBox.Show(message, "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
