// ============================================================================
// FILE: LicenseViewModel.cs
// LOCATION: FleetManagementSystem.WPF/ViewModels/LicenseViewModel.cs
// PURPOSE: License tracking list and form ViewModel
// ============================================================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Core.Services;

namespace FleetManagementSystem.WPF.ViewModels
{
    public class LicenseViewModel : ListViewModelBase<LicenseDto>
    {
        private readonly ILicenseService _licenseService;
        private LicenseFormViewModel _formViewModel;
        private bool _showForm;

        public LicenseFormViewModel FormViewModel
        {
            get => _formViewModel;
            set => SetProperty(ref _formViewModel, value);
        }

        public bool ShowForm
        {
            get => _showForm;
            set => SetProperty(ref _showForm, value);
        }

        public LicenseViewModel(ILicenseService licenseService)
        {
            _licenseService = licenseService;
            FormViewModel = new LicenseFormViewModel(licenseService);

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            RefreshCommand = new AsyncRelayCommand(LoadItemsAsync);
            SearchCommand = new AsyncRelayCommand(LoadItemsAsync);
            AddCommand = new RelayCommand(_ => OpenAddForm());
            EditCommand = new RelayCommand<LicenseDto>(license => OpenEditForm(license), _ => SelectedItem != null);
            DeleteCommand = new AsyncRelayCommand<LicenseDto>(DeleteItemAsync, _ => SelectedItem != null);
        }

        public override async Task LoadItemsAsync()
        {
            IsLoading = true;
            ClearMessages();

            try
            {
                var result = await _licenseService.GetLicensesAsync(
                    searchText: SearchText,
                    pageNumber: CurrentPage,
                    pageSize: PageSize);

                Items.Clear();
                foreach (var license in result.Items)
                {
                    Items.Add(license);
                }

                TotalCount = result.TotalCount;
                ShowInfo($"تم تحميل {Items.Count} رخصة");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تحميل الرخص: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public override async Task DeleteItemAsync(LicenseDto license)
        {
            if (license == null) return;

            var confirmed = MessageBox.Show(
                $"هل تريد حذف الرخصة؟",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmed == MessageBoxResult.Yes)
            {
                IsLoading = true;
                try
                {
                    await _licenseService.DeleteLicenseAsync(license.Id);
                    Items.Remove(license);
                    ShowInfo("تم حذف الرخصة بنجاح");
                    await LoadItemsAsync();
                }
                catch (Exception ex)
                {
                    ShowError($"خطأ في حذف الرخصة: {ex.Message}");
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
            FormViewModel.Entity = new LicenseDto();
            ShowForm = true;
        }

        private void OpenEditForm(LicenseDto license)
        {
            if (license == null) return;
            FormViewModel.IsNew = false;
            FormViewModel.Entity = license;
            ShowForm = true;
        }
    }

    public class LicenseFormViewModel : FormViewModelBase<LicenseDto>
    {
        private readonly ILicenseService _licenseService;
        private ObservableCollection<string> _drivers;
        private ObservableCollection<string> _licenseTypes;

        public ObservableCollection<string> Drivers
        {
            get => _drivers;
            set => SetProperty(ref _drivers, value);
        }

        public ObservableCollection<string> LicenseTypes
        {
            get => _licenseTypes;
            set => SetProperty(ref _licenseTypes, value);
        }

        public LicenseFormViewModel(ILicenseService licenseService)
        {
            _licenseService = licenseService;
            Drivers = new ObservableCollection<string>();
            LicenseTypes = new ObservableCollection<string>();

            SaveCommand = new AsyncRelayCommand(SaveEntityAsync, () => !HasErrors);
            CancelCommand = new RelayCommand(_ => OnCancel());
        }

        public override bool ValidateEntity()
        {
            ClearErrors();
            bool isValid = true;

            if (Entity?.DriverId <= 0)
            {
                AddError(nameof(Entity.DriverId), "السائق مطلوب");
                isValid = false;
            }

            if (!ValidationHelper.IsNotEmpty(Entity?.LicenseNumber))
            {
                AddError(nameof(Entity.LicenseNumber), "رقم الرخصة مطلوب");
                isValid = false;
            }

            if (!ValidationHelper.IsDateRangeValid(Entity?.IssuanceDate ?? DateTime.Now, Entity?.ExpiryDate ?? DateTime.Now))
            {
                AddError(nameof(Entity.ExpiryDate), "تاريخ الانتهاء يجب أن يكون بعد تاريخ الإصدار");
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
                    await _licenseService.CreateLicenseAsync(Entity);
                    ShowInfo("تم إضافة الرخصة بنجاح");
                }
                else
                {
                    await _licenseService.UpdateLicenseAsync(Entity);
                    ShowInfo("تم تحديث الرخصة بنجاح");
                }

                OnSave();
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في حفظ الرخصة: {ex.Message}");
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
// FILE: InsuranceViewModel.cs
// LOCATION: FleetManagementSystem.WPF/ViewModels/InsuranceViewModel.cs
// PURPOSE: Insurance policy list and form ViewModel
// ============================================================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Core.Services;

namespace FleetManagementSystem.WPF.ViewModels
{
    public class InsuranceViewModel : ListViewModelBase<InsuranceDto>
    {
        private readonly IInsuranceService _insuranceService;
        private InsuranceFormViewModel _formViewModel;
        private bool _showForm;

        public InsuranceFormViewModel FormViewModel
        {
            get => _formViewModel;
            set => SetProperty(ref _formViewModel, value);
        }

        public bool ShowForm
        {
            get => _showForm;
            set => SetProperty(ref _showForm, value);
        }

        public InsuranceViewModel(IInsuranceService insuranceService)
        {
            _insuranceService = insuranceService;
            FormViewModel = new InsuranceFormViewModel(insuranceService);

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            RefreshCommand = new AsyncRelayCommand(LoadItemsAsync);
            SearchCommand = new AsyncRelayCommand(LoadItemsAsync);
            AddCommand = new RelayCommand(_ => OpenAddForm());
            EditCommand = new RelayCommand<InsuranceDto>(insurance => OpenEditForm(insurance), _ => SelectedItem != null);
            DeleteCommand = new AsyncRelayCommand<InsuranceDto>(DeleteItemAsync, _ => SelectedItem != null);
        }

        public override async Task LoadItemsAsync()
        {
            IsLoading = true;
            ClearMessages();

            try
            {
                var result = await _insuranceService.GetInsurancesAsync(
                    searchText: SearchText,
                    pageNumber: CurrentPage,
                    pageSize: PageSize);

                Items.Clear();
                foreach (var insurance in result.Items)
                {
                    Items.Add(insurance);
                }

                TotalCount = result.TotalCount;
                ShowInfo($"تم تحميل {Items.Count} بوليصة تأمين");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تحميل بوليصات التأمين: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public override async Task DeleteItemAsync(InsuranceDto insurance)
        {
            if (insurance == null) return;

            var confirmed = MessageBox.Show(
                $"هل تريد حذف بوليصة التأمين؟",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmed == MessageBoxResult.Yes)
            {
                IsLoading = true;
                try
                {
                    await _insuranceService.DeleteInsuranceAsync(insurance.Id);
                    Items.Remove(insurance);
                    ShowInfo("تم حذف بوليصة التأمين بنجاح");
                    await LoadItemsAsync();
                }
                catch (Exception ex)
                {
                    ShowError($"خطأ في حذف بوليصة التأمين: {ex.Message}");
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
            FormViewModel.Entity = new InsuranceDto();
            ShowForm = true;
        }

        private void OpenEditForm(InsuranceDto insurance)
        {
            if (insurance == null) return;
            FormViewModel.IsNew = false;
            FormViewModel.Entity = insurance;
            ShowForm = true;
        }
    }

    public class InsuranceFormViewModel : FormViewModelBase<InsuranceDto>
    {
        private readonly IInsuranceService _insuranceService;
        private ObservableCollection<string> _vehicles;

        public ObservableCollection<string> Vehicles
        {
            get => _vehicles;
            set => SetProperty(ref _vehicles, value);
        }

        public InsuranceFormViewModel(IInsuranceService insuranceService)
        {
            _insuranceService = insuranceService;
            Vehicles = new ObservableCollection<string>();

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

            if (!ValidationHelper.IsNotEmpty(Entity?.InsuranceCompany))
            {
                AddError(nameof(Entity.InsuranceCompany), "شركة التأمين مطلوبة");
                isValid = false;
            }

            if (!ValidationHelper.IsNotEmpty(Entity?.PolicyNumber))
            {
                AddError(nameof(Entity.PolicyNumber), "رقم البوليصة مطلوب");
                isValid = false;
            }

            if (Entity?.CoverageAmount <= 0)
            {
                AddError(nameof(Entity.CoverageAmount), "المبلغ المؤمن يجب أن يكون أكبر من صفر");
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
                    await _insuranceService.CreateInsuranceAsync(Entity);
                    ShowInfo("تم إضافة بوليصة التأمين بنجاح");
                }
                else
                {
                    await _insuranceService.UpdateInsuranceAsync(Entity);
                    ShowInfo("تم تحديث بوليصة التأمين بنجاح");
                }

                OnSave();
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في حفظ بوليصة التأمين: {ex.Message}");
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
// FILE: CustodyViewModel.cs
// LOCATION: FleetManagementSystem.WPF/ViewModels/CustodyViewModel.cs
// PURPOSE: Custody/Asset tracking list and form ViewModel
// ============================================================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Core.Services;

namespace FleetManagementSystem.WPF.ViewModels
{
    public class CustodyViewModel : ListViewModelBase<CustodyDto>
    {
        private readonly ICustodyService _custodyService;
        private CustodyFormViewModel _formViewModel;
        private bool _showForm;

        public CustodyFormViewModel FormViewModel
        {
            get => _formViewModel;
            set => SetProperty(ref _formViewModel, value);
        }

        public bool ShowForm
        {
            get => _showForm;
            set => SetProperty(ref _showForm, value);
        }

        public CustodyViewModel(ICustodyService custodyService)
        {
            _custodyService = custodyService;
            FormViewModel = new CustodyFormViewModel(custodyService);

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            RefreshCommand = new AsyncRelayCommand(LoadItemsAsync);
            SearchCommand = new AsyncRelayCommand(LoadItemsAsync);
            AddCommand = new RelayCommand(_ => OpenAddForm());
            EditCommand = new RelayCommand<CustodyDto>(custody => OpenEditForm(custody), _ => SelectedItem != null);
            DeleteCommand = new AsyncRelayCommand<CustodyDto>(DeleteItemAsync, _ => SelectedItem != null);
        }

        public override async Task LoadItemsAsync()
        {
            IsLoading = true;
            ClearMessages();

            try
            {
                var result = await _custodyService.GetCustodiesAsync(
                    searchText: SearchText,
                    pageNumber: CurrentPage,
                    pageSize: PageSize);

                Items.Clear();
                foreach (var custody in result.Items)
                {
                    Items.Add(custody);
                }

                TotalCount = result.TotalCount;
                ShowInfo($"تم تحميل {Items.Count} عهدة");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تحميل العهد: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public override async Task DeleteItemAsync(CustodyDto custody)
        {
            if (custody == null) return;

            var confirmed = MessageBox.Show(
                $"هل تريد حذف العهدة؟",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmed == MessageBoxResult.Yes)
            {
                IsLoading = true;
                try
                {
                    await _custodyService.DeleteCustodyAsync(custody.Id);
                    Items.Remove(custody);
                    ShowInfo("تم حذف العهدة بنجاح");
                    await LoadItemsAsync();
                }
                catch (Exception ex)
                {
                    ShowError($"خطأ في حذف العهدة: {ex.Message}");
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
            FormViewModel.Entity = new CustodyDto();
            ShowForm = true;
        }

        private void OpenEditForm(CustodyDto custody)
        {
            if (custody == null) return;
            FormViewModel.IsNew = false;
            FormViewModel.Entity = custody;
            ShowForm = true;
        }
    }

    public class CustodyFormViewModel : FormViewModelBase<CustodyDto>
    {
        private readonly ICustodyService _custodyService;
        private ObservableCollection<string> _employees;

        public ObservableCollection<string> Employees
        {
            get => _employees;
            set => SetProperty(ref _employees, value);
        }

        public CustodyFormViewModel(ICustodyService custodyService)
        {
            _custodyService = custodyService;
            Employees = new ObservableCollection<string>();

            SaveCommand = new AsyncRelayCommand(SaveEntityAsync, () => !HasErrors);
            CancelCommand = new RelayCommand(_ => OnCancel());
        }

        public override bool ValidateEntity()
        {
            ClearErrors();
            bool isValid = true;

            if (Entity?.EmployeeId <= 0)
            {
                AddError(nameof(Entity.EmployeeId), "الموظف مطلوب");
                isValid = false;
            }

            if (!ValidationHelper.IsNotEmpty(Entity?.ItemType))
            {
                AddError(nameof(Entity.ItemType), "نوع الصنف مطلوب");
                isValid = false;
            }

            if (Entity?.ItemValue < 0)
            {
                AddError(nameof(Entity.ItemValue), "قيمة الصنف يجب أن تكون موجبة");
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
                    await _custodyService.CreateCustodyAsync(Entity);
                    ShowInfo("تم إضافة العهدة بنجاح");
                }
                else
                {
                    await _custodyService.UpdateCustodyAsync(Entity);
                    ShowInfo("تم تحديث العهدة بنجاح");
                }

                OnSave();
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في حفظ العهدة: {ex.Message}");
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
// FILE: MasterDataViewModel.cs
// LOCATION: FleetManagementSystem.WPF/ViewModels/MasterDataViewModel.cs
// PURPOSE: Master data configuration (vehicle types, statuses, etc.)
// ============================================================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Core.Services;

namespace FleetManagementSystem.WPF.ViewModels
{
    public class MasterDataViewModel : BaseViewModel
    {
        private readonly IMasterDataService _masterDataService;
        private ObservableCollection<VehicleTypeDto> _vehicleTypes;
        private ObservableCollection<ContractStatusDto> _contractStatuses;
        private ObservableCollection<MaintenanceTypeDto> _maintenanceTypes;
        private ObservableCollection<ServiceProviderDto> _serviceProviders;
        private int _selectedTabIndex;

        public ObservableCollection<VehicleTypeDto> VehicleTypes
        {
            get => _vehicleTypes;
            set => SetProperty(ref _vehicleTypes, value);
        }

        public ObservableCollection<ContractStatusDto> ContractStatuses
        {
            get => _contractStatuses;
            set => SetProperty(ref _contractStatuses, value);
        }

        public ObservableCollection<MaintenanceTypeDto> MaintenanceTypes
        {
            get => _maintenanceTypes;
            set => SetProperty(ref _maintenanceTypes, value);
        }

        public ObservableCollection<ServiceProviderDto> ServiceProviders
        {
            get => _serviceProviders;
            set => SetProperty(ref _serviceProviders, value);
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        public ICommand LoadMasterDataCommand { get; private set; }
        public ICommand AddVehicleTypeCommand { get; private set; }
        public ICommand DeleteVehicleTypeCommand { get; private set; }
        public ICommand AddServiceProviderCommand { get; private set; }
        public ICommand DeleteServiceProviderCommand { get; private set; }

        public MasterDataViewModel(IMasterDataService masterDataService)
        {
            _masterDataService = masterDataService;
            VehicleTypes = new ObservableCollection<VehicleTypeDto>();
            ContractStatuses = new ObservableCollection<ContractStatusDto>();
            MaintenanceTypes = new ObservableCollection<MaintenanceTypeDto>();
            ServiceProviders = new ObservableCollection<ServiceProviderDto>();

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            LoadMasterDataCommand = new AsyncRelayCommand(LoadMasterDataAsync);
            AddVehicleTypeCommand = new RelayCommand(_ => AddVehicleType());
            DeleteVehicleTypeCommand = new AsyncRelayCommand<VehicleTypeDto>(DeleteVehicleTypeAsync);
            AddServiceProviderCommand = new RelayCommand(_ => AddServiceProvider());
            DeleteServiceProviderCommand = new AsyncRelayCommand<ServiceProviderDto>(DeleteServiceProviderAsync);
        }

        public async Task LoadMasterDataAsync()
        {
            IsLoading = true;
            ClearMessages();

            try
            {
                var vehicleTypes = await _masterDataService.GetVehicleTypesAsync();
                VehicleTypes.Clear();
                foreach (var type in vehicleTypes)
                    VehicleTypes.Add(type);

                var contractStatuses = await _masterDataService.GetContractStatusesAsync();
                ContractStatuses.Clear();
                foreach (var status in contractStatuses)
                    ContractStatuses.Add(status);

                var maintenanceTypes = await _masterDataService.GetMaintenanceTypesAsync();
                MaintenanceTypes.Clear();
                foreach (var type in maintenanceTypes)
                    MaintenanceTypes.Add(type);

                var serviceProviders = await _masterDataService.GetServiceProvidersAsync();
                ServiceProviders.Clear();
                foreach (var provider in serviceProviders)
                    ServiceProviders.Add(provider);

                ShowInfo("تم تحميل بيانات الماستر بنجاح");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تحميل بيانات الماستر: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddVehicleType()
        {
            // Open dialog to add new vehicle type
        }

        private async Task DeleteVehicleTypeAsync(VehicleTypeDto vehicleType)
        {
            if (vehicleType == null) return;

            IsLoading = true;
            try
            {
                await _masterDataService.DeleteVehicleTypeAsync(vehicleType.Id);
                VehicleTypes.Remove(vehicleType);
                ShowInfo("تم حذف نوع المركبة بنجاح");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في حذف نوع المركبة: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddServiceProvider()
        {
            // Open dialog to add new service provider
        }

        private async Task DeleteServiceProviderAsync(ServiceProviderDto provider)
        {
            if (provider == null) return;

            IsLoading = true;
            try
            {
                await _masterDataService.DeleteServiceProviderAsync(provider.Id);
                ServiceProviders.Remove(provider);
                ShowInfo("تم حذف مقدم الخدمة بنجاح");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في حذف مقدم الخدمة: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}

// ============================================================================
// FILE: ReportsViewModel.cs
// LOCATION: FleetManagementSystem.WPF/ViewModels/ReportsViewModel.cs
// PURPOSE: Reports generation and export
// ============================================================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Core.Services;

namespace FleetManagementSystem.WPF.ViewModels
{
    public class ReportsViewModel : BaseViewModel
    {
        private readonly IReportingService _reportingService;
        private DateTime _startDate = DateTime.Now.AddMonths(-1);
        private DateTime _endDate = DateTime.Now;
        private ObservableCollection<ReportItem> _reportData;

        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public ObservableCollection<ReportItem> ReportData
        {
            get => _reportData;
            set => SetProperty(ref _reportData, value);
        }

        public ICommand GenerateVehicleReportCommand { get; private set; }
        public ICommand GenerateContractReportCommand { get; private set; }
        public ICommand GenerateMaintenanceReportCommand { get; private set; }
        public ICommand ExportToExcelCommand { get; private set; }
        public ICommand ExportToPdfCommand { get; private set; }
        public ICommand PrintReportCommand { get; private set; }

        public ReportsViewModel(IReportingService reportingService)
        {
            _reportingService = reportingService;
            ReportData = new ObservableCollection<ReportItem>();

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            GenerateVehicleReportCommand = new AsyncRelayCommand(GenerateVehicleReportAsync);
            GenerateContractReportCommand = new AsyncRelayCommand(GenerateContractReportAsync);
            GenerateMaintenanceReportCommand = new AsyncRelayCommand(GenerateMaintenanceReportAsync);
            ExportToExcelCommand = new AsyncRelayCommand(ExportToExcelAsync, () => ReportData.Count > 0);
            ExportToPdfCommand = new AsyncRelayCommand(ExportToPdfAsync, () => ReportData.Count > 0);
            PrintReportCommand = new AsyncRelayCommand(PrintReportAsync, () => ReportData.Count > 0);
        }

        private async Task GenerateVehicleReportAsync()
        {
            IsLoading = true;
            ClearMessages();

            try
            {
                var report = await _reportingService.GenerateVehicleReportAsync(StartDate, EndDate);
                ReportData.Clear();
                foreach (var item in report.Items)
                {
                    ReportData.Add(new ReportItem { Data = item });
                }
                ShowInfo($"تم توليد التقرير بنجاح - {ReportData.Count} سجل");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في توليد التقرير: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateContractReportAsync()
        {
            IsLoading = true;
            ClearMessages();

            try
            {
                var report = await _reportingService.GenerateContractReportAsync(StartDate, EndDate);
                ReportData.Clear();
                foreach (var item in report.Items)
                {
                    ReportData.Add(new ReportItem { Data = item });
                }
                ShowInfo($"تم توليد التقرير بنجاح - {ReportData.Count} سجل");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في توليد التقرير: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateMaintenanceReportAsync()
        {
            IsLoading = true;
            ClearMessages();

            try
            {
                var report = await _reportingService.GenerateMaintenanceReportAsync(StartDate, EndDate);
                ReportData.Clear();
                foreach (var item in report.Items)
                {
                    ReportData.Add(new ReportItem { Data = item });
                }
                ShowInfo($"تم توليد التقرير بنجاح - {ReportData.Count} سجل");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في توليد التقرير: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportToExcelAsync()
        {
            IsLoading = true;
            try
            {
                await _reportingService.ExportToExcelAsync(ReportData, "تقرير_المركبات.xlsx");
                ShowInfo("تم تصدير التقرير إلى Excel بنجاح");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تصدير التقرير: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportToPdfAsync()
        {
            IsLoading = true;
            try
            {
                await _reportingService.ExportToPdfAsync(ReportData, "تقرير_المركبات.pdf");
                ShowInfo("تم تصدير التقرير إلى PDF بنجاح");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تصدير التقرير: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task PrintReportAsync()
        {
            IsLoading = true;
            try
            {
                await _reportingService.PrintReportAsync(ReportData);
                ShowInfo("تم طباعة التقرير بنجاح");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في طباعة التقرير: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    public class ReportItem
    {
        public object Data { get; set; }
    }
}

// ============================================================================
// FILE: SettingsViewModel.cs
// LOCATION: FleetManagementSystem.WPF/ViewModels/SettingsViewModel.cs
// PURPOSE: Application settings and user management
// ============================================================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Core.Services;

namespace FleetManagementSystem.WPF.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ISettingsService _settingsService;
        private readonly IUserService _userService;
        private CompanySettingsDto _companySettings;
        private ObservableCollection<UserDto> _users;
        private ObservableCollection<RoleDto> _roles;
        private string _currentPassword;
        private string _newPassword;
        private string _confirmPassword;
        private int _selectedTabIndex;

        public CompanySettingsDto CompanySettings
        {
            get => _companySettings;
            set => SetProperty(ref _companySettings, value);
        }

        public ObservableCollection<UserDto> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        public ObservableCollection<RoleDto> Roles
        {
            get => _roles;
            set => SetProperty(ref _roles, value);
        }

        public string CurrentPassword
        {
            get => _currentPassword;
            set => SetProperty(ref _currentPassword, value);
        }

        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        public ICommand LoadSettingsCommand { get; private set; }
        public ICommand SaveCompanySettingsCommand { get; private set; }
        public ICommand ChangePasswordCommand { get; private set; }
        public ICommand AddUserCommand { get; private set; }
        public ICommand DeleteUserCommand { get; private set; }
        public ICommand EditUserCommand { get; private set; }

        public SettingsViewModel(ISettingsService settingsService, IUserService userService)
        {
            _settingsService = settingsService;
            _userService = userService;
            Users = new ObservableCollection<UserDto>();
            Roles = new ObservableCollection<RoleDto>();

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            LoadSettingsCommand = new AsyncRelayCommand(LoadSettingsAsync);
            SaveCompanySettingsCommand = new AsyncRelayCommand(SaveCompanySettingsAsync);
            ChangePasswordCommand = new AsyncRelayCommand(ChangePasswordAsync);
            AddUserCommand = new RelayCommand(_ => AddUser());
            DeleteUserCommand = new AsyncRelayCommand<UserDto>(DeleteUserAsync);
            EditUserCommand = new RelayCommand<UserDto>(user => EditUser(user));
        }

        public async Task LoadSettingsAsync()
        {
            IsLoading = true;
            ClearMessages();

            try
            {
                CompanySettings = await _settingsService.GetCompanySettingsAsync();
                var users = await _userService.GetAllUsersAsync();
                Users.Clear();
                foreach (var user in users)
                    Users.Add(user);

                var roles = await _userService.GetAllRolesAsync();
                Roles.Clear();
                foreach (var role in roles)
                    Roles.Add(role);

                ShowInfo("تم تحميل الإعدادات بنجاح");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تحميل الإعدادات: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveCompanySettingsAsync()
        {
            IsLoading = true;
            ClearMessages();

            try
            {
                await _settingsService.SaveCompanySettingsAsync(CompanySettings);
                ShowInfo("تم حفظ إعدادات الشركة بنجاح");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في حفظ الإعدادات: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ChangePasswordAsync()
        {
            if (string.IsNullOrEmpty(CurrentPassword) || string.IsNullOrEmpty(NewPassword))
            {
                ShowError("يرجى ملء جميع الحقول");
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                ShowError("كلمات المرور غير متطابقة");
                return;
            }

            IsLoading = true;
            ClearMessages();

            try
            {
                await _userService.ChangePasswordAsync(CurrentPassword, NewPassword);
                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
                ConfirmPassword = string.Empty;
                ShowInfo("تم تغيير كلمة المرور بنجاح");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تغيير كلمة المرور: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddUser()
        {
            // Open dialog to add new user
        }

        private async Task DeleteUserAsync(UserDto user)
        {
            if (user == null) return;

            IsLoading = true;
            try
            {
                await _userService.DeleteUserAsync(user.Id);
                Users.Remove(user);
                ShowInfo("تم حذف المستخدم بنجاح");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في حذف المستخدم: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void EditUser(UserDto user)
        {
            // Open dialog to edit user
        }
    }
}
