// ============================================================================
// FILE: EmployeeViewModel.cs
// LOCATION: FleetManagementSystem.WPF/ViewModels/EmployeeViewModel.cs
// PURPOSE: Employee list and form ViewModel with role management
// ============================================================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Core.Services;

namespace FleetManagementSystem.WPF.ViewModels
{
    public class EmployeeViewModel : ListViewModelBase<EmployeeDto>
    {
        private readonly IEmployeeService _employeeService;
        private ObservableCollection<string> _departments;
        private string _selectedDepartment;
        private EmployeeFormViewModel _formViewModel;
        private bool _showForm;

        public ObservableCollection<string> Departments
        {
            get => _departments;
            set => SetProperty(ref _departments, value);
        }

        public string SelectedDepartment
        {
            get => _selectedDepartment;
            set
            {
                if (SetProperty(ref _selectedDepartment, value))
                {
                    CurrentPage = 1;
                    _ = LoadItemsAsync();
                }
            }
        }

        public EmployeeFormViewModel FormViewModel
        {
            get => _formViewModel;
            set => SetProperty(ref _formViewModel, value);
        }

        public bool ShowForm
        {
            get => _showForm;
            set => SetProperty(ref _showForm, value);
        }

        public EmployeeViewModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
            Departments = new ObservableCollection<string>();
            FormViewModel = new EmployeeFormViewModel(employeeService);

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            RefreshCommand = new AsyncRelayCommand(LoadItemsAsync);
            SearchCommand = new AsyncRelayCommand(LoadItemsAsync);
            AddCommand = new RelayCommand(_ => OpenAddForm());
            EditCommand = new RelayCommand<EmployeeDto>(emp => OpenEditForm(emp), _ => SelectedItem != null);
            DeleteCommand = new AsyncRelayCommand<EmployeeDto>(DeleteItemAsync, _ => SelectedItem != null);
        }

        public override async Task LoadItemsAsync()
        {
            IsLoading = true;
            ClearMessages();

            try
            {
                var result = await _employeeService.GetEmployeesAsync(
                    searchText: SearchText,
                    department: SelectedDepartment,
                    pageNumber: CurrentPage,
                    pageSize: PageSize);

                Items.Clear();
                foreach (var employee in result.Items)
                {
                    Items.Add(employee);
                }

                TotalCount = result.TotalCount;
                ShowInfo($"تم تحميل {Items.Count} موظف");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تحميل الموظفين: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public override async Task DeleteItemAsync(EmployeeDto employee)
        {
            if (employee == null) return;

            var confirmed = MessageBox.Show(
                $"هل تريد حذف الموظف {employee.FullName}؟",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmed == MessageBoxResult.Yes)
            {
                IsLoading = true;
                try
                {
                    await _employeeService.DeleteEmployeeAsync(employee.Id);
                    Items.Remove(employee);
                    ShowInfo("تم حذف الموظف بنجاح");
                    await LoadItemsAsync();
                }
                catch (Exception ex)
                {
                    ShowError($"خطأ في حذف الموظف: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private void OpenAddForm()
        {
            FormViewModel.IsNew = true;
            FormViewModel.Entity = new EmployeeDto();
            ShowForm = true;
        }

        private void OpenEditForm(EmployeeDto employee)
        {
            if (employee == null) return;
            FormViewModel.IsNew = false;
            FormViewModel.Entity = employee;
            ShowForm = true;
        }
    }

    public class EmployeeFormViewModel : FormViewModelBase<EmployeeDto>
    {
        private readonly IEmployeeService _employeeService;
        private ObservableCollection<string> _departments;
        private ObservableCollection<string> _roles;

        public ObservableCollection<string> Departments
        {
            get => _departments;
            set => SetProperty(ref _departments, value);
        }

        public ObservableCollection<string> Roles
        {
            get => _roles;
            set => SetProperty(ref _roles, value);
        }

        public EmployeeFormViewModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
            Departments = new ObservableCollection<string>();
            Roles = new ObservableCollection<string>();

            SaveCommand = new AsyncRelayCommand(SaveEntityAsync, () => !HasErrors);
            CancelCommand = new RelayCommand(_ => OnCancel());
        }

        public override bool ValidateEntity()
        {
            ClearErrors();
            bool isValid = true;

            if (!ValidationHelper.IsNotEmpty(Entity?.FullName))
            {
                AddError(nameof(Entity.FullName), "الاسم الكامل مطلوب");
                isValid = false;
            }

            if (!ValidationHelper.IsValidEmail(Entity?.Email))
            {
                AddError(nameof(Entity.Email), "البريد الإلكتروني غير صحيح");
                isValid = false;
            }

            if (!ValidationHelper.IsValidPhone(Entity?.PhoneNumber))
            {
                AddError(nameof(Entity.PhoneNumber), "رقم الهاتف غير صحيح");
                isValid = false;
            }

            return isValid;
        }

        public override async Task SaveEntityAsync()
        {
            if (!ValidateEntity())
            {
                ShowError("يرجى تصحيح الأخطاء أعلاه");
                return;
            }

            IsLoading = true;
            ClearMessages();

            try
            {
                if (IsNew)
                {
                    await _employeeService.CreateEmployeeAsync(Entity);
                    ShowInfo("تم إضافة الموظف بنجاح");
                }
                else
                {
                    await _employeeService.UpdateEmployeeAsync(Entity);
                    ShowInfo("تم تحديث الموظف بنجاح");
                }

                OnSave();
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في حفظ الموظف: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnSave() { }
        private void OnCancel() { }
    }
}

// ============================================================================
// FILE: TripViewModel.cs
// LOCATION: FleetManagementSystem.WPF/ViewModels/TripViewModel.cs
// PURPOSE: Trip/Operations list and form ViewModel
// ============================================================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Core.Services;

namespace FleetManagementSystem.WPF.ViewModels
{
    public class TripViewModel : ListViewModelBase<TripDto>
    {
        private readonly ITripService _tripService;
        private ObservableCollection<string> _tripStatuses;
        private string _selectedStatus;
        private TripFormViewModel _formViewModel;
        private bool _showForm;

        public ObservableCollection<string> TripStatuses
        {
            get => _tripStatuses;
            set => SetProperty(ref _tripStatuses, value);
        }

        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (SetProperty(ref _selectedStatus, value))
                {
                    CurrentPage = 1;
                    _ = LoadItemsAsync();
                }
            }
        }

        public TripFormViewModel FormViewModel
        {
            get => _formViewModel;
            set => SetProperty(ref _formViewModel, value);
        }

        public bool ShowForm
        {
            get => _showForm;
            set => SetProperty(ref _showForm, value);
        }

        public TripViewModel(ITripService tripService)
        {
            _tripService = tripService;
            TripStatuses = new ObservableCollection<string>();
            FormViewModel = new TripFormViewModel(tripService);

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            RefreshCommand = new AsyncRelayCommand(LoadItemsAsync);
            SearchCommand = new AsyncRelayCommand(LoadItemsAsync);
            AddCommand = new RelayCommand(_ => OpenAddForm());
            EditCommand = new RelayCommand<TripDto>(trip => OpenEditForm(trip), _ => SelectedItem != null);
            DeleteCommand = new AsyncRelayCommand<TripDto>(DeleteItemAsync, _ => SelectedItem != null);
        }

        public override async Task LoadItemsAsync()
        {
            IsLoading = true;
            ClearMessages();

            try
            {
                var result = await _tripService.GetTripsAsync(
                    searchText: SearchText,
                    status: SelectedStatus,
                    pageNumber: CurrentPage,
                    pageSize: PageSize);

                Items.Clear();
                foreach (var trip in result.Items)
                {
                    Items.Add(trip);
                }

                TotalCount = result.TotalCount;
                ShowInfo($"تم تحميل {Items.Count} رحلة");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تحميل الرحلات: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public override async Task DeleteItemAsync(TripDto trip)
        {
            if (trip == null) return;

            var confirmed = MessageBox.Show(
                $"هل تريد حذف الرحلة؟",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmed == MessageBoxResult.Yes)
            {
                IsLoading = true;
                try
                {
                    await _tripService.DeleteTripAsync(trip.Id);
                    Items.Remove(trip);
                    ShowInfo("تم حذف الرحلة بنجاح");
                    await LoadItemsAsync();
                }
                catch (Exception ex)
                {
                    ShowError($"خطأ في حذف الرحلة: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private void OpenAddForm()
        {
            FormViewModel.IsNew = true;
            FormViewModel.Entity = new TripDto();
            ShowForm = true;
        }

        private void OpenEditForm(TripDto trip)
        {
            if (trip == null) return;
            FormViewModel.IsNew = false;
            FormViewModel.Entity = trip;
            ShowForm = true;
        }
    }

    public class TripFormViewModel : FormViewModelBase<TripDto>
    {
        private readonly ITripService _tripService;
        private ObservableCollection<string> _vehicles;
        private ObservableCollection<string> _drivers;
        private ObservableCollection<string> _tripStatuses;

        public ObservableCollection<string> Vehicles
        {
            get => _vehicles;
            set => SetProperty(ref _vehicles, value);
        }

        public ObservableCollection<string> Drivers
        {
            get => _drivers;
            set => SetProperty(ref _drivers, value);
        }

        public ObservableCollection<string> TripStatuses
        {
            get => _tripStatuses;
            set => SetProperty(ref _tripStatuses, value);
        }

        public TripFormViewModel(ITripService tripService)
        {
            _tripService = tripService;
            Vehicles = new ObservableCollection<string>();
            Drivers = new ObservableCollection<string>();
            TripStatuses = new ObservableCollection<string>();

            SaveCommand = new AsyncRelayCommand(SaveEntityAsync, () => !HasErrors);
            CancelCommand = new RelayCommand(_ => OnCancel());
        }

        public override bool ValidateEntity()
        {
            ClearErrors();
            bool isValid = true;

            if (Entity?.VehicleId <= 0)
            {
                AddError(nameof(Entity.VehicleId), "المركبة مطلوبة");
                isValid = false;
            }

            if (Entity?.DriverId <= 0)
            {
                AddError(nameof(Entity.DriverId), "السائق مطلوب");
                isValid = false;
            }

            if (!ValidationHelper.IsNotEmpty(Entity?.StartLocation))
            {
                AddError(nameof(Entity.StartLocation), "نقطة البداية مطلوبة");
                isValid = false;
            }

            if (!ValidationHelper.IsNotEmpty(Entity?.EndLocation))
            {
                AddError(nameof(Entity.EndLocation), "نقطة النهاية مطلوبة");
                isValid = false;
            }

            return isValid;
        }

        public override async Task SaveEntityAsync()
        {
            if (!ValidateEntity())
            {
                ShowError("يرجى تصحيح الأخطاء أعلاه");
                return;
            }

            IsLoading = true;
            ClearMessages();

            try
            {
                if (IsNew)
                {
                    await _tripService.CreateTripAsync(Entity);
                    ShowInfo("تم إضافة الرحلة بنجاح");
                }
                else
                {
                    await _tripService.UpdateTripAsync(Entity);
                    ShowInfo("تم تحديث الرحلة بنجاح");
                }

                OnSave();
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في حفظ الرحلة: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnSave() { }
        private void OnCancel() { }
    }
}

// ============================================================================
// FILE: FuelViewModel.cs
// LOCATION: FleetManagementSystem.WPF/ViewModels/FuelViewModel.cs
// PURPOSE: Fuel transaction list and form ViewModel
// ============================================================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Core.Services;

namespace FleetManagementSystem.WPF.ViewModels
{
    public class FuelViewModel : ListViewModelBase<FuelTransactionDto>
    {
        private readonly IFuelService _fuelService;
        private ObservableCollection<string> _fuelTypes;
        private FuelFormViewModel _formViewModel;
        private bool _showForm;

        public ObservableCollection<string> FuelTypes
        {
            get => _fuelTypes;
            set => SetProperty(ref _fuelTypes, value);
        }

        public FuelFormViewModel FormViewModel
        {
            get => _formViewModel;
            set => SetProperty(ref _formViewModel, value);
        }

        public bool ShowForm
        {
            get => _showForm;
            set => SetProperty(ref _showForm, value);
        }

        public FuelViewModel(IFuelService fuelService)
        {
            _fuelService = fuelService;
            FuelTypes = new ObservableCollection<string>();
            FormViewModel = new FuelFormViewModel(fuelService);

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            RefreshCommand = new AsyncRelayCommand(LoadItemsAsync);
            SearchCommand = new AsyncRelayCommand(LoadItemsAsync);
            AddCommand = new RelayCommand(_ => OpenAddForm());
            EditCommand = new RelayCommand<FuelTransactionDto>(fuel => OpenEditForm(fuel), _ => SelectedItem != null);
            DeleteCommand = new AsyncRelayCommand<FuelTransactionDto>(DeleteItemAsync, _ => SelectedItem != null);
        }

        public override async Task LoadItemsAsync()
        {
            IsLoading = true;
            ClearMessages();

            try
            {
                var result = await _fuelService.GetFuelTransactionsAsync(
                    searchText: SearchText,
                    pageNumber: CurrentPage,
                    pageSize: PageSize);

                Items.Clear();
                foreach (var transaction in result.Items)
                {
                    Items.Add(transaction);
                }

                TotalCount = result.TotalCount;
                ShowInfo($"تم تحميل {Items.Count} معاملة وقود");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تحميل معاملات الوقود: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public override async Task DeleteItemAsync(FuelTransactionDto transaction)
        {
            if (transaction == null) return;

            var confirmed = MessageBox.Show(
                $"هل تريد حذف معاملة الوقود؟",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmed == MessageBoxResult.Yes)
            {
                IsLoading = true;
                try
                {
                    await _fuelService.DeleteFuelTransactionAsync(transaction.Id);
                    Items.Remove(transaction);
                    ShowInfo("تم حذف معاملة الوقود بنجاح");
                    await LoadItemsAsync();
                }
                catch (Exception ex)
                {
                    ShowError($"خطأ في حذف معاملة الوقود: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private void OpenAddForm()
        {
            FormViewModel.IsNew = true;
            FormViewModel.Entity = new FuelTransactionDto();
            ShowForm = true;
        }

        private void OpenEditForm(FuelTransactionDto transaction)
        {
            if (transaction == null) return;
            FormViewModel.IsNew = false;
            FormViewModel.Entity = transaction;
            ShowForm = true;
        }
    }

    public class FuelFormViewModel : FormViewModelBase<FuelTransactionDto>
    {
        private readonly IFuelService _fuelService;
        private ObservableCollection<string> _vehicles;
        private ObservableCollection<string> _fuelTypes;

        public ObservableCollection<string> Vehicles
        {
            get => _vehicles;
            set => SetProperty(ref _vehicles, value);
        }

        public ObservableCollection<string> FuelTypes
        {
            get => _fuelTypes;
            set => SetProperty(ref _fuelTypes, value);
        }

        public FuelFormViewModel(IFuelService fuelService)
        {
            _fuelService = fuelService;
            Vehicles = new ObservableCollection<string>();
            FuelTypes = new ObservableCollection<string>();

            SaveCommand = new AsyncRelayCommand(SaveEntityAsync, () => !HasErrors);
            CancelCommand = new RelayCommand(_ => OnCancel());
        }

        public override bool ValidateEntity()
        {
            ClearErrors();
            bool isValid = true;

            if (Entity?.VehicleId <= 0)
            {
                AddError(nameof(Entity.VehicleId), "المركبة مطلوبة");
                isValid = false;
            }

            if (Entity?.Quantity <= 0)
            {
                AddError(nameof(Entity.Quantity), "الكمية يجب أن تكون أكبر من صفر");
                isValid = false;
            }

            if (Entity?.UnitPrice <= 0)
            {
                AddError(nameof(Entity.UnitPrice), "سعر الوحدة يجب أن يكون أكبر من صفر");
                isValid = false;
            }

            return isValid;
        }

        public override async Task SaveEntityAsync()
        {
            if (!ValidateEntity())
            {
                ShowError("يرجى تصحيح الأخطاء أعلاه");
                return;
            }

            IsLoading = true;
            ClearMessages();

            try
            {
                if (IsNew)
                {
                    await _fuelService.CreateFuelTransactionAsync(Entity);
                    ShowInfo("تم إضافة معاملة الوقود بنجاح");
                }
                else
                {
                    await _fuelService.UpdateFuelTransactionAsync(Entity);
                    ShowInfo("تم تحديث معاملة الوقود بنجاح");
                }

                OnSave();
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في حفظ معاملة الوقود: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnSave() { }
        private void OnCancel() { }
    }
}

// ============================================================================
// FILE: ExpenseViewModel.cs
// LOCATION: FleetManagementSystem.WPF/ViewModels/ExpenseViewModel.cs
// PURPOSE: Expense list and form ViewModel
// ============================================================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Core.Services;

namespace FleetManagementSystem.WPF.ViewModels
{
    public class ExpenseViewModel : ListViewModelBase<ExpenseDto>
    {
        private readonly IExpenseService _expenseService;
        private ObservableCollection<string> _expenseTypes;
        private string _selectedType;
        private ExpenseFormViewModel _formViewModel;
        private bool _showForm;

        public ObservableCollection<string> ExpenseTypes
        {
            get => _expenseTypes;
            set => SetProperty(ref _expenseTypes, value);
        }

        public string SelectedType
        {
            get => _selectedType;
            set
            {
                if (SetProperty(ref _selectedType, value))
                {
                    CurrentPage = 1;
                    _ = LoadItemsAsync();
                }
            }
        }

        public ExpenseFormViewModel FormViewModel
        {
            get => _formViewModel;
            set => SetProperty(ref _formViewModel, value);
        }

        public bool ShowForm
        {
            get => _showForm;
            set => SetProperty(ref _showForm, value);
        }

        public ExpenseViewModel(IExpenseService expenseService)
        {
            _expenseService = expenseService;
            ExpenseTypes = new ObservableCollection<string>();
            FormViewModel = new ExpenseFormViewModel(expenseService);

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            RefreshCommand = new AsyncRelayCommand(LoadItemsAsync);
            SearchCommand = new AsyncRelayCommand(LoadItemsAsync);
            AddCommand = new RelayCommand(_ => OpenAddForm());
            EditCommand = new RelayCommand<ExpenseDto>(expense => OpenEditForm(expense), _ => SelectedItem != null);
            DeleteCommand = new AsyncRelayCommand<ExpenseDto>(DeleteItemAsync, _ => SelectedItem != null);
        }

        public override async Task LoadItemsAsync()
        {
            IsLoading = true;
            ClearMessages();

            try
            {
                var result = await _expenseService.GetExpensesAsync(
                    searchText: SearchText,
                    expenseType: SelectedType,
                    pageNumber: CurrentPage,
                    pageSize: PageSize);

                Items.Clear();
                foreach (var expense in result.Items)
                {
                    Items.Add(expense);
                }

                TotalCount = result.TotalCount;
                ShowInfo($"تم تحميل {Items.Count} مصروف");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تحميل المصروفات: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public override async Task DeleteItemAsync(ExpenseDto expense)
        {
            if (expense == null) return;

            var confirmed = MessageBox.Show(
                $"هل تريد حذف المصروف؟",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmed == MessageBoxResult.Yes)
            {
                IsLoading = true;
                try
                {
                    await _expenseService.DeleteExpenseAsync(expense.Id);
                    Items.Remove(expense);
                    ShowInfo("تم حذف المصروف بنجاح");
                    await LoadItemsAsync();
                }
                catch (Exception ex)
                {
                    ShowError($"خطأ في حذف المصروف: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private void OpenAddForm()
        {
            FormViewModel.IsNew = true;
            FormViewModel.Entity = new ExpenseDto();
            ShowForm = true;
        }

        private void OpenEditForm(ExpenseDto expense)
        {
            if (expense == null) return;
            FormViewModel.IsNew = false;
            FormViewModel.Entity = expense;
            ShowForm = true;
        }
    }

    public class ExpenseFormViewModel : FormViewModelBase<ExpenseDto>
    {
        private readonly IExpenseService _expenseService;
        private ObservableCollection<string> _vehicles;
        private ObservableCollection<string> _expenseTypes;

        public ObservableCollection<string> Vehicles
        {
            get => _vehicles;
            set => SetProperty(ref _vehicles, value);
        }

        public ObservableCollection<string> ExpenseTypes
        {
            get => _expenseTypes;
            set => SetProperty(ref _expenseTypes, value);
        }

        public ExpenseFormViewModel(IExpenseService expenseService)
        {
            _expenseService = expenseService;
            Vehicles = new ObservableCollection<string>();
            ExpenseTypes = new ObservableCollection<string>();

            SaveCommand = new AsyncRelayCommand(SaveEntityAsync, () => !HasErrors);
            CancelCommand = new RelayCommand(_ => OnCancel());
        }

        public override bool ValidateEntity()
        {
            ClearErrors();
            bool isValid = true;

            if (Entity?.VehicleId <= 0)
            {
                AddError(nameof(Entity.VehicleId), "المركبة مطلوبة");
                isValid = false;
            }

            if (!ValidationHelper.IsNotEmpty(Entity?.ExpenseType))
            {
                AddError(nameof(Entity.ExpenseType), "نوع المصروف مطلوب");
                isValid = false;
            }

            if (Entity?.Amount <= 0)
            {
                AddError(nameof(Entity.Amount), "المبلغ يجب أن يكون أكبر من صفر");
                isValid = false;
            }

            return isValid;
        }

        public override async Task SaveEntityAsync()
        {
            if (!ValidateEntity())
            {
                ShowError("يرجى تصحيح الأخطاء أعلاه");
                return;
            }

            IsLoading = true;
            ClearMessages();

            try
            {
                if (IsNew)
                {
                    await _expenseService.CreateExpenseAsync(Entity);
                    ShowInfo("تم إضافة المصروف بنجاح");
                }
                else
                {
                    await _expenseService.UpdateExpenseAsync(Entity);
                    ShowInfo("تم تحديث المصروف بنجاح");
                }

                OnSave();
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في حفظ المصروف: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnSave() { }
        private void OnCancel() { }
    }
}
