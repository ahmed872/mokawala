using System.Printing;
using System.Windows.Controls;
using System.Windows.Media;

namespace FleetManagementSystem.WPF;

internal static class DirectPrintHelper
{
    public static void PrintVisualToDefaultPrinter(Visual visual, string jobName)
    {
        ArgumentNullException.ThrowIfNull(visual);

        var defaultQueue = LocalPrintServer.GetDefaultPrintQueue()
            ?? throw new InvalidOperationException("لا توجد طابعة افتراضية معرفة على الجهاز.");
        defaultQueue.Refresh();

        if (defaultQueue.IsOffline || defaultQueue.IsNotAvailable)
        {
            throw new InvalidOperationException($"الطابعة الافتراضية غير متاحة حاليا: {defaultQueue.FullName}");
        }

        var printDialog = new PrintDialog
        {
            PrintQueue = defaultQueue,
            PrintTicket = defaultQueue.DefaultPrintTicket
        };

        printDialog.PrintVisual(visual, string.IsNullOrWhiteSpace(jobName) ? "مستند" : jobName);
    }

    public static string BuildDirectPrintErrorMessage(Exception exception)
    {
        var details = exception.InnerException?.Message ?? exception.Message;
        return $"تعذر إرسال النموذج للطابعة الافتراضية مباشرة. تأكد أن الطابعة الافتراضية متصلة وجاهزة.\n\nالتفاصيل: {details}";
    }
}
