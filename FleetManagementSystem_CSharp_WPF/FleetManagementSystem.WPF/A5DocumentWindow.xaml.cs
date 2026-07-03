using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FleetManagementSystem.Core.DTOs;

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
            ? "شركة جوميكس للحركة والمعدات"
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
            var scrollViewer = FindScrollViewer(this);
            var savedScrollOffset = scrollViewer?.VerticalOffset ?? 0;
            var wasHostedInScrollViewer = scrollViewer != null;

            if (wasHostedInScrollViewer)
            {
                // Reset scroll to avoid visual clipping
                scrollViewer!.ScrollToTop();
                scrollViewer.UpdateLayout();
            }

            var hostWidth = FormRoot.ActualWidth > 0 ? FormRoot.ActualWidth : FormRoot.Width;
            var hostHeight = FormRoot.ActualHeight > 0 ? FormRoot.ActualHeight : FormRoot.Height;

            // Force layout pass for printing
            FormRoot.Measure(new Size(hostWidth, hostHeight));
            FormRoot.Arrange(new Rect(0, 0, hostWidth, hostHeight));
            FormRoot.UpdateLayout();

            // Print the exact A5 visual; DirectPrintHelper handles the PDF orientation correction.
            DirectPrintHelper.PrintVisualToDefaultPrinter(FormRoot, DocumentTitleTextBlock.Text);

            // Restore state
            if (wasHostedInScrollViewer)
            {
                scrollViewer!.ScrollToVerticalOffset(savedScrollOffset);
                scrollViewer.UpdateLayout();
            }
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

    private static ScrollViewer? FindScrollViewer(DependencyObject parent)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is ScrollViewer scrollViewer)
            {
                return scrollViewer;
            }

            var nested = FindScrollViewer(child);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

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
