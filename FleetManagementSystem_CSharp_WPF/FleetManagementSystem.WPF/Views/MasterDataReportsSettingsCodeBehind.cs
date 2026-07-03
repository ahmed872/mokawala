// ============================================================================
// FILE: MasterDataView.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/MasterData/MasterDataView.xaml.cs
// ============================================================================

using System.Windows;
using System.Windows.Controls;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.MasterData
{
    public partial class MasterDataView : UserControl
    {
        public MasterDataView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MasterDataViewModel viewModel)
            {
                viewModel.LoadVehicleTypesCommand.Execute(null);
                viewModel.LoadContractStatusesCommand.Execute(null);
                viewModel.LoadMaintenanceTypesCommand.Execute(null);
                viewModel.LoadServiceProvidersCommand.Execute(null);
            }
        }

        // Vehicle Types Tab
        private void AddVehicleTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MasterDataViewModel viewModel)
            {
                viewModel.OpenAddVehicleTypeDialogCommand.Execute(null);
            }
        }

        private void EditVehicleTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MasterDataViewModel viewModel && VehicleTypeDataGrid.SelectedItem != null)
            {
                viewModel.OpenEditVehicleTypeDialogCommand.Execute(VehicleTypeDataGrid.SelectedItem);
            }
        }

        private void DeleteVehicleTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MasterDataViewModel viewModel && VehicleTypeDataGrid.SelectedItem != null)
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد من حذف نوع السيارة؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    viewModel.DeleteVehicleTypeCommand.Execute(VehicleTypeDataGrid.SelectedItem);
                }
            }
        }

        // Contract Statuses Tab
        private void AddContractStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MasterDataViewModel viewModel)
            {
                viewModel.OpenAddContractStatusDialogCommand.Execute(null);
            }
        }

        private void EditContractStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MasterDataViewModel viewModel && ContractStatusDataGrid.SelectedItem != null)
            {
                viewModel.OpenEditContractStatusDialogCommand.Execute(ContractStatusDataGrid.SelectedItem);
            }
        }

        private void DeleteContractStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MasterDataViewModel viewModel && ContractStatusDataGrid.SelectedItem != null)
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد من حذف حالة العقد؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    viewModel.DeleteContractStatusCommand.Execute(ContractStatusDataGrid.SelectedItem);
                }
            }
        }

        // Maintenance Types Tab
        private void AddMaintenanceTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MasterDataViewModel viewModel)
            {
                viewModel.OpenAddMaintenanceTypeDialogCommand.Execute(null);
            }
        }

        private void EditMaintenanceTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MasterDataViewModel viewModel && MaintenanceTypeDataGrid.SelectedItem != null)
            {
                viewModel.OpenEditMaintenanceTypeDialogCommand.Execute(MaintenanceTypeDataGrid.SelectedItem);
            }
        }

        private void DeleteMaintenanceTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MasterDataViewModel viewModel && MaintenanceTypeDataGrid.SelectedItem != null)
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد من حذف نوع الصيانة؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    viewModel.DeleteMaintenanceTypeCommand.Execute(MaintenanceTypeDataGrid.SelectedItem);
                }
            }
        }

        // Service Providers Tab
        private void AddServiceProviderButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MasterDataViewModel viewModel)
            {
                viewModel.OpenAddServiceProviderDialogCommand.Execute(null);
            }
        }

        private void EditServiceProviderButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MasterDataViewModel viewModel && ServiceProviderDataGrid.SelectedItem != null)
            {
                viewModel.OpenEditServiceProviderDialogCommand.Execute(ServiceProviderDataGrid.SelectedItem);
            }
        }

        private void DeleteServiceProviderButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MasterDataViewModel viewModel && ServiceProviderDataGrid.SelectedItem != null)
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد من حذف مزود الخدمة؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    viewModel.DeleteServiceProviderCommand.Execute(ServiceProviderDataGrid.SelectedItem);
                }
            }
        }
    }
}

// ============================================================================
// FILE: ServiceProviderFormDialog.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/MasterData/ServiceProviderFormDialog.xaml.cs
// ============================================================================

using System.Windows;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.MasterData
{
    public partial class ServiceProviderFormDialog : Window
    {
        public ServiceProviderFormDialog()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ServiceProviderFormViewModel viewModel)
            {
                if (viewModel.SaveCommand.CanExecute(null))
                {
                    viewModel.SaveCommand.Execute(null);
                    this.DialogResult = true;
                    this.Close();
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}

// ============================================================================
// FILE: ReportsView.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Reports/ReportsView.xaml.cs
// ============================================================================

using System.Windows;
using System.Windows.Controls;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Reports
{
    public partial class ReportsView : UserControl
    {
        public ReportsView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ReportsViewModel viewModel)
            {
                viewModel.LoadReportsCommand.Execute(null);
            }
        }

        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ReportsViewModel viewModel && ReportComboBox.SelectedItem != null)
            {
                viewModel.GenerateReportCommand.Execute(ReportComboBox.SelectedItem.ToString());
            }
        }

        private void ExportExcelButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ReportsViewModel viewModel)
            {
                viewModel.ExportToExcelCommand.Execute(null);
            }
        }

        private void ExportPdfButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ReportsViewModel viewModel)
            {
                viewModel.ExportToPdfCommand.Execute(null);
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ReportsViewModel viewModel)
            {
                viewModel.PrintReportCommand.Execute(null);
            }
        }

        private void StartDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ReportsViewModel viewModel && StartDatePicker.SelectedDate.HasValue)
            {
                viewModel.StartDate = StartDatePicker.SelectedDate.Value;
            }
        }

        private void EndDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ReportsViewModel viewModel && EndDatePicker.SelectedDate.HasValue)
            {
                viewModel.EndDate = EndDatePicker.SelectedDate.Value;
            }
        }
    }
}

// ============================================================================
// FILE: SettingsView.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Settings/SettingsView.xaml.cs
// ============================================================================

using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Settings
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                viewModel.LoadSettingsCommand.Execute(null);
                viewModel.LoadUsersCommand.Execute(null);
            }
        }

        // Company Settings
        private void SaveCompanySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                viewModel.SaveCompanySettingsCommand.Execute(null);
            }
        }

        private void UploadLogoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp";
            
            if (openFileDialog.ShowDialog() == true)
            {
                if (DataContext is SettingsViewModel viewModel)
                {
                    viewModel.UploadLogoCommand.Execute(openFileDialog.FileName);
                }
            }
        }

        // User Management
        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                viewModel.OpenAddUserDialogCommand.Execute(null);
            }
        }

        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel && UserDataGrid.SelectedItem != null)
            {
                viewModel.OpenEditUserDialogCommand.Execute(UserDataGrid.SelectedItem);
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel && UserDataGrid.SelectedItem != null)
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد من حذف المستخدم؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    viewModel.DeleteUserCommand.Execute(UserDataGrid.SelectedItem);
                }
            }
        }

        // Password Change
        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                // Sync password fields
                viewModel.CurrentPassword = CurrentPasswordBox.Password;
                viewModel.NewPassword = NewPasswordBox.Password;
                viewModel.ConfirmPassword = ConfirmPasswordBox.Password;

                if (viewModel.ChangePasswordCommand.CanExecute(null))
                {
                    viewModel.ChangePasswordCommand.Execute(null);
                    
                    // Clear password fields
                    CurrentPasswordBox.Clear();
                    NewPasswordBox.Clear();
                    ConfirmPasswordBox.Clear();
                }
            }
        }

        private void CurrentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                viewModel.CurrentPassword = CurrentPasswordBox.Password;
            }
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                viewModel.NewPassword = NewPasswordBox.Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                viewModel.ConfirmPassword = ConfirmPasswordBox.Password;
            }
        }
    }
}

// ============================================================================
// FILE: UserFormDialog.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Settings/UserFormDialog.xaml.cs
// ============================================================================

using System.Windows;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Settings
{
    public partial class UserFormDialog : Window
    {
        public UserFormDialog()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UsernameTextBox?.Focus();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is UserFormViewModel viewModel)
            {
                // Sync password field
                if (PasswordBox != null)
                {
                    viewModel.Password = PasswordBox.Password;
                }

                if (viewModel.SaveCommand.CanExecute(null))
                {
                    viewModel.SaveCommand.Execute(null);
                    this.DialogResult = true;
                    this.Close();
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UserFormViewModel viewModel && PasswordBox != null)
            {
                viewModel.Password = PasswordBox.Password;
            }
        }
    }
}
