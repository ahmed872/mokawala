// ============================================================================
// FILE: ContractViewModel.cs
// LOCATION: FleetManagementSystem.WPF/ViewModels/ContractViewModel.cs
// PURPOSE: Contract list and form ViewModel with status lifecycle
// ============================================================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Core.Services;

namespace FleetManagementSystem.WPF.ViewModels
{
    public class ContractViewModel : ListViewModelBase<ContractDto>
    {
        private readonly IContractService _contractService;
        private ObservableCollection<string> _contractStatuses;
        private string _selectedStatus;
        private ContractFormViewModel _formViewModel;
        private bool _showForm;

        public ObservableCollection<string> ContractStatuses
        {
            get => _contractStatuses;
            set => SetProperty(ref _contractStatuses, value);
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

        public ContractFormViewModel FormViewModel
        {
            get => _formViewModel;
            set => SetProperty(ref _formViewModel, value);
        }

        public bool ShowForm
        {
            get => _showForm;
            set => SetProperty(ref _showForm, value);
        }

        public ICommand ActivateCommand { get; private set; }
        public ICommand ExpireCommand { get; private set; }
        public ICommand TerminateCommand { get; private set; }

        public ContractViewModel(IContractService contractService)
        {
            _contractService = contractService;
            ContractStatuses = new ObservableCollection<string>();
            FormViewModel = new ContractFormViewModel(contractService);

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            RefreshCommand = new AsyncRelayCommand(LoadItemsAsync);
            SearchCommand = new AsyncRelayCommand(LoadItemsAsync);
            AddCommand = new RelayCommand(_ => OpenAddForm());
            EditCommand = new RelayCommand<ContractDto>(contract => OpenEditForm(contract), _ => SelectedItem != null);
            DeleteCommand = new AsyncRelayCommand<ContractDto>(DeleteItemAsync, _ => SelectedItem != null);
            ActivateCommand = new AsyncRelayCommand<ContractDto>(ActivateContractAsync, _ => SelectedItem != null);
            ExpireCommand = new AsyncRelayCommand<ContractDto>(ExpireContractAsync, _ => SelectedItem != null);
            TerminateCommand = new AsyncRelayCommand<ContractDto>(TerminateContractAsync, _ => SelectedItem != null);
        }

        public override async Task LoadItemsAsync()
        {
            IsLoading = true;
            ClearMessages();

            try
            {
                var result = await _contractService.GetContractsAsync(
                    searchText: SearchText,
                    status: SelectedStatus,
                    pageNumber: CurrentPage,
                    pageSize: PageSize);

                Items.Clear();
                foreach (var contract in result.Items)
                {
                    Items.Add(contract);
                }

                TotalCount = result.TotalCount;
                ShowInfo($"تم تحميل {Items.Count} عقد");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تحميل العقود: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public override async Task DeleteItemAsync(ContractDto contract)
        {
            if (contract == null) return;

            var confirmed = MessageBox.Show(
                $"هل تريد حذف العقد {contract.ContractNumber}؟",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmed == MessageBoxResult.Yes)
            {
                IsLoading = true;
                try
                {
                    await _contractService.DeleteContractAsync(contract.Id);
                    Items.Remove(contract);
                    ShowInfo("تم حذف العقد بنجاح");
                    await LoadItemsAsync();
                }
                catch (Exception ex)
                {
                    ShowError($"خطأ في حذف العقد: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task ActivateContractAsync(ContractDto contract)
        {
            if (contract == null) return;

            IsLoading = true;
            try
            {
                await _contractService.ChangeContractStatusAsync(contract.Id, "Active");
                contract.Status = "Active";
                ShowInfo("تم تفعيل العقد بنجاح");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تفعيل العقد: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExpireContractAsync(ContractDto contract)
        {
            if (contract == null) return;

            IsLoading = true;
            try
            {
                await _contractService.ChangeContractStatusAsync(contract.Id, "Expired");
                contract.Status = "Expired";
                ShowInfo("تم انتهاء العقد بنجاح");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في انتهاء العقد: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task TerminateContractAsync(ContractDto contract)
        {
            if (contract == null) return;

            IsLoading = true;
            try
            {
                await _contractService.ChangeContractStatusAsync(contract.Id, "Terminated");
                contract.Status = "Terminated";
                ShowInfo("تم إنهاء العقد بنجاح");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في إنهاء العقد: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OpenAddForm()
        {
            FormViewModel.IsNew = true;
            FormViewModel.Entity = new ContractDto();
            ShowForm = true;
        }

        private void OpenEditForm(ContractDto contract)
        {
            if (contract == null) return;
            FormViewModel.IsNew = false;
            FormViewModel.Entity = contract;
            ShowForm = true;
        }
    }

    public class ContractFormViewModel : FormViewModelBase<ContractDto>
    {
        private readonly IContractService _contractService;
        private ObservableCollection<string> _contractStatuses;

        public ObservableCollection<string> ContractStatuses
        {
            get => _contractStatuses;
            set => SetProperty(ref _contractStatuses, value);
        }

        public ContractFormViewModel(IContractService contractService)
        {
            _contractService = contractService;
            ContractStatuses = new ObservableCollection<string>();

            SaveCommand = new AsyncRelayCommand(SaveEntityAsync, () => !HasErrors);
            CancelCommand = new RelayCommand(_ => OnCancel());
        }

        public override bool ValidateEntity()
        {
            ClearErrors();
            bool isValid = true;

            if (!ValidationHelper.IsValidContractNumber(Entity?.ContractNumber))
            {
                AddError(nameof(Entity.ContractNumber), "رقم العقد غير صحيح");
                isValid = false;
            }

            if (!ValidationHelper.IsNotEmpty(Entity?.ClientName))
            {
                AddError(nameof(Entity.ClientName), "اسم العميل مطلوب");
                isValid = false;
            }

            if (!ValidationHelper.IsDateRangeValid(Entity?.StartDate ?? DateTime.Now, Entity?.EndDate ?? DateTime.Now))
            {
                AddError(nameof(Entity.EndDate), "تاريخ الانتهاء يجب أن يكون بعد تاريخ البداية");
                isValid = false;
            }

            if (Entity?.ContractValue <= 0)
            {
                AddError(nameof(Entity.ContractValue), "قيمة العقد يجب أن تكون أكبر من صفر");
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
                    await _contractService.CreateContractAsync(Entity);
                    ShowInfo("تم إضافة العقد بنجاح");
                }
                else
                {
                    await _contractService.UpdateContractAsync(Entity);
                    ShowInfo("تم تحديث العقد بنجاح");
                }

                OnSave();
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في حفظ العقد: {ex.Message}");
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
// FILE: MaintenanceViewModel.cs
// LOCATION: FleetManagementSystem.WPF/ViewModels/MaintenanceViewModel.cs
// PURPOSE: Maintenance request list and form ViewModel
// ============================================================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Core.Services;

namespace FleetManagementSystem.WPF.ViewModels
{
    public class MaintenanceViewModel : ListViewModelBase<MaintenanceRequestDto>
    {
        private readonly IMaintenanceService _maintenanceService;
        private ObservableCollection<string> _maintenanceTypes;
        private ObservableCollection<string> _maintenanceStatuses;
        private string _selectedStatus;
        private MaintenanceFormViewModel _formViewModel;
        private bool _showForm;

        public ObservableCollection<string> MaintenanceTypes
        {
            get => _maintenanceTypes;
            set => SetProperty(ref _maintenanceTypes, value);
        }

        public ObservableCollection<string> MaintenanceStatuses
        {
            get => _maintenanceStatuses;
            set => SetProperty(ref _maintenanceStatuses, value);
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

        public MaintenanceFormViewModel FormViewModel
        {
            get => _formViewModel;
            set => SetProperty(ref _formViewModel, value);
        }

        public bool ShowForm
        {
            get => _showForm;
            set => SetProperty(ref _showForm, value);
        }

        public ICommand CompleteCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public MaintenanceViewModel(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
            MaintenanceTypes = new ObservableCollection<string>();
            MaintenanceStatuses = new ObservableCollection<string>();
            FormViewModel = new MaintenanceFormViewModel(maintenanceService);

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            RefreshCommand = new AsyncRelayCommand(LoadItemsAsync);
            SearchCommand = new AsyncRelayCommand(LoadItemsAsync);
            AddCommand = new RelayCommand(_ => OpenAddForm());
            EditCommand = new RelayCommand<MaintenanceRequestDto>(m => OpenEditForm(m), _ => SelectedItem != null);
            DeleteCommand = new AsyncRelayCommand<MaintenanceRequestDto>(DeleteItemAsync, _ => SelectedItem != null);
            CompleteCommand = new AsyncRelayCommand<MaintenanceRequestDto>(CompleteMaintenanceAsync, _ => SelectedItem != null);
            CancelCommand = new AsyncRelayCommand<MaintenanceRequestDto>(CancelMaintenanceAsync, _ => SelectedItem != null);
        }

        public override async Task LoadItemsAsync()
        {
            IsLoading = true;
            ClearMessages();

            try
            {
                var result = await _maintenanceService.GetMaintenanceRequestsAsync(
                    searchText: SearchText,
                    status: SelectedStatus,
                    pageNumber: CurrentPage,
                    pageSize: PageSize);

                Items.Clear();
                foreach (var request in result.Items)
                {
                    Items.Add(request);
                }

                TotalCount = result.TotalCount;
                ShowInfo($"تم تحميل {Items.Count} طلب صيانة");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تحميل طلبات الصيانة: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public override async Task DeleteItemAsync(MaintenanceRequestDto request)
        {
            if (request == null) return;

            var confirmed = MessageBox.Show(
                $"هل تريد حذف طلب الصيانة؟",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmed == MessageBoxResult.Yes)
            {
                IsLoading = true;
                try
                {
                    await _maintenanceService.DeleteMaintenanceRequestAsync(request.Id);
                    Items.Remove(request);
                    ShowInfo("تم حذف طلب الصيانة بنجاح");
                    await LoadItemsAsync();
                }
                catch (Exception ex)
                {
                    ShowError($"خطأ في حذف طلب الصيانة: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task CompleteMaintenanceAsync(MaintenanceRequestDto request)
        {
            if (request == null) return;

            IsLoading = true;
            try
            {
                await _maintenanceService.CompleteMaintenanceRequestAsync(request.Id);
                request.Status = "Completed";
                ShowInfo("تم إكمال طلب الصيانة بنجاح");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في إكمال طلب الصيانة: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CancelMaintenanceAsync(MaintenanceRequestDto request)
        {
            if (request == null) return;

            IsLoading = true;
            try
            {
                await _maintenanceService.CancelMaintenanceRequestAsync(request.Id);
                request.Status = "Cancelled";
                ShowInfo("تم إلغاء طلب الصيانة بنجاح");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في إلغاء طلب الصيانة: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OpenAddForm()
        {
            FormViewModel.IsNew = true;
            FormViewModel.Entity = new MaintenanceRequestDto();
            ShowForm = true;
        }

        private void OpenEditForm(MaintenanceRequestDto request)
        {
            if (request == null) return;
            FormViewModel.IsNew = false;
            FormViewModel.Entity = request;
            ShowForm = true;
        }
    }

    public class MaintenanceFormViewModel : FormViewModelBase<MaintenanceRequestDto>
    {
        private readonly IMaintenanceService _maintenanceService;
        private ObservableCollection<string> _maintenanceTypes;
        private ObservableCollection<string> _maintenanceStatuses;
        private ObservableCollection<string> _serviceProviders;

        public ObservableCollection<string> MaintenanceTypes
        {
            get => _maintenanceTypes;
            set => SetProperty(ref _maintenanceTypes, value);
        }

        public ObservableCollection<string> MaintenanceStatuses
        {
            get => _maintenanceStatuses;
            set => SetProperty(ref _maintenanceStatuses, value);
        }

        public ObservableCollection<string> ServiceProviders
        {
            get => _serviceProviders;
            set => SetProperty(ref _serviceProviders, value);
        }

        public MaintenanceFormViewModel(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
            MaintenanceTypes = new ObservableCollection<string>();
            MaintenanceStatuses = new ObservableCollection<string>();
            ServiceProviders = new ObservableCollection<string>();

            SaveCommand = new AsyncRelayCommand(SaveEntityAsync, () => !HasErrors);
            CancelCommand = new RelayCommand(_ => OnCancel());
        }

        public override bool ValidateEntity()
        {
            ClearErrors();
            bool isValid = true;

            if (!ValidationHelper.IsNotEmpty(Entity?.MaintenanceType))
            {
                AddError(nameof(Entity.MaintenanceType), "نوع الصيانة مطلوب");
                isValid = false;
            }

            if (Entity?.MaintenanceDate > DateTime.Now)
            {
                AddError(nameof(Entity.MaintenanceDate), "تاريخ الصيانة لا يمكن أن يكون في المستقبل");
                isValid = false;
            }

            if (Entity?.Cost < 0)
            {
                AddError(nameof(Entity.Cost), "تكلفة الصيانة يجب أن تكون موجبة");
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
                    await _maintenanceService.CreateMaintenanceRequestAsync(Entity);
                    ShowInfo("تم إضافة طلب الصيانة بنجاح");
                }
                else
                {
                    await _maintenanceService.UpdateMaintenanceRequestAsync(Entity);
                    ShowInfo("تم تحديث طلب الصيانة بنجاح");
                }

                OnSave();
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في حفظ طلب الصيانة: {ex.Message}");
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
// FILE: DriverViewModel.cs
// LOCATION: FleetManagementSystem.WPF/ViewModels/DriverViewModel.cs
// PURPOSE: Driver list and form ViewModel with license tracking
// ============================================================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Core.Services;

namespace FleetManagementSystem.WPF.ViewModels
{
    public class DriverViewModel : ListViewModelBase<DriverDto>
    {
        private readonly IDriverService _driverService;
        private DriverFormViewModel _formViewModel;
        private bool _showForm;

        public DriverFormViewModel FormViewModel
        {
            get => _formViewModel;
            set => SetProperty(ref _formViewModel, value);
        }

        public bool ShowForm
        {
            get => _showForm;
            set => SetProperty(ref _showForm, value);
        }

        public DriverViewModel(IDriverService driverService)
        {
            _driverService = driverService;
            FormViewModel = new DriverFormViewModel(driverService);

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            RefreshCommand = new AsyncRelayCommand(LoadItemsAsync);
            SearchCommand = new AsyncRelayCommand(LoadItemsAsync);
            AddCommand = new RelayCommand(_ => OpenAddForm());
            EditCommand = new RelayCommand<DriverDto>(driver => OpenEditForm(driver), _ => SelectedItem != null);
            DeleteCommand = new AsyncRelayCommand<DriverDto>(DeleteItemAsync, _ => SelectedItem != null);
        }

        public override async Task LoadItemsAsync()
        {
            IsLoading = true;
            ClearMessages();

            try
            {
                var result = await _driverService.GetDriversAsync(
                    searchText: SearchText,
                    pageNumber: CurrentPage,
                    pageSize: PageSize);

                Items.Clear();
                foreach (var driver in result.Items)
                {
                    Items.Add(driver);
                }

                TotalCount = result.TotalCount;
                ShowInfo($"تم تحميل {Items.Count} سائق");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تحميل السائقين: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public override async Task DeleteItemAsync(DriverDto driver)
        {
            if (driver == null) return;

            var confirmed = MessageBox.Show(
                $"هل تريد حذف السائق {driver.FullName}؟",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmed == MessageBoxResult.Yes)
            {
                IsLoading = true;
                try
                {
                    await _driverService.DeleteDriverAsync(driver.Id);
                    Items.Remove(driver);
                    ShowInfo("تم حذف السائق بنجاح");
                    await LoadItemsAsync();
                }
                catch (Exception ex)
                {
                    ShowError($"خطأ في حذف السائق: {ex.Message}");
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
            FormViewModel.Entity = new DriverDto();
            ShowForm = true;
        }

        private void OpenEditForm(DriverDto driver)
        {
            if (driver == null) return;
            FormViewModel.IsNew = false;
            FormViewModel.Entity = driver;
            ShowForm = true;
        }
    }

    public class DriverFormViewModel : FormViewModelBase<DriverDto>
    {
        private readonly IDriverService _driverService;

        public DriverFormViewModel(IDriverService driverService)
        {
            _driverService = driverService;

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

            if (!ValidationHelper.IsValidPhone(Entity?.PhoneNumber))
            {
                AddError(nameof(Entity.PhoneNumber), "رقم الهاتف غير صحيح");
                isValid = false;
            }

            if (!ValidationHelper.IsNotEmpty(Entity?.LicenseNumber))
            {
                AddError(nameof(Entity.LicenseNumber), "رقم الرخصة مطلوب");
                isValid = false;
            }

            if (Entity?.LicenseExpiryDate < DateTime.Now)
            {
                AddError(nameof(Entity.LicenseExpiryDate), "الرخصة منتهية الصلاحية");
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
                    await _driverService.CreateDriverAsync(Entity);
                    ShowInfo("تم إضافة السائق بنجاح");
                }
                else
                {
                    await _driverService.UpdateDriverAsync(Entity);
                    ShowInfo("تم تحديث السائق بنجاح");
                }

                OnSave();
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في حفظ السائق: {ex.Message}");
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
