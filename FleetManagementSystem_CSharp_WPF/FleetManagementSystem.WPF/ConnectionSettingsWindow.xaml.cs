using FleetManagementSystem.WPF.Infrastructure;
using System.Windows;
using System.Windows.Controls;

namespace FleetManagementSystem.WPF;

public partial class ConnectionSettingsWindow : Window
{
    private readonly ConnectionSettingsStore _settingsStore;

    public ConnectionSettingsWindow(ConnectionSettingsStore settingsStore, ClientConnectionProfile profile)
    {
        InitializeComponent();
        _settingsStore = settingsStore;
        ConnectionProfile = profile.Clone();
        CurrentPathTextBlock.Text = $"يتم حفظ إعدادات الاتصال بشكل مشفّر على هذا الجهاز:\n{_settingsStore.FilePath}";
        LoadProfile(ConnectionProfile);
    }

    public ClientConnectionProfile ConnectionProfile { get; private set; }

    private async void TestConnectionButton_Click(object sender, RoutedEventArgs e)
    {
        ResultTextBlock.Text = "جارٍ اختبار الاتصال...";
        ResultTextBlock.Foreground = (System.Windows.Media.Brush)FindResource("MutedBrush");

        var profile = ReadProfileFromForm();
        var result = await _settingsStore.TestAsync(profile);
        ResultTextBlock.Text = result.Message;
        ResultTextBlock.Foreground = result.IsSuccess
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(15, 81, 50))
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(185, 28, 28));
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var profile = ReadProfileFromForm();
        var result = await _settingsStore.TestAsync(profile);
        ResultTextBlock.Text = result.Message;
        ResultTextBlock.Foreground = result.IsSuccess
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(15, 81, 50))
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(185, 28, 28));

        if (!result.IsSuccess)
        {
            return;
        }

        _settingsStore.Save(profile);
        ConnectionProfile = profile;
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void ProviderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshProviderPanels();
    }

    private void LoadProfile(ClientConnectionProfile profile)
    {
        ProviderComboBox.SelectedIndex = profile.IsSqlite ? 1 : 0;
        ServerTextBox.Text = profile.Server;
        PortTextBox.Text = profile.Port.ToString();
        DatabaseTextBox.Text = profile.Database;
        UsernameTextBox.Text = profile.Username;
        PasswordBox.Password = profile.Password;
        UseSslCheckBox.IsChecked = profile.UseSsl;
        SqlitePathTextBox.Text = profile.SqlitePath;
        RefreshProviderPanels();
    }

    private ClientConnectionProfile ReadProfileFromForm()
    {
        var providerTag = (ProviderComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();

        return new ClientConnectionProfile
        {
            Provider = string.IsNullOrWhiteSpace(providerTag) ? "MySql" : providerTag,
            Server = ServerTextBox.Text.Trim(),
            Port = int.TryParse(PortTextBox.Text, out var port) ? port : 3306,
            Database = DatabaseTextBox.Text.Trim(),
            Username = UsernameTextBox.Text.Trim(),
            Password = PasswordBox.Password,
            UseSsl = UseSslCheckBox.IsChecked == true,
            SqlitePath = SqlitePathTextBox.Text.Trim()
        };
    }

    private void RefreshProviderPanels()
    {
        var isSqlite = (ProviderComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() == "Sqlite";
        SqlitePanel.Visibility = isSqlite ? Visibility.Visible : Visibility.Collapsed;
        ServerTextBox.IsEnabled = !isSqlite;
        PortTextBox.IsEnabled = !isSqlite;
        DatabaseTextBox.IsEnabled = !isSqlite;
        UsernameTextBox.IsEnabled = !isSqlite;
        PasswordBox.IsEnabled = !isSqlite;
        UseSslCheckBox.IsEnabled = !isSqlite;
    }
}
