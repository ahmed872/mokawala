using FleetManagementSystem.Core.DTOs;
using System.Windows;
using System.Windows.Controls;

namespace FleetManagementSystem.WPF;

public partial class TripPermitWindow : Window
{
    public TripPermitWindow(TripDto trip, VehicleDto? vehicle, CompanySettingsDto? settings)
    {
        InitializeComponent();

        CompanyNameTextBlock.Text = string.IsNullOrWhiteSpace(settings?.CompanyName)
            ? "نظام إدارة الأسطول"
            : settings.CompanyName;
        CompanyAddressTextBlock.Text = settings?.Address ?? string.Empty;

        TripNumberTextBlock.Text = trip.Id == 0 ? "غير محفوظ" : trip.Id.ToString();
        TripDateTextBlock.Text = trip.StartDate.ToString("yyyy-MM-dd HH:mm");
        TripStatusTextBlock.Text = NormalizeStatus(trip.Status);

        VehiclePlateTextBlock.Text = ValueOrDash(vehicle?.PlateNumber, trip.VehiclePlateNumber);
        VehicleModelTextBlock.Text = ValueOrDash(vehicle?.Model);
        VehicleChassisTextBlock.Text = ValueOrDash(vehicle?.ChassisNumber);
        VehicleEngineTextBlock.Text = ValueOrDash(vehicle?.EngineNumber);
        RegistrationStartTextBlock.Text = FormatDate(vehicle?.RegistrationStartDate);
        RegistrationExpiryTextBlock.Text = FormatDate(vehicle?.RegistrationExpiryDate);
        InsuranceDetailsTextBlock.Text = BuildInsuranceText(vehicle);

        DriverNameTextBlock.Text = ValueOrDash(trip.DriverName);
        RequesterNameTextBlock.Text = ValueOrDash(trip.RequesterName);
        SupervisorNameTextBlock.Text = ValueOrDash(trip.SupervisorName);
        RouteTextBlock.Text = BuildRouteText(trip.StartLocation, trip.EndLocation);
        PurposeTextBlock.Text = string.IsNullOrWhiteSpace(trip.Notes)
            ? ValueOrDash(trip.Purpose)
            : $"{ValueOrDash(trip.Purpose)}\n{trip.Notes}";
    }

    private void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var scrollViewer = FindScrollViewer(this);
            var savedScrollOffset = scrollViewer?.VerticalOffset ?? 0;

            if (scrollViewer != null)
            {
                // Reset scroll to avoid visual clipping
                scrollViewer.ScrollToTop();
                scrollViewer.UpdateLayout();
            }

            var hostWidth = FormRoot.ActualWidth > 0 ? FormRoot.ActualWidth : FormRoot.Width;
            var hostHeight = FormRoot.ActualHeight > 0 ? FormRoot.ActualHeight : FormRoot.Height;

            // Force layout pass
            FormRoot.Measure(new Size(hostWidth, hostHeight));
            FormRoot.Arrange(new Rect(0, 0, hostWidth, hostHeight));
            FormRoot.UpdateLayout();

            // Print the exact A5 visual; DirectPrintHelper handles the PDF orientation correction.
            DirectPrintHelper.PrintVisualToDefaultPrinter(FormRoot, "نموذج تشغيل مركبة");

            if (scrollViewer != null)
            {
                // Restore state
                scrollViewer.ScrollToVerticalOffset(savedScrollOffset);
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
        for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is ScrollViewer scrollViewer) return scrollViewer;
            var nested = FindScrollViewer(child);
            if (nested != null) return nested;
        }
        return null;
    }

    private static string BuildInsuranceText(VehicleDto? vehicle)
    {
        if (vehicle is null)
        {
            return "—";
        }

        return string.IsNullOrWhiteSpace(vehicle.AccidentInsuranceDetails)
            ? "تأمين حوادث: غير مسجل"
            : $"تأمين حوادث: {vehicle.AccidentInsuranceDetails}";
    }

    private static string FormatDate(DateTime? value) =>
        value.HasValue ? value.Value.ToString("yyyy-MM-dd") : "غير مسجل";

    private static string ValueOrDash(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return "—";
    }

    private static string BuildRouteText(string? startLocation, string? endLocation)
        => $"{ValueOrDash(startLocation)} إلى {ValueOrDash(endLocation)}";

    private static string NormalizeStatus(string? status) =>
        status?.Trim() switch
        {
            "Open" => "مفتوحة",
            "Closed" => "مغلقة",
            "Planned" => "مخططة",
            "InTrip" => "جارية",
            "InProgress" => "جارية",
            { Length: > 0 } value => value,
            _ => "—"
        };
}
