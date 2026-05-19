using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Services;
using FleetManagementSystem.WPF.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Brush = System.Windows.Media.Brush;

namespace FleetManagementSystem.WPF;

public partial class MainWindow : Window
{
    private const double DefaultGridColumnWidth = 190;
    private const double NarrowGridColumnWidth = 95;
    private const double CompactGridColumnWidth = 140;
    private const double DateGridColumnWidth = 170;
    private const double WideGridColumnWidth = 260;
    private const double ExtraWideGridColumnWidth = 320;

    private static readonly HashSet<string> HiddenColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "VehicleType",
        "VehiclePlateNumber",
        "DriverName",
        "RequesterName",
        "SupervisorName",
        "MaintenanceType",
        "ServiceProvider",
        "Items",
        "ConfirmPassword"
    };

    private static readonly Dictionary<string, string> ColumnHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Id"] = "رقم",
        ["PlateNumber"] = "رقم العربية",
        ["VehicleId"] = "كود العربية",
        ["VehicleTypeId"] = "نوع العربية",
        ["Model"] = "الموديل",
        ["Year"] = "سنة الصنع",
        ["Manufacturer"] = "الشركة المصنعة",
        ["Color"] = "اللون",
        ["ChassisNumber"] = "رقم الشاسيه",
        ["EngineNumber"] = "رقم الموتور",
        ["CurrentMileage"] = "العداد الحالي",
        ["Status"] = "الحالة",
        ["AssignedTo"] = "مخصص لـ",
        ["PurchaseDate"] = "تاريخ الشراء",
        ["PurchasePrice"] = "سعر الشراء",
        ["RegistrationStartDate"] = "بداية الترخيص",
        ["RegistrationExpiryDate"] = "نهاية الترخيص",
        ["AccidentInsuranceDetails"] = "تأمين الحوادث",
        ["SocialInsuranceDetails"] = "التأمين الاجتماعي",
        ["OilChangeIntervalKm"] = "دورية الزيت",
        ["MaintenanceIntervalKm"] = "دورية الصيانة",
        ["Notes"] = "ملاحظات",
        ["ContractNumber"] = "رقم العقد",
        ["ClientName"] = "العميل",
        ["StartDate"] = "تاريخ البداية",
        ["EndDate"] = "تاريخ النهاية",
        ["ContractValue"] = "قيمة العقد",
        ["PaidAmount"] = "المدفوع",
        ["PaymentTerms"] = "شروط الدفع",
        ["ContractTerms"] = "شروط العقد",
        ["RequestDate"] = "تاريخ الطلب",
        ["CompletionDate"] = "تاريخ الإكمال",
        ["Description"] = "الوصف",
        ["EstimatedCost"] = "التكلفة المتوقعة",
        ["ActualCost"] = "التكلفة الفعلية",
        ["WorkPerformed"] = "الأعمال المنفذة",
        ["FullName"] = "الاسم",
        ["NationalId"] = "الرقم القومي",
        ["PhoneNumber"] = "رقم الهاتف",
        ["Email"] = "البريد الإلكتروني",
        ["Address"] = "العنوان",
        ["DateOfBirth"] = "تاريخ الميلاد",
        ["LicenseNumber"] = "رقم الرخصة",
        ["LicenseExpiryDate"] = "انتهاء الرخصة",
        ["LicenseType"] = "نوع الرخصة",
        ["IsActive"] = "نشط",
        ["EmployeeId"] = "كود الموظف",
        ["Department"] = "القسم",
        ["Position"] = "الوظيفة",
        ["HireDate"] = "تاريخ التعيين",
        ["TerminationDate"] = "تاريخ الانتهاء",
        ["TransactionDate"] = "التاريخ",
        ["TransactionType"] = "نوع الحركة",
        ["Amount"] = "المبلغ",
        ["RelatedEntityType"] = "مرتبط بـ",
        ["RelatedEntityId"] = "رقم المرتبط",
        ["PaymentMethod"] = "طريقة الدفع",
        ["PolicyNumber"] = "رقم الوثيقة",
        ["InsuranceCompany"] = "شركة التأمين",
        ["PolicyType"] = "نوع الوثيقة",
        ["ExpiryDate"] = "تاريخ الانتهاء",
        ["PremiumAmount"] = "القسط",
        ["CoverageAmount"] = "مبلغ التغطية",
        ["CoverageDetails"] = "تفاصيل التغطية",
        ["AgentName"] = "مندوب التأمين",
        ["AgentPhoneNumber"] = "هاتف المندوب",
        ["CustodyNumber"] = "رقم العهدة",
        ["CustodianName"] = "المستلم",
        ["CustodianPosition"] = "وظيفة المستلم",
        ["HandoverDate"] = "تاريخ التسليم",
        ["ReturnDate"] = "تاريخ الإرجاع",
        ["VehicleConditionRating"] = "تقييم الحالة",
        ["Username"] = "اسم المستخدم",
        ["Role"] = "الدور",
        ["Password"] = "كلمة المرور"
    };

    private readonly IServiceProvider _serviceProvider;
    private readonly IVehicleService _vehicleService;
    private readonly IContractService _contractService;
    private readonly IMaintenanceService _maintenanceService;
    private readonly IDriverService _driverService;
    private readonly IEmployeeService _employeeService;
    private readonly ITripService _tripService;
    private readonly IFuelService _fuelService;
    private readonly IExpenseService _expenseService;
    private readonly IOilChangeService _oilChangeService;
    private readonly ITreasuryService _treasuryService;
    private readonly IInsuranceService _insuranceService;
    private readonly ICustodyService _custodyService;
    private readonly IMasterDataService _masterDataService;
    private readonly IReportingService _reportingService;
    private readonly ISettingsService _settingsService;
    private readonly INotificationService _notificationService;
    private readonly IAuthenticationService _authenticationService;
    private readonly ConnectionSettingsStore _connectionSettingsStore;
    private readonly ClientConnectionProfile _connectionProfile;

    private readonly ObservableCollection<VehicleDto> _vehicles = new();
    private readonly ObservableCollection<ContractDto> _contracts = new();
    private readonly ObservableCollection<MaintenanceRequestDto> _maintenance = new();
    private readonly ObservableCollection<DriverDto> _drivers = new();
    private readonly ObservableCollection<EmployeeDto> _employees = new();
    private readonly ObservableCollection<TripDto> _trips = new();
    private readonly ObservableCollection<FuelTransactionDto> _fuel = new();
    private readonly ObservableCollection<ExpenseDto> _expenses = new();
    private readonly ObservableCollection<OilChangeDto> _oilChanges = new();
    private readonly ObservableCollection<TreasuryTransactionDto> _treasuryTransactions = new();
    private readonly ObservableCollection<VehicleLicenseRow> _vehicleLicenses = new();
    private readonly ObservableCollection<InsuranceDto> _insurance = new();
    private readonly ObservableCollection<CustodyDto> _custodies = new();
    private List<AlertDto> _allDashboardAlerts = new();
    private readonly ObservableCollection<VehicleTypeDto> _vehicleTypes = new();
    private readonly ObservableCollection<ContractStatusDto> _contractStatuses = new();
    private readonly ObservableCollection<MaintenanceTypeDto> _maintenanceTypes = new();
    private readonly ObservableCollection<ServiceProviderDto> _serviceProviders = new();
    private readonly ObservableCollection<NotificationDto> _notifications = new();
    private readonly ObservableCollection<UserFormDto> _users = new();

    private UserDto? _currentUser;
    private CompanySettingsDto? _settings;
    private ReportDataDto? _currentReport;
    private readonly DispatcherTimer _clockTimer;

    public ObservableCollection<VehicleDto> VehiclesForBinding => _vehicles;
    public ObservableCollection<DriverDto> DriversForBinding => _drivers;
    public ObservableCollection<EmployeeDto> EmployeesForBinding => _employees;
    public ObservableCollection<ContractStatusDto> ContractStatusesForBinding => _contractStatuses;
    public ObservableCollection<MaintenanceTypeDto> MaintenanceTypesForBinding => _maintenanceTypes;
    public ObservableCollection<ServiceProviderDto> ServiceProvidersForBinding => _serviceProviders;
    public IReadOnlyList<string> TreasuryTransactionTypes { get; } = new[] { "إيراد", "صرف" };
    public IReadOnlyList<string> PaymentMethods { get; } = new[] { "نقدي", "تحويل بنكي", "بطاقة" };

    public sealed class VehicleLicenseRow
    {
        public int VehicleId { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public DateTime? RegistrationStartDate { get; set; }
        public DateTime? RegistrationExpiryDate { get; set; }
        public int? DaysUntilExpiry => RegistrationExpiryDate.HasValue
            ? (RegistrationExpiryDate.Value.Date - DateTime.Today).Days
            : null;
        public string Status => DaysUntilExpiry switch
        {
            null => "غير مسجل",
            < 0 => "منتهي",
            <= 30 => "قارب الانتهاء",
            _ => "ساري"
        };
        public string ExpiryAlert => DaysUntilExpiry switch
        {
            null => "أدخل بداية ونهاية الترخيص",
            < 0 => "الترخيص منتهي",
            <= 30 => $"يحتاج تجديد خلال {DaysUntilExpiry} يوم",
            _ => "لا يوجد إنذار"
        };

        public static VehicleLicenseRow FromVehicle(VehicleDto vehicle) => new()
        {
            VehicleId = vehicle.Id,
            PlateNumber = vehicle.PlateNumber,
            Model = vehicle.Model,
            RegistrationStartDate = vehicle.RegistrationStartDate,
            RegistrationExpiryDate = vehicle.RegistrationExpiryDate
        };
    }

    public sealed class DashboardAlertItem
    {
        private static readonly Brush CriticalAccentBrush = CreateBrush("#B64153");
        private static readonly Brush CriticalBackgroundBrush = CreateBrush("#FFF5F7");
        private static readonly Brush CriticalBorderBrush = CreateBrush("#F0C4CB");
        private static readonly Brush CriticalChipBrush = CreateBrush("#F8DEE3");

        private static readonly Brush UrgentAccentBrush = CreateBrush("#A66A16");
        private static readonly Brush UrgentBackgroundBrush = CreateBrush("#FFF8ED");
        private static readonly Brush UrgentBorderBrush = CreateBrush("#F0D1A2");
        private static readonly Brush UrgentChipBrush = CreateBrush("#FCE8C8");

        private static readonly Brush SoonAccentBrush = CreateBrush("#123B53");
        private static readonly Brush SoonBackgroundBrush = CreateBrush("#F4F8FB");
        private static readonly Brush SoonBorderBrush = CreateBrush("#C9DAE4");
        private static readonly Brush SoonChipBrush = CreateBrush("#EAF2F7");

        private static readonly Brush TextBrushValue = CreateBrush("#1F2D3A");

        public string Title { get; private init; } = string.Empty;
        public string Message { get; private init; } = string.Empty;
        public string CategoryText { get; private init; } = string.Empty;
        public string CategoryInitial { get; private init; } = string.Empty;
        public string StatusText { get; private init; } = string.Empty;
        public string TimingText { get; private init; } = string.Empty;
        public string DueDateText { get; private init; } = string.Empty;
        public DateTime? DueDate { get; private init; }
        public int SortPriority { get; private init; }
        public int SortDistance { get; private init; }
        public Brush AccentBrush { get; private init; } = SoonAccentBrush;
        public Brush BackgroundBrush { get; private init; } = SoonBackgroundBrush;
        public Brush BorderBrush { get; private init; } = SoonBorderBrush;
        public Brush ChipBrush { get; private init; } = SoonChipBrush;
        public Brush TextBrush { get; private init; } = TextBrushValue;

        public static DashboardAlertItem FromAlert(AlertDto alert)
        {
            var dueDate = alert.DueDate?.Date;
            var days = dueDate.HasValue ? (dueDate.Value - DateTime.Today).Days : (int?)null;
            var priority = days switch
            {
                null => 3,
                < 0 => 0,
                <= 7 => 1,
                _ => 2
            };

            var (accent, background, border, chip) = priority switch
            {
                0 => (CriticalAccentBrush, CriticalBackgroundBrush, CriticalBorderBrush, CriticalChipBrush),
                1 => (UrgentAccentBrush, UrgentBackgroundBrush, UrgentBorderBrush, UrgentChipBrush),
                _ => (SoonAccentBrush, SoonBackgroundBrush, SoonBorderBrush, SoonChipBrush)
            };

            return new DashboardAlertItem
            {
                Title = alert.Title,
                Message = alert.Message,
                CategoryText = GetCategoryText(alert.RelatedEntityType),
                CategoryInitial = GetCategoryInitial(alert.RelatedEntityType),
                StatusText = GetStatusText(days),
                TimingText = GetTimingText(days),
                DueDateText = dueDate.HasValue ? $"تاريخ الانتهاء {dueDate:yyyy-MM-dd}" : "لا يوجد تاريخ انتهاء",
                DueDate = dueDate,
                SortPriority = priority,
                SortDistance = days.HasValue ? Math.Abs(days.Value) : int.MaxValue,
                AccentBrush = accent,
                BackgroundBrush = background,
                BorderBrush = border,
                ChipBrush = chip,
                TextBrush = TextBrushValue
            };
        }

        private static string GetCategoryText(string relatedEntityType) =>
            relatedEntityType switch
            {
                "Vehicle" => "ترخيص",
                "Insurance" => "تأمين",
                _ => "متابعة"
            };

        private static string GetCategoryInitial(string relatedEntityType) =>
            relatedEntityType switch
            {
                "Vehicle" => "ر",
                "Insurance" => "ت",
                _ => "!"
            };

        private static string GetStatusText(int? days) =>
            days switch
            {
                null => "متابعة",
                < 0 => "منتهي",
                0 => "اليوم",
                <= 7 => "عاجل",
                _ => "قريب"
            };

        private static string GetTimingText(int? days) =>
            days switch
            {
                null => "بدون تاريخ محدد",
                < 0 => $"متأخر {FormatDayCount(Math.Abs(days.Value))}",
                0 => "ينتهي اليوم",
                _ => $"متبقي {FormatDayCount(days.Value)}"
            };

        private static string FormatDayCount(int days) =>
            days switch
            {
                1 => "يوم واحد",
                2 => "يومان",
                >= 3 and <= 10 => $"{days} أيام",
                _ => $"{days} يوم"
            };

        private static Brush CreateBrush(string color)
        {
            var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(color)!;
            brush.Freeze();
            return brush;
        }
    }

    public MainWindow(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        StateChanged += MainWindow_StateChanged;

        _serviceProvider = serviceProvider;
        _vehicleService = serviceProvider.GetRequiredService<IVehicleService>();
        _contractService = serviceProvider.GetRequiredService<IContractService>();
        _maintenanceService = serviceProvider.GetRequiredService<IMaintenanceService>();
        _driverService = serviceProvider.GetRequiredService<IDriverService>();
        _employeeService = serviceProvider.GetRequiredService<IEmployeeService>();
        _tripService = serviceProvider.GetRequiredService<ITripService>();
        _fuelService = serviceProvider.GetRequiredService<IFuelService>();
        _expenseService = serviceProvider.GetRequiredService<IExpenseService>();
        _oilChangeService = serviceProvider.GetRequiredService<IOilChangeService>();
        _treasuryService = serviceProvider.GetRequiredService<ITreasuryService>();
        _insuranceService = serviceProvider.GetRequiredService<IInsuranceService>();
        _custodyService = serviceProvider.GetRequiredService<ICustodyService>();
        _masterDataService = serviceProvider.GetRequiredService<IMasterDataService>();
        _reportingService = serviceProvider.GetRequiredService<IReportingService>();
        _settingsService = serviceProvider.GetRequiredService<ISettingsService>();
        _notificationService = serviceProvider.GetRequiredService<INotificationService>();
        _authenticationService = serviceProvider.GetRequiredService<IAuthenticationService>();
        _connectionSettingsStore = serviceProvider.GetRequiredService<ConnectionSettingsStore>();
        _connectionProfile = serviceProvider.GetRequiredService<ClientConnectionProfile>();

        VehiclesGrid.ItemsSource = _vehicles;
        ContractsGrid.ItemsSource = _contracts;
        MaintenanceGrid.ItemsSource = _maintenance;
        DriversGrid.ItemsSource = _drivers;
        EmployeesGrid.ItemsSource = _employees;
        TripsGrid.ItemsSource = _trips;
        FuelGrid.ItemsSource = _fuel;
        ExpensesGrid.ItemsSource = _expenses;
        OilChangesGrid.ItemsSource = _oilChanges;
        TreasuryGrid.ItemsSource = _treasuryTransactions;
        LicensesGrid.ItemsSource = _vehicleLicenses;
        InsuranceGrid.ItemsSource = _insurance;
        CustodyGrid.ItemsSource = _custodies;
        VehicleTypesGrid.ItemsSource = _vehicleTypes;
        ContractStatusesGrid.ItemsSource = _contractStatuses;
        MaintenanceTypesGrid.ItemsSource = _maintenanceTypes;
        ServiceProvidersGrid.ItemsSource = _serviceProviders;
        NotificationsListBox.ItemsSource = _notifications;
        UsersGrid.ItemsSource = _users;
        ReportVehicleComboBox.ItemsSource = _vehicles;
        UpdateReportVehicleFilterVisibility();
        ApplyReadableGridColumnWidths();

        _clockTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _clockTimer.Tick += (_, _) => CurrentDateTimeTextBlock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        _clockTimer.Start();
        CurrentDateTimeTextBlock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        UpdateTripsSummary();

        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Normal)
        {
            WindowState = WindowState.Maximized;
        }
    }

    public void SetCurrentUser(UserDto user)
    {
        _currentUser = user;
        CurrentUserTextBlock.Text = string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName;
        UserInitialsTextBlock.Text = BuildInitials(CurrentUserTextBlock.Text);
        AdminAddUserButton.Visibility = string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible
            : Visibility.Collapsed;
        ConnectionInfoTextBlock.Text = GetOperationalStatusText();
        DatabaseSettingsSummaryTextBlock.Text = GetConnectionSummaryText();
        ApplyRoleAccess(user);
        SelectTabByTag("Dashboard");
        UpdateShellForSelectedTab();
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await RunSafeAsync(RefreshAllAsync);
        await Dispatcher.InvokeAsync(ApplyReadableGridColumnWidths, DispatcherPriority.Loaded);
    }

    private async Task RefreshAllAsync()
    {
        await LoadDashboardAsync();
        await LoadVehiclesAsync();
        await LoadContractsAsync();
        await LoadMaintenanceAsync();
        await LoadDriversAsync();
        await LoadEmployeesAsync();
        await LoadTripsAsync();
        await LoadFuelAsync();
        await LoadExpensesAsync();
        await LoadOilChangesAsync();
        await LoadTreasuryAsync();
        await LoadLicensesAsync();
        await LoadInsuranceAsync();
        await LoadCustodyAsync();
        await LoadMasterDataAsync();
        await LoadSettingsAsync();
        await LoadNotificationsAsync();
        await LoadUsersAsync();
    }

    private async Task LoadDashboardAsync()
    {
        var metrics = await _reportingService.GetDashboardMetricsAsync();
        TotalVehiclesTextBlock.Text = metrics.TotalVehicles.ToString();
        AvailableVehiclesTextBlock.Text = metrics.AvailableVehicles.ToString();
        VehiclesInTripTextBlock.Text = metrics.VehiclesInTrip.ToString();
        ActiveContractsTextBlock.Text = metrics.ActiveContracts.ToString();
        OpenMaintenanceTextBlock.Text = metrics.OpenMaintenanceRequests.ToString();
        ExpiringLicensesTextBlock.Text = metrics.VehicleLicensesExpiring.ToString();
        OpenTripsTextBlock.Text = metrics.OpenTrips.ToString();
        TripsTodayTextBlock.Text = metrics.TripsToday.ToString();
        OilDueTextBlock.Text = metrics.OilChangesDue.ToString();
        TotalExpensesTextBlock.Text = metrics.TotalExpenses.ToString("0.##");
        TotalFuelCostTextBlock.Text = metrics.TotalFuelCost.ToString("0.##");
        TreasuryBalanceTextBlock.Text = metrics.TreasuryBalance.ToString("0.##");
        _allDashboardAlerts = (metrics.Alerts ?? new List<AlertDto>())
            .Where(IsDashboardRenewalAlert)
            .ToList();

        AlertsListBox.ItemsSource = metrics.Alerts ?? new List<AlertDto>();
        ActivityListBox.ItemsSource = metrics.RecentActivities;
    }

    private static bool IsDashboardRenewalAlert(AlertDto alert) =>
        string.Equals(alert.RelatedEntityType, "Vehicle", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(alert.RelatedEntityType, "Insurance", StringComparison.OrdinalIgnoreCase);

    private static string BuildDashboardAlertCountText(int count) =>
        count switch
        {
            0 => "لا توجد إنذارات",
            1 => "إنذار واحد",
            2 => "إنذاران",
            _ => $"{count} إنذارات"
        };

    private static string BuildDashboardAlertSummaryText(IReadOnlyCollection<DashboardAlertItem> alerts)
    {
        if (alerts.Count == 0)
        {
            return "كل التراخيص والتأمينات المسجلة خارج نطاق الخطر الحالي.";
        }

        var expiredCount = alerts.Count(alert => alert.SortPriority == 0);
        var urgentCount = alerts.Count(alert => alert.SortPriority == 1);

        if (expiredCount > 0)
        {
            return urgentCount > 0
                ? $"{expiredCount} منتهي و{urgentCount} يحتاج متابعة عاجلة خلال أسبوع."
                : $"{expiredCount} منتهي ويحتاج إجراء تجديد مباشر.";
        }

        return urgentCount > 0
            ? $"{urgentCount} يحتاج متابعة عاجلة خلال أسبوع."
            : "إنذارات قريبة من موعد التجديد خلال 30 يوم.";
    }

    private async Task LoadVehiclesAsync()
    {
        ReplaceCollection(_vehicles, await _vehicleService.GetAllAsync());

        if (ReportVehicleComboBox is not null && ReportVehicleComboBox.SelectedItem is null && _vehicles.Count > 0)
        {
            ReportVehicleComboBox.SelectedItem = _vehicles[0];
        }
    }
    private async Task LoadContractsAsync() => ReplaceCollection(_contracts, await _contractService.GetAllAsync());
    private async Task LoadMaintenanceAsync() => ReplaceCollection(_maintenance, await _maintenanceService.GetAllAsync());
    private async Task LoadDriversAsync() => ReplaceCollection(_drivers, await _driverService.GetAllAsync());

    private async Task LoadEmployeesAsync() => ReplaceCollection(_employees, await _employeeService.GetAllAsync());

    private async Task LoadTripsAsync()
    {
        var selectedTripId = Selected<TripDto>(TripsGrid)?.Id ?? 0;
        ReplaceCollection(_trips, await _tripService.GetAllAsync());

        if (selectedTripId > 0)
        {
            var selectedTrip = _trips.FirstOrDefault(t => t.Id == selectedTripId);
            if (selectedTrip is not null)
            {
                TripsGrid.SelectedItem = selectedTrip;
                TripsGrid.ScrollIntoView(selectedTrip);
            }
        }

        UpdateTripsSummary();
    }
    private async Task LoadFuelAsync() => ReplaceCollection(_fuel, await _fuelService.GetAllAsync());
    private async Task LoadExpensesAsync() => ReplaceCollection(_expenses, await _expenseService.GetAllAsync());
    private async Task LoadOilChangesAsync() => ReplaceCollection(_oilChanges, await _oilChangeService.GetAllAsync());
    private async Task LoadTreasuryAsync() => ReplaceCollection(_treasuryTransactions, await _treasuryService.GetAllAsync());
    private Task LoadLicensesAsync()
    {
        ReplaceCollection(_vehicleLicenses, _vehicles.Select(VehicleLicenseRow.FromVehicle));
        return Task.CompletedTask;
    }
    private async Task LoadInsuranceAsync() => ReplaceCollection(_insurance, await _insuranceService.GetAllAsync());
    private async Task LoadCustodyAsync() => ReplaceCollection(_custodies, await _custodyService.GetAllAsync());
    private async Task LoadUsersAsync()
    {
        var users = await _authenticationService.GetUsersAsync();
        ReplaceCollection(_users, users.Select(user => new UserFormDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive
        }));
    }

    private async Task LoadMasterDataAsync()
    {
        ReplaceCollection(_vehicleTypes, await _masterDataService.GetVehicleTypesAsync());
        ReplaceCollection(_contractStatuses, await _masterDataService.GetContractStatusesAsync());
        ReplaceCollection(_maintenanceTypes, await _masterDataService.GetMaintenanceTypesAsync());
        ReplaceCollection(_serviceProviders, await _masterDataService.GetServiceProvidersAsync());
    }

    private async Task LoadSettingsAsync()
    {
        _settings = await _settingsService.GetSettingsAsync();
        CompanyNameTextBox.Text = _settings.CompanyName;
        CompanyNameEnTextBox.Text = _settings.CompanyNameEn;
        CompanyAddressTextBox.Text = _settings.Address;
        CompanyPhoneTextBox.Text = _settings.PhoneNumber;
        CompanyEmailTextBox.Text = _settings.Email;
        CompanyWebsiteTextBox.Text = _settings.Website;
        DatabaseSettingsSummaryTextBlock.Text = GetConnectionSummaryText();
        CompanyHeaderTextBlock.Text = string.IsNullOrWhiteSpace(_settings.CompanyName) ? "نظام إدارة الأسطول" : _settings.CompanyName;
        UpdateCompanyLogo();
    }

    private async Task LoadNotificationsAsync() => ReplaceCollection(_notifications, await _notificationService.GetActiveNotificationsAsync());

    private async Task RunSafeAsync(Func<Task> action)
    {
        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            await action();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"لم يتم تنفيذ العملية:\n{ex.Message}", "تنبيه واضح", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    private void RestartApplication()
    {
        var executablePath = Environment.ProcessPath;
        if (!string.IsNullOrWhiteSpace(executablePath))
        {
            Process.Start(new ProcessStartInfo(executablePath) { UseShellExecute = true });
        }

        Application.Current.Shutdown();
    }

    private void OpenConnectionSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentUser is not null && !string.Equals(_currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("إعدادات قاعدة البيانات متاحة لمدير النظام فقط.", "صلاحيات", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var window = new ConnectionSettingsWindow(_connectionSettingsStore, _connectionProfile)
        {
            Owner = this
        };

        if (window.ShowDialog() != true)
        {
            return;
        }

        MessageBox.Show(
            "تم حفظ إعدادات الاتصال. سيُعاد تشغيل التطبيق لتطبيق الاتصال الجديد.",
            "تم الحفظ",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        RestartApplication();
    }

    private string GetOperationalStatusText() =>
        _connectionProfile.IsSqlite
            ? "وضع المعاينة المحلي مفعل حاليًا."
            : "النظام جاهز للعمل على بيئة التشغيل الداخلية.";

    private string GetConnectionSummaryText() =>
        _connectionProfile.IsSqlite
            ? "يعمل التطبيق الآن في وضع معاينة محلي. يمكن لمدير النظام التحويل لاحقًا إلى قاعدة الشركة من إعدادات النظام."
            : "إعدادات الاتصال محفوظة على هذا الجهاز بشكل آمن، ويمكن تعديلها فقط من خلال مدير النظام عند الحاجة.";

    private static string GetArabicRoleName(string? role) => role?.Trim() switch
    {
        "Admin" => "مدير النظام",
        "OperationsManager" => "مدير التشغيل",
        "OperationsDataEntry" => "إدخال بيانات التشغيل",
        "MaintenanceOfficer" => "مسؤول الصيانة",
        "TreasuryOfficer" => "مسؤول الخزينة",
        "Viewer" => "عرض فقط",
        "Staff" => "موظف",
        _ => "مستخدم النظام"
    };

    private void ApplyRoleAccess(UserDto user)
    {
        var allowed = new HashSet<string>(user.AllowedModules, StringComparer.OrdinalIgnoreCase);

        foreach (var tab in MainTabs.Items.OfType<TabItem>())
        {
            var module = tab.Tag?.ToString();
            if (string.IsNullOrWhiteSpace(module))
            {
                continue;
            }

            tab.Visibility = allowed.Contains(module) ? Visibility.Visible : Visibility.Collapsed;
        }

        if (string.Equals(user.Role, "Viewer", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var button in FindVisualChildren<Button>(MainTabs))
            {
                var content = button.Content?.ToString() ?? string.Empty;
                if (content.Contains("إضافة", StringComparison.OrdinalIgnoreCase) ||
                    content.Contains("حفظ", StringComparison.OrdinalIgnoreCase) ||
                    content.Contains("حذف", StringComparison.OrdinalIgnoreCase) ||
                    content.Contains("تفعيل", StringComparison.OrdinalIgnoreCase))
                {
                    button.IsEnabled = false;
                }
            }

            foreach (var grid in FindVisualChildren<DataGrid>(MainTabs))
            {
                grid.IsReadOnly = true;
            }
        }
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject dependencyObject) where T : DependencyObject
    {
        for (var index = 0; index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(dependencyObject); index++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(dependencyObject, index);
            if (child is T typedChild)
            {
                yield return typedChild;
            }

            foreach (var nestedChild in FindVisualChildren<T>(child))
            {
                yield return nestedChild;
            }
        }
    }

    private static void ReplaceCollection<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    private void ApplyReadableGridColumnWidths()
    {
        foreach (var grid in GetMainDataGrids())
        {
            grid.ColumnWidth = new DataGridLength(DefaultGridColumnWidth);
            grid.MinColumnWidth = 130;
            grid.CanUserResizeColumns = true;
            ScrollViewer.SetHorizontalScrollBarVisibility(grid, ScrollBarVisibility.Auto);
            ScrollViewer.SetVerticalScrollBarVisibility(grid, ScrollBarVisibility.Auto);

            foreach (var column in grid.Columns)
            {
                ApplyReadableColumnWidth(column, GetColumnWidthKey(column));
            }
        }
    }

    private IEnumerable<DataGrid> GetMainDataGrids()
    {
        yield return VehiclesGrid;
        yield return ContractsGrid;
        yield return MaintenanceGrid;
        yield return DriversGrid;
        yield return EmployeesGrid;
        yield return TripsGrid;
        yield return FuelGrid;
        yield return ExpensesGrid;
        yield return OilChangesGrid;
        yield return TreasuryGrid;
        yield return LicensesGrid;
        yield return InsuranceGrid;
        yield return CustodyGrid;
        yield return VehicleTypesGrid;
        yield return ContractStatusesGrid;
        yield return MaintenanceTypesGrid;
        yield return ServiceProvidersGrid;
        yield return ReportsGrid;
        yield return UsersGrid;
    }

    private static string GetColumnWidthKey(DataGridColumn column)
    {
        if (column.Header is TextBlock headerTextBlock)
        {
            return headerTextBlock.ToolTip?.ToString() ?? headerTextBlock.Text;
        }

        if (column is DataGridBoundColumn boundColumn &&
            boundColumn.Binding is Binding binding &&
            !string.IsNullOrWhiteSpace(binding.Path?.Path))
        {
            return binding.Path.Path;
        }

        return column.Header?.ToString() ?? string.Empty;
    }

    private static void ApplyReadableColumnWidth(DataGridColumn column, string key)
    {
        var width = GetReadableColumnWidth(key);
        column.Width = new DataGridLength(width);
        column.MinWidth = Math.Min(width, 130);
    }

    private static double GetReadableColumnWidth(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return DefaultGridColumnWidth;
        }

        var normalizedKey = key.Trim();

        if (normalizedKey.Equals("Id", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Equals("رقم", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Equals("A5", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Equals("نموذج", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Equals("مستند", StringComparison.OrdinalIgnoreCase))
        {
            return NarrowGridColumnWidth;
        }

        if (normalizedKey.Contains("Date", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("تاريخ", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("بداية", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("نهاية", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("Expiry", StringComparison.OrdinalIgnoreCase))
        {
            return DateGridColumnWidth;
        }

        if (normalizedKey.Contains("Notes", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("Description", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("Details", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("ملاحظات", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("الوصف", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("تفاصيل", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("العنوان", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("الشروط", StringComparison.OrdinalIgnoreCase))
        {
            return ExtraWideGridColumnWidth;
        }

        if (normalizedKey.Contains("Name", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("Email", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("Phone", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("اسم", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("المستلم", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("البريد", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("هاتف", StringComparison.OrdinalIgnoreCase))
        {
            return WideGridColumnWidth;
        }

        if (normalizedKey.Contains("Status", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("Type", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("Amount", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("Mileage", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("الحالة", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("نوع", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("المبلغ", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("التكلفة", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("القسط", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("العداد", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("المسافة", StringComparison.OrdinalIgnoreCase))
        {
            return CompactGridColumnWidth;
        }

        return DefaultGridColumnWidth;
    }

    private void UpdateTripsSummary()
    {
        if (TripsSummaryTextBlock is null)
        {
            return;
        }

        TripsSummaryTextBlock.Text = $"إجمالي التشغيلات المسجلة: {_trips.Count}";
    }

    private static T? Selected<T>(DataGrid grid) where T : class => grid.SelectedItem as T;

    private static void CommitGridEdit(DataGrid grid)
    {
        grid.CommitEdit(DataGridEditingUnit.Cell, true);
        grid.CommitEdit(DataGridEditingUnit.Row, true);
    }

    private static void AddNewItem<T>(ObservableCollection<T> collection, DataGrid grid, T item)
    {
        collection.Insert(0, item);
        grid.SelectedItem = item;
        grid.ScrollIntoView(item);
    }

    private static bool ConfirmDelete() =>
        MessageBox.Show("هل تريد حذف السجل المحدد؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    private VehicleDto EnsureVehicleExists(int vehicleId, string recordName)
    {
        if (vehicleId <= 0)
        {
            throw new InvalidOperationException($"اختر العربية الصحيحة قبل حفظ {recordName}.");
        }

        return _vehicles.FirstOrDefault(v => v.Id == vehicleId)
            ?? throw new InvalidOperationException($"كود العربية {vehicleId} غير موجود. اختر عربية موجودة من شاشة المركبات أو أضفها من صفحة التراخيص أولًا.");
    }

    private async Task DeleteSelectedAsync<T>(DataGrid grid, ObservableCollection<T> collection, Func<T, int> idSelector, Func<int, Task> deleteAction, Func<Task> reload)
        where T : class
    {
        var selected = Selected<T>(grid) ?? throw new InvalidOperationException("اختر سجلًا أولًا.");
        if (idSelector(selected) == 0)
        {
            collection.Remove(selected);
            return;
        }

        if (!ConfirmDelete())
        {
            return;
        }

        await deleteAction(idSelector(selected));
        await reload();
        await LoadDashboardAsync();
        await LoadNotificationsAsync();
    }

    private static DataTable ToDataTable(ReportDataDto report)
    {
        var table = new DataTable(report.ReportTitle);
        foreach (var column in report.Columns)
        {
            table.Columns.Add(column);
        }

        foreach (var row in report.Data)
        {
            var newRow = table.NewRow();
            foreach (var column in report.Columns)
            {
                newRow[column] = row.TryGetValue(column, out var value) ? value ?? DBNull.Value : DBNull.Value;
            }

            table.Rows.Add(newRow);
        }

        return table;
    }

    private static string SafeFileName(string value)
    {
        var cleaned = string.IsNullOrWhiteSpace(value) ? "تقرير" : value.Trim();
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
        {
            cleaned = cleaned.Replace(invalidChar, '-');
        }

        return cleaned.Length > 80 ? cleaned[..80] : cleaned;
    }

    private static string BuildCsv(ReportDataDto report)
    {
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(",", report.Columns.Select(EscapeCsv)));

        foreach (var row in report.Data)
        {
            builder.AppendLine(string.Join(",", report.Columns.Select(column => EscapeCsv(GetReportValue(row, column)))));
        }

        return builder.ToString();
    }

    private static string BuildExcelHtml(ReportDataDto report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<!doctype html>");
        builder.AppendLine("<html><head><meta charset=\"utf-8\"><style>");
        builder.AppendLine("body{font-family:'Segoe UI',Tahoma,sans-serif;direction:rtl}table{border-collapse:collapse;width:100%}th,td{border:1px solid #b8c2cc;padding:6px 8px;text-align:right}th{background:#e9f0f4}");
        builder.AppendLine("</style></head><body>");
        builder.AppendLine($"<h2>{EscapeHtml(report.ReportTitle)}</h2>");
        builder.AppendLine($"<p>تاريخ التوليد: {EscapeHtml(report.GeneratedDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture))}</p>");
        builder.AppendLine("<table><thead><tr>");
        foreach (var column in report.Columns)
        {
            builder.Append("<th>").Append(EscapeHtml(column)).AppendLine("</th>");
        }

        builder.AppendLine("</tr></thead><tbody>");
        foreach (var row in report.Data)
        {
            builder.AppendLine("<tr>");
            foreach (var column in report.Columns)
            {
                builder.Append("<td>").Append(EscapeHtml(GetReportValue(row, column))).AppendLine("</td>");
            }

            builder.AppendLine("</tr>");
        }

        builder.AppendLine("</tbody></table></body></html>");
        return builder.ToString();
    }

    private static FlowDocument BuildReportDocument(ReportDataDto report)
    {
        var document = new FlowDocument
        {
            FlowDirection = FlowDirection.RightToLeft,
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
            FontSize = report.Columns.Count > 8 ? 9 : 11,
            PagePadding = new Thickness(36)
        };

        document.Blocks.Add(new Paragraph(new Run(report.ReportTitle))
        {
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(0, 0, 0, 8)
        });

        document.Blocks.Add(new Paragraph(new Run($"تاريخ التوليد: {report.GeneratedDate.ToLocalTime():yyyy-MM-dd HH:mm} - عدد السجلات: {report.Data.Count}"))
        {
            FontSize = 11,
            Foreground = System.Windows.Media.Brushes.DimGray,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(0, 0, 0, 14)
        });

        var table = new Table { CellSpacing = 0 };
        foreach (var _ in report.Columns)
        {
            table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        }

        var rowGroup = new TableRowGroup();
        var headerRow = new TableRow();
        foreach (var column in report.Columns)
        {
            headerRow.Cells.Add(CreateReportCell(column, isHeader: true));
        }

        rowGroup.Rows.Add(headerRow);

        foreach (var row in report.Data)
        {
            var tableRow = new TableRow();
            foreach (var column in report.Columns)
            {
                tableRow.Cells.Add(CreateReportCell(GetReportValue(row, column), isHeader: false));
            }

            rowGroup.Rows.Add(tableRow);
        }

        if (report.Data.Count == 0)
        {
            rowGroup.Rows.Add(new TableRow
            {
                Cells =
                {
                    new TableCell(new Paragraph(new Run("لا توجد بيانات.")))
                    {
                        ColumnSpan = Math.Max(report.Columns.Count, 1),
                        Padding = new Thickness(8),
                        BorderBrush = System.Windows.Media.Brushes.LightGray,
                        BorderThickness = new Thickness(0.5)
                    }
                }
            });
        }

        table.RowGroups.Add(rowGroup);
        document.Blocks.Add(table);
        return document;
    }

    private static TableCell CreateReportCell(string value, bool isHeader)
    {
        return new TableCell(new Paragraph(new Run(value))
        {
            Margin = new Thickness(0),
            TextAlignment = TextAlignment.Right
        })
        {
            Padding = new Thickness(6),
            BorderBrush = System.Windows.Media.Brushes.LightGray,
            BorderThickness = new Thickness(0.5),
            Background = isHeader ? System.Windows.Media.Brushes.Gainsboro : System.Windows.Media.Brushes.Transparent,
            FontWeight = isHeader ? FontWeights.SemiBold : FontWeights.Normal
        };
    }

    private static string GetReportValue(Dictionary<string, object> row, string column)
    {
        row.TryGetValue(column, out var value);
        return FormatReportValue(value);
    }

    private static string EscapeCsv(string value)
    {
        var escaped = value.Replace("\"", "\"\"");
        return escaped.Contains(',') || escaped.Contains('"') || escaped.Contains('\n') || escaped.Contains('\r')
            ? $"\"{escaped}\""
            : escaped;
    }

    private static string EscapeHtml(string value) =>
        value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");

    private static A5DocumentWindow.A5DocumentSection Section(string title, params A5DocumentWindow.A5DocumentRow[] rows) =>
        new(title, rows);

    private static A5DocumentWindow.A5DocumentRow Row(string label, object? value) =>
        new(label, value is DBNull ? null : value);

    private static string ValueOrDash(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return "-";
    }

    private static string FormatReportValue(object? value) =>
        value switch
        {
            null => "-",
            DBNull => "-",
            DateTime date => date.ToString("yyyy-MM-dd"),
            DateTimeOffset date => date.ToString("yyyy-MM-dd"),
            decimal amount => amount.ToString("0.##"),
            double number => number.ToString("0.##"),
            float number => number.ToString("0.##"),
            _ when string.IsNullOrWhiteSpace(value.ToString()) => "-",
            _ => value.ToString()!.Trim()
        };

    private static string BuildReportLine(IReadOnlyList<string> columns, Dictionary<string, object> row)
    {
        var selectedColumns = columns.Count > 0 ? columns : row.Keys.ToList();
        return string.Join(" | ", selectedColumns.Take(5).Select(column =>
        {
            row.TryGetValue(column, out var value);
            return $"{column}: {FormatReportValue(value)}";
        }));
    }

    private static IEnumerable<A5DocumentWindow.A5DocumentSection> BuildReportA5Sections(ReportDataDto report, DataRowView? selectedRow)
    {
        yield return Section("ملخص التقرير",
            Row("اسم التقرير", report.ReportTitle),
            Row("تاريخ التوليد", report.GeneratedDate.ToLocalTime()),
            Row("عدد السجلات", report.Data.Count),
            Row("الأعمدة", string.Join("، ", report.Columns)));

        if (selectedRow is not null)
        {
            var detailRows = report.Columns
                .Select(column => Row(column, selectedRow.Row.Table.Columns.Contains(column) ? selectedRow[column] : null))
                .ToArray();

            yield return Section("بيانات السجل المحدد", detailRows.Length > 0 ? detailRows : new[] { Row("السجل", "لا توجد بيانات") });
            yield break;
        }

        var displayedRows = report.Data.Take(8)
            .Select((row, index) => Row($"سجل {index + 1}", BuildReportLine(report.Columns, row)))
            .ToList();

        if (displayedRows.Count == 0)
        {
            displayedRows.Add(Row("السجلات", "لا توجد بيانات"));
        }
        else if (report.Data.Count > displayedRows.Count)
        {
            displayedRows.Add(Row("باقي السجلات", $"يوجد {report.Data.Count - displayedRows.Count} سجل إضافي في جدول التقرير."));
        }

        yield return Section("أول سجلات التقرير", displayedRows.ToArray());
    }

    private VehicleFormDto ToVehicleFormFromLicense(VehicleLicenseRow licenseRow, VehicleDto? existingVehicle)
    {
        var vehicleTypeId = existingVehicle?.VehicleTypeId > 0
            ? existingVehicle.VehicleTypeId
            : _vehicleTypes.FirstOrDefault()?.Id
                ?? throw new InvalidOperationException("أضف نوع مركبة أولًا من البيانات الأساسية قبل إضافة عربية من صفحة التراخيص.");

        return new VehicleFormDto
        {
            Id = existingVehicle?.Id ?? 0,
            PlateNumber = licenseRow.PlateNumber.Trim(),
            VehicleTypeId = vehicleTypeId,
            Model = licenseRow.Model?.Trim() ?? existingVehicle?.Model ?? string.Empty,
            Year = existingVehicle?.Year > 0 ? existingVehicle.Year : DateTime.Today.Year,
            Manufacturer = existingVehicle?.Manufacturer ?? string.Empty,
            Color = existingVehicle?.Color ?? string.Empty,
            ChassisNumber = existingVehicle?.ChassisNumber ?? string.Empty,
            EngineNumber = existingVehicle?.EngineNumber ?? string.Empty,
            CurrentMileage = existingVehicle?.CurrentMileage ?? 0,
            Status = string.IsNullOrWhiteSpace(existingVehicle?.Status) ? "Available" : existingVehicle.Status,
            AssignedTo = existingVehicle?.AssignedTo ?? string.Empty,
            PurchaseDate = existingVehicle is null || existingVehicle.PurchaseDate == default ? DateTime.Today : existingVehicle.PurchaseDate,
            PurchasePrice = existingVehicle?.PurchasePrice ?? 0,
            RegistrationStartDate = licenseRow.RegistrationStartDate,
            RegistrationExpiryDate = licenseRow.RegistrationExpiryDate,
            AccidentInsuranceDetails = existingVehicle?.AccidentInsuranceDetails ?? string.Empty,
            SocialInsuranceDetails = existingVehicle?.SocialInsuranceDetails ?? string.Empty,
            OilChangeIntervalKm = existingVehicle?.OilChangeIntervalKm > 0 ? existingVehicle.OilChangeIntervalKm : 10000,
            MaintenanceIntervalKm = existingVehicle?.MaintenanceIntervalKm > 0 ? existingVehicle.MaintenanceIntervalKm : 15000,
            Notes = existingVehicle?.Notes ?? string.Empty
        };
    }

    private VehicleFormDto ToForm(VehicleDto dto) => new()
    {
        Id = dto.Id,
        PlateNumber = dto.PlateNumber,
        VehicleTypeId = dto.VehicleTypeId,
        Model = dto.Model,
        Year = dto.Year,
        Manufacturer = dto.Manufacturer,
        Color = dto.Color,
        ChassisNumber = dto.ChassisNumber,
        EngineNumber = dto.EngineNumber,
        CurrentMileage = dto.CurrentMileage,
        Status = dto.Status,
        AssignedTo = dto.AssignedTo,
        PurchaseDate = dto.PurchaseDate,
        PurchasePrice = dto.PurchasePrice,
        RegistrationStartDate = dto.RegistrationStartDate,
        RegistrationExpiryDate = dto.RegistrationExpiryDate,
        AccidentInsuranceDetails = dto.AccidentInsuranceDetails,
        SocialInsuranceDetails = dto.SocialInsuranceDetails,
        OilChangeIntervalKm = dto.OilChangeIntervalKm,
        MaintenanceIntervalKm = dto.MaintenanceIntervalKm,
        Notes = dto.Notes
    };

    private ContractFormDto ToForm(ContractDto dto) => new()
    {
        Id = dto.Id,
        ContractNumber = dto.ContractNumber,
        VehicleId = dto.VehicleId,
        ContractStatusId = dto.ContractStatusId,
        ClientName = dto.ClientName,
        ClientEmail = dto.ClientEmail,
        ClientPhoneNumber = dto.ClientPhoneNumber,
        ClientAddress = dto.ClientAddress,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        ContractValue = dto.ContractValue,
        PaidAmount = dto.PaidAmount,
        PaymentTerms = dto.PaymentTerms,
        ContractTerms = dto.ContractTerms,
        DocumentUrl = dto.DocumentUrl,
        Notes = dto.Notes
    };

    private MaintenanceRequestFormDto ToForm(MaintenanceRequestDto dto) => new()
    {
        Id = dto.Id,
        VehicleId = dto.VehicleId,
        MaintenanceTypeId = dto.MaintenanceTypeId,
        ServiceProviderId = dto.ServiceProviderId,
        RequestDate = dto.RequestDate,
        CompletionDate = dto.CompletionDate,
        Status = dto.Status,
        Description = dto.Description,
        EstimatedCost = dto.EstimatedCost,
        ActualCost = dto.ActualCost,
        WorkPerformed = dto.WorkPerformed,
        Notes = dto.Notes,
        DocumentUrl = dto.DocumentUrl
    };

    private DriverFormDto ToForm(DriverDto dto) => new()
    {
        Id = dto.Id,
        FullName = dto.FullName,
        NationalId = dto.NationalId,
        PhoneNumber = dto.PhoneNumber,
        Email = dto.Email,
        Address = dto.Address,
        DateOfBirth = dto.DateOfBirth,
        LicenseNumber = dto.LicenseNumber,
        LicenseExpiryDate = dto.LicenseExpiryDate,
        LicenseType = dto.LicenseType,
        IsActive = dto.IsActive,
        Notes = dto.Notes
    };

    private EmployeeFormDto ToForm(EmployeeDto dto) => new()
    {
        Id = dto.Id,
        FullName = dto.FullName,
        EmployeeId = dto.EmployeeId,
        PhoneNumber = dto.PhoneNumber,
        Email = dto.Email,
        Department = dto.Department,
        Position = dto.Position,
        HireDate = dto.HireDate,
        TerminationDate = dto.TerminationDate,
        Status = dto.Status,
        Notes = dto.Notes
    };

    private TripFormDto ToForm(TripDto dto) => new()
    {
        Id = dto.Id,
        VehicleId = dto.VehicleId,
        DriverId = dto.DriverId,
        RequesterEmployeeId = dto.RequesterEmployeeId,
        RequesterNameText = dto.RequesterNameText,
        SupervisorEmployeeId = dto.SupervisorEmployeeId,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        StartLocation = dto.StartLocation,
        EndLocation = dto.EndLocation,
        StartMileage = dto.StartMileage,
        EndMileage = dto.EndMileage,
        Distance = dto.Distance,
        Purpose = dto.Purpose,
        Status = dto.Status,
        FuelConsumed = dto.FuelConsumed,
        TripCost = dto.TripCost,
        Notes = dto.Notes
    };

    private FuelTransactionFormDto ToForm(FuelTransactionDto dto) => new()
    {
        Id = dto.Id,
        VehicleId = dto.VehicleId,
        TransactionDate = dto.TransactionDate,
        FuelType = dto.FuelType,
        Quantity = dto.Quantity,
        UnitPrice = dto.UnitPrice,
        TotalCost = dto.TotalCost,
        FuelStation = dto.FuelStation,
        Odometer = dto.Odometer,
        PaidFromTreasury = dto.PaidFromTreasury,
        TreasuryTransactionId = dto.TreasuryTransactionId,
        Notes = dto.Notes
    };

    private ExpenseFormDto ToForm(ExpenseDto dto) => new()
    {
        Id = dto.Id,
        VehicleId = dto.VehicleId,
        ExpenseDate = dto.ExpenseDate,
        Category = dto.Category,
        Amount = dto.Amount,
        Description = dto.Description,
        Vendor = dto.Vendor,
        ReceiptUrl = dto.ReceiptUrl,
        PaidFromTreasury = dto.PaidFromTreasury,
        TreasuryTransactionId = dto.TreasuryTransactionId,
        Status = dto.Status,
        Notes = dto.Notes
    };

    private OilChangeFormDto ToForm(OilChangeDto dto) => new()
    {
        Id = dto.Id,
        VehicleId = dto.VehicleId,
        ChangeDate = dto.ChangeDate,
        OdometerAtChange = dto.OdometerAtChange,
        OilType = dto.OilType,
        Quantity = dto.Quantity,
        Cost = dto.Cost,
        NextOilChangeOdometer = dto.NextOilChangeOdometer,
        Status = dto.Status,
        Notes = dto.Notes
    };

    private TreasuryTransactionFormDto ToForm(TreasuryTransactionDto dto) => new()
    {
        Id = dto.Id,
        TransactionDate = dto.TransactionDate,
        TransactionType = dto.TransactionType,
        Amount = dto.Amount,
        Description = dto.Description,
        RelatedEntityType = dto.RelatedEntityType,
        RelatedEntityId = dto.RelatedEntityId,
        PaymentMethod = dto.PaymentMethod,
        Notes = dto.Notes
    };

    private InsuranceFormDto ToForm(InsuranceDto dto) => new()
    {
        Id = dto.Id,
        VehicleId = dto.VehicleId,
        PolicyNumber = dto.PolicyNumber,
        InsuranceCompany = dto.InsuranceCompany,
        PolicyType = dto.PolicyType,
        StartDate = dto.StartDate,
        ExpiryDate = dto.ExpiryDate,
        PremiumAmount = dto.PremiumAmount,
        CoverageAmount = dto.CoverageAmount,
        CoverageDetails = dto.CoverageDetails,
        AgentName = dto.AgentName,
        AgentPhoneNumber = dto.AgentPhoneNumber,
        Status = dto.Status,
        DocumentUrl = dto.DocumentUrl,
        Notes = dto.Notes
    };

    private CustodyFormDto ToForm(CustodyDto dto) => new()
    {
        Id = dto.Id,
        VehicleId = dto.VehicleId,
        CustodyNumber = dto.CustodyNumber,
        CustodianName = dto.CustodianName,
        CustodianPosition = dto.CustodianPosition,
        HandoverDate = dto.HandoverDate,
        ReturnDate = dto.ReturnDate,
        Status = dto.Status,
        VehicleConditionRating = dto.VehicleConditionRating,
        Notes = dto.Notes,
        DocumentUrl = dto.DocumentUrl
    };

    private bool TryCreateLookupColumn(string propertyName, out DataGridColumn column)
    {
        column = null!;

        if (string.Equals(propertyName, "VehicleId", StringComparison.OrdinalIgnoreCase))
        {
            column = CreateLookupColumn(propertyName, "العربية", _vehicles, "PlateNumber", "Id");
            return true;
        }

        if (string.Equals(propertyName, "DriverId", StringComparison.OrdinalIgnoreCase))
        {
            column = CreateLookupColumn(propertyName, "السائق", _drivers, "FullName", "Id");
            return true;
        }

        if (string.Equals(propertyName, "RequesterEmployeeId", StringComparison.OrdinalIgnoreCase))
        {
            column = CreateLookupColumn(propertyName, "طالب التشغيل", _employees, "FullName", "Id");
            return true;
        }

        if (string.Equals(propertyName, "SupervisorEmployeeId", StringComparison.OrdinalIgnoreCase))
        {
            column = CreateLookupColumn(propertyName, "المشرف", _employees, "FullName", "Id");
            return true;
        }

        if (string.Equals(propertyName, "ContractStatusId", StringComparison.OrdinalIgnoreCase))
        {
            column = CreateLookupColumn(propertyName, "حالة العقد", _contractStatuses, "Name", "Id");
            return true;
        }

        if (string.Equals(propertyName, "MaintenanceTypeId", StringComparison.OrdinalIgnoreCase))
        {
            column = CreateLookupColumn(propertyName, "نوع الصيانة", _maintenanceTypes, "Name", "Id");
            return true;
        }

        if (string.Equals(propertyName, "ServiceProviderId", StringComparison.OrdinalIgnoreCase))
        {
            column = CreateLookupColumn(propertyName, "مزود الخدمة", _serviceProviders, "Name", "Id");
            return true;
        }

        return false;
    }

    private static DataGridComboBoxColumn CreateLookupColumn(
        string propertyName,
        string header,
        System.Collections.IEnumerable itemsSource,
        string displayMemberPath,
        string selectedValuePath) =>
        new()
        {
            Header = header,
            ItemsSource = itemsSource,
            DisplayMemberPath = displayMemberPath,
            SelectedValuePath = selectedValuePath,
            SelectedValueBinding = new Binding(propertyName)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            },
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        };

    private void Grid_AutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        if (HiddenColumns.Contains(e.PropertyName))
        {
            e.Cancel = true;
            return;
        }

        if (TryCreateLookupColumn(e.PropertyName, out var lookupColumn))
        {
            e.Column = lookupColumn;
            ApplyReadableColumnWidth(e.Column, e.PropertyName);
            return;
        }

        if (e.PropertyName.Equals("Id", StringComparison.OrdinalIgnoreCase))
        {
            e.Column.IsReadOnly = true;
        }

        if (ColumnHeaders.TryGetValue(e.PropertyName, out var header))
        {
            e.Column.Header = header;
        }

        if (e.Column is DataGridTextColumn textColumn && textColumn.Binding is Binding binding)
        {
            if (e.PropertyType == typeof(DateTime) || e.PropertyType == typeof(DateTime?))
            {
                binding.StringFormat = "yyyy-MM-dd";
            }
            else if (e.PropertyType == typeof(decimal) || e.PropertyType == typeof(decimal?))
            {
                binding.StringFormat = "0.##";
            }
        }

        ApplyReadableColumnWidth(e.Column, e.PropertyName);
    }

    private async void RefreshAllButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(RefreshAllAsync);

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
        Application.Current.MainWindow = loginWindow;
        loginWindow.Show();
        Close();
    }

    private void GoToDashboardButton_Click(object sender, RoutedEventArgs e) => SelectTabByTag("Dashboard");

    private void SelectTabByTag(string tag)
    {
        var tab = MainTabs.Items
            .OfType<TabItem>()
            .FirstOrDefault(item => string.Equals(item.Tag?.ToString(), tag, StringComparison.OrdinalIgnoreCase) && item.Visibility == Visibility.Visible);

        if (tab is not null)
        {
            MainTabs.SelectedItem = tab;
            UpdateShellForSelectedTab();
        }
    }

    private void QuickOpenTripsButton_Click(object sender, RoutedEventArgs e) => SelectTabByTag("Trips");
    private void QuickOpenMaintenanceButton_Click(object sender, RoutedEventArgs e) => SelectTabByTag("Maintenance");
    private void QuickOpenLicensesAndInsuranceButton_Click(object sender, RoutedEventArgs e) => SelectTabByTag("Licenses");
    private void QuickOpenCustodyButton_Click(object sender, RoutedEventArgs e) => SelectTabByTag("Custody");
    private void QuickOpenTreasuryButton_Click(object sender, RoutedEventArgs e) => SelectTabByTag("Treasury");
    private void QuickOpenReportsButton_Click(object sender, RoutedEventArgs e) => SelectTabByTag("Reports");
    private void DashboardAddUserButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.Equals(_currentUser?.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        SelectTabByTag("Users");
        AddUserButton_Click(sender, e);
    }

    private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        UpdateShellForSelectedTab();
    }

    private void UpdateShellForSelectedTab()
    {
        var selectedTab = MainTabs.SelectedItem as TabItem;
        var isDashboard = string.Equals(selectedTab?.Tag?.ToString(), "Dashboard", StringComparison.OrdinalIgnoreCase);

        NavigationBarBorder.Visibility = isDashboard ? Visibility.Collapsed : Visibility.Visible;
        NavigationBarTitleTextBlock.Text = selectedTab?.Header?.ToString() ?? "النظام";
        NavigationBarSubtitleTextBlock.Text = _settings?.CompanyName switch
        {
            { Length: > 0 } companyName => $"{companyName} - {_currentUser?.FullName ?? _currentUser?.Username ?? "مستخدم النظام"}",
            _ => _currentUser?.FullName ?? _currentUser?.Username ?? "مستخدم النظام"
        };
    }

    private void UpdateCompanyLogo()
    {
        CompanyLogoImage.Source = null;
        CompanyLogoImage.Visibility = Visibility.Collapsed;
        CompanyLogoPlaceholderTextBlock.Visibility = Visibility.Visible;

        var logoPath = _settings?.LogoUrl?.Trim();
        if (string.IsNullOrWhiteSpace(logoPath))
        {
            return;
        }

        try
        {
            Uri logoUri;
            if (Uri.TryCreate(logoPath, UriKind.Absolute, out var absoluteUri))
            {
                logoUri = absoluteUri;
            }
            else
            {
                var fullPath = Path.GetFullPath(logoPath);
                if (!File.Exists(fullPath))
                {
                    return;
                }

                logoUri = new Uri(fullPath, UriKind.Absolute);
            }

            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = logoUri;
            image.EndInit();
            image.Freeze();

            CompanyLogoImage.Source = image;
            CompanyLogoImage.Visibility = Visibility.Visible;
            CompanyLogoPlaceholderTextBlock.Visibility = Visibility.Collapsed;
        }
        catch
        {
            // Keep the placeholder visible when logo loading fails.
        }
    }

    private static string BuildInitials(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "م";
        }

        var parts = value
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(2)
            .Select(part => part[0].ToString());

        var initials = string.Concat(parts);
        return string.IsNullOrWhiteSpace(initials) ? "م" : initials;
    }

    private async void RefreshVehiclesButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(LoadVehiclesAsync);
    private void AddVehicleButton_Click(object sender, RoutedEventArgs e) => AddNewItem(_vehicles, VehiclesGrid, new VehicleDto { PurchaseDate = DateTime.Today, Year = DateTime.Today.Year, VehicleTypeId = _vehicleTypes.FirstOrDefault()?.Id ?? 1, Status = "Active", OilChangeIntervalKm = 10000, MaintenanceIntervalKm = 15000 });
    private async void SaveVehicleButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () => { var item = Selected<VehicleDto>(VehiclesGrid) ?? throw new InvalidOperationException("اختر مركبة أولًا."); await _vehicleService.SaveAsync(ToForm(item)); await LoadVehiclesAsync(); await LoadDashboardAsync(); await LoadNotificationsAsync(); });
    private async void DeleteVehicleButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(() => DeleteSelectedAsync(VehiclesGrid, _vehicles, x => x.Id, _vehicleService.DeleteAsync, LoadVehiclesAsync));

    private async void RefreshContractsButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(LoadContractsAsync);
    private void AddContractButton_Click(object sender, RoutedEventArgs e) => AddNewItem(_contracts, ContractsGrid, new ContractDto { StartDate = DateTime.Today, EndDate = DateTime.Today.AddMonths(1), VehicleId = _vehicles.FirstOrDefault()?.Id ?? 0, ContractStatusId = _contractStatuses.FirstOrDefault()?.Id ?? 1 });
    private async void SaveContractButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () => { var item = Selected<ContractDto>(ContractsGrid) ?? throw new InvalidOperationException("اختر عقدًا أولًا."); await _contractService.SaveAsync(ToForm(item)); await LoadContractsAsync(); await LoadDashboardAsync(); await LoadNotificationsAsync(); });
    private async void DeleteContractButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(() => DeleteSelectedAsync(ContractsGrid, _contracts, x => x.Id, _contractService.DeleteAsync, LoadContractsAsync));
    private async void AttachContractDocumentButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        var item = Selected<ContractDto>(ContractsGrid) ?? throw new InvalidOperationException("اختر عقدًا أولًا.");
        var filePath = PickDocumentPath();
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        item.DocumentUrl = filePath;
        await _contractService.SaveAsync(ToForm(item));
        await LoadContractsAsync();
    });

    private async void RefreshMaintenanceButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(LoadMaintenanceAsync);
    private void AddMaintenanceButton_Click(object sender, RoutedEventArgs e) => AddNewItem(_maintenance, MaintenanceGrid, new MaintenanceRequestDto { VehicleId = _vehicles.FirstOrDefault()?.Id ?? 0, MaintenanceTypeId = _maintenanceTypes.FirstOrDefault()?.Id ?? 1, RequestDate = DateTime.Today, Status = "Open" });
    private async void SaveMaintenanceButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () => { var item = Selected<MaintenanceRequestDto>(MaintenanceGrid) ?? throw new InvalidOperationException("اختر طلب صيانة أولًا."); await _maintenanceService.SaveAsync(ToForm(item)); await LoadMaintenanceAsync(); await LoadDashboardAsync(); await LoadNotificationsAsync(); });
    private async void DeleteMaintenanceButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(() => DeleteSelectedAsync(MaintenanceGrid, _maintenance, x => x.Id, _maintenanceService.DeleteAsync, LoadMaintenanceAsync));
    private async void AttachMaintenanceDocumentButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        var item = Selected<MaintenanceRequestDto>(MaintenanceGrid) ?? throw new InvalidOperationException("اختر طلب صيانة أولًا.");
        var filePath = PickDocumentPath();
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        item.DocumentUrl = filePath;
        await _maintenanceService.SaveAsync(ToForm(item));
        await LoadMaintenanceAsync();
    });

    private async void RefreshDriversButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(LoadDriversAsync);
    private void AddDriverButton_Click(object sender, RoutedEventArgs e) => AddNewItem(_drivers, DriversGrid, new DriverDto { DateOfBirth = DateTime.Today.AddYears(-30), LicenseExpiryDate = DateTime.Today.AddYears(1), IsActive = true });
    private async void SaveDriverButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () => { var item = Selected<DriverDto>(DriversGrid) ?? throw new InvalidOperationException("اختر سائقًا أولًا."); await _driverService.SaveAsync(ToForm(item)); await LoadDriversAsync(); await LoadDashboardAsync(); });
    private async void DeleteDriverButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(() => DeleteSelectedAsync(DriversGrid, _drivers, x => x.Id, _driverService.DeleteAsync, LoadDriversAsync));

    private async void RefreshEmployeesButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(LoadEmployeesAsync);
    private void AddEmployeeButton_Click(object sender, RoutedEventArgs e) => AddNewItem(_employees, EmployeesGrid, new EmployeeDto { HireDate = DateTime.Today, Status = "Active" });
    private async void SaveEmployeeButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () => { var item = Selected<EmployeeDto>(EmployeesGrid) ?? throw new InvalidOperationException("اختر موظفًا أولًا."); await _employeeService.SaveAsync(ToForm(item)); await LoadEmployeesAsync(); });
    private async void DeleteEmployeeButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(() => DeleteSelectedAsync(EmployeesGrid, _employees, x => x.Id, _employeeService.DeleteAsync, LoadEmployeesAsync));

    private void TripFilter_Changed(object sender, RoutedEventArgs e)
    {
        var view = CollectionViewSource.GetDefaultView(TripsGrid.ItemsSource);
        if (view == null) return;

        view.Filter = item =>
        {
            // تم تغيير Models إلى DTOs
            if (item is not FleetManagementSystem.Core.DTOs.TripDto trip) return false;

            if (TripFilterId != null && !string.IsNullOrWhiteSpace(TripFilterId.Text) && !trip.Id.ToString().Contains(TripFilterId.Text.Trim(), StringComparison.OrdinalIgnoreCase))
                return false;

            if (TripFilterDate != null && TripFilterDate.SelectedDate.HasValue && trip.StartDate.Date != TripFilterDate.SelectedDate.Value.Date)
                return false;

            if (TripFilterPlate != null && !string.IsNullOrWhiteSpace(TripFilterPlate.Text) && (trip.VehiclePlateNumber == null || !trip.VehiclePlateNumber.Contains(TripFilterPlate.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                return false;

            if (TripFilterStartLocation != null && !string.IsNullOrWhiteSpace(TripFilterStartLocation.Text) && (trip.StartLocation == null || !trip.StartLocation.Contains(TripFilterStartLocation.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                return false;

            if (TripFilterEndLocation != null && !string.IsNullOrWhiteSpace(TripFilterEndLocation.Text) && (trip.EndLocation == null || !trip.EndLocation.Contains(TripFilterEndLocation.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                return false;

            if (TripFilterDriver != null && !string.IsNullOrWhiteSpace(TripFilterDriver.Text) && (trip.DriverName == null || !trip.DriverName.Contains(TripFilterDriver.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                return false;

            // استخدام TripFilterPerson لفلترة الموصي (RequesterName)
            if (TripFilterPerson != null && !string.IsNullOrWhiteSpace(TripFilterPerson.Text) && (trip.RequesterName == null || !trip.RequesterName.Contains(TripFilterPerson.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                return false;

            if (TripFilterPurpose != null && !string.IsNullOrWhiteSpace(TripFilterPurpose.Text) && (trip.Purpose == null || !trip.Purpose.Contains(TripFilterPurpose.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                return false;

            return true;
        };
    }
    private async void RefreshTripsButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(LoadTripsAsync);
    private async void AddTripButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedVehicleId = Selected<TripDto>(TripsGrid)?.VehicleId;
        var window = new TripEntryWindow(_vehicles, _drivers, _employees, selectedVehicleId)
        {
            Owner = this
        };

        if (window.ShowDialog() != true || window.TripForm is null)
        {
            return;
        }

        await RunSafeAsync(async () =>
        {
            await _tripService.SaveAsync(window.TripForm);
            await LoadVehiclesAsync();
            await LoadTripsAsync();
            await LoadDashboardAsync();
            await LoadNotificationsAsync();
        });
    }

    private async void DeleteTripButton_Click(object sender, RoutedEventArgs e) =>
        await RunSafeAsync(() => DeleteSelectedAsync(TripsGrid, _trips, x => x.Id, _tripService.DeleteAsync, LoadTripsAsync));

    private void PrintTripsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var pd = new PrintDialog();
            if (pd.ShowDialog() != true) return;

            var flowDoc = new FlowDocument
            {
                ColumnWidth = pd.PrintableAreaWidth,
                PageWidth = pd.PrintableAreaWidth,
                PageHeight = pd.PrintableAreaHeight,
                PagePadding = new Thickness(30),
                FontFamily = new FontFamily("Cairo, Arial, Tahoma")
            };

            var title = new Paragraph(new Run("سجل التشغيلات"))
            {
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            flowDoc.Blocks.Add(title);

            var table = new Table { CellSpacing = 0, BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) };
            int columnsCount = 10;
            for (int i = 0; i < columnsCount; i++) table.Columns.Add(new TableColumn());

            var headerGroup = new TableRowGroup();
            var headerRow = new TableRow { Background = Brushes.LightGray, FontWeight = FontWeights.Bold };
            string[] headers = { "سيريال", "التاريخ", "السيارة", "من/إلى", "اسم السائق", "الموصي", "المشرف", "الغرض", "الحالة", "المسافة" };

            foreach (var h in headers)
            {
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run(h)) { TextAlignment = TextAlignment.Center, Padding = new Thickness(5) }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
            }
            headerGroup.Rows.Add(headerRow);
            table.RowGroups.Add(headerGroup);

            var dataGroup = new TableRowGroup();
            var view = CollectionViewSource.GetDefaultView(TripsGrid.ItemsSource);
            if (view != null)
            {
                foreach (TripDto trip in view)
                {
                    var row = new TableRow();
                    row.Cells.Add(new TableCell(new Paragraph(new Run(trip.Id.ToString()))) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1), Padding = new Thickness(5), TextAlignment = TextAlignment.Center });
                    row.Cells.Add(new TableCell(new Paragraph(new Run(trip.StartDate.ToString("yyyy-MM-dd")))) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1), Padding = new Thickness(5), TextAlignment = TextAlignment.Center });
                    row.Cells.Add(new TableCell(new Paragraph(new Run(trip.VehiclePlateNumber ?? ""))) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1), Padding = new Thickness(5), TextAlignment = TextAlignment.Center });
                    row.Cells.Add(new TableCell(new Paragraph(new Run($"{trip.StartLocation} - {trip.EndLocation}"))) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1), Padding = new Thickness(5), TextAlignment = TextAlignment.Center });
                    row.Cells.Add(new TableCell(new Paragraph(new Run(trip.DriverName ?? ""))) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1), Padding = new Thickness(5), TextAlignment = TextAlignment.Center });
                    row.Cells.Add(new TableCell(new Paragraph(new Run(trip.RequesterName ?? ""))) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1), Padding = new Thickness(5), TextAlignment = TextAlignment.Center });
                    row.Cells.Add(new TableCell(new Paragraph(new Run(trip.SupervisorName ?? ""))) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1), Padding = new Thickness(5), TextAlignment = TextAlignment.Center });
                    row.Cells.Add(new TableCell(new Paragraph(new Run(trip.Purpose ?? ""))) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1), Padding = new Thickness(5), TextAlignment = TextAlignment.Center });
                    row.Cells.Add(new TableCell(new Paragraph(new Run(trip.Status ?? ""))) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1), Padding = new Thickness(5), TextAlignment = TextAlignment.Center });
                    row.Cells.Add(new TableCell(new Paragraph(new Run(trip.Distance.ToString("0.##")))) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1), Padding = new Thickness(5), TextAlignment = TextAlignment.Center });
                    dataGroup.Rows.Add(row);
                }
            }
            table.RowGroups.Add(dataGroup);
            flowDoc.Blocks.Add(table);

            pd.PrintDocument(((IDocumentPaginatorSource)flowDoc).DocumentPaginator, "تقرير التشغيلات");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"عذراً، حدث خطأ أثناء الطباعة: {ex.Message}", "خطأ في الطباعة", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void CloseTripButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        var trip = Selected<TripDto>(TripsGrid)
            ?? throw new InvalidOperationException("اختر تشغيلة أولًا.");

        var window = new TripCloseWindow(trip)
        {
            Owner = this
        };

        if (window.ShowDialog() != true || window.TripForm is null)
        {
            return;
        }

        await _tripService.SaveAsync(window.TripForm);
        await LoadVehiclesAsync();
        await LoadTripsAsync();
        await LoadDashboardAsync();
        await LoadNotificationsAsync();
    });

    private void ShowTripFormButton_Click(object sender, RoutedEventArgs e)
    {
        var trip = (sender as FrameworkElement)?.DataContext as TripDto ?? Selected<TripDto>(TripsGrid);
        if (trip is null)
        {
            MessageBox.Show("اختر تشغيلة أولًا.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var vehicle = _vehicles.FirstOrDefault(v => v.Id == trip.VehicleId);
        var window = new TripPermitWindow(trip, vehicle, _settings)
        {
            Owner = this
        };
        window.ShowDialog();
    }

    private void ShowVehicleLicenseFormButton_Click(object sender, RoutedEventArgs e)
    {
        var item = (sender as FrameworkElement)?.DataContext as VehicleLicenseRow ?? Selected<VehicleLicenseRow>(LicensesGrid);
        if (item is null)
        {
            MessageBox.Show("اختر ترخيص عربية أولًا.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var vehicle = _vehicles.FirstOrDefault(v => v.Id == item.VehicleId);
        var window = new A5DocumentWindow(
            "نموذج ترخيص عربية",
            $"رقم العربية: {ValueOrDash(item.PlateNumber)}",
            new[]
            {
                Section("بيانات الترخيص",
                    Row("بداية الترخيص", item.RegistrationStartDate),
                    Row("نهاية الترخيص", item.RegistrationExpiryDate),
                    Row("الحالة", item.Status),
                    Row("الإنذار", item.ExpiryAlert)),
                Section("بيانات العربية",
                    Row("رقم العربية", item.PlateNumber),
                    Row("الموديل", item.Model),
                    Row("نوع العربية", vehicle?.VehicleType),
                    Row("سنة الصنع", vehicle?.Year),
                    Row("رقم الشاسيه", vehicle?.ChassisNumber),
                    Row("رقم الموتور", vehicle?.EngineNumber))
            },
            _settings)
        {
            Owner = this
        };

        window.ShowDialog();
    }

    private void ShowInsuranceFormButton_Click(object sender, RoutedEventArgs e)
    {
        var item = (sender as FrameworkElement)?.DataContext as InsuranceDto ?? Selected<InsuranceDto>(InsuranceGrid);
        if (item is null)
        {
            MessageBox.Show("اختر وثيقة تأمين أولًا.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var vehicle = _vehicles.FirstOrDefault(v => v.Id == item.VehicleId);
        var window = new A5DocumentWindow(
            "نموذج تأمين عربية",
            $"رقم العربية: {ValueOrDash(item.VehiclePlateNumber, vehicle?.PlateNumber)}",
            new[]
            {
                Section("بيانات الوثيقة",
                    Row("رقم الوثيقة", item.PolicyNumber),
                    Row("شركة التأمين", item.InsuranceCompany),
                    Row("نوع الوثيقة", item.PolicyType),
                    Row("الحالة", item.Status),
                    Row("الإنذار", item.ExpiryAlert)),
                Section("المدة والقيمة",
                    Row("بداية التأمين", item.StartDate),
                    Row("نهاية التأمين", item.ExpiryDate),
                    Row("القسط", item.PremiumAmount),
                    Row("مبلغ التغطية", item.CoverageAmount)),
                Section("بيانات العربية والتواصل",
                    Row("رقم العربية", ValueOrDash(item.VehiclePlateNumber, vehicle?.PlateNumber)),
                    Row("الموديل", vehicle?.Model),
                    Row("مندوب التأمين", item.AgentName),
                    Row("هاتف المندوب", item.AgentPhoneNumber),
                    Row("ملاحظات", ValueOrDash(item.CoverageDetails, item.Notes)))
            },
            _settings)
        {
            Owner = this
        };

        window.ShowDialog();
    }

    private async void RefreshFuelButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(LoadFuelAsync);
    private void AddFuelButton_Click(object sender, RoutedEventArgs e) => AddNewItem(_fuel, FuelGrid, new FuelTransactionDto { VehicleId = _vehicles.FirstOrDefault()?.Id ?? 0, TransactionDate = DateTime.Today, FuelType = "Gasoline" });
    private async void SaveFuelButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () => { var item = Selected<FuelTransactionDto>(FuelGrid) ?? throw new InvalidOperationException("اختر سجل وقود أولًا."); await _fuelService.SaveAsync(ToForm(item)); await LoadFuelAsync(); await LoadVehiclesAsync(); await LoadTreasuryAsync(); await LoadDashboardAsync(); });
    private async void DeleteFuelButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () => { await DeleteSelectedAsync(FuelGrid, _fuel, x => x.Id, _fuelService.DeleteAsync, LoadFuelAsync); await LoadTreasuryAsync(); });

    private async void RefreshExpensesButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(LoadExpensesAsync);
    private void AddExpenseButton_Click(object sender, RoutedEventArgs e) => AddNewItem(_expenses, ExpensesGrid, new ExpenseDto { VehicleId = _vehicles.FirstOrDefault()?.Id ?? 0, ExpenseDate = DateTime.Today, Status = "Pending" });
    private async void SaveExpenseButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () => { var item = Selected<ExpenseDto>(ExpensesGrid) ?? throw new InvalidOperationException("اختر مصروفًا أولًا."); await _expenseService.SaveAsync(ToForm(item)); await LoadExpensesAsync(); await LoadTreasuryAsync(); await LoadDashboardAsync(); });
    private async void DeleteExpenseButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () => { await DeleteSelectedAsync(ExpensesGrid, _expenses, x => x.Id, _expenseService.DeleteAsync, LoadExpensesAsync); await LoadTreasuryAsync(); });

    private async void RefreshOilChangesButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(LoadOilChangesAsync);
    private void AddOilChangeButton_Click(object sender, RoutedEventArgs e) => AddNewItem(_oilChanges, OilChangesGrid, new OilChangeDto { VehicleId = _vehicles.FirstOrDefault()?.Id ?? 0, ChangeDate = DateTime.Today, OdometerAtChange = 0, NextOilChangeOdometer = 10000, Status = "Scheduled" });
    private async void SaveOilChangeButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () => { var item = Selected<OilChangeDto>(OilChangesGrid) ?? throw new InvalidOperationException("اختر سجل زيت أولًا."); await _oilChangeService.SaveAsync(ToForm(item)); await LoadOilChangesAsync(); await LoadVehiclesAsync(); await LoadDashboardAsync(); });
    private async void DeleteOilChangeButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(() => DeleteSelectedAsync(OilChangesGrid, _oilChanges, x => x.Id, _oilChangeService.DeleteAsync, LoadOilChangesAsync));

    private async void RefreshTreasuryButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(LoadTreasuryAsync);
    private void AddTreasuryButton_Click(object sender, RoutedEventArgs e) => AddNewItem(_treasuryTransactions, TreasuryGrid, new TreasuryTransactionDto { TransactionDate = DateTime.Today, TransactionType = "إيراد", PaymentMethod = "نقدي" });
    private async void SaveTreasuryButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () => { var item = Selected<TreasuryTransactionDto>(TreasuryGrid) ?? throw new InvalidOperationException("اختر حركة خزينة أولًا."); await _treasuryService.SaveAsync(ToForm(item)); await LoadTreasuryAsync(); await LoadDashboardAsync(); });
    private async void DeleteTreasuryButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(() => DeleteSelectedAsync(TreasuryGrid, _treasuryTransactions, x => x.Id, _treasuryService.DeleteAsync, LoadTreasuryAsync));

    private async void RefreshLicensesButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        await LoadVehiclesAsync();
        await LoadLicensesAsync();
    });

    private void AddVehicleFromLicenseButton_Click(object sender, RoutedEventArgs e) =>
        AddNewItem(_vehicleLicenses, LicensesGrid, new VehicleLicenseRow
        {
            RegistrationStartDate = DateTime.Today,
            RegistrationExpiryDate = DateTime.Today.AddYears(1)
        });

    private async void SaveVehicleLicenseButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        CommitGridEdit(LicensesGrid);
        var item = Selected<VehicleLicenseRow>(LicensesGrid)
            ?? throw new InvalidOperationException("اختر ترخيص عربية أولًا.");

        if (string.IsNullOrWhiteSpace(item.PlateNumber))
        {
            throw new InvalidOperationException("رقم العربية مطلوب قبل حفظ الترخيص.");
        }

        if (item.RegistrationStartDate.HasValue &&
            item.RegistrationExpiryDate.HasValue &&
            item.RegistrationExpiryDate.Value.Date < item.RegistrationStartDate.Value.Date)
        {
            throw new InvalidOperationException("تاريخ نهاية الترخيص يجب أن يكون بعد تاريخ البداية.");
        }

        var vehicle = item.VehicleId == 0
            ? null
            : _vehicles.FirstOrDefault(v => v.Id == item.VehicleId)
                ?? throw new InvalidOperationException("العربية غير موجودة.");

        await _vehicleService.SaveAsync(ToVehicleFormFromLicense(item, vehicle));
        await LoadVehiclesAsync();
        await LoadLicensesAsync();
        await LoadDashboardAsync();
        await LoadNotificationsAsync();
    });

    private async void DeleteVehicleFromLicenseButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        var item = Selected<VehicleLicenseRow>(LicensesGrid)
            ?? throw new InvalidOperationException("اختر عربية أولًا.");

        if (item.VehicleId == 0)
        {
            _vehicleLicenses.Remove(item);
            return;
        }

        if (!ConfirmDelete())
        {
            return;
        }

        await _vehicleService.DeleteAsync(item.VehicleId);
        await LoadVehiclesAsync();
        await LoadLicensesAsync();
        await LoadDashboardAsync();
        await LoadNotificationsAsync();
    });

    private async void ClearVehicleLicenseButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        var item = Selected<VehicleLicenseRow>(LicensesGrid)
            ?? throw new InvalidOperationException("اختر ترخيص عربية أولًا.");

        var vehicle = _vehicles.FirstOrDefault(v => v.Id == item.VehicleId)
            ?? throw new InvalidOperationException("العربية غير موجودة.");

        vehicle.RegistrationStartDate = null;
        vehicle.RegistrationExpiryDate = null;
        await _vehicleService.SaveAsync(ToForm(vehicle));
        await LoadVehiclesAsync();
        await LoadLicensesAsync();
        await LoadDashboardAsync();
        await LoadNotificationsAsync();
    });

    private async void RefreshInsuranceButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(LoadInsuranceAsync);
    private void AddInsuranceButton_Click(object sender, RoutedEventArgs e) => AddNewItem(_insurance, InsuranceGrid, new InsuranceDto { VehicleId = _vehicles.FirstOrDefault()?.Id ?? 0, StartDate = DateTime.Today, ExpiryDate = DateTime.Today.AddYears(1), Status = "Active" });
    private async void SaveInsuranceButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        CommitGridEdit(InsuranceGrid);
        var item = Selected<InsuranceDto>(InsuranceGrid) ?? throw new InvalidOperationException("اختر وثيقة تأمين أولًا.");
        EnsureVehicleExists(item.VehicleId, "وثيقة التأمين");
        await _insuranceService.SaveAsync(ToForm(item));
        await LoadInsuranceAsync();
        await LoadDashboardAsync();
    });
    private async void DeleteInsuranceButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(() => DeleteSelectedAsync(InsuranceGrid, _insurance, x => x.Id, _insuranceService.DeleteAsync, LoadInsuranceAsync));
    private async void AttachInsuranceDocumentButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        var item = Selected<InsuranceDto>(InsuranceGrid) ?? throw new InvalidOperationException("اختر وثيقة تأمين أولًا.");
        var filePath = PickDocumentPath();
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        item.DocumentUrl = filePath;
        await _insuranceService.SaveAsync(ToForm(item));
        await LoadInsuranceAsync();
    });

    private async void RefreshCustodyButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(LoadCustodyAsync);
    private void AddCustodyButton_Click(object sender, RoutedEventArgs e) => AddNewItem(_custodies, CustodyGrid, new CustodyDto { VehicleId = _vehicles.FirstOrDefault()?.Id ?? 0, HandoverDate = DateTime.Today, Status = "Active", VehicleConditionRating = 5 });
    private async void SaveCustodyButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        CommitGridEdit(CustodyGrid);
        var item = Selected<CustodyDto>(CustodyGrid) ?? throw new InvalidOperationException("اختر سجل عهدة أولًا.");
        EnsureVehicleExists(item.VehicleId, "سجل العهدة");
        await _custodyService.SaveAsync(ToForm(item));
        await LoadCustodyAsync();
    });
    private async void DeleteCustodyButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(() => DeleteSelectedAsync(CustodyGrid, _custodies, x => x.Id, _custodyService.DeleteAsync, LoadCustodyAsync));
    private async void AttachCustodyDocumentButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        var item = Selected<CustodyDto>(CustodyGrid) ?? throw new InvalidOperationException("اختر سجل عهدة أولًا.");
        var filePath = PickDocumentPath();
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        item.DocumentUrl = filePath;
        await _custodyService.SaveAsync(ToForm(item));
        await LoadCustodyAsync();
    });

    private void AddVehicleTypeButton_Click(object sender, RoutedEventArgs e) => AddNewItem(_vehicleTypes, VehicleTypesGrid, new VehicleTypeDto { IsActive = true });
    private async void SaveVehicleTypeButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () => { var item = Selected<VehicleTypeDto>(VehicleTypesGrid) ?? throw new InvalidOperationException("اختر نوع مركبة أولًا."); await _masterDataService.SaveVehicleTypeAsync(item); await LoadMasterDataAsync(); });
    private async void DeleteVehicleTypeButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(() => DeleteSelectedAsync(VehicleTypesGrid, _vehicleTypes, x => x.Id, _masterDataService.DeleteVehicleTypeAsync, LoadMasterDataAsync));

    private void AddContractStatusButton_Click(object sender, RoutedEventArgs e) => AddNewItem(_contractStatuses, ContractStatusesGrid, new ContractStatusDto { Color = "#1F2937", IsActive = true });
    private async void SaveContractStatusButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () => { var item = Selected<ContractStatusDto>(ContractStatusesGrid) ?? throw new InvalidOperationException("اختر حالة عقد أولًا."); await _masterDataService.SaveContractStatusAsync(item); await LoadMasterDataAsync(); });
    private async void DeleteContractStatusButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(() => DeleteSelectedAsync(ContractStatusesGrid, _contractStatuses, x => x.Id, _masterDataService.DeleteContractStatusAsync, LoadMasterDataAsync));

    private void AddMaintenanceTypeButton_Click(object sender, RoutedEventArgs e) => AddNewItem(_maintenanceTypes, MaintenanceTypesGrid, new MaintenanceTypeDto { IsActive = true, EstimatedDurationDays = 1 });
    private async void SaveMaintenanceTypeButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () => { var item = Selected<MaintenanceTypeDto>(MaintenanceTypesGrid) ?? throw new InvalidOperationException("اختر نوع صيانة أولًا."); await _masterDataService.SaveMaintenanceTypeAsync(item); await LoadMasterDataAsync(); });
    private async void DeleteMaintenanceTypeButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(() => DeleteSelectedAsync(MaintenanceTypesGrid, _maintenanceTypes, x => x.Id, _masterDataService.DeleteMaintenanceTypeAsync, LoadMasterDataAsync));

    private void AddServiceProviderButton_Click(object sender, RoutedEventArgs e) => AddNewItem(_serviceProviders, ServiceProvidersGrid, new ServiceProviderDto { IsActive = true, AverageRating = 4 });
    private async void SaveServiceProviderButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () => { var item = Selected<ServiceProviderDto>(ServiceProvidersGrid) ?? throw new InvalidOperationException("اختر مزود خدمة أولًا."); await _masterDataService.SaveServiceProviderAsync(item); await LoadMasterDataAsync(); });
    private async void DeleteServiceProviderButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(() => DeleteSelectedAsync(ServiceProvidersGrid, _serviceProviders, x => x.Id, _masterDataService.DeleteServiceProviderAsync, LoadMasterDataAsync));

    private async void RefreshUsersButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(LoadUsersAsync);
    private void AddUserButton_Click(object sender, RoutedEventArgs e) => AddNewItem(_users, UsersGrid, new UserFormDto { IsActive = true, Role = "OperationsDataEntry" });
    private async void SaveUserButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        if (!string.Equals(_currentUser?.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("إدارة المستخدمين متاحة لمدير النظام فقط.");
        }

        var item = Selected<UserFormDto>(UsersGrid) ?? throw new InvalidOperationException("اختر مستخدمًا أولًا.");
        item.ConfirmPassword = item.Password;
        await _authenticationService.SaveUserAsync(item);
        await LoadUsersAsync();
    });
    private async void ToggleUserStatusButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        if (!string.Equals(_currentUser?.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("إدارة المستخدمين متاحة لمدير النظام فقط.");
        }

        var item = Selected<UserFormDto>(UsersGrid) ?? throw new InvalidOperationException("اختر مستخدمًا أولًا.");
        if (item.Id == 0)
        {
            throw new InvalidOperationException("احفظ المستخدم الجديد أولًا.");
        }

        await _authenticationService.ToggleUserStatusAsync(item.Id, !item.IsActive);
        await LoadUsersAsync();
    });

    private async void GenerateReportButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        await GenerateCurrentReportAsync();
    });

    private async void ShowReportA5Button_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        var selectedRow = _currentReport is null ? null : ReportsGrid.SelectedItem as DataRowView;
        var report = await GenerateCurrentReportAsync();
        OpenReportA5(report, selectedRow);
    });

    private async void ShowReportRowA5Button_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        var selectedRow = (sender as FrameworkElement)?.DataContext as DataRowView ?? ReportsGrid.SelectedItem as DataRowView;
        if (_currentReport is null)
        {
            await GenerateCurrentReportAsync();
            selectedRow = ReportsGrid.SelectedItem as DataRowView;
        }

        if (selectedRow is null)
        {
            throw new InvalidOperationException("اختر سجل تقرير أولًا.");
        }

        OpenReportA5(_currentReport!, selectedRow);
    });

    private void DeleteReportButton_Click(object sender, RoutedEventArgs e)
    {
        if (ReportsGrid.ItemsSource is not DataView view)
        {
            MessageBox.Show("لا يوجد تقرير معروض للحذف.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (ReportsGrid.SelectedItem is DataRowView selectedRow)
        {
            var table = selectedRow.Row.Table;
            selectedRow.Row.Delete();
            table?.AcceptChanges();
        }
        else
        {
            ReportsGrid.ItemsSource = null;
        }

        _currentReport = null;
    }

    private async void ExportReportCsvButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        var report = await GenerateCurrentReportAsync();
        var dialog = new SaveFileDialog
        {
            Title = "حفظ التقرير CSV",
            Filter = "CSV (*.csv)|*.csv",
            FileName = $"{SafeFileName(report.ReportTitle)}.csv"
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        await File.WriteAllTextAsync(dialog.FileName, BuildCsv(report), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        MessageBox.Show("تم تصدير التقرير CSV بنجاح.", "تم", MessageBoxButton.OK, MessageBoxImage.Information);
    });

    private async void ExportReportExcelButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        var report = await GenerateCurrentReportAsync();
        var dialog = new SaveFileDialog
        {
            Title = "حفظ التقرير Excel",
            Filter = "Excel (*.xls)|*.xls",
            FileName = $"{SafeFileName(report.ReportTitle)}.xls"
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        await File.WriteAllTextAsync(dialog.FileName, BuildExcelHtml(report), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        MessageBox.Show("تم تصدير التقرير Excel بنجاح.", "تم", MessageBoxButton.OK, MessageBoxImage.Information);
    });

    private async void PrintReportPdfButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        var report = await GenerateCurrentReportAsync();
        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() != true)
        {
            return;
        }

        var document = BuildReportDocument(report);
        document.PageWidth = printDialog.PrintableAreaWidth;
        document.PageHeight = printDialog.PrintableAreaHeight;
        document.PagePadding = new Thickness(36);
        printDialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, report.ReportTitle);
    });

    private void OpenReportA5(ReportDataDto report, DataRowView? selectedRow)
    {
        var subtitle = selectedRow is null
            ? $"تقرير حالي - {report.GeneratedDate.ToLocalTime():yyyy-MM-dd HH:mm}"
            : $"سجل محدد - {report.GeneratedDate.ToLocalTime():yyyy-MM-dd HH:mm}";

        var window = new A5DocumentWindow(
            "نموذج تقرير A5",
            $"{report.ReportTitle} - {subtitle}",
            BuildReportA5Sections(report, selectedRow),
            _settings)
        {
            Owner = this
        };

        window.ShowDialog();
    }

    private async Task<ReportDataDto> GenerateCurrentReportAsync()
    {
        var report = await _reportingService.GenerateReportAsync(BuildCurrentReportFilter());
        _currentReport = report;
        ReportsGrid.ItemsSource = ToDataTable(report).DefaultView;
        return report;
    }

    private ReportFilterDto BuildCurrentReportFilter()
    {
        var selectedItem = ReportTypeComboBox.SelectedItem as ComboBoxItem;
        var reportType = selectedItem?.Tag?.ToString() ?? "vehicles";
        int? vehicleId = ReportVehicleComboBox.SelectedValue is int selectedVehicleId
            ? selectedVehicleId
            : null;

        return new ReportFilterDto
        {
            ReportType = reportType,
            VehicleId = vehicleId,
            StartDate = ReportStartDatePicker.SelectedDate,
            EndDate = ReportEndDatePicker.SelectedDate
        };
    }

    private void ReportTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _currentReport = null;
        UpdateReportVehicleFilterVisibility();
    }

    private void UpdateReportVehicleFilterVisibility()
    {
        if (ReportTypeComboBox is null || ReportVehicleFilterPanel is null)
        {
            return;
        }

        var selectedItem = ReportTypeComboBox.SelectedItem as ComboBoxItem;
        var reportType = selectedItem?.Tag?.ToString();
        ReportVehicleFilterPanel.Visibility = string.Equals(reportType, "vehicletrips", StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private string? PickDocumentPath()
    {
        var dialog = new OpenFileDialog
        {
            Title = "اختر مستندًا للإرفاق",
            Filter = "Documents (*.pdf;*.jpg;*.jpeg;*.png;*.doc;*.docx)|*.pdf;*.jpg;*.jpeg;*.png;*.doc;*.docx|All files (*.*)|*.*",
            CheckFileExists = true
        };

        return dialog.ShowDialog(this) == true ? dialog.FileName : null;
    }

    private async void SaveSettingsButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        var dto = _settings ?? new CompanySettingsDto();
        dto.CompanyName = CompanyNameTextBox.Text;
        dto.CompanyNameEn = CompanyNameEnTextBox.Text;
        dto.Address = CompanyAddressTextBox.Text;
        dto.PhoneNumber = CompanyPhoneTextBox.Text;
        dto.Email = CompanyEmailTextBox.Text;
        dto.Website = CompanyWebsiteTextBox.Text;
        _settings = await _settingsService.SaveSettingsAsync(dto);
        await LoadSettingsAsync();
    });

    private async void RefreshNotificationsButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(LoadNotificationsAsync);
}
