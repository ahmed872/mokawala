using System.Printing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingColor = System.Drawing.Color;
using DrawingGraphics = System.Drawing.Graphics;
using DrawingRectangle = System.Drawing.Rectangle;
using WpfSize = System.Windows.Size;

namespace FleetManagementSystem.WPF;

internal static class DirectPrintHelper
{
    private const double RenderDpi = 300d;
    private const double WpfDpi = 96d;

    public static void PrintVisualToDefaultPrinter(Visual visual, string jobName)
    {
        ArgumentNullException.ThrowIfNull(visual);

        var defaultQueue = GetReadyDefaultPrintQueue();
        var bitmapSource = RenderVisualToBitmap(visual);
        using var bitmap = ConvertBitmapSourceToDrawingBitmap(bitmapSource);
        using var printableBitmap = CreatePrinterBitmap(bitmap);
        PrintBitmapToDefaultPrinter(defaultQueue, printableBitmap, jobName);
    }

    public static string BuildDirectPrintErrorMessage(Exception exception)
    {
        var details = exception.InnerException?.Message ?? exception.Message;
        return $"تعذر إرسال النموذج للطابعة الافتراضية مباشرة. تأكد أن الطابعة الافتراضية متصلة وجاهزة.\n\nالتفاصيل: {details}";
    }

    private static PrintQueue GetReadyDefaultPrintQueue()
    {
        var defaultQueue = LocalPrintServer.GetDefaultPrintQueue()
            ?? throw new InvalidOperationException("لا توجد طابعة افتراضية معرفة على الجهاز.");
        defaultQueue.Refresh();

        if (defaultQueue.IsOffline || defaultQueue.IsNotAvailable)
        {
            throw new InvalidOperationException($"الطابعة الافتراضية غير متاحة حاليا: {defaultQueue.FullName}");
        }

        return defaultQueue;
    }

    private static void PrintBitmapToDefaultPrinter(PrintQueue defaultQueue, DrawingBitmap bitmap, string jobName)
    {
        using var printDocument = new System.Drawing.Printing.PrintDocument
        {
            DocumentName = string.IsNullOrWhiteSpace(jobName) ? "مستند A5" : jobName,
            PrintController = new System.Drawing.Printing.StandardPrintController()
        };

        printDocument.PrinterSettings.PrinterName = defaultQueue.FullName;
        printDocument.DefaultPageSettings.Landscape = false;
        printDocument.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(0, 0, 0, 0);
        printDocument.DefaultPageSettings.PaperSize = FindA5PaperSize(printDocument)
            ?? new System.Drawing.Printing.PaperSize("A5", 583, 827);

        printDocument.PrintPage += (_, e) =>
        {
            var graphics = e.Graphics
                ?? throw new InvalidOperationException("تعذر فتح صفحة الطباعة من تعريف الطابعة الافتراضية.");
            var pageBounds = e.MarginBounds.Width > 0 && e.MarginBounds.Height > 0
                ? e.MarginBounds
                : e.PageBounds;
            var destination = FitRectangle(bitmap.Width, bitmap.Height, pageBounds);

            graphics.Clear(DrawingColor.White);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            graphics.DrawImage(bitmap, destination);
            e.HasMorePages = false;
        };

        printDocument.Print();
    }

    private static System.Drawing.Printing.PaperSize? FindA5PaperSize(System.Drawing.Printing.PrintDocument printDocument)
    {
        foreach (System.Drawing.Printing.PaperSize paperSize in printDocument.PrinterSettings.PaperSizes)
        {
            if (paperSize.Kind == System.Drawing.Printing.PaperKind.A5 ||
                paperSize.PaperName.Contains("A5", StringComparison.OrdinalIgnoreCase))
            {
                return paperSize;
            }
        }

        return null;
    }

    private static DrawingRectangle FitRectangle(int sourceWidth, int sourceHeight, DrawingRectangle bounds)
    {
        var scale = Math.Min((double)bounds.Width / sourceWidth, (double)bounds.Height / sourceHeight);
        var width = Math.Max(1, (int)Math.Round(sourceWidth * scale));
        var height = Math.Max(1, (int)Math.Round(sourceHeight * scale));
        var left = bounds.Left + (bounds.Width - width) / 2;
        var top = bounds.Top + (bounds.Height - height) / 2;
        return new DrawingRectangle(left, top, width, height);
    }

    private static BitmapSource RenderVisualToBitmap(Visual visual)
    {
        if (visual is FrameworkElement element)
        {
            element.UpdateLayout();
        }

        var visualSize = GetVisualSize(visual);
        var scale = RenderDpi / WpfDpi;
        var pixelWidth = Math.Max(1, (int)Math.Ceiling(visualSize.Width * scale));
        var pixelHeight = Math.Max(1, (int)Math.Ceiling(visualSize.Height * scale));

        var bitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, RenderDpi, RenderDpi, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }

    private static DrawingBitmap ConvertBitmapSourceToDrawingBitmap(BitmapSource bitmapSource)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

        using var stream = new System.IO.MemoryStream();
        encoder.Save(stream);
        stream.Position = 0;

        using var sourceBitmap = new DrawingBitmap(stream);
        var bitmap = new DrawingBitmap(
            sourceBitmap.Width,
            sourceBitmap.Height,
            System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        bitmap.SetResolution((float)bitmapSource.DpiX, (float)bitmapSource.DpiY);

        using var graphics = DrawingGraphics.FromImage(bitmap);
        graphics.Clear(DrawingColor.White);
        graphics.DrawImageUnscaled(sourceBitmap, 0, 0);
        return bitmap;
    }

    private static DrawingBitmap CreatePrinterBitmap(DrawingBitmap sourceBitmap)
    {
        var bitmap = new DrawingBitmap(
            sourceBitmap.Width,
            sourceBitmap.Height,
            System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        bitmap.SetResolution(sourceBitmap.HorizontalResolution, sourceBitmap.VerticalResolution);

        using (var graphics = DrawingGraphics.FromImage(bitmap))
        {
            graphics.Clear(DrawingColor.White);
            graphics.DrawImageUnscaled(sourceBitmap, 0, 0);
        }

        // The Windows print pipeline / RenderTargetBitmap mirrors RTL WPF documents when they are rendered into bitmaps.
        // Pre-flipping here (RotateNoneFlipX) makes the final output readable instead of mirrored.
        bitmap.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipX);
        return bitmap;
    }

    private static WpfSize GetVisualSize(Visual visual)
    {
        if (visual is FrameworkElement { ActualWidth: > 0, ActualHeight: > 0 } element)
        {
            return new WpfSize(element.ActualWidth, element.ActualHeight);
        }

        var bounds = VisualTreeHelper.GetDescendantBounds(visual);
        if (bounds.Width > 0 && bounds.Height > 0)
        {
            return bounds.Size;
        }

        throw new InvalidOperationException("تعذر قراءة مقاس النموذج قبل الطباعة.");
    }
}
