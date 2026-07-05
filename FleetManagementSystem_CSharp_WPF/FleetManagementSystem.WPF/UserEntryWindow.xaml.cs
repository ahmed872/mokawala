using FleetManagementSystem.Core.DTOs;
using System.Windows;

namespace FleetManagementSystem.WPF;

public partial class UserEntryWindow : Window
{
    public sealed record RoleOption(string Value, string Display);

    private readonly UserFormDto? _source;

    public UserFormDto? UserForm { get; private set; }

    public UserEntryWindow(IEnumerable<RoleOption> roleOptions, UserFormDto? source = null)
    {
        InitializeComponent();

        _source = source;
        var roles = roleOptions.ToList();
        RoleComboBox.ItemsSource = roles;

        if (source is not null)
        {
            Title = "تعديل مستخدم";
            TitleTextBlock.Text = $"تعديل المستخدم: {source.Username}";
            UsernameTextBox.Text = source.Username;
            FullNameTextBox.Text = source.FullName;
            EmailTextBox.Text = source.Email;
            PhoneTextBox.Text = source.PhoneNumber;
            IsActiveCheckBox.IsChecked = source.IsActive;
            RoleComboBox.SelectedValue = source.Role;
        }

        if (RoleComboBox.SelectedItem is null)
        {
            RoleComboBox.SelectedValue = "OperationsDataEntry";
        }

        if (RoleComboBox.SelectedItem is null && roles.Count > 0)
        {
            RoleComboBox.SelectedIndex = 0;
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorTextBlock.Text = string.Empty;

        var username = UsernameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(username))
        {
            ErrorTextBlock.Text = "اكتب اسم المستخدم أولًا.";
            return;
        }

        if (RoleComboBox.SelectedValue is not string role || string.IsNullOrWhiteSpace(role))
        {
            ErrorTextBlock.Text = "اختر دور المستخدم أولًا.";
            return;
        }

        var password = PasswordBox.Password;
        var confirmPassword = ConfirmPasswordBox.Password;
        if (!string.IsNullOrWhiteSpace(password) && !string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            ErrorTextBlock.Text = "تأكيد كلمة المرور غير مطابق.";
            return;
        }

        UserForm = new UserFormDto
        {
            Id = _source?.Id ?? 0,
            Username = username,
            FullName = FullNameTextBox.Text.Trim(),
            Email = EmailTextBox.Text.Trim(),
            PhoneNumber = PhoneTextBox.Text.Trim(),
            Role = role,
            IsActive = IsActiveCheckBox.IsChecked == true,
            Password = password,
            ConfirmPassword = confirmPassword
        };

        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
