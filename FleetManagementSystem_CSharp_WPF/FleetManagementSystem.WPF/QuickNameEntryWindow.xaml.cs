using System.Windows;

namespace FleetManagementSystem.WPF;

public partial class QuickNameEntryWindow : Window
{
    public QuickNameEntryWindow(string title, string hint)
    {
        InitializeComponent();
        Title = title;
        TitleTextBlock.Text = title;
        HintTextBlock.Text = hint;
        Loaded += (_, _) => NameTextBox.Focus();
    }

    public string EnteredName { get; private set; } = string.Empty;

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var value = NameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            MessageBox.Show("اكتب الاسم أولًا.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            NameTextBox.Focus();
            return;
        }

        EnteredName = value;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
