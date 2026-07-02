using FleetManagementSystem.Core.DTOs;
using System.Windows;

namespace FleetManagementSystem.WPF;

public partial class DriverEntryWindow : Window
{
    public DriverFormDto Draft { get; }
    public DriverFormDto? DriverForm { get; private set; }

    public DriverEntryWindow(DriverFormDto? source = null, bool isEditMode = false)
    {
        InitializeComponent();
        Draft = Clone(source ?? CreateDefaultDriver());
        DataContext = this;
        if (isEditMode)
        {
            Title = "تعديل سائق";
            DriverEntryTitleTextBlock.Text = "تعديل بيانات السائق";
            DriverEntrySubtitleTextBlock.Text = "عدّل بيانات السائق والرخصة ثم احفظ التغييرات.";
        }
    }

    private static DriverFormDto CreateDefaultDriver() => new()
    {
        DateOfBirth = DateTime.Today.AddYears(-30),
        LicenseStartDate = DateTime.Today,
        LicenseExpiryDate = DateTime.Today.AddYears(1),
        LicenseType = "خاصة",
        WorkLocation = "الموقع الرئيسي",
        IsActive = true
    };

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        DriverEntryDateOfBirthPicker.GetBindingExpression(System.Windows.Controls.DatePicker.SelectedDateProperty)?.UpdateSource();
        DriverEntryLicenseStartDatePicker.GetBindingExpression(System.Windows.Controls.DatePicker.SelectedDateProperty)?.UpdateSource();
        DriverEntryLicenseExpiryDatePicker.GetBindingExpression(System.Windows.Controls.DatePicker.SelectedDateProperty)?.UpdateSource();

        if (!Validate(out var message))
        {
            MessageBox.Show(message, "راجع بيانات السائق", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DriverForm = Clone(Draft);
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private bool Validate(out string message)
    {
        if (string.IsNullOrWhiteSpace(Draft.FullName))
        {
            message = "اسم السائق مطلوب.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Draft.LicenseNumber))
        {
            message = "رقم رخصة السائق مطلوب.";
            return false;
        }

        if (Draft.DateOfBirth != default && Draft.DateOfBirth.Date > DateTime.Today)
        {
            message = "تاريخ ميلاد السائق لا يمكن أن يكون في المستقبل.";
            return false;
        }

        if (Draft.LicenseStartDate == default)
        {
            message = "تاريخ بداية رخصة السائق مطلوب.";
            return false;
        }

        if (Draft.LicenseExpiryDate == default)
        {
            message = "تاريخ انتهاء رخصة السائق مطلوب.";
            return false;
        }

        if (Draft.LicenseExpiryDate.Date < Draft.LicenseStartDate.Date)
        {
            message = "تاريخ انتهاء رخصة السائق يجب أن يكون بعد تاريخ البداية.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    private static DriverFormDto Clone(DriverFormDto source) => new()
    {
        Id = source.Id,
        FullName = Normalize(source.FullName),
        NationalId = Normalize(source.NationalId),
        PhoneNumber = Normalize(source.PhoneNumber),
        Email = Normalize(source.Email),
        Address = string.IsNullOrWhiteSpace(source.FullAddress) ? Normalize(source.Address) : Normalize(source.FullAddress),
        DateOfBirth = source.DateOfBirth == default ? DateTime.Today.AddYears(-30) : source.DateOfBirth.Date,
        LicenseNumber = Normalize(source.LicenseNumber),
        LicenseStartDate = source.LicenseStartDate == default ? DateTime.Today : source.LicenseStartDate.Date,
        LicenseExpiryDate = source.LicenseExpiryDate == default ? DateTime.Today.AddYears(1) : source.LicenseExpiryDate.Date,
        LicenseType = string.IsNullOrWhiteSpace(source.LicenseType) ? "خاصة" : Normalize(source.LicenseType),
        IsCompanyInsured = source.IsCompanyInsured,
        Governorate = Normalize(source.Governorate),
        FullAddress = string.IsNullOrWhiteSpace(source.FullAddress) ? Normalize(source.Address) : Normalize(source.FullAddress),
        TrafficUnit = Normalize(source.TrafficUnit),
        WorkLocation = string.IsNullOrWhiteSpace(source.WorkLocation) ? "الموقع الرئيسي" : Normalize(source.WorkLocation),
        IsActive = source.IsActive,
        Notes = Normalize(source.Notes)
    };

    private static string Normalize(string? value) => value?.Trim() ?? string.Empty;
}
