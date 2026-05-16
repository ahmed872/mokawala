// ============================================================================
// FILE: INavigationService.cs
// LOCATION: FleetManagementSystem.WPF/Services/INavigationService.cs
// ============================================================================

using System;
using System.Threading.Tasks;

namespace FleetManagementSystem.WPF.Services
{
    /// <summary>
    /// Navigation service interface for view switching and dialog management.
    /// </summary>
    public interface INavigationService
    {
        event EventHandler<NavigationEventArgs> NavigationRequested;
        event EventHandler<DialogEventArgs> DialogRequested;

        void NavigateTo(string viewName, object parameter = null);
        void ShowDialog(string dialogName, object parameter = null);
        void CloseDialog(string dialogName, bool? result = null);
        void NavigateToMainWindow();
        void NavigateToLoginWindow();
    }

    public class NavigationEventArgs : EventArgs
    {
        public string ViewName { get; set; }
        public object Parameter { get; set; }
    }

    public class DialogEventArgs : EventArgs
    {
        public string DialogName { get; set; }
        public object Parameter { get; set; }
        public bool? Result { get; set; }
    }
}

// ============================================================================
// FILE: NavigationService.cs
// LOCATION: FleetManagementSystem.WPF/Services/NavigationService.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace FleetManagementSystem.WPF.Services
{
    /// <summary>
    /// Navigation service implementation for managing view switching and dialogs.
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _viewRegistry;
        private readonly Dictionary<string, Type> _dialogRegistry;

        public event EventHandler<NavigationEventArgs> NavigationRequested;
        public event EventHandler<DialogEventArgs> DialogRequested;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _viewRegistry = new Dictionary<string, Type>();
            _dialogRegistry = new Dictionary<string, Type>();
            RegisterViews();
            RegisterDialogs();
        }

        private void RegisterViews()
        {
            _viewRegistry["Dashboard"] = typeof(DashboardView);
            _viewRegistry["Vehicles"] = typeof(VehicleListView);
            _viewRegistry["Contracts"] = typeof(ContractListView);
            _viewRegistry["Maintenance"] = typeof(MaintenanceListView);
            _viewRegistry["Drivers"] = typeof(DriverListView);
            _viewRegistry["Employees"] = typeof(EmployeeListView);
            _viewRegistry["Trips"] = typeof(TripListView);
            _viewRegistry["Fuel"] = typeof(FuelListView);
            _viewRegistry["Expenses"] = typeof(ExpenseListView);
            _viewRegistry["Licenses"] = typeof(LicenseListView);
            _viewRegistry["Insurance"] = typeof(InsuranceListView);
            _viewRegistry["Custody"] = typeof(CustodyListView);
            _viewRegistry["MasterData"] = typeof(MasterDataView);
            _viewRegistry["Reports"] = typeof(ReportsView);
            _viewRegistry["Settings"] = typeof(SettingsView);
        }

        private void RegisterDialogs()
        {
            _dialogRegistry["VehicleForm"] = typeof(VehicleFormDialog);
            _dialogRegistry["ContractForm"] = typeof(ContractFormDialog);
            _dialogRegistry["MaintenanceForm"] = typeof(MaintenanceFormDialog);
            _dialogRegistry["DriverForm"] = typeof(DriverFormDialog);
            _dialogRegistry["EmployeeForm"] = typeof(EmployeeFormDialog);
            _dialogRegistry["TripForm"] = typeof(TripFormDialog);
            _dialogRegistry["FuelForm"] = typeof(FuelFormDialog);
            _dialogRegistry["ExpenseForm"] = typeof(ExpenseFormDialog);
            _dialogRegistry["LicenseForm"] = typeof(LicenseFormDialog);
            _dialogRegistry["InsuranceForm"] = typeof(InsuranceFormDialog);
            _dialogRegistry["CustodyForm"] = typeof(CustodyFormDialog);
            _dialogRegistry["ServiceProviderForm"] = typeof(ServiceProviderFormDialog);
            _dialogRegistry["UserForm"] = typeof(UserFormDialog);
        }

        public void NavigateTo(string viewName, object parameter = null)
        {
            if (_viewRegistry.TryGetValue(viewName, out var viewType))
            {
                NavigationRequested?.Invoke(this, new NavigationEventArgs
                {
                    ViewName = viewName,
                    Parameter = parameter
                });
            }
            else
            {
                throw new ArgumentException($"View '{viewName}' not registered", nameof(viewName));
            }
        }

        public void ShowDialog(string dialogName, object parameter = null)
        {
            if (_dialogRegistry.TryGetValue(dialogName, out var dialogType))
            {
                DialogRequested?.Invoke(this, new DialogEventArgs
                {
                    DialogName = dialogName,
                    Parameter = parameter
                });
            }
            else
            {
                throw new ArgumentException($"Dialog '{dialogName}' not registered", nameof(dialogName));
            }
        }

        public void CloseDialog(string dialogName, bool? result = null)
        {
            DialogRequested?.Invoke(this, new DialogEventArgs
            {
                DialogName = dialogName,
                Result = result
            });
        }

        public void NavigateToMainWindow()
        {
            NavigationRequested?.Invoke(this, new NavigationEventArgs
            {
                ViewName = "MainWindow"
            });
        }

        public void NavigateToLoginWindow()
        {
            NavigationRequested?.Invoke(this, new NavigationEventArgs
            {
                ViewName = "LoginWindow"
            });
        }
    }
}

// ============================================================================
// FILE: IDialogService.cs
// LOCATION: FleetManagementSystem.WPF/Services/IDialogService.cs
// ============================================================================

using System.Threading.Tasks;

namespace FleetManagementSystem.WPF.Services
{
    /// <summary>
    /// Dialog service interface for showing message boxes and dialogs.
    /// </summary>
    public interface IDialogService
    {
        Task<bool> ShowConfirmationAsync(string title, string message);
        Task ShowInformationAsync(string title, string message);
        Task ShowErrorAsync(string title, string message);
        Task ShowWarningAsync(string title, string message);
        Task<string> ShowInputAsync(string title, string message, string defaultValue = "");
        Task<T> ShowSelectionAsync<T>(string title, string message, System.Collections.Generic.List<T> items);
    }
}

// ============================================================================
// FILE: DialogService.cs
// LOCATION: FleetManagementSystem.WPF/Services/DialogService.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace FleetManagementSystem.WPF.Services
{
    /// <summary>
    /// Dialog service implementation for showing message boxes and dialogs.
    /// </summary>
    public class DialogService : IDialogService
    {
        public async Task<bool> ShowConfirmationAsync(string title, string message)
        {
            return await Task.Run(() =>
            {
                var result = MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );
                return result == MessageBoxResult.Yes;
            });
        }

        public async Task ShowInformationAsync(string title, string message)
        {
            await Task.Run(() =>
            {
                MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            });
        }

        public async Task ShowErrorAsync(string title, string message)
        {
            await Task.Run(() =>
            {
                MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            });
        }

        public async Task ShowWarningAsync(string title, string message)
        {
            await Task.Run(() =>
            {
                MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            });
        }

        public async Task<string> ShowInputAsync(string title, string message, string defaultValue = "")
        {
            return await Task.Run(() =>
            {
                // For simplicity, using InputBox-like functionality
                // In production, implement a custom InputDialog
                return defaultValue;
            });
        }

        public async Task<T> ShowSelectionAsync<T>(string title, string message, List<T> items)
        {
            return await Task.Run(() =>
            {
                // For simplicity, returning first item
                // In production, implement a custom SelectionDialog
                return items.Count > 0 ? items[0] : default(T);
            });
        }
    }
}

// ============================================================================
// FILE: IMessageService.cs & MessageService.cs
// LOCATION: FleetManagementSystem.WPF/Services/MessageService.cs
// ============================================================================

using System;
using System.Threading.Tasks;

namespace FleetManagementSystem.WPF.Services
{
    public interface IMessageService
    {
        void ShowMessage(string message, string title = "إشعار");
        void ShowError(string message, string title = "خطأ");
        void ShowWarning(string message, string title = "تحذير");
        void ShowSuccess(string message, string title = "نجاح");
    }

    public class MessageService : IMessageService
    {
        public void ShowMessage(string message, string title = "إشعار")
        {
            System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        public void ShowError(string message, string title = "خطأ")
        {
            System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }

        public void ShowWarning(string message, string title = "تحذير")
        {
            System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
        }

        public void ShowSuccess(string message, string title = "نجاح")
        {
            System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
    }
}

// ============================================================================
// FILE: IProgressService.cs & ProgressService.cs
// LOCATION: FleetManagementSystem.WPF/Services/ProgressService.cs
// ============================================================================

using System;
using System.Threading.Tasks;

namespace FleetManagementSystem.WPF.Services
{
    public interface IProgressService
    {
        void ShowProgress(string message);
        void HideProgress();
        void UpdateProgress(int percentage, string message = "");
    }

    public class ProgressService : IProgressService
    {
        private bool _isShowing = false;

        public void ShowProgress(string message)
        {
            _isShowing = true;
            // In production, show progress dialog or overlay
        }

        public void HideProgress()
        {
            _isShowing = false;
            // In production, hide progress dialog or overlay
        }

        public void UpdateProgress(int percentage, string message = "")
        {
            if (_isShowing)
            {
                // In production, update progress bar and message
            }
        }
    }
}

// ============================================================================
// FILE: ISessionManager.cs & SessionManager.cs
// LOCATION: FleetManagementSystem.WPF/Services/SessionManager.cs
// ============================================================================

using System;
using FleetManagementSystem.Core.DTOs;

namespace FleetManagementSystem.WPF.Services
{
    public interface ISessionManager
    {
        UserDto CurrentUser { get; }
        bool IsAuthenticated { get; }
        void SetCurrentUser(UserDto user);
        void ClearSession();
        bool HasPermission(string permission);
        bool IsAdmin { get; }
        bool IsStaff { get; }
    }

    public class SessionManager : ISessionManager
    {
        private UserDto _currentUser;

        public UserDto CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;
        public bool IsAdmin => _currentUser?.Role == "Admin";
        public bool IsStaff => _currentUser?.Role == "Staff";

        public void SetCurrentUser(UserDto user)
        {
            _currentUser = user;
        }

        public void ClearSession()
        {
            _currentUser = null;
        }

        public bool HasPermission(string permission)
        {
            if (!IsAuthenticated)
                return false;

            // Admin has all permissions
            if (IsAdmin)
                return true;

            // Staff has limited permissions
            // Implement permission checking logic here
            return true;
        }
    }
}
