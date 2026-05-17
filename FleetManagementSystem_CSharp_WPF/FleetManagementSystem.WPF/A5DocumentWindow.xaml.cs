using FleetManagementSystem.Core.DTOs;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace FleetManagementSystem.WPF;

public partial class A5DocumentWindow : Window
{
    public A5DocumentWindow(
        string title,
        string subtitle,
        IEnumerable<A5DocumentSection> sections,
        CompanySettingsDto? settings)
    {
        InitializeComponent();

        CompanyNameTextBlock.Text = string.IsNullOrWhiteSpace(settings?.CompanyName)
            ? "نظام إدارة الأسطول"
            : settings.CompanyName;
        CompanyAddressTextBlock.Text = settings?.Address ?? string.Empty;
        DocumentTitleTextBlock.Text = title;
        DocumentSubtitleTextBlock.Text = subtitle;

        var documentSections = sections.ToList();
        SectionsItemsControl.ItemsSource = new ObservableCollection<A5DocumentSection>(documentSections);
    }

    public sealed class A5DocumentSection
    {
        public A5DocumentSection(string title, IEnumerable<A5DocumentRow> rows)
        {
            Title = title;
            Rows = new ObservableCollection<A5DocumentRow>(rows);
        }

        public string Title { get; }
        public ObservableCollection<A5DocumentRow> Rows { get; }
    }

    public sealed class A5DocumentRow
    {
        public A5DocumentRow(string label, object? value)
        {
            Label = label;
            Value = FormatValue(value);
        }

        public string Label { get; }
        public string Value { get; }
    }

    private void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DirectPrintHelper.PrintVisualToDefaultPrinter(FormRoot, DocumentTitleTextBlock.Text);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                DirectPrintHelper.BuildDirectPrintErrorMessage(ex),
                "تعذر الطباعة",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private static string FormatValue(object? value) =>
        value switch
        {
            null => "-",
            DateTime date => date.ToString("yyyy-MM-dd"),
            DateTimeOffset date => date.ToString("yyyy-MM-dd"),
            decimal amount => amount.ToString("0.##"),
            double number => number.ToString("0.##"),
            float number => number.ToString("0.##"),
            _ when string.IsNullOrWhiteSpace(value.ToString()) => "-",
            _ => value.ToString()!.Trim()
        };
}
