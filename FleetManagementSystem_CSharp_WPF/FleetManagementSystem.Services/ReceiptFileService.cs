namespace FleetManagementSystem.Services;

/// <summary>
/// Local file-system implementation of <see cref="IReceiptFileService"/>. Scanned receipts are
/// copied into {application base}/Uploads/Receipts with a collision-free generated name, and the
/// relative path (e.g. "Uploads/Receipts/20260704120000_ab12….pdf") is what gets persisted in
/// SQLite. Validation is defense-in-depth: extension allow-list, non-empty content, size ceiling,
/// and file-signature (magic bytes) verification so a renamed .exe cannot pass as a receipt.
/// </summary>
public sealed class ReceiptFileService : IReceiptFileService
{
    private const long MaxReceiptSizeBytes = 20 * 1024 * 1024; // 20 MB

    private static readonly string[] AllowedExtensions = [".png", ".jpg", ".jpeg", ".pdf"];

    private static readonly string ReceiptsRelativeDirectory = Path.Combine("Uploads", "Receipts");

    private readonly string _storageRoot;

    public ReceiptFileService()
        : this(AppContext.BaseDirectory)
    {
    }

    /// <summary>Storage root override used by tests; production resolves under the application base directory.</summary>
    public ReceiptFileService(string storageRoot)
    {
        if (string.IsNullOrWhiteSpace(storageRoot))
        {
            throw new ArgumentException("Storage root must be a non-empty path.", nameof(storageRoot));
        }

        _storageRoot = Path.GetFullPath(storageRoot);
    }

    public async Task<string> SaveReceiptAsync(string sourceFilePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceFilePath))
        {
            throw new InvalidOperationException("لا يمكن حفظ التسوية بدون إرفاق فاتورة أو إيصال ممسوح ضوئيًا.");
        }

        var sourceInfo = new FileInfo(sourceFilePath);
        if (!sourceInfo.Exists)
        {
            throw new InvalidOperationException("ملف الفاتورة المحدد غير موجود على الجهاز.");
        }

        if (sourceInfo.Length == 0)
        {
            throw new InvalidOperationException("ملف الفاتورة فارغ ولا يمكن اعتماده كمستند مالي.");
        }

        if (sourceInfo.Length > MaxReceiptSizeBytes)
        {
            throw new InvalidOperationException("حجم ملف الفاتورة يتجاوز الحد المسموح (20 ميجابايت).");
        }

        var extension = sourceInfo.Extension.ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("صيغة الفاتورة غير مدعومة. الصيغ المقبولة: PNG أو JPEG أو PDF.");
        }

        await ValidateFileSignatureAsync(sourceInfo.FullName, extension, cancellationToken);

        var receiptsDirectory = Path.Combine(_storageRoot, ReceiptsRelativeDirectory);
        Directory.CreateDirectory(receiptsDirectory);

        var storedFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
        var destinationPath = Path.Combine(receiptsDirectory, storedFileName);

        await using (var source = new FileStream(
            sourceInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 81920, FileOptions.Asynchronous | FileOptions.SequentialScan))
        await using (var destination = new FileStream(
            destinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            bufferSize: 81920, FileOptions.Asynchronous))
        {
            await source.CopyToAsync(destination, cancellationToken);
            await destination.FlushAsync(cancellationToken);
        }

        return Path.Combine(ReceiptsRelativeDirectory, storedFileName).Replace('\\', '/');
    }

    public string ResolveAbsolutePath(string storedReceiptPath)
    {
        if (string.IsNullOrWhiteSpace(storedReceiptPath))
        {
            throw new ArgumentException("Stored receipt path must be non-empty.", nameof(storedReceiptPath));
        }

        var normalized = storedReceiptPath.Replace('/', Path.DirectorySeparatorChar);
        var absolute = Path.GetFullPath(Path.Combine(_storageRoot, normalized));

        // Reject stored values that escape the storage root (e.g. "../../secret").
        if (!absolute.StartsWith(_storageRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("مسار الفاتورة المخزن غير صالح.");
        }

        return absolute;
    }

    private static async Task ValidateFileSignatureAsync(string filePath, string extension, CancellationToken cancellationToken)
    {
        var header = new byte[8];
        await using (var stream = new FileStream(
            filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 16, FileOptions.Asynchronous))
        {
            var read = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, cancellationToken);
            if (read < 4)
            {
                throw new InvalidOperationException("محتوى ملف الفاتورة غير مكتمل ولا يمكن التحقق منه.");
            }
        }

        var isValid = extension switch
        {
            ".png" => header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47,
            ".jpg" or ".jpeg" => header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".pdf" => header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46, // %PDF
            _ => false
        };

        if (!isValid)
        {
            throw new InvalidOperationException("محتوى الملف لا يطابق صيغته المعلنة؛ ارفع فاتورة أصلية بصيغة PNG أو JPEG أو PDF.");
        }
    }
}
