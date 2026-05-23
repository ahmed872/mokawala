using System.Windows;

namespace FleetManagementSystem.WPF;

public partial class VehicleLicenseEntryWindow : Window
{
    public MainWindow.VehicleLicenseRow LicenseRow { get; }
    public IReadOnlyList<string> RegistrationTypes { get; }

    public VehicleLicenseEntryWindow(MainWindow.VehicleLicenseRow source, IEnumerable<string> registrationTypes)
    {
        InitializeComponent();
        LicenseRow = new MainWindow.VehicleLicenseRow
        {
            VehicleId = source.VehicleId,
            PlateNumber = source.PlateNumber,
            Model = source.Model,
            Year = source.Year > 0 ? source.Year : DateTime.Today.Year,
            ChassisNumber = source.ChassisNumber,
            EngineNumber = source.EngineNumber,
            OilChangeIntervalKm = source.OilChangeIntervalKm > 0 ? source.OilChangeIntervalKm : 10000,
            RegistrationType = string.IsNullOrWhiteSpace(source.RegistrationType) ? "ترخيص" : source.RegistrationType,
            RegistrationStartDate = source.RegistrationStartDate ?? DateTime.Today,
            RegistrationExpiryDate = source.RegistrationExpiryDate ?? DateTime.Today.AddYears(1)
        };
        RegistrationTypes = registrationTypes.ToList();
        DataContext = this;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(LicenseRow.PlateNumber))
        {
            MessageBox.Show("رقم العربية مطلوب.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (LicenseRow.Year is < 1900 or > 2100)
        {
            MessageBox.Show("سنة الصنع يجب أن تكون بين 1900 و2100.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (LicenseRow.OilChangeIntervalKm <= 0)
        {
            MessageBox.Show("قيمة تغيير الزيت كل كام كم يجب أن تكون أكبر من صفر.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (LicenseRow.RegistrationStartDate.HasValue &&
            LicenseRow.RegistrationExpiryDate.HasValue &&
            LicenseRow.RegistrationExpiryDate.Value.Date < LicenseRow.RegistrationStartDate.Value.Date)
        {
            MessageBox.Show("تاريخ نهاية الترخيص يجب أن يكون بعد تاريخ البداية.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
