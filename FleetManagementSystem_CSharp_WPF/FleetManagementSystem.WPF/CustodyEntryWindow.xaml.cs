using FleetManagementSystem.Core.DTOs;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace FleetManagementSystem.WPF;

public partial class CustodyEntryWindow : Window
{
    private readonly IReadOnlyList<UserFormDto> _users;
    private readonly CustodyDto? _source;

    public CustodyFormDto? CustodyForm { get; private set; }

    public CustodyEntryWindow(IEnumerable<UserFormDto> users, int? preselectedUserId = null, CustodyDto? source = null)
    {
        InitializeComponent();

        _source = source;
        _users = users
            .Where(user => user.IsActive)
            .OrderBy(user => user.FullName)
            .ToList();

        UserComboBox.ItemsSource = _users;

        if (source is not null)
        {
            Title = "تعديل عهدة";
            CustodyNumberTextBox.Text = source.CustodyNumber;
            HandoverDatePicker.SelectedDate = source.HandoverDate == default ? DateTime.Today : source.HandoverDate;
            ReturnDatePicker.SelectedDate = source.ReturnDate;
            AmountTextBox.Text = source.Amount <= 0 ? "0" : source.Amount.ToString("0.##", CultureInfo.InvariantCulture);
            NotesTextBox.Text = source.Notes;
            SelectStatus(source.Status);
            UserComboBox.SelectedValue = source.UserId;
        }
        else
        {
            CustodyNumberTextBox.Text = $"CU-{DateTime.Now:yyyyMMdd-HHmm}";
            HandoverDatePicker.SelectedDate = DateTime.Today;
            AmountTextBox.Text = "0";
        }

        if (source is null && preselectedUserId.HasValue)
        {
            UserComboBox.SelectedValue = preselectedUserId.Value;
        }
        else if (source is null && _users.Count > 0)
        {
            UserComboBox.SelectedItem = _users[0];
        }

        UpdateUserSummary();
    }

    private void UserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateUserSummary();

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var user = UserComboBox.SelectedItem as UserFormDto;
        if (user is null)
        {
            ShowValidation("اختر المستخدم صاحب العهدة أولًا.");
            return;
        }

        if (string.IsNullOrWhiteSpace(CustodyNumberTextBox.Text))
        {
            ShowValidation("اكتب رقم العهدة أولًا.");
            return;
        }

        var handoverDate = HandoverDatePicker.SelectedDate ?? DateTime.Today;
        var returnDate = ReturnDatePicker.SelectedDate;
        if (returnDate.HasValue && returnDate.Value.Date < handoverDate.Date)
        {
            ShowValidation("تاريخ الإرجاع يجب أن يكون بعد تاريخ التسليم.");
            return;
        }

        if (!decimal.TryParse(AmountTextBox.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount < 0)
        {
            ShowValidation("مبلغ العهدة يجب أن يكون رقمًا صحيحًا وغير سالب.");
            return;
        }

        CustodyForm = new CustodyFormDto
        {
            Id = _source?.Id ?? 0,
            UserId = user.Id,
            CustodyNumber = CustodyNumberTextBox.Text.Trim(),
            HandoverDate = handoverDate,
            ReturnDate = returnDate,
            Status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Active",
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

    private void UpdateUserSummary()
    {
        var user = UserComboBox.SelectedItem as UserFormDto;
        UserFullNameTextBlock.Text = user is null ? "اختر مستخدمًا" : user.FullName;
        UsernameTextBlock.Text = string.IsNullOrWhiteSpace(user?.Username) ? "-" : user.Username;
        UserRoleTextBlock.Text = string.IsNullOrWhiteSpace(user?.Role) ? "-" : user.Role;
        UserStatusTextBlock.Text = user is null ? "-" : (user.IsActive ? "مفعل" : "غير مفعل");
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
