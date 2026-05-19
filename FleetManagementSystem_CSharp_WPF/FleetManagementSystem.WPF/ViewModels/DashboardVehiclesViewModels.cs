// ============================================================================
// FILE: DashboardViewModel.cs
// LOCATION: FleetManagementSystem.WPF/ViewModels/DashboardViewModel.cs
// PURPOSE: Dashboard with KPI cards, alerts, and recent activity
// ============================================================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManagementSystem.Core.Services;

namespace FleetManagementSystem.WPF.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IReportingService _reportingService;
        private readonly IVehicleService _vehicleService;
        private readonly IContractService _contractService;
        private readonly IMaintenanceService _maintenanceService;

        private int _totalVehicles;
        private int _activeContracts;
        private int _pendingMaintenance;
        private int _expiredLicenses;
        private decimal _totalFleetValue;
        private ObservableCollection<AlertItem> _alerts;
        private ObservableCollection<ActivityItem> _recentActivity;

        public int TotalVehicles
        {
            get => _totalVehicles;
            set => SetProperty(ref _totalVehicles, value);
        }

        public int ActiveContracts
        {
            get => _activeContracts;
            set => SetProperty(ref _activeContracts, value);
        }

        public int PendingMaintenance
        {
            get => _pendingMaintenance;
            set => SetProperty(ref _pendingMaintenance, value);
        }

        public int ExpiredLicenses
        {
            get => _expiredLicenses;
            set => SetProperty(ref _expiredLicenses, value);
        }

        public decimal TotalFleetValue
        {
            get => _totalFleetValue;
            set => SetProperty(ref _totalFleetValue, value);
        }

        public ObservableCollection<AlertItem> Alerts
        {
            get => _alerts;
            set => SetProperty(ref _alerts, value);
        }

        public ObservableCollection<ActivityItem> RecentActivity
        {
            get => _recentActivity;
            set => SetProperty(ref _recentActivity, value);
        }

        public ICommand RefreshCommand { get; private set; }
        public ICommand ViewVehiclesCommand { get; private set; }
        public ICommand ViewContractsCommand { get; private set; }
        public ICommand ViewMaintenanceCommand { get; private set; }

        public DashboardViewModel(
            IReportingService reportingService,
            IVehicleService vehicleService,
            IContractService contractService,
            IMaintenanceService maintenanceService)
        {
            _reportingService = reportingService;
            _vehicleService = vehicleService;
            _contractService = contractService;
            _maintenanceService = maintenanceService;

            Alerts = new ObservableCollection<AlertItem>();
            RecentActivity = new ObservableCollection<ActivityItem>();

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            RefreshCommand = new AsyncRelayCommand(RefreshDashboardAsync);
            ViewVehiclesCommand = new RelayCommand(_ => OnNavigate("Vehicles"));
            ViewContractsCommand = new RelayCommand(_ => OnNavigate("Contracts"));
            ViewMaintenanceCommand = new RelayCommand(_ => OnNavigate("Maintenance"));
        }

        public async Task LoadDashboardAsync()
        {
            IsLoading = true;
            ClearMessages();

            try
            {
                // Load KPI metrics
                var metrics = await _reportingService.GetDashboardMetricsAsync();
                TotalVehicles = metrics.TotalVehicles;
                ActiveContracts = metrics.ActiveContracts;
                PendingMaintenance = metrics.PendingMaintenance;
                ExpiredLicenses = metrics.ExpiredLicenses;
                TotalFleetValue = metrics.TotalFleetValue;

                // Load alerts
                var alerts = await _reportingService.GetAlertsAsync();
                Alerts.Clear();
                foreach (var alert in alerts)
                {
                    Alerts.Add(new AlertItem
                    {
                        Id = alert.Id,
                        Title = alert.Title,
                        Message = alert.Message,
                        Severity = alert.Severity,
                        CreatedAt = alert.CreatedAt
                    });
                }

                // Load recent activity
                var activities = await _reportingService.GetRecentActivityAsync(10);
                RecentActivity.Clear();
                foreach (var activity in activities)
                {
                    RecentActivity.Add(new ActivityItem
                    {
                        Id = activity.Id,
                        Description = activity.Description,
                        Action = activity.Action,
                        Timestamp = activity.Timestamp,
                        User = activity.User
                    });
                }

                ShowInfo("تم تحديث لوحة التحكم بنجاح");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تحميل لوحة التحكم: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshDashboardAsync()
        {
            await LoadDashboardAsync();
        }

        private void OnNavigate(string module)
        {
            // This would be handled by a navigation service
            // For now, just a placeholder
        }
    }

    public class AlertItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Severity { get; set; } // Info, Warning, Error
        public DateTime CreatedAt { get; set; }
    }

    public class ActivityItem
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
        public string User { get; set; }
    }
}

// ============================================================================
// FILE: VehicleViewModel.cs
// LOCATION: FleetManagementSystem.WPF/ViewModels/VehicleViewModel.cs
// PURPOSE: Vehicle list and form ViewModel
// ============================================================================

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Core.Services;

namespace FleetManagementSystem.WPF.ViewModels
{
    public class VehicleViewModel : ListViewModelBase<VehicleDto>
    {
        private readonly IVehicleService _vehicleService;
        private ObservableCollection<string> _vehicleTypes;
        private ObservableCollection<string> _vehicleStatuses;
        private VehicleFormViewModel _formViewModel;
        private bool _showForm;

        public ObservableCollection<string> VehicleTypes
        {
            get => _vehicleTypes;
            set => SetProperty(ref _vehicleTypes, value);
        }

        public ObservableCollection<string> VehicleStatuses
        {
            get => _vehicleStatuses;
            set => SetProperty(ref _vehicleStatuses, value);
        }

        public VehicleFormViewModel FormViewModel
        {
            get => _formViewModel;
            set => SetProperty(ref _formViewModel, value);
        }

        public bool ShowForm
        {
            get => _showForm;
            set => SetProperty(ref _showForm, value);
        }

        public VehicleViewModel(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
            VehicleTypes = new ObservableCollection<string>();
            VehicleStatuses = new ObservableCollection<string>();
            FormViewModel = new VehicleFormViewModel(vehicleService);

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            RefreshCommand = new AsyncRelayCommand(LoadItemsAsync);
            SearchCommand = new AsyncRelayCommand(LoadItemsAsync);
            AddCommand = new RelayCommand(_ => OpenAddForm());
            EditCommand = new RelayCommand<VehicleDto>(vehicle => OpenEditForm(vehicle), _ => SelectedItem != null);
            DeleteCommand = new AsyncRelayCommand<VehicleDto>(DeleteItemAsync, _ => SelectedItem != null);
            PreviousPageCommand = new RelayCommand(_ => PreviousPage(), _ => CurrentPage > 1);
            NextPageCommand = new RelayCommand(_ => NextPage(), _ => CurrentPage < TotalPages);
        }

        public override async Task LoadItemsAsync()
        {
            IsLoading = true;
            ClearMessages();

            try
            {
                var result = await _vehicleService.GetVehiclesAsync(
                    searchText: SearchText,
                    pageNumber: CurrentPage,
                    pageSize: PageSize);

                Items.Clear();
                foreach (var vehicle in result.Items)
                {
                    Items.Add(vehicle);
                }

                TotalCount = result.TotalCount;
                ShowInfo($"تم تحميل {Items.Count} مركبة");
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تحميل المركبات: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public override async Task DeleteItemAsync(VehicleDto vehicle)
        {
            if (vehicle == null) return;

            var confirmed = MessageBox.Show(
                $"هل تريد حذف المركبة {vehicle.PlateNumber}؟",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmed == MessageBoxResult.Yes)
            {
                IsLoading = true;
                try
                {
                    await _vehicleService.DeleteVehicleAsync(vehicle.Id);
                    Items.Remove(vehicle);
                    ShowInfo("تم حذف المركبة بنجاح");
                    await LoadItemsAsync();
                }
                catch (Exception ex)
                {
                    ShowError($"خطأ في حذف المركبة: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        protected override void OnSearchTextChanged()
        {
            CurrentPage = 1;
        }

        private void OpenAddForm()
        {
            FormViewModel.IsNew = true;
            FormViewModel.Entity = new VehicleDto();
            ShowForm = true;
        }

        private void OpenEditForm(VehicleDto vehicle)
        {
            if (vehicle == null) return;
            FormViewModel.IsNew = false;
            FormViewModel.Entity = vehicle;
            ShowForm = true;
        }

        private void PreviousPage()
        {
            if (CurrentPage > 1)
                CurrentPage--;
        }

        private void NextPage()
        {
            if (CurrentPage < TotalPages)
                CurrentPage++;
        }
    }

    public class VehicleFormViewModel : FormViewModelBase<VehicleDto>
    {
        private readonly IVehicleService _vehicleService;
        private ObservableCollection<string> _vehicleTypes;
        private ObservableCollection<string> _vehicleStatuses;

        public ObservableCollection<string> VehicleTypes
        {
            get => _vehicleTypes;
            set => SetProperty(ref _vehicleTypes, value);
        }

        public ObservableCollection<string> VehicleStatuses
        {
            get => _vehicleStatuses;
            set => SetProperty(ref _vehicleStatuses, value);
        }

        public VehicleFormViewModel(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
            VehicleTypes = new ObservableCollection<string>();
            VehicleStatuses = new ObservableCollection<string>();

            SaveCommand = new AsyncRelayCommand(SaveEntityAsync, () => !HasErrors);
            CancelCommand = new RelayCommand(_ => OnCancel());
        }

        public override bool ValidateEntity()
        {
            ClearErrors();
            bool isValid = true;

            if (!ValidationHelper.IsValidPlateNumber(Entity?.PlateNumber))
            {
                AddError(nameof(Entity.PlateNumber), "رقم اللوحة غير صحيح");
                isValid = false;
            }

            if (!ValidationHelper.IsNotEmpty(Entity?.VehicleType))
            {
                AddError(nameof(Entity.VehicleType), "نوع المركبة مطلوب");
                isValid = false;
            }

            if (!ValidationHelper.IsNotEmpty(Entity?.Model))
            {
                AddError(nameof(Entity.Model), "موديل المركبة مطلوب");
                isValid = false;
            }

            if (Entity?.Year < 1900 || Entity?.Year > DateTime.Now.Year + 1)
            {
                AddError(nameof(Entity.Year), "سنة الصنع غير صحيحة");
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
                    await _vehicleService.CreateVehicleAsync(Entity);
                    ShowInfo("تم إضافة المركبة بنجاح");
                }
                else
                {
                    await _vehicleService.UpdateVehicleAsync(Entity);
                    ShowInfo("تم تحديث المركبة بنجاح");
                }

                OnSave();
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في حفظ المركبة: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnSave()
        {
            // This would trigger closing the form dialog
        }

        private void OnCancel()
        {
            // This would trigger closing the form dialog
        }
    }
}
