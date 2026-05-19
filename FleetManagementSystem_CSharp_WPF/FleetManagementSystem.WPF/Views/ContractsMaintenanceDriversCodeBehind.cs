// ============================================================================
// FILE: ContractListView.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Contracts/ContractListView.xaml.cs
// ============================================================================

using System.Windows;
using System.Windows.Controls;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Contracts
{
    public partial class ContractListView : UserControl
    {
        public ContractListView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ContractViewModel viewModel)
            {
                viewModel.LoadContractsCommand.Execute(null);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ContractViewModel viewModel)
            {
                viewModel.OpenAddDialogCommand.Execute(null);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ContractViewModel viewModel && ContractDataGrid.SelectedItem != null)
            {
                viewModel.OpenEditDialogCommand.Execute(ContractDataGrid.SelectedItem);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ContractViewModel viewModel && ContractDataGrid.SelectedItem != null)
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد من حذف هذا العقد؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    viewModel.DeleteContractCommand.Execute(ContractDataGrid.SelectedItem);
                }
            }
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ContractViewModel viewModel && StatusComboBox.SelectedItem != null)
            {
                viewModel.FilterByStatusCommand.Execute(StatusComboBox.SelectedItem.ToString());
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is ContractViewModel viewModel)
            {
                viewModel.SearchContractsCommand.Execute(SearchTextBox.Text);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ContractViewModel viewModel)
            {
                viewModel.LoadContractsCommand.Execute(null);
            }
        }
    }
}

// ============================================================================
// FILE: ContractFormDialog.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Contracts/ContractFormDialog.xaml.cs
// ============================================================================

using System.Windows;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Contracts
{
    public partial class ContractFormDialog : Window
    {
        public ContractFormDialog()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ContractNumberTextBox?.Focus();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ContractFormViewModel viewModel)
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
// FILE: MaintenanceListView.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Maintenance/MaintenanceListView.xaml.cs
// ============================================================================

using System.Windows;
using System.Windows.Controls;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Maintenance
{
    public partial class MaintenanceListView : UserControl
    {
        public MaintenanceListView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MaintenanceViewModel viewModel)
            {
                viewModel.LoadMaintenanceRequestsCommand.Execute(null);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MaintenanceViewModel viewModel)
            {
                viewModel.OpenAddDialogCommand.Execute(null);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MaintenanceViewModel viewModel && MaintenanceDataGrid.SelectedItem != null)
            {
                viewModel.OpenEditDialogCommand.Execute(MaintenanceDataGrid.SelectedItem);
            }
        }

        private void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MaintenanceViewModel viewModel && MaintenanceDataGrid.SelectedItem != null)
            {
                viewModel.MarkAsCompleteCommand.Execute(MaintenanceDataGrid.SelectedItem);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MaintenanceViewModel viewModel && MaintenanceDataGrid.SelectedItem != null)
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد من حذف طلب الصيانة؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    viewModel.DeleteMaintenanceRequestCommand.Execute(MaintenanceDataGrid.SelectedItem);
                }
            }
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MaintenanceViewModel viewModel && StatusComboBox.SelectedItem != null)
            {
                viewModel.FilterByStatusCommand.Execute(StatusComboBox.SelectedItem.ToString());
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MaintenanceViewModel viewModel)
            {
                viewModel.LoadMaintenanceRequestsCommand.Execute(null);
            }
        }
    }
}

// ============================================================================
// FILE: MaintenanceFormDialog.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Maintenance/MaintenanceFormDialog.xaml.cs
// ============================================================================

using System.Windows;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Maintenance
{
    public partial class MaintenanceFormDialog : Window
    {
        public MaintenanceFormDialog()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            VehicleComboBox?.Focus();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MaintenanceFormViewModel viewModel)
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
// FILE: DriverListView.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Drivers/DriverListView.xaml.cs
// ============================================================================

using System.Windows;
using System.Windows.Controls;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Drivers
{
    public partial class DriverListView : UserControl
    {
        public DriverListView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is DriverViewModel viewModel)
            {
                viewModel.LoadDriversCommand.Execute(null);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DriverViewModel viewModel)
            {
                viewModel.OpenAddDialogCommand.Execute(null);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DriverViewModel viewModel && DriverDataGrid.SelectedItem != null)
            {
                viewModel.OpenEditDialogCommand.Execute(DriverDataGrid.SelectedItem);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DriverViewModel viewModel && DriverDataGrid.SelectedItem != null)
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد من حذف السائق؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    viewModel.DeleteDriverCommand.Execute(DriverDataGrid.SelectedItem);
                }
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is DriverViewModel viewModel)
            {
                viewModel.SearchDriversCommand.Execute(SearchTextBox.Text);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DriverViewModel viewModel)
            {
                viewModel.LoadDriversCommand.Execute(null);
            }
        }
    }
}

// ============================================================================
// FILE: DriverFormDialog.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Drivers/DriverFormDialog.xaml.cs
// ============================================================================

using System.Windows;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Drivers
{
    public partial class DriverFormDialog : Window
    {
        public DriverFormDialog()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FullNameTextBox?.Focus();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DriverFormViewModel viewModel)
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
