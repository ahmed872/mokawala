using FleetManagementSystem.Core.DTOs;
using System.Windows;
using System.Windows.Controls;

namespace FleetManagementSystem.WPF;

public partial class InsuranceEntryWindow : Window
{
    public InsuranceDto Insurance { get; }
    public IReadOnlyList<VehicleDto> Vehicles { get; }

    public InsuranceEntryWindow(IEnumerable<VehicleDto> vehicles, InsuranceDto source)
    {
        InitializeComponent();
        Vehicles = vehicles.ToList();
        Insurance = new InsuranceDto
        {
            Id = source.Id,
            VehicleId = source.VehicleId > 0 ? source.VehicleId : Vehicles.FirstOrDefault()?.Id ?? 0,
            VehiclePlateNumber = source.VehiclePlateNumber,
            VehicleModel = source.VehicleModel,
            VehicleYear = source.VehicleYear,
            VehicleChassisNumber = source.VehicleChassisNumber,
            VehicleEngineNumber = source.VehicleEngineNumber,
            PolicyNumber = source.PolicyNumber,
            InsuranceCompany = source.InsuranceCompany,
            PolicyType = string.IsNullOrWhiteSpace(source.PolicyType) ? "تأمين حوادث" : source.PolicyType,
            StartDate = source.StartDate == default ? DateTime.Today : source.StartDate,
            ExpiryDate = source.ExpiryDate == default ? DateTime.Today.AddYears(1) : source.ExpiryDate,
            PremiumAmount = source.PremiumAmount,
            CoverageAmount = source.CoverageAmount,
            CoverageDetails = source.CoverageDetails,
            AgentName = source.AgentName,
            AgentPhoneNumber = source.AgentPhoneNumber,
            Status = string.IsNullOrWhiteSpace(source.Status) ? "Active" : source.Status,
            DocumentUrl = source.DocumentUrl,
            Notes = source.Notes
        };

        DataContext = this;
        ApplySelectedVehicleDetails();
    }

    private void VehicleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplySelectedVehicleDetails();

    private void ApplySelectedVehicleDetails()
    {
        var vehicle = Vehicles.FirstOrDefault(v => v.Id == Insurance.VehicleId);
        if (vehicle is null)
        {
            return;
        }

        Insurance.VehiclePlateNumber = vehicle.PlateNumber;
        Insurance.VehicleModel = vehicle.Model;
        Insurance.VehicleYear = vehicle.Year;
        Insurance.VehicleChassisNumber = vehicle.ChassisNumber;
        Insurance.VehicleEngineNumber = vehicle.EngineNumber;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (Insurance.VehicleId <= 0)
        {
            MessageBox.Show("اختر العربية المرتبطة بوثيقة التأمين.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(Insurance.PolicyNumber))
        {
            MessageBox.Show("رقم وثيقة التأمين مطلوب.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(Insurance.InsuranceCompany))
        {
            MessageBox.Show("شركة التأمين مطلوبة.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(Insurance.PolicyType))
        {
            MessageBox.Show("نوع وثيقة التأمين مطلوب.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (Insurance.ExpiryDate.Date < Insurance.StartDate.Date)
        {
            MessageBox.Show("تاريخ نهاية التأمين يجب أن يكون بعد تاريخ البداية.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (Insurance.PremiumAmount < 0 || Insurance.CoverageAmount < 0)
        {
            MessageBox.Show("قيم التأمين لا يمكن أن تكون سالبة.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ApplySelectedVehicleDetails();
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
