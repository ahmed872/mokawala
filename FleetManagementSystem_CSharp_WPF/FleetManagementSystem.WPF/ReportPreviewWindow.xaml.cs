using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FleetManagementSystem.WPF;

public partial class ReportPreviewWindow : Window
{
    private readonly FlowDocument _document;
    private readonly string _jobName;

    public ReportPreviewWindow(string title, FlowDocument document)
    {
        InitializeComponent();
        _document = document;
        _jobName = string.IsNullOrWhiteSpace(title) ? "تقرير" : title;
        TitleTextBlock.Text = _jobName;
        PreviewDocumentViewer.MinZoom = 50;
        PreviewDocumentViewer.MaxZoom = 220;
        PreviewDocumentViewer.Zoom = 100;
        PreviewDocumentViewer.Document = _document;
    }

    private void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() != true)
        {
            return;
        }

        _document.PageWidth = Math.Max(_document.PageWidth, printDialog.PrintableAreaWidth);
        _document.PageHeight = printDialog.PrintableAreaHeight;
        _document.PagePadding = new Thickness(30);
        _document.ColumnWidth = _document.PageWidth;
        printDialog.PrintDocument(((IDocumentPaginatorSource)_document).DocumentPaginator, _jobName);
    }

    private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
    {
        PreviewDocumentViewer.Zoom = Math.Max(PreviewDocumentViewer.MinZoom, PreviewDocumentViewer.Zoom - 10);
    }

    private void ResetZoomButton_Click(object sender, RoutedEventArgs e)
    {
        PreviewDocumentViewer.Zoom = 100;
    }

    private void ZoomInButton_Click(object sender, RoutedEventArgs e)
    {
        PreviewDocumentViewer.Zoom = Math.Min(PreviewDocumentViewer.MaxZoom, PreviewDocumentViewer.Zoom + 10);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
