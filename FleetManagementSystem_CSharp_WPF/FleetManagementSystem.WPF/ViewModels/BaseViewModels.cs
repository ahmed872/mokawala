// ============================================================================
// FILE: BaseViewModel.cs
// LOCATION: FleetManagementSystem.WPF/ViewModels/BaseViewModel.cs
// PURPOSE: Base class for all ViewModels with INotifyPropertyChanged
// ============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FleetManagementSystem.WPF.ViewModels
{
    /// <summary>
    /// Base ViewModel class implementing INotifyPropertyChanged for all ViewModels
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        private bool _isLoading;
        private string _errorMessage;
        private string _infoMessage;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string InfoMessage
        {
            get => _infoMessage;
            set => SetProperty(ref _infoMessage, value);
        }

        /// <summary>
        /// Generic property setter with change notification
        /// </summary>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Raise PropertyChanged event
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Show error message to user
        /// </summary>
        protected void ShowError(string message)
        {
            ErrorMessage = message;
            InfoMessage = null;
        }

        /// <summary>
        /// Show info message to user
        /// </summary>
        protected void ShowInfo(string message)
        {
            InfoMessage = message;
            ErrorMessage = null;
        }

        /// <summary>
        /// Clear all messages
        /// </summary>
        protected void ClearMessages()
        {
            ErrorMessage = null;
            InfoMessage = null;
        }
    }
}

// ============================================================================
// FILE: RelayCommand.cs
// LOCATION: FleetManagementSystem.WPF/ViewModels/RelayCommand.cs
// PURPOSE: Synchronous command implementation
// ============================================================================

using System;
using System.Windows.Input;

namespace FleetManagementSystem.WPF.ViewModels
{
    /// <summary>
    /// Synchronous relay command implementation
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter) => _execute(parameter);
    }

    /// <summary>
    /// Generic synchronous relay command implementation
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (parameter is T t)
                return _canExecute?.Invoke(t) ?? true;
            return false;
        }

        public void Execute(object parameter)
        {
            if (parameter is T t)
                _execute(t);
        }
    }
}

// ============================================================================
// FILE: AsyncRelayCommand.cs
// LOCATION: FleetManagementSystem.WPF/ViewModels/AsyncRelayCommand.cs
// PURPOSE: Asynchronous command implementation
// ============================================================================

using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FleetManagementSystem.WPF.ViewModels
{
    /// <summary>
    /// Asynchronous relay command implementation
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;
        private bool _isExecuting;

        public event EventHandler CanExecuteChanged;

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => !_isExecuting && (_canExecute?.Invoke() ?? true);

        public async void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                try
                {
                    _isExecuting = true;
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                    await _execute();
                }
                finally
                {
                    _isExecuting = false;
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    /// <summary>
    /// Generic asynchronous relay command implementation
    /// </summary>
    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T, Task> _execute;
        private readonly Predicate<T> _canExecute;
        private bool _isExecuting;

        public event EventHandler CanExecuteChanged;

        public AsyncRelayCommand(Func<T, Task> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (parameter is T t)
                return !_isExecuting && (_canExecute?.Invoke(t) ?? true);
            return false;
        }

        public async void Execute(object parameter)
        {
            if (parameter is T t && CanExecute(parameter))
            {
                try
                {
                    _isExecuting = true;
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                    await _execute(t);
                }
                finally
                {
                    _isExecuting = false;
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}

// ============================================================================
// FILE: ListViewModelBase.cs
// LOCATION: FleetManagementSystem.WPF/ViewModels/ListViewModelBase.cs
// PURPOSE: Base class for list ViewModels with search, filter, pagination
// ============================================================================

using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FleetManagementSystem.WPF.ViewModels
{
    /// <summary>
    /// Base class for list ViewModels with common CRUD and filtering functionality
    /// </summary>
    public abstract class ListViewModelBase<T> : BaseViewModel
    {
        private ObservableCollection<T> _items;
        private string _searchText;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalCount;
        private T _selectedItem;

        public ObservableCollection<T> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    CurrentPage = 1;
                    OnSearchTextChanged();
                }
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public int PageSize
        {
            get => _pageSize;
            set => SetProperty(ref _pageSize, value);
        }

        public int TotalCount
        {
            get => _totalCount;
            set => SetProperty(ref _totalCount, value);
        }

        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

        public T SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public ICommand RefreshCommand { get; protected set; }
        public ICommand SearchCommand { get; protected set; }
        public ICommand AddCommand { get; protected set; }
        public ICommand EditCommand { get; protected set; }
        public ICommand DeleteCommand { get; protected set; }
        public ICommand PreviousPageCommand { get; protected set; }
        public ICommand NextPageCommand { get; protected set; }

        protected ListViewModelBase()
        {
            Items = new ObservableCollection<T>();
        }

        /// <summary>
        /// Called when search text changes - override in derived classes
        /// </summary>
        protected virtual void OnSearchTextChanged()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Load items from service - override in derived classes
        /// </summary>
        public abstract Task LoadItemsAsync();

        /// <summary>
        /// Delete item - override in derived classes
        /// </summary>
        public abstract Task DeleteItemAsync(T item);
    }
}

// ============================================================================
// FILE: FormViewModelBase.cs
// LOCATION: FleetManagementSystem.WPF/ViewModels/FormViewModelBase.cs
// PURPOSE: Base class for form/dialog ViewModels
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace FleetManagementSystem.WPF.ViewModels
{
    /// <summary>
    /// Base class for form/dialog ViewModels with validation
    /// </summary>
    public abstract class FormViewModelBase<T> : BaseViewModel
    {
        private T _entity;
        private Dictionary<string, List<string>> _errors;
        private bool _isNew;

        public T Entity
        {
            get => _entity;
            set => SetProperty(ref _entity, value);
        }

        public Dictionary<string, List<string>> Errors
        {
            get => _errors;
            set => SetProperty(ref _errors, value);
        }

        public bool IsNew
        {
            get => _isNew;
            set => SetProperty(ref _isNew, value);
        }

        public bool HasErrors => Errors?.Any(e => e.Value.Any()) ?? false;

        public ICommand SaveCommand { get; protected set; }
        public ICommand CancelCommand { get; protected set; }

        protected FormViewModelBase()
        {
            Errors = new Dictionary<string, List<string>>();
        }

        /// <summary>
        /// Validate the entity - override in derived classes
        /// </summary>
        public abstract bool ValidateEntity();

        /// <summary>
        /// Save the entity - override in derived classes
        /// </summary>
        public abstract Task SaveEntityAsync();

        /// <summary>
        /// Add validation error
        /// </summary>
        protected void AddError(string propertyName, string errorMessage)
        {
            if (!Errors.ContainsKey(propertyName))
                Errors[propertyName] = new List<string>();

            if (!Errors[propertyName].Contains(errorMessage))
                Errors[propertyName].Add(errorMessage);

            OnPropertyChanged(nameof(HasErrors));
        }

        /// <summary>
        /// Clear validation errors for a property
        /// </summary>
        protected void ClearErrors(string propertyName = null)
        {
            if (propertyName == null)
                Errors.Clear();
            else if (Errors.ContainsKey(propertyName))
                Errors.Remove(propertyName);

            OnPropertyChanged(nameof(HasErrors));
        }

        /// <summary>
        /// Get errors for a property
        /// </summary>
        public List<string> GetErrors(string propertyName)
        {
            return Errors.ContainsKey(propertyName) ? Errors[propertyName] : new List<string>();
        }
    }
}

// ============================================================================
// FILE: ValidationHelper.cs
// LOCATION: FleetManagementSystem.WPF/ViewModels/ValidationHelper.cs
// PURPOSE: Common validation methods
// ============================================================================

using System;
using System.Text.RegularExpressions;

namespace FleetManagementSystem.WPF.ViewModels
{
    /// <summary>
    /// Helper class for common validation logic
    /// </summary>
    public static class ValidationHelper
    {
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidPhone(string phone)
        {
            return Regex.IsMatch(phone, @"^\d{7,15}$");
        }

        public static bool IsValidPlateNumber(string plateNumber)
        {
            return !string.IsNullOrWhiteSpace(plateNumber) && plateNumber.Length >= 4;
        }

        public static bool IsValidContractNumber(string contractNumber)
        {
            return !string.IsNullOrWhiteSpace(contractNumber) && contractNumber.Length >= 3;
        }

        public static bool IsNotEmpty(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        public static bool IsValidDecimal(string value)
        {
            return decimal.TryParse(value, out _);
        }

        public static bool IsValidInteger(string value)
        {
            return int.TryParse(value, out _);
        }

        public static bool IsDateInFuture(DateTime date)
        {
            return date > DateTime.Now;
        }

        public static bool IsDateInPast(DateTime date)
        {
            return date < DateTime.Now;
        }

        public static bool IsDateRangeValid(DateTime startDate, DateTime endDate)
        {
            return startDate <= endDate;
        }
    }
}
