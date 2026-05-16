// ============================================================================
// FILE: LoginWindow.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/LoginWindow.xaml.cs
// ============================================================================

using System.Windows;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set DataContext to LoginViewModel
            if (DataContext is LoginViewModel viewModel)
            {
                // Focus on username field
                UsernameTextBox?.Focus();
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Bind password to ViewModel
            if (DataContext is LoginViewModel viewModel && sender is System.Windows.Controls.PasswordBox passwordBox)
            {
                viewModel.Password = passwordBox.Password;
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Ensure password is synced before login
            if (DataContext is LoginViewModel viewModel && PasswordBox != null)
            {
                viewModel.Password = PasswordBox.Password;
                
                // Execute login command
                if (viewModel.LoginCommand.CanExecute(null))
                {
                    viewModel.LoginCommand.Execute(null);
                }
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Allow Enter key to trigger login
            if (e.Key == System.Windows.Input.Key.Return)
            {
                LoginButton_Click(null, null);
                e.Handled = true;
            }
        }
    }
}

// ============================================================================
// FILE: MainWindow.xaml.cs
// LOCATION: FleetManagementSystem.WPF/MainWindow.xaml.cs
// ============================================================================

using System;
using System.Windows;
using System.Windows.Controls;
using FleetManagementSystem.WPF.ViewModels;
using FleetManagementSystem.WPF.Views;

namespace FleetManagementSystem.WPF
{
    public partial class MainWindow : Window
    {
        private readonly INavigationService _navigationService;

        public MainWindow(INavigationService navigationService)
        {
            InitializeComponent();
            _navigationService = navigationService;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set DataContext to MainWindowViewModel
            if (DataContext is MainWindowViewModel viewModel)
            {
                // Subscribe to navigation events
                _navigationService.NavigationRequested += OnNavigationRequested;
                _navigationService.DialogRequested += OnDialogRequested;

                // Load initial dashboard
                viewModel.NavigateToDashboardCommand.Execute(null);
            }
        }

        private void OnNavigationRequested(object sender, NavigationEventArgs e)
        {
            // Clear previous content
            ContentArea.Content = null;

            // Load new view
            var view = _navigationService.GetView(e.ViewName);
            if (view != null)
            {
                // Set DataContext from service provider if available
                var viewModel = _navigationService.GetViewModel(e.ViewName);
                if (viewModel != null)
                {
                    view.DataContext = viewModel;
                }

                ContentArea.Content = view;

                // Update sidebar selection
                UpdateSidebarSelection(e.ViewName);
            }
        }

        private void OnDialogRequested(object sender, DialogEventArgs e)
        {
            // Get dialog view and ViewModel from service
            var dialog = _navigationService.GetDialog(e.DialogName) as Window;
            if (dialog != null)
            {
                var viewModel = _navigationService.GetDialogViewModel(e.DialogName);
                if (viewModel != null)
                {
                    dialog.DataContext = viewModel;
                }

                // Show dialog
                dialog.Owner = this;
                dialog.ShowDialog();
            }
        }

        private void UpdateSidebarSelection(string viewName)
        {
            // Update sidebar menu selection based on view name
            if (SidebarMenu != null)
            {
                foreach (MenuItem item in SidebarMenu.Items)
                {
                    if (item.Tag?.ToString() == viewName)
                    {
                        item.IsSelected = true;
                    }
                    else
                    {
                        item.IsSelected = false;
                    }
                }
            }
        }

        private void SidebarMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Handle sidebar menu click
            if (sender is MenuItem menuItem && menuItem.Tag is string viewName)
            {
                _navigationService.Navigate(viewName);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle logout
            if (DataContext is MainWindowViewModel viewModel)
            {
                if (viewModel.LogoutCommand.CanExecute(null))
                {
                    viewModel.LogoutCommand.Execute(null);
                    
                    // Return to login window
                    LoginWindow loginWindow = new LoginWindow();
                    loginWindow.Show();
                    this.Close();
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Unsubscribe from events
            if (_navigationService != null)
            {
                _navigationService.NavigationRequested -= OnNavigationRequested;
                _navigationService.DialogRequested -= OnDialogRequested;
            }
        }
    }

    // Navigation event arguments
    public class NavigationEventArgs : EventArgs
    {
        public string ViewName { get; set; }
    }

    public class DialogEventArgs : EventArgs
    {
        public string DialogName { get; set; }
    }

    // Navigation service interface
    public interface INavigationService
    {
        event EventHandler<NavigationEventArgs> NavigationRequested;
        event EventHandler<DialogEventArgs> DialogRequested;

        void Navigate(string viewName);
        void ShowDialog(string dialogName);
        object GetView(string viewName);
        object GetViewModel(string viewName);
        object GetDialog(string dialogName);
        object GetDialogViewModel(string dialogName);
    }
}

// ============================================================================
// FILE: DashboardView.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/DashboardView.xaml.cs
// ============================================================================

using System.Windows;
using System.Windows.Controls;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize dashboard data if needed
            if (DataContext is DashboardViewModel viewModel)
            {
                viewModel.LoadDashboardCommand.Execute(null);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // Refresh dashboard data
            if (DataContext is DashboardViewModel viewModel)
            {
                viewModel.LoadDashboardCommand.Execute(null);
            }
        }

        private void KPICard_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Handle KPI card click to navigate to module
            if (sender is Border card && card.Tag is string moduleName)
            {
                // Navigate to module (handled by ViewModel)
                if (DataContext is DashboardViewModel viewModel)
                {
                    // Create navigation command based on module name
                    switch (moduleName)
                    {
                        case "Vehicles":
                            viewModel.NavigateToVehiclesCommand?.Execute(null);
                            break;
                        case "Contracts":
                            viewModel.NavigateToContractsCommand?.Execute(null);
                            break;
                        case "Maintenance":
                            viewModel.NavigateToMaintenanceCommand?.Execute(null);
                            break;
                        // Add other modules as needed
                    }
                }
            }
        }
    }
}

// ============================================================================
// FILE: VehicleListView.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Vehicles/VehicleListView.xaml.cs
// ============================================================================

using System.Windows;
using System.Windows.Controls;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Vehicles
{
    public partial class VehicleListView : UserControl
    {
        public VehicleListView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Load vehicles on view load
            if (DataContext is VehicleViewModel viewModel)
            {
                viewModel.LoadVehiclesCommand.Execute(null);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Open add vehicle dialog
            if (DataContext is VehicleViewModel viewModel)
            {
                viewModel.OpenAddDialogCommand.Execute(null);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // Open edit vehicle dialog
            if (DataContext is VehicleViewModel viewModel && VehicleDataGrid.SelectedItem != null)
            {
                viewModel.OpenEditDialogCommand.Execute(VehicleDataGrid.SelectedItem);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Delete selected vehicle
            if (DataContext is VehicleViewModel viewModel && VehicleDataGrid.SelectedItem != null)
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد من حذف هذه السيارة؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    viewModel.DeleteVehicleCommand.Execute(VehicleDataGrid.SelectedItem);
                }
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Search vehicles
            if (DataContext is VehicleViewModel viewModel)
            {
                viewModel.SearchVehiclesCommand.Execute(SearchTextBox.Text);
            }
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Filter by status
            if (DataContext is VehicleViewModel viewModel && StatusComboBox.SelectedItem != null)
            {
                viewModel.FilterByStatusCommand.Execute(StatusComboBox.SelectedItem.ToString());
            }
        }

        private void VehicleDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Open edit dialog on double-click
            if (DataContext is VehicleViewModel viewModel && VehicleDataGrid.SelectedItem != null)
            {
                viewModel.OpenEditDialogCommand.Execute(VehicleDataGrid.SelectedItem);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // Refresh vehicle list
            if (DataContext is VehicleViewModel viewModel)
            {
                viewModel.LoadVehiclesCommand.Execute(null);
            }
        }
    }
}

// ============================================================================
// FILE: VehicleFormDialog.xaml.cs
// LOCATION: FleetManagementSystem.WPF/Views/Vehicles/VehicleFormDialog.xaml.cs
// ============================================================================

using System.Windows;
using FleetManagementSystem.WPF.ViewModels;

namespace FleetManagementSystem.WPF.Views.Vehicles
{
    public partial class VehicleFormDialog : Window
    {
        public VehicleFormDialog()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Focus on first input field
            PlateNumberTextBox?.Focus();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Save vehicle
            if (DataContext is VehicleFormViewModel viewModel)
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
            // Cancel and close
            this.DialogResult = false;
            this.Close();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Allow Escape to close
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                CancelButton_Click(null, null);
                e.Handled = true;
            }
        }
    }
}
