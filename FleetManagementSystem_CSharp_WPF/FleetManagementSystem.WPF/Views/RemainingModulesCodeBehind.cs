// ============================================================================
// FILE: EmployeeListView.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Employees/EmployeeListView.xaml.cs
// ============================================================================

using System.Windows;
using System.Windows.Controls;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Employees
{
    public partial class EmployeeListView : UserControl
    {
        public EmployeeListView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is EmployeeViewModel viewModel)
            {
                viewModel.LoadEmployeesCommand.Execute(null);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EmployeeViewModel viewModel)
            {
                viewModel.OpenAddDialogCommand.Execute(null);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EmployeeViewModel viewModel && EmployeeDataGrid.SelectedItem != null)
            {
                viewModel.OpenEditDialogCommand.Execute(EmployeeDataGrid.SelectedItem);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EmployeeViewModel viewModel && EmployeeDataGrid.SelectedItem != null)
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد من حذف الموظف؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    viewModel.DeleteEmployeeCommand.Execute(EmployeeDataGrid.SelectedItem);
                }
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is EmployeeViewModel viewModel)
            {
                viewModel.SearchEmployeesCommand.Execute(SearchTextBox.Text);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EmployeeViewModel viewModel)
            {
                viewModel.LoadEmployeesCommand.Execute(null);
            }
        }
    }
}

// ============================================================================
// FILE: EmployeeFormDialog.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Employees/EmployeeFormDialog.xaml.cs
// ============================================================================

using System.Windows;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Employees
{
    public partial class EmployeeFormDialog : Window
    {
        public EmployeeFormDialog()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EmployeeFormViewModel viewModel)
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
// FILE: TripListView.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Trips/TripListView.xaml.cs
// ============================================================================

using System.Windows;
using System.Windows.Controls;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Trips
{
    public partial class TripListView : UserControl
    {
        public TripListView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is TripViewModel viewModel)
            {
                viewModel.LoadTripsCommand.Execute(null);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is TripViewModel viewModel)
            {
                viewModel.OpenAddDialogCommand.Execute(null);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is TripViewModel viewModel && TripDataGrid.SelectedItem != null)
            {
                viewModel.OpenEditDialogCommand.Execute(TripDataGrid.SelectedItem);
            }
        }

        private void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is TripViewModel viewModel && TripDataGrid.SelectedItem != null)
            {
                viewModel.MarkAsCompleteCommand.Execute(TripDataGrid.SelectedItem);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is TripViewModel viewModel)
            {
                viewModel.LoadTripsCommand.Execute(null);
            }
        }
    }
}

// ============================================================================
// FILE: TripFormDialog.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Trips/TripFormDialog.xaml.cs
// ============================================================================

using System.Windows;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Trips
{
    public partial class TripFormDialog : Window
    {
        public TripFormDialog()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is TripFormViewModel viewModel)
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
// FILE: FuelListView.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Fuel/FuelListView.xaml.cs
// ============================================================================

using System.Windows;
using System.Windows.Controls;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Fuel
{
    public partial class FuelListView : UserControl
    {
        public FuelListView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is FuelViewModel viewModel)
            {
                viewModel.LoadFuelTransactionsCommand.Execute(null);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FuelViewModel viewModel)
            {
                viewModel.OpenAddDialogCommand.Execute(null);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FuelViewModel viewModel && FuelDataGrid.SelectedItem != null)
            {
                viewModel.OpenEditDialogCommand.Execute(FuelDataGrid.SelectedItem);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FuelViewModel viewModel && FuelDataGrid.SelectedItem != null)
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد من حذف معاملة الوقود؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    viewModel.DeleteFuelTransactionCommand.Execute(FuelDataGrid.SelectedItem);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FuelViewModel viewModel)
            {
                viewModel.LoadFuelTransactionsCommand.Execute(null);
            }
        }
    }
}

// ============================================================================
// FILE: FuelFormDialog.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Fuel/FuelFormDialog.xaml.cs
// ============================================================================

using System.Windows;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Fuel
{
    public partial class FuelFormDialog : Window
    {
        public FuelFormDialog()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FuelFormViewModel viewModel)
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
// FILE: ExpenseListView.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Expenses/ExpenseListView.xaml.cs
// ============================================================================

using System.Windows;
using System.Windows.Controls;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Expenses
{
    public partial class ExpenseListView : UserControl
    {
        public ExpenseListView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ExpenseViewModel viewModel)
            {
                viewModel.LoadExpensesCommand.Execute(null);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ExpenseViewModel viewModel)
            {
                viewModel.OpenAddDialogCommand.Execute(null);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ExpenseViewModel viewModel && ExpenseDataGrid.SelectedItem != null)
            {
                viewModel.OpenEditDialogCommand.Execute(ExpenseDataGrid.SelectedItem);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ExpenseViewModel viewModel && ExpenseDataGrid.SelectedItem != null)
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد من حذف المصروف؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    viewModel.DeleteExpenseCommand.Execute(ExpenseDataGrid.SelectedItem);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ExpenseViewModel viewModel)
            {
                viewModel.LoadExpensesCommand.Execute(null);
            }
        }
    }
}

// ============================================================================
// FILE: ExpenseFormDialog.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Expenses/ExpenseFormDialog.xaml.cs
// ============================================================================

using System.Windows;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Expenses
{
    public partial class ExpenseFormDialog : Window
    {
        public ExpenseFormDialog()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ExpenseFormViewModel viewModel)
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
// FILE: LicenseListView.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Licenses/LicenseListView.xaml.cs
// ============================================================================

using System.Windows;
using System.Windows.Controls;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Licenses
{
    public partial class LicenseListView : UserControl
    {
        public LicenseListView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is LicenseViewModel viewModel)
            {
                viewModel.LoadLicensesCommand.Execute(null);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LicenseViewModel viewModel)
            {
                viewModel.OpenAddDialogCommand.Execute(null);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LicenseViewModel viewModel && LicenseDataGrid.SelectedItem != null)
            {
                viewModel.OpenEditDialogCommand.Execute(LicenseDataGrid.SelectedItem);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LicenseViewModel viewModel)
            {
                viewModel.LoadLicensesCommand.Execute(null);
            }
        }
    }
}

// ============================================================================
// FILE: LicenseFormDialog.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Licenses/LicenseFormDialog.xaml.cs
// ============================================================================

using System.Windows;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Licenses
{
    public partial class LicenseFormDialog : Window
    {
        public LicenseFormDialog()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LicenseFormViewModel viewModel)
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
// FILE: InsuranceListView.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Insurance/InsuranceListView.xaml.cs
// ============================================================================

using System.Windows;
using System.Windows.Controls;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Insurance
{
    public partial class InsuranceListView : UserControl
    {
        public InsuranceListView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is InsuranceViewModel viewModel)
            {
                viewModel.LoadInsurancePoliciesCommand.Execute(null);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is InsuranceViewModel viewModel)
            {
                viewModel.OpenAddDialogCommand.Execute(null);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is InsuranceViewModel viewModel && InsuranceDataGrid.SelectedItem != null)
            {
                viewModel.OpenEditDialogCommand.Execute(InsuranceDataGrid.SelectedItem);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is InsuranceViewModel viewModel)
            {
                viewModel.LoadInsurancePoliciesCommand.Execute(null);
            }
        }
    }
}

// ============================================================================
// FILE: InsuranceFormDialog.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Insurance/InsuranceFormDialog.xaml.cs
// ============================================================================

using System.Windows;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Insurance
{
    public partial class InsuranceFormDialog : Window
    {
        public InsuranceFormDialog()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is InsuranceFormViewModel viewModel)
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
// FILE: CustodyListView.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Custody/CustodyListView.xaml.cs
// ============================================================================

using System.Windows;
using System.Windows.Controls;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Custody
{
    public partial class CustodyListView : UserControl
    {
        public CustodyListView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is CustodyViewModel viewModel)
            {
                viewModel.LoadCustodyRecordsCommand.Execute(null);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is CustodyViewModel viewModel)
            {
                viewModel.OpenAddDialogCommand.Execute(null);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is CustodyViewModel viewModel && CustodyDataGrid.SelectedItem != null)
            {
                viewModel.OpenEditDialogCommand.Execute(CustodyDataGrid.SelectedItem);
            }
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is CustodyViewModel viewModel && CustodyDataGrid.SelectedItem != null)
            {
                viewModel.MarkAsReturnedCommand.Execute(CustodyDataGrid.SelectedItem);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is CustodyViewModel viewModel)
            {
                viewModel.LoadCustodyRecordsCommand.Execute(null);
            }
        }
    }
}

// ============================================================================
// FILE: CustodyFormDialog.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Custody/CustodyFormDialog.xaml.cs
// ============================================================================

using System.Windows;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Custody
{
    public partial class CustodyFormDialog : Window
    {
        public CustodyFormDialog()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is CustodyFormViewModel viewModel)
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
