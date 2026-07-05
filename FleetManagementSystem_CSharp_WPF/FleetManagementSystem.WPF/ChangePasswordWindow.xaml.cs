using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Services;
using System.Windows;

namespace FleetManagementSystem.WPF;

public partial class ChangePasswordWindow : Window
{
    private readonly IAuthenticationService _authenticationService;
    private readonly int _userId;

    public ChangePasswordWindow(IAuthenticationService authenticationService, int userId)
    {
        InitializeComponent();
        _authenticationService = authenticationService;
        _userId = userId;
        Loaded += (_, _) => CurrentPasswordBox.Focus();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorTextBlock.Text = string.Empty;

        try
        {
            SaveButton.IsEnabled = false;
            await _authenticationService.ChangePasswordAsync(new ChangePasswordDto
            {
                UserId = _userId,
                CurrentPassword = CurrentPasswordBox.Password,
                NewPassword = NewPasswordBox.Password,
                ConfirmPassword = ConfirmPasswordBox.Password
            });

            MessageBox.Show(
                "تم تغيير كلمة المرور بنجاح.",
                "تم",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
        }
        catch (Exception ex)
        {
            ErrorTextBlock.Text = ex.Message;
        }
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
