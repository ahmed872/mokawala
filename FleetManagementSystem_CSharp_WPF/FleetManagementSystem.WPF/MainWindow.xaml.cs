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
using System.Windows.Automation;
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
    private const int VehicleRegistrationAlertDays = 60;
    private const double DefaultGridColumnWidth = 190;
    private const double NarrowGridColumnWidth = 95;
    private const double CompactGridColumnWidth = 140;
    private const double DateGridColumnWidth = 170;
    private const double WideGridColumnWidth = 260;
    private const double ExtraWideGridColumnWidth = 320;
    private const string TemporarilyLockedModuleTooltip = "القسم مغلق مؤقتًا لحين مراجعة المنطق والبيانات.";

    private static readonly HashSet<string> HiddenColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "VehicleType",
        "VehiclePlateNumber",
        "DriverName",
        "RequesterName",
        "SupervisorName",
        "SocialInsuranceDetails",
        "TripSummary",
        "MaintenanceType",
        "ServiceProvider",
        "Items",
        "ConfirmPassword",
        "DriverId"
    };

    private static readonly HashSet<string> TemporarilyLockedModules = new(StringComparer.OrdinalIgnoreCase);

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
        ["RegistrationType"] = "نوع الرخصة",
        ["RegistrationStartDate"] = "بداية الترخيص",
        ["RegistrationExpiryDate"] = "نهاية الترخيص",
        ["AccidentInsuranceDetails"] = "تأمين الحوادث",
        ["SocialInsuranceDetails"] = "التأمين الاجتماعي",
        ["OilChangeIntervalKm"] = "تغيير الزيت كل كام كم",
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
        ["LicenseStartDate"] = "بداية الرخصة",
        ["LicenseExpiryDate"] = "انتهاء الرخصة",
        ["LicenseType"] = "نوع الرخصة",
        ["IsCompanyInsured"] = "متأمن عليه",
        ["Governorate"] = "المحافظة",
        ["FullAddress"] = "العنوان بالكامل",
        ["TrafficUnit"] = "وحدة المرور",
        ["WorkLocation"] = "موقع العمل",
        ["LicenseExpiryAlert"] = "إنذار الرخصة",
        ["IsActive"] = "نشط",
        ["DayName"] = "اليوم",
        ["WorkDate"] = "التاريخ",
        ["DriverWorkLocation"] = "موقع عمل السائق",
        ["SaturdayStatus"] = "السبت",
        ["SundayStatus"] = "الأحد",
        ["MondayStatus"] = "الاثنين",
        ["TuesdayStatus"] = "الثلاثاء",
        ["WednesdayStatus"] = "الأربعاء",
        ["ThursdayStatus"] = "الخميس",
        ["FridayStatus"] = "الجمعة",
        ["PresentDays"] = "أيام الحضور",
        ["AbsentDays"] = "أيام الغياب",
        ["LeaveDays"] = "أيام الإجازة",
        ["WorkedFridays"] = "جمعات عمل",
        ["EarnedRestDays"] = "راحات مستحقة",
        ["RemainingRestDays"] = "رصيد الراحة",
        ["EmployeeId"] = "كود الموظف",
        ["Department"] = "القسم",
        ["Position"] = "الوظيفة",
        ["HireDate"] = "تاريخ التعيين",
        ["TerminationDate"] = "تاريخ الانتهاء",
        ["TransactionDate"] = "التاريخ",
        ["TransactionType"] = "نوع الحركة",
        ["TripId"] = "رقم التشغيلة",
        ["TripSummary"] = "التشغيلة",
        ["FuelType"] = "نوع الوقود",
        ["Quantity"] = "عدد اللترات",
        ["UnitPrice"] = "سعر اللتر",
        ["TotalCost"] = "إجمالي البنزين",
        ["FuelStation"] = "محطة البنزين",
        ["Odometer"] = "عداد التموين",
        ["PaidFromTreasury"] = "من الخزينة",
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
        ["NextOilChangeOdometer"] = "التغيير القادم",
        ["CurrentVehicleMileage"] = "عداد العربية الحالي",
        ["KmSinceOilChange"] = "المقطوع من آخر تغيير",
        ["RemainingKm"] = "المتبقي كم",
        ["OilAlert"] = "إنذار الزيت",
        ["IsDue"] = "يحتاج متابعة",
        ["Username"] = "اسم المستخدم",
        ["Role"] = "الدور",
        ["Password"] = "كلمة المرور"
    };

    private readonly IServiceProvider _serviceProvider;
    private readonly IVehicleService _vehicleService;
    private readonly IContractService _contractService;
    private readonly IMaintenanceService _maintenanceService;
    private readonly IDriverService _driverService;
    private readonly IDriverAttendanceService _driverAttendanceService;
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
    private readonly ObservableCollection<DriverDto> _driverReportDriverOptions = new();
    private readonly ObservableCollection<DriverWeeklyAttendanceRow> _driverAttendanceWeekRows = new();
    private readonly ObservableCollection<DailyAttendanceRow> _dailyAttendanceRows = new();
    private readonly ObservableCollection<DriverAttendanceDto> _absenceHistoryRows = new();
    private readonly ObservableCollection<DriverReportDto> _driverReportRows = new();
    private readonly ObservableCollection<EmployeeDto> _employees = new();
    private readonly ObservableCollection<TripDto> _trips = new();
    private readonly ObservableCollection<FuelTransactionDto> _fuel = new();
    private readonly ObservableCollection<ExpenseDto> _expenses = new();
    private readonly ObservableCollection<OilChangeDto> _oilChanges = new();
    private readonly ObservableCollection<TreasuryTransactionDto> _treasuryTransactions = new();
    private readonly ObservableCollection<VehicleLicenseRow> _vehicleLicenses = new();
    private readonly ObservableCollection<InsuranceDto> _insurance = new();
    private readonly ObservableCollection<CustodyDto> _custodies = new();
    private readonly ObservableCollection<VehicleTypeDto> _vehicleTypes = new();
    private readonly ObservableCollection<ContractStatusDto> _contractStatuses = new();
    private readonly ObservableCollection<MaintenanceTypeDto> _maintenanceTypes = new();
    private readonly ObservableCollection<ServiceProviderDto> _serviceProviders = new();
    private readonly ObservableCollection<NotificationDto> _notifications = new();
    private readonly ObservableCollection<UserFormDto> _users = new();
    private readonly IReadOnlyList<UserRoleOption> _userRoleOptions = new List<UserRoleOption>
    {
        new("Admin", "مدير النظام"),
        new("OperationsManager", "مدير التشغيل"),
        new("OperationsDataEntry", "إدخال بيانات التشغيل"),
        new("MaintenanceOfficer", "مسؤول الصيانة"),
        new("TreasuryOfficer", "مسؤول الخزينة"),
        new("TripsLicensesOfficer", "مسؤول التشغيلات والتراخيص"),
        new("InsuranceOfficer", "مسؤول التأمينات"),
        new("Viewer", "عرض فقط"),
        new("Staff", "موظف"),
        new("Custodian", "أمين عهدة (يشوف عهدته فقط)")
    };
    private readonly List<DashboardAlertItem> _dashboardAlertItems = new();

    private UserDto? _currentUser;
    private CompanySettingsDto? _settings;
    private ReportDataDto? _currentReport;
    private readonly Dictionary<string, string> _reportColumnFilters = new(StringComparer.OrdinalIgnoreCase);
    private readonly DispatcherTimer _clockTimer;
    private readonly DispatcherTimer _driverAutoSaveTimer;
    private readonly DispatcherTimer _driverAttendanceAutoSaveTimer;
    private bool _isLoadingDriverAttendanceWeek;
    private bool _isSavingDriverAttendanceWeek;
    private bool _isNormalizingDriverAttendanceWeekPicker;
    private bool _isNormalizingDriverReportRange;
    private bool _isSavingDriver;

    public ObservableCollection<VehicleDto> VehiclesForBinding => _vehicles;
    public ObservableCollection<TripDto> TripsForBinding => _trips;
    public ObservableCollection<DriverDto> DriversForBinding => _drivers;
    public ObservableCollection<EmployeeDto> EmployeesForBinding => _employees;
    public ObservableCollection<ContractStatusDto> ContractStatusesForBinding => _contractStatuses;
    public ObservableCollection<MaintenanceTypeDto> MaintenanceTypesForBinding => _maintenanceTypes;
    public ObservableCollection<ServiceProviderDto> ServiceProvidersForBinding => _serviceProviders;
    public IReadOnlyList<string> TreasuryTransactionTypes { get; } = new[] { "إيراد", "صرف" };
    public IReadOnlyList<string> PaymentMethods { get; } = new[] { "نقدي", "تحويل بنكي", "بطاقة" };
    public IReadOnlyList<string> RegistrationTypes { get; } = new[] { "ترخيص" };

    public sealed class VehicleLicenseRow
    {
        public int VehicleId { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; } = DateTime.Today.Year;
        public string ChassisNumber { get; set; } = string.Empty;
        public string EngineNumber { get; set; } = string.Empty;
        public decimal OilChangeIntervalKm { get; set; } = 10000;
        public string RegistrationType { get; set; } = "ترخيص";
        public DateTime? RegistrationStartDate { get; set; }
        public DateTime? RegistrationExpiryDate { get; set; }
        public int? DaysUntilExpiry => RegistrationExpiryDate.HasValue
            ? (RegistrationExpiryDate.Value.Date - DateTime.Today).Days
            : null;
        public string Status => DaysUntilExpiry switch
        {
            null => "غير مسجل",
            < 0 => "منتهي",
            <= VehicleRegistrationAlertDays => "قارب الانتهاء",
            _ => "ساري"
        };
        public string ExpiryAlert => DaysUntilExpiry switch
        {
            null => "أدخل بداية ونهاية الترخيص",
            < 0 => "الترخيص منتهي",
            <= VehicleRegistrationAlertDays => $"يحتاج تجديد خلال {DaysUntilExpiry} يوم",
            _ => "لا يوجد إنذار"
        };

        public static VehicleLicenseRow FromVehicle(VehicleDto vehicle) => new()
        {
            VehicleId = vehicle.Id,
            PlateNumber = vehicle.PlateNumber,
            Model = vehicle.Model,
            Year = vehicle.Year,
            ChassisNumber = vehicle.ChassisNumber,
            EngineNumber = vehicle.EngineNumber,
            OilChangeIntervalKm = vehicle.OilChangeIntervalKm > 0 ? vehicle.OilChangeIntervalKm : 10000,
            RegistrationType = "ترخيص",
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
        public string CategoryKey { get; private init; } = string.Empty;
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
            var priority = days.HasValue ? days switch
            {
                < 0 => 0,
                <= 7 => 1,
                _ => 2
            } : alert.Type switch
            {
                "Critical" or "Error" => 0,
                "Warning" => 1,
                _ => 3
            };

            var (accent, background, border, chip) = priority switch
            {
                0 => (CriticalAccentBrush, CriticalBackgroundBrush, CriticalBorderBrush, CriticalChipBrush),
                1 => (UrgentAccentBrush, UrgentBackgroundBrush, UrgentBorderBrush, UrgentChipBrush),
                _ => (SoonAccentBrush, SoonBackgroundBrush, SoonBorderBrush, SoonChipBrush)
            };

            var isOilAlert = string.Equals(alert.RelatedEntityType, "OilChange", StringComparison.OrdinalIgnoreCase);

            return new DashboardAlertItem
            {
                Title = alert.Title,
                Message = alert.Message,
                CategoryText = GetCategoryText(alert.RelatedEntityType),
                CategoryKey = alert.RelatedEntityType,
                CategoryInitial = GetCategoryInitial(alert.RelatedEntityType),
                StatusText = isOilAlert ? (alert.Type is "Critical" or "Error" ? "متأخر" : "قريب") : GetStatusText(days),
                TimingText = isOilAlert ? "حسب قراءة العداد" : GetTimingText(days),
                DueDateText = isOilAlert
                    ? "إنذار الزيت مرتبط بالعداد"
                    : dueDate.HasValue ? $"تاريخ الانتهاء {dueDate:yyyy-MM-dd}" : GetNoDateText(alert.RelatedEntityType),
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
                "Driver" => "رخصة سائق",
                "License" => "رخصة سائق",
                "Insurance" => "تأمين",
                "OilChange" => "زيت",
                _ => "متابعة"
            };

        private static string GetCategoryInitial(string relatedEntityType) =>
            relatedEntityType switch
            {
                "Vehicle" => "ر",
                "Driver" => "س",
                "License" => "س",
                "Insurance" => "ت",
                "OilChange" => "ز",
                _ => "!"
            };

        private static string GetNoDateText(string relatedEntityType) =>
            string.Equals(relatedEntityType, "OilChange", StringComparison.OrdinalIgnoreCase)
                ? "الإنذار مرتبط بقراءة العداد"
                : "لا يوجد تاريخ انتهاء";

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
        _driverAttendanceService = serviceProvider.GetRequiredService<IDriverAttendanceService>();
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
        DriverAttendanceGrid.ItemsSource = _driverAttendanceWeekRows;
        DailyAttendanceGrid.ItemsSource = _dailyAttendanceRows;
        AbsenceHistoryGrid.ItemsSource = _absenceHistoryRows;
        AbsenceHistoryDriverComboBox.ItemsSource = _drivers;
        AbsenceHistoryFromPicker.SelectedDate = DateTime.Today.AddMonths(-3);
        AbsenceHistoryToPicker.SelectedDate = DateTime.Today;
        DriverReportsGrid.ItemsSource = _driverReportRows;
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
        DriverReportDriverComboBox.ItemsSource = _driverReportDriverOptions;
        DriverAttendanceWeekPicker.SelectedDate = StartOfDriverWeek(DateTime.Today);
        UpdateDriverAttendanceWeekRangeText(StartOfDriverWeek(DateTime.Today));
        DriverReportStartDatePicker.SelectedDate = StartOfDriverWeek(DateTime.Today);
        DriverReportEndDatePicker.SelectedDate = StartOfDriverWeek(DateTime.Today).AddDays(6);
        UpdateReportVehicleFilterVisibility();
        ApplyReadableGridColumnWidths();

        _clockTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _clockTimer.Tick += (_, _) => CurrentDateTimeTextBlock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        _clockTimer.Start();

        _driverAutoSaveTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(900)
        };
        _driverAutoSaveTimer.Tick += async (_, _) =>
        {
            _driverAutoSaveTimer.Stop();
            await RunSafeAsync(SaveSelectedDriverSilentlyAsync);
        };

        _driverAttendanceAutoSaveTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(900)
        };
        _driverAttendanceAutoSaveTimer.Tick += async (_, _) =>
        {
            _driverAttendanceAutoSaveTimer.Stop();
            await RunSafeAsync(SaveDriverAttendanceWeekSilentlyAsync);
        };
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
        var adminShortcutVisibility = string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible
            : Visibility.Collapsed;
        AdminAddUserButton.Visibility = adminShortcutVisibility;
        AdminAddSupervisorButton.Visibility = adminShortcutVisibility;
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
        await LoadDailyAttendanceAsync();
        await LoadDriverAttendanceWeekAsync();
        await LoadEmployeesAsync();
        await LoadTripsAsync();
        await LoadFuelAsync();
        await LoadExpensesAsync();
        await LoadOilChangesAsync();
        await LoadTreasuryIfUnlockedAsync();
        await LoadLicensesAsync();
        await LoadInsuranceAsync();
        await LoadCustodyIfUnlockedAsync();
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
        var alerts = (metrics.Alerts ?? new List<AlertDto>())
            .Where(IsDashboardRenewalAlert)
            .ToList();

        var dashboardAlerts = alerts
            .Select(DashboardAlertItem.FromAlert)
            .OrderBy(alert => alert.SortPriority)
            .ThenBy(alert => alert.SortDistance)
            .ThenBy(alert => alert.DueDate ?? DateTime.MaxValue)
            .ToList();

        _dashboardAlertItems.Clear();
        _dashboardAlertItems.AddRange(dashboardAlerts);

        AlertsListBox.ItemsSource = alerts;
        ApplyDashboardAlertMonthFilter();
        ActivityListBox.ItemsSource = metrics.RecentActivities;
    }

    private void DashboardAlertFilter_Changed(object sender, SelectionChangedEventArgs e) => ApplyDashboardAlertMonthFilter();

    private void ApplyDashboardAlertMonthFilter()
    {
        if (DashboardAlertsListBox is null ||
            DashboardAlertCountTextBlock is null ||
            DashboardAlertSummaryTextBlock is null ||
            DashboardNoAlertsPanel is null)
        {
            return;
        }

        var selectedMonth = GetSelectedDashboardAlertMonth();
        var selectedType = GetSelectedDashboardAlertType();
        var filteredAlerts = _dashboardAlertItems
            .Where(alert => string.IsNullOrWhiteSpace(selectedType) ||
                            string.Equals(alert.CategoryKey, selectedType, StringComparison.OrdinalIgnoreCase))
            .Where(alert => !selectedMonth.HasValue || alert.DueDate?.Month == selectedMonth.Value)
            .ToList();

        DashboardAlertsListBox.ItemsSource = filteredAlerts;
        DashboardAlertCountTextBlock.Text = BuildDashboardAlertCountText(filteredAlerts.Count);
        DashboardAlertSummaryTextBlock.Text = BuildDashboardAlertSummaryText(filteredAlerts);
        DashboardAlertsListBox.Visibility = filteredAlerts.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        DashboardNoAlertsPanel.Visibility = filteredAlerts.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    private int? GetSelectedDashboardAlertMonth()
    {
        if (DashboardAlertMonthComboBox?.SelectedItem is not ComboBoxItem item ||
            !int.TryParse(item.Tag?.ToString(), out var month) ||
            month is < 1 or > 12)
        {
            return null;
        }

        return month;
    }

    private string? GetSelectedDashboardAlertType()
    {
        if (DashboardAlertTypeComboBox?.SelectedItem is not ComboBoxItem item)
        {
            return null;
        }

        var type = item.Tag?.ToString();
        return string.IsNullOrWhiteSpace(type) || string.Equals(type, "All", StringComparison.OrdinalIgnoreCase)
            ? null
            : type;
    }

    private static bool IsDashboardRenewalAlert(AlertDto alert) =>
        string.Equals(alert.RelatedEntityType, "Vehicle", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(alert.RelatedEntityType, "Driver", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(alert.RelatedEntityType, "License", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(alert.RelatedEntityType, "Insurance", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(alert.RelatedEntityType, "OilChange", StringComparison.OrdinalIgnoreCase);

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
            return "كل التراخيص والتأمينات والزيوت المسجلة خارج نطاق الخطر الحالي.";
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

    }
    private async Task LoadContractsAsync() => ReplaceCollection(_contracts, await _contractService.GetAllAsync());
    private async Task LoadMaintenanceAsync() => ReplaceCollection(_maintenance, await _maintenanceService.GetAllAsync());
    private async Task LoadDriversAsync()
    {
        var selectedDriverId = DriverReportDriverComboBox.SelectedValue is int id ? id : 0;
        ReplaceCollection(_drivers, await _driverService.GetAllAsync());
        ApplyDriverFilters();
        _driverReportDriverOptions.Clear();
        _driverReportDriverOptions.Add(new DriverDto { Id = 0, FullName = "كل السائقين" });
        foreach (var driver in _drivers)
        {
            _driverReportDriverOptions.Add(driver);
        }

        DriverReportDriverComboBox.SelectedValue = _driverReportDriverOptions.Any(x => x.Id == selectedDriverId)
            ? selectedDriverId
            : 0;
    }

    private async Task LoadDriverAttendanceWeekAsync()
    {
        _isLoadingDriverAttendanceWeek = true;
        try
        {
            var weekStart = GetSelectedDriverWeekStart();
            UpdateDriverAttendanceWeekRangeText(weekStart);
            var records = await _driverAttendanceService.GetWeekAsync(weekStart);
            var rows = _drivers
                .Where(driver => driver.IsActive)
                .OrderBy(driver => driver.FullName, StringComparer.CurrentCultureIgnoreCase)
                .Select(driver => BuildDriverWeekRow(driver, records, weekStart))
                .ToList();

            ReplaceCollection(_driverAttendanceWeekRows, rows);
            ApplyDriverAttendanceFilters();
        }
        finally
        {
            _isLoadingDriverAttendanceWeek = false;
        }
    }

    private async Task LoadEmployeesAsync() => ReplaceCollection(_employees, await _employeeService.GetAllAsync());

    private DateTime GetSelectedDriverWeekStart() =>
        StartOfDriverWeek(DriverAttendanceWeekPicker.SelectedDate ?? DateTime.Today);

    private async void DriverAttendanceWeekPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isNormalizingDriverAttendanceWeekPicker || DriverAttendanceWeekPicker.SelectedDate is not DateTime selectedDate)
        {
            return;
        }

        await SetDriverAttendanceWeekAsync(selectedDate, IsLoaded);
    }

    private async Task SetDriverAttendanceWeekAsync(DateTime selectedDate, bool reload)
    {
        var weekStart = StartOfDriverWeek(selectedDate);
        if (DriverAttendanceWeekPicker.SelectedDate?.Date != weekStart)
        {
            _isNormalizingDriverAttendanceWeekPicker = true;
            DriverAttendanceWeekPicker.SelectedDate = weekStart;
            DriverAttendanceWeekPicker.DisplayDate = weekStart;
            _isNormalizingDriverAttendanceWeekPicker = false;
        }
        else
        {
            DriverAttendanceWeekPicker.DisplayDate = weekStart;
        }

        UpdateDriverAttendanceWeekRangeText(weekStart);

        if (!reload)
        {
            return;
        }

        _driverAttendanceAutoSaveTimer.Stop();
        await RunSafeAsync(LoadDriverAttendanceWeekAsync);
    }

    private void UpdateDriverAttendanceWeekRangeText(DateTime weekStart)
    {
        DriverAttendanceWeekRangeTextBlock.Text =
            $"الأسبوع: {weekStart:yyyy-MM-dd} إلى {weekStart.AddDays(6):yyyy-MM-dd}";
    }

    private async void PreviousDriverAttendanceWeekButton_Click(object sender, RoutedEventArgs e) =>
        await SetDriverAttendanceWeekAsync(GetSelectedDriverWeekStart().AddDays(-7), true);

    private async void NextDriverAttendanceWeekButton_Click(object sender, RoutedEventArgs e) =>
        await SetDriverAttendanceWeekAsync(GetSelectedDriverWeekStart().AddDays(7), true);

    private void DriverReportStartDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isNormalizingDriverReportRange || DriverReportStartDatePicker.SelectedDate is not DateTime selectedDate)
        {
            return;
        }

        var start = selectedDate.Date;
        if (DriverReportEndDatePicker.SelectedDate is null || DriverReportEndDatePicker.SelectedDate.Value.Date < start)
        {
            _isNormalizingDriverReportRange = true;
            DriverReportEndDatePicker.SelectedDate = start;
            _isNormalizingDriverReportRange = false;
        }
    }

    private void DriverReportEndDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isNormalizingDriverReportRange || DriverReportEndDatePicker.SelectedDate is not DateTime selectedDate)
        {
            return;
        }

        var end = selectedDate.Date;
        if (DriverReportStartDatePicker.SelectedDate is null || DriverReportStartDatePicker.SelectedDate.Value.Date > end)
        {
            _isNormalizingDriverReportRange = true;
            DriverReportStartDatePicker.SelectedDate = end;
            _isNormalizingDriverReportRange = false;
        }
    }

    private static DateTime StartOfDriverWeek(DateTime date)
    {
        var value = date.Date;
        while (value.DayOfWeek != DayOfWeek.Saturday)
        {
            value = value.AddDays(-1);
        }

        return value;
    }

    private static DriverWeeklyAttendanceRow BuildDriverWeekRow(DriverDto driver, IReadOnlyCollection<DriverAttendanceDto> records, DateTime weekStart)
    {
        var row = new DriverWeeklyAttendanceRow
        {
            DriverId = driver.Id,
            DriverName = driver.FullName,
            WorkLocation = driver.WorkLocation
        };

        for (var dayOffset = 0; dayOffset < 7; dayOffset++)
        {
            var date = weekStart.AddDays(dayOffset).Date;
            var record = records.FirstOrDefault(x => x.DriverId == driver.Id && x.WorkDate.Date == date);
            if (record is null)
            {
                continue;
            }

            SetDriverDayValues(row, dayOffset, record.Status);
            if (!string.IsNullOrWhiteSpace(record.WorkLocation))
            {
                row.WorkLocation = record.WorkLocation;
            }
        }

        return row;
    }

    private static void SetDriverDayValues(DriverWeeklyAttendanceRow row, int dayOffset, string status)
    {
        var displayStatus = NormalizeDriverAttendanceStatusForUi(status);

        switch (dayOffset)
        {
            case 0:
                row.SaturdayStatus = displayStatus;
                break;
            case 1:
                row.SundayStatus = displayStatus;
                break;
            case 2:
                row.MondayStatus = displayStatus;
                break;
            case 3:
                row.TuesdayStatus = displayStatus;
                break;
            case 4:
                row.WednesdayStatus = displayStatus;
                break;
            case 5:
                row.ThursdayStatus = displayStatus;
                break;
            case 6:
                row.FridayStatus = displayStatus;
                break;
        }
    }

    private static string NormalizeDriverAttendanceStatusForUi(string status)
    {
        var normalized = DriverAttendanceStatusStorage(status);
        return normalized switch
        {
            "Present" => "حاضر",
            "Absent" => "غائب",
            "Leave" => "إجازة",
            "CompensatoryRest" => "إجازة",
            "Rest" => string.Empty,
            _ => string.IsNullOrWhiteSpace(normalized) ? string.Empty : status
        };
    }

    private static string DriverAttendanceStatusStorage(string status)
    {
        var value = status?.Trim().ToLowerInvariant() ?? string.Empty;
        return value switch
        {
            "present" or "حاضر" or "حضور" => "Present",
            "absent" or "غائب" or "غياب" => "Absent",
            "leave" or "اجازة" or "إجازة" or "أجازة" => "Leave",
            "compensatoryrest" or "compensatory rest" or "راحة مستحقة" or "راحة تعويضية" => "CompensatoryRest",
            "rest" or "راحة" or "راحة جمعة" or "راحة أسبوعية" or "راحة اسبوعية" => "Rest",
            _ => string.IsNullOrWhiteSpace(value) ? "Present" : value
        };
    }

    private static IEnumerable<DriverAttendanceFormDto> BuildDriverAttendanceForms(DriverWeeklyAttendanceRow row, DateTime weekStart)
    {
        var values = new[]
        {
            (Offset: 0, Status: row.SaturdayStatus),
            (Offset: 1, Status: row.SundayStatus),
            (Offset: 2, Status: row.MondayStatus),
            (Offset: 3, Status: row.TuesdayStatus),
            (Offset: 4, Status: row.WednesdayStatus),
            (Offset: 5, Status: row.ThursdayStatus),
            (Offset: 6, Status: row.FridayStatus)
        };

        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value.Status))
            {
                continue;
            }

            yield return new DriverAttendanceFormDto
            {
                DriverId = row.DriverId,
                WorkDate = weekStart.AddDays(value.Offset),
                WorkLocation = row.WorkLocation,
                Status = value.Status,
                AbsenceReason = string.Empty
            };
        }
    }

    private async Task LoadTripsAsync()
    {
        var selectedTripId = Selected<TripDto>(TripsGrid)?.Id ?? 0;
        var trips = await _tripService.GetAllAsync();

        for (var index = 0; index < trips.Count; index++)
        {
            trips[index].Serial = index + 1;
        }

        ReplaceCollection(_trips, trips);

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
    private async Task LoadTreasuryAsync()
    {
        ReplaceCollection(_treasuryTransactions, await _treasuryService.GetAllAsync());
        UpdateTreasurySummary();
    }
    private Task LoadTreasuryIfUnlockedAsync() =>
        IsModuleTemporarilyLocked("Treasury") ? Task.CompletedTask : LoadTreasuryAsync();

    private Task LoadLicensesAsync()
    {
        ReplaceCollection(_vehicleLicenses, _vehicles.Select(VehicleLicenseRow.FromVehicle));
        return Task.CompletedTask;
    }
    private async Task LoadInsuranceAsync() => ReplaceCollection(_insurance, await _insuranceService.GetAllAsync());
    private async Task LoadCustodyAsync()
    {
        ReplaceCollection(_custodies, await _custodyService.GetAllAsync());
        UpdateCustodySummary();
    }
    private Task LoadCustodyIfUnlockedAsync() =>
        IsModuleTemporarilyLocked("Custody") ? Task.CompletedTask : LoadCustodyAsync();

    private void UpdateTreasurySummary()
    {
        var income = _treasuryTransactions
            .Where(t => string.Equals(t.TransactionType, "إيراد", StringComparison.OrdinalIgnoreCase))
            .Sum(t => t.Amount);
        var expense = _treasuryTransactions
            .Where(t => string.Equals(t.TransactionType, "صرف", StringComparison.OrdinalIgnoreCase))
            .Sum(t => t.Amount);
        var balance = income - expense;

        TreasuryTotalIncomeTextBlock.Text = income.ToString("N2");
        TreasuryTotalExpenseTextBlock.Text = expense.ToString("N2");
        TreasuryNetBalanceTextBlock.Text = balance.ToString("N2");
        TreasuryNetBalanceTextBlock.Foreground = balance >= 0
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x18, 0x5A, 0x30))
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xC6, 0x28, 0x28));
    }

    private void UpdateCustodySummary()
    {
        var total = _custodies.Count;
        var pending = _custodies.Count(c => string.Equals(c.Status, "بانتظار الاعتماد", StringComparison.OrdinalIgnoreCase));
        var closed = _custodies.Count(c =>
            string.Equals(c.Status, "مرتجع", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.Status, "مسترجعة", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.Status, "تمت التسوية", StringComparison.OrdinalIgnoreCase));
        var active = total - pending - closed;

        CustodyTotalCountTextBlock.Text = total.ToString();
        CustodyActiveCountTextBlock.Text = active.ToString();
        CustodyPendingCountTextBlock.Text = pending.ToString();
        CustodyReturnedCountTextBlock.Text = closed.ToString();
    }

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
        CompanyHeaderTextBlock.Text = string.IsNullOrWhiteSpace(_settings.CompanyName) ? "شركة جوميكس للحركة والمعدات" : _settings.CompanyName;
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
            MessageBox.Show($"لم يتم تنفيذ العملية:\n{DescribeError(ex)}", "تنبيه واضح", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    private static string DescribeError(Exception ex)
    {
        var messages = new List<string>();
        for (var current = ex; current is not null; current = current.InnerException)
        {
            if (messages.Count == 0 || !string.Equals(messages[^1], current.Message, StringComparison.Ordinal))
            {
                messages.Add(current.Message);
            }
        }

        return string.Join("\n↳ ", messages);
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
        "TripsLicensesOfficer" => "مسؤول التشغيلات والتراخيص",
        "InsuranceOfficer" => "مسؤول التأمينات",
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

            tab.Visibility = allowed.Contains(module) && !IsModuleTemporarilyLocked(module)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        DashboardTripsShortcutButton.Visibility = allowed.Contains("Trips") ? Visibility.Visible : Visibility.Collapsed;
        DashboardLicensesInsuranceShortcutButton.Visibility =
            allowed.Contains("Licenses") || allowed.Contains("Insurance") ? Visibility.Visible : Visibility.Collapsed;
        ApplyDashboardShortcutAccess(DashboardCustodyShortcutButton, allowed, "Custody");
        ApplyDashboardShortcutAccess(DashboardTreasuryShortcutButton, allowed, "Treasury");
        DashboardReportsShortcutButton.Visibility = allowed.Contains("Reports") ? Visibility.Visible : Visibility.Collapsed;
        DashboardDriversShortcutButton.Visibility = allowed.Contains("Drivers") ? Visibility.Visible : Visibility.Collapsed;
        OpenInsuranceFromLicensesButton.Visibility = allowed.Contains("Insurance") ? Visibility.Visible : Visibility.Collapsed;
        BackToLicensesFromInsuranceButton.Visibility = allowed.Contains("Licenses") ? Visibility.Visible : Visibility.Collapsed;

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

    private static bool IsModuleTemporarilyLocked(string? module) =>
        !string.IsNullOrWhiteSpace(module) && TemporarilyLockedModules.Contains(module);

    private static void ApplyDashboardShortcutAccess(Button button, ISet<string> allowedModules, string module)
    {
        if (!allowedModules.Contains(module))
        {
            button.Visibility = Visibility.Collapsed;
            return;
        }

        button.Visibility = Visibility.Visible;
        var isLocked = IsModuleTemporarilyLocked(module);
        button.IsEnabled = !isLocked;
        button.ToolTip = isLocked ? TemporarilyLockedModuleTooltip : null;
        AutomationProperties.SetHelpText(button, isLocked ? TemporarilyLockedModuleTooltip : string.Empty);
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
        yield return DriverAttendanceGrid;
        yield return DriverReportsGrid;
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
        var headerText = GetHeaderText(column.Header);
        if (!string.IsNullOrWhiteSpace(headerText))
        {
            return headerText;
        }

        if (column is DataGridBoundColumn boundColumn &&
            boundColumn.Binding is Binding binding &&
            !string.IsNullOrWhiteSpace(binding.Path?.Path))
        {
            return binding.Path.Path;
        }

        return column.Header?.ToString() ?? string.Empty;
    }

    private static string GetHeaderText(object? header)
    {
        if (header is TextBlock textBlock)
        {
            return textBlock.ToolTip?.ToString() ?? textBlock.Text;
        }

        if (header is Panel panel)
        {
            foreach (var child in panel.Children)
            {
                var value = GetHeaderText(child);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }

        if (header is ContentControl contentControl)
        {
            return GetHeaderText(contentControl.Content);
        }

        return string.Empty;
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

        if (normalizedKey is "السبت" or "الأحد" or "الاثنين" or "الثلاثاء" or "الأربعاء" or "الخميس" or "الجمعة")
        {
            return 230;
        }

        if (normalizedKey.Contains("رقم قومي", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("رقم الرخصة", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("وحدة المرور", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("موقع العمل", StringComparison.OrdinalIgnoreCase))
        {
            return 210;
        }

        if (normalizedKey is "من" or "إلى" ||
            normalizedKey.Contains("الغرض", StringComparison.OrdinalIgnoreCase))
        {
            return ExtraWideGridColumnWidth;
        }

        if (normalizedKey.Contains("بنزين", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("وقود", StringComparison.OrdinalIgnoreCase))
        {
            return CompactGridColumnWidth;
        }

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
                newRow[column] = row.TryGetValue(column, out var value) ? FormatReportCellValue(value) : DBNull.Value;
            }

            table.Rows.Add(newRow);
        }

        return table;
    }

    private static object FormatReportCellValue(object? value) => value switch
    {
        null => DBNull.Value,
        DBNull => DBNull.Value,
        DateTime date => date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
        DateTimeOffset date => date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
        _ => value
    };

    private ReportDataDto BuildDisplayedReportSnapshot(ReportDataDto source)
    {
        if (ReportsGrid.ItemsSource is not DataView view)
        {
            return source;
        }

        var table = view.Table;
        if (table is null)
        {
            return source;
        }

        var columns = table.Columns
            .Cast<DataColumn>()
            .Select(column => column.ColumnName)
            .ToList();

        var rows = view
            .Cast<DataRowView>()
            .Select(row =>
            {
                var values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (var column in columns)
                {
                    values[column] = table.Columns.Contains(column) ? row[column] : DBNull.Value;
                }

                return values;
            })
            .ToList();

        return new ReportDataDto
        {
            ReportTitle = source.ReportTitle,
            GeneratedDate = source.GeneratedDate,
            GeneratedBy = source.GeneratedBy,
            Columns = columns,
            Data = rows
        };
    }

    public sealed class DriverWeeklyAttendanceRow
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string WorkLocation { get; set; } = string.Empty;
        public string SaturdayStatus { get; set; } = string.Empty;
        public string SundayStatus { get; set; } = string.Empty;
        public string MondayStatus { get; set; } = string.Empty;
        public string TuesdayStatus { get; set; } = string.Empty;
        public string WednesdayStatus { get; set; } = string.Empty;
        public string ThursdayStatus { get; set; } = string.Empty;
        public string FridayStatus { get; set; } = string.Empty;
    }

    public sealed class DailyAttendanceRow : System.ComponentModel.INotifyPropertyChanged
    {
        private string _status = "حاضر";
        private string _absenceReason = string.Empty;
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string WorkLocation { get; set; } = string.Empty;
        public string Status
        {
            get => _status;
            set
            {
                if (_status == value) return;
                _status = value;
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Status)));
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsAbsent)));
            }
        }
        public string AbsenceReason
        {
            get => _absenceReason;
            set
            {
                if (_absenceReason == value) return;
                _absenceReason = value;
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(AbsenceReason)));
            }
        }
        public bool IsAbsent => string.Equals(_status, "غائب", StringComparison.OrdinalIgnoreCase);
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }

    private static ReportDataDto BuildSelectedReportRowSnapshot(ReportDataDto source, DataRowView selectedRow)
    {
        var columns = selectedRow.Row.Table.Columns
            .Cast<DataColumn>()
            .Select(column => column.ColumnName)
            .ToList();

        var values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        foreach (var column in columns)
        {
            values[column] = selectedRow.Row.Table.Columns.Contains(column) ? selectedRow[column] : DBNull.Value;
        }

        return new ReportDataDto
        {
            ReportTitle = $"{source.ReportTitle} - سجل محدد",
            GeneratedDate = source.GeneratedDate,
            GeneratedBy = source.GeneratedBy,
            Columns = columns,
            Data = new List<Dictionary<string, object>> { values }
        };
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
        var pageWidth = Math.Max(980, report.Columns.Sum(GetReportColumnPrintWidth) + 72);
        var document = new FlowDocument
        {
            FlowDirection = FlowDirection.RightToLeft,
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = report.Columns.Count > 10 ? 9.5 : 11,
            PagePadding = new Thickness(36),
            PageWidth = pageWidth,
            MinPageWidth = pageWidth,
            MaxPageWidth = pageWidth,
            ColumnWidth = pageWidth
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
        foreach (var column in report.Columns)
        {
            table.Columns.Add(new TableColumn { Width = new GridLength(GetReportColumnPrintWidth(column)) });
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

    private static double GetReportColumnPrintWidth(string column)
    {
        if (string.IsNullOrWhiteSpace(column))
        {
            return 125;
        }

        var normalized = column.Trim();
        if (normalized is "سيريال" or "رقم" or "Id")
        {
            return 80;
        }

        if (normalized.Contains("تاريخ", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("بداية", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("نهاية", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("Expiry", StringComparison.OrdinalIgnoreCase))
        {
            return 140;
        }

        if (normalized.Contains("الأسباب", StringComparison.OrdinalIgnoreCase))
        {
            return 280;
        }

        if (normalized.Contains("الغرض", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("ملاحظات", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("تفاصيل", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("الوصف", StringComparison.OrdinalIgnoreCase))
        {
            return 220;
        }

        if (normalized.Contains("السائق", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("الموصي", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("المشرف", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("رقم السيارة", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("محطة", StringComparison.OrdinalIgnoreCase))
        {
            return 155;
        }

        if (normalized is "من" or "إلى" ||
            normalized.Contains("منطقة", StringComparison.OrdinalIgnoreCase))
        {
            return 165;
        }

        if (normalized.Contains("وقود", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("تكلفة", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("المسافة", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("العداد", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("الحالة", StringComparison.OrdinalIgnoreCase))
        {
            return 125;
        }

        return 135;
    }

    private static TableCell CreateReportCell(string value, bool isHeader)
    {
        var paragraph = new Paragraph
        {
            Margin = new Thickness(0),
            TextAlignment = TextAlignment.Right
        };

        var lines = (value ?? string.Empty).Replace("\r\n", "\n").Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            if (index > 0)
            {
                paragraph.Inlines.Add(new LineBreak());
            }

            paragraph.Inlines.Add(new Run(lines[index]));
        }

        return new TableCell(paragraph)
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

    private static string FormatDateOnly(DateTime? date) =>
        date.HasValue ? date.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty;

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

    private IReadOnlyList<VehicleLicenseRow> GetVisibleLicenseRows()
    {
        var view = CollectionViewSource.GetDefaultView(LicensesGrid.ItemsSource);
        return view is null
            ? _vehicleLicenses.ToList()
            : view.Cast<object>().OfType<VehicleLicenseRow>().ToList();
    }

    private IReadOnlyList<InsuranceDto> GetVisibleInsuranceRows()
    {
        var view = CollectionViewSource.GetDefaultView(InsuranceGrid.ItemsSource);
        return view is null
            ? _insurance.ToList()
            : view.Cast<object>().OfType<InsuranceDto>().ToList();
    }

    private IReadOnlyList<FuelTransactionDto> GetVisibleFuelRows()
    {
        var view = CollectionViewSource.GetDefaultView(FuelGrid.ItemsSource);
        return view is null
            ? _fuel.ToList()
            : view.Cast<object>().OfType<FuelTransactionDto>().ToList();
    }

    private ReportDataDto BuildVehicleLicensesReportSnapshot(IEnumerable<VehicleLicenseRow> licenses, string title)
    {
        var rows = licenses.ToList();
        return new ReportDataDto
        {
            ReportTitle = title,
            GeneratedDate = DateTime.UtcNow,
            GeneratedBy = _currentUser?.Username ?? "System",
            Columns = new List<string>
            {
                "رقم السيارة",
                "الموديل",
                "سنة الصنع",
                "رقم الشاسيه",
                "رقم الموتور",
                "تغيير الزيت كل كام كم",
                "بداية الترخيص",
                "نهاية الترخيص",
                "الحالة",
                "الإنذار"
            },
            Data = rows.Select(license => new Dictionary<string, object>
            {
                ["رقم السيارة"] = license.PlateNumber,
                ["الموديل"] = license.Model,
                ["سنة الصنع"] = license.Year,
                ["رقم الشاسيه"] = license.ChassisNumber,
                ["رقم الموتور"] = license.EngineNumber,
                ["تغيير الزيت كل كام كم"] = license.OilChangeIntervalKm,
                ["بداية الترخيص"] = FormatDateOnly(license.RegistrationStartDate),
                ["نهاية الترخيص"] = FormatDateOnly(license.RegistrationExpiryDate),
                ["الحالة"] = license.Status,
                ["الإنذار"] = license.ExpiryAlert
            }).ToList()
        };
    }

    private ReportDataDto BuildInsuranceReportSnapshot(IEnumerable<InsuranceDto> insuranceItems, string title)
    {
        var rows = insuranceItems.ToList();
        return new ReportDataDto
        {
            ReportTitle = title,
            GeneratedDate = DateTime.UtcNow,
            GeneratedBy = _currentUser?.Username ?? "System",
            Columns = new List<string>
            {
                "رقم السيارة",
                "الموديل",
                "سنة الصنع",
                "رقم الشاسيه",
                "رقم الموتور",
                "رقم الوثيقة",
                "شركة التأمين",
                "نوع الوثيقة",
                "بداية التأمين",
                "نهاية التأمين",
                "القسط",
                "مبلغ التغطية",
                "تفاصيل التغطية",
                "مندوب التأمين",
                "هاتف المندوب",
                "الحالة",
                "الإنذار",
                "ملاحظات"
            },
            Data = rows.Select(insurance => new Dictionary<string, object>
            {
                ["رقم السيارة"] = insurance.VehiclePlateNumber,
                ["الموديل"] = insurance.VehicleModel,
                ["سنة الصنع"] = insurance.VehicleYear,
                ["رقم الشاسيه"] = insurance.VehicleChassisNumber,
                ["رقم الموتور"] = insurance.VehicleEngineNumber,
                ["رقم الوثيقة"] = insurance.PolicyNumber,
                ["شركة التأمين"] = insurance.InsuranceCompany,
                ["نوع الوثيقة"] = insurance.PolicyType,
                ["بداية التأمين"] = FormatDateOnly(insurance.StartDate),
                ["نهاية التأمين"] = FormatDateOnly(insurance.ExpiryDate),
                ["القسط"] = insurance.PremiumAmount,
                ["مبلغ التغطية"] = insurance.CoverageAmount,
                ["تفاصيل التغطية"] = insurance.CoverageDetails,
                ["مندوب التأمين"] = insurance.AgentName,
                ["هاتف المندوب"] = insurance.AgentPhoneNumber,
                ["الحالة"] = insurance.Status,
                ["الإنذار"] = insurance.ExpiryAlert,
                ["ملاحظات"] = insurance.Notes
            }).ToList()
        };
    }

    private ReportDataDto BuildVehiclesReportSnapshot(IEnumerable<VehicleDto> vehicles)
    {
        var rows = vehicles.ToList();
        var latestInsuranceByVehicle = _insurance
            .GroupBy(insurance => insurance.VehicleId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(insurance => insurance.ExpiryDate)
                    .ThenByDescending(insurance => insurance.Id)
                    .First());

        return new ReportDataDto
        {
            ReportTitle = "تقرير المركبات",
            GeneratedDate = DateTime.UtcNow,
            GeneratedBy = _currentUser?.Username ?? "System",
            Columns = new List<string>
            {
                "رقم السيارة",
                "الموديل",
                "سنة الصنع",
                "رقم الشاسيه",
                "رقم الموتور",
                "العداد الحالي",
                "الحالة",
                "بداية الترخيص",
                "نهاية الترخيص",
                "شركة التأمين",
                "رقم وثيقة التأمين",
                "نهاية التأمين",
                "مبلغ التغطية",
                "تغيير الزيت كل كام كم",
                "ملاحظات"
            },
            Data = rows.Select(vehicle =>
            {
                latestInsuranceByVehicle.TryGetValue(vehicle.Id, out var insurance);
                return new Dictionary<string, object>
                {
                    ["رقم السيارة"] = vehicle.PlateNumber,
                    ["الموديل"] = vehicle.Model,
                    ["سنة الصنع"] = vehicle.Year,
                    ["رقم الشاسيه"] = vehicle.ChassisNumber,
                    ["رقم الموتور"] = vehicle.EngineNumber,
                    ["العداد الحالي"] = vehicle.CurrentMileage,
                    ["الحالة"] = vehicle.Status,
                    ["بداية الترخيص"] = FormatDateOnly(vehicle.RegistrationStartDate),
                    ["نهاية الترخيص"] = FormatDateOnly(vehicle.RegistrationExpiryDate),
                    ["شركة التأمين"] = insurance?.InsuranceCompany ?? string.Empty,
                    ["رقم وثيقة التأمين"] = insurance?.PolicyNumber ?? string.Empty,
                    ["نهاية التأمين"] = FormatDateOnly(insurance?.ExpiryDate),
                    ["مبلغ التغطية"] = insurance?.CoverageAmount ?? 0,
                    ["تغيير الزيت كل كام كم"] = vehicle.OilChangeIntervalKm,
                    ["ملاحظات"] = vehicle.Notes
                };
            }).ToList()
        };
    }

    private ReportDataDto BuildFuelReportSnapshot(IEnumerable<FuelTransactionDto> fuelItems, string? title = null)
    {
        var rows = fuelItems.ToList();
        return new ReportDataDto
        {
            ReportTitle = string.IsNullOrWhiteSpace(title) ? "تقرير البنزين" : title,
            GeneratedDate = DateTime.UtcNow,
            GeneratedBy = _currentUser?.Username ?? "System",
            Columns = new List<string>
            {
                "مسلسل",
                "رقم العربية",
                "رقم التشغيلة",
                "التاريخ",
                "نوع الوقود",
                "عدد اللترات",
                "سعر اللتر",
                "إجمالي البنزين",
                "محطة البنزين",
                "عداد التموين",
                "من الخزينة",
                "ملاحظات"
            },
            Data = rows.Select((fuel, index) => new Dictionary<string, object>
            {
                ["مسلسل"] = index + 1,
                ["رقم العربية"] = fuel.VehiclePlateNumber,
                ["رقم التشغيلة"] = fuel.TripId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                ["التاريخ"] = fuel.TransactionDate,
                ["نوع الوقود"] = fuel.FuelType,
                ["عدد اللترات"] = fuel.Quantity,
                ["سعر اللتر"] = fuel.UnitPrice,
                ["إجمالي البنزين"] = fuel.TotalCost,
                ["محطة البنزين"] = fuel.FuelStation,
                ["عداد التموين"] = fuel.Odometer,
                ["من الخزينة"] = fuel.PaidFromTreasury ? "نعم" : "لا",
                ["ملاحظات"] = fuel.Notes
            }).ToList()
        };
    }

    private ReportDataDto BuildContractsReportSnapshot(IEnumerable<ContractDto> contracts)
    {
        var rows = contracts.ToList();
        return new ReportDataDto
        {
            ReportTitle = "تقرير العقود",
            GeneratedDate = DateTime.UtcNow,
            GeneratedBy = _currentUser?.Username ?? "System",
            Columns = new List<string>
            {
                "رقم العقد",
                "رقم السيارة",
                "العميل",
                "بداية العقد",
                "نهاية العقد",
                "قيمة العقد",
                "المدفوع",
                "الحالة",
                "ملاحظات"
            },
            Data = rows.Select(contract => new Dictionary<string, object>
            {
                ["رقم العقد"] = contract.ContractNumber,
                ["رقم السيارة"] = contract.VehiclePlateNumber,
                ["العميل"] = contract.ClientName,
                ["بداية العقد"] = contract.StartDate,
                ["نهاية العقد"] = contract.EndDate,
                ["قيمة العقد"] = contract.ContractValue,
                ["المدفوع"] = contract.PaidAmount,
                ["الحالة"] = contract.Status,
                ["ملاحظات"] = contract.Notes
            }).ToList()
        };
    }

    private ReportDataDto BuildMaintenanceReportSnapshot(IEnumerable<MaintenanceRequestDto> maintenanceItems)
    {
        var rows = maintenanceItems.ToList();
        return new ReportDataDto
        {
            ReportTitle = "تقرير الصيانة",
            GeneratedDate = DateTime.UtcNow,
            GeneratedBy = _currentUser?.Username ?? "System",
            Columns = new List<string>
            {
                "رقم العربية",
                "نوع الصيانة",
                "مركز الخدمة",
                "تاريخ الطلب",
                "تاريخ الإكمال",
                "الحالة",
                "التكلفة المتوقعة",
                "التكلفة الفعلية",
                "الوصف",
                "الأعمال المنفذة",
                "ملاحظات"
            },
            Data = rows.Select(maintenance => new Dictionary<string, object>
            {
                ["رقم العربية"] = maintenance.VehiclePlateNumber,
                ["نوع الصيانة"] = maintenance.MaintenanceType,
                ["مركز الخدمة"] = maintenance.ServiceProvider,
                ["تاريخ الطلب"] = maintenance.RequestDate,
                ["تاريخ الإكمال"] = maintenance.CompletionDate ?? (object)string.Empty,
                ["الحالة"] = maintenance.Status,
                ["التكلفة المتوقعة"] = maintenance.EstimatedCost,
                ["التكلفة الفعلية"] = maintenance.ActualCost,
                ["الوصف"] = maintenance.Description,
                ["الأعمال المنفذة"] = maintenance.WorkPerformed,
                ["ملاحظات"] = maintenance.Notes
            }).ToList()
        };
    }

    private ReportDataDto BuildOilChangesReportSnapshot(IEnumerable<OilChangeDto> oilChanges)
    {
        var rows = oilChanges.ToList();
        var vehicles = _vehicles
            .OrderBy(vehicle => vehicle.PlateNumber, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return new ReportDataDto
        {
            ReportTitle = "تقرير الزيوت",
            GeneratedDate = DateTime.UtcNow,
            GeneratedBy = _currentUser?.Username ?? "System",
            Columns = new List<string>
            {
                "رقم العربية",
                "آخر عداد غيار زيت",
                "عداد اليوم",
                "المقطوع من آخر غيار",
                "تغيير الزيت كل كام كم",
                "التغيير القادم",
                "حالة الإنذار"
            },
            Data = vehicles.Select(vehicle =>
            {
                var vehicleOilRows = rows
                    .Where(oil => oil.VehicleId == vehicle.Id)
                    .ToList();
                var latestOilChange = vehicleOilRows
                    .Where(oil => oil.IsOilChanged)
                    .OrderByDescending(oil => oil.OdometerAtChange)
                    .ThenByDescending(oil => oil.ChangeDate)
                    .ThenByDescending(oil => oil.Id)
                    .FirstOrDefault();
                var latestOdometerReading = vehicleOilRows
                    .Where(oil => oil.CurrentOdometer > 0)
                    .OrderByDescending(oil => oil.CurrentOdometerDate ?? oil.ChangeDate.Date)
                    .ThenByDescending(oil => oil.ChangeDate)
                    .ThenByDescending(oil => oil.Id)
                    .FirstOrDefault();

                var currentOdometer = latestOdometerReading?.CurrentOdometer > 0
                    ? latestOdometerReading.CurrentOdometer
                    : vehicle.CurrentMileage;
                var lastOilOdometer = latestOilChange?.OdometerAtChange;
                var kmSinceOilChange = lastOilOdometer.HasValue
                    ? Math.Max(0, currentOdometer - lastOilOdometer.Value)
                    : (decimal?)null;
                var nextOilChangeOdometer = vehicle.OilChangeIntervalKm <= 0
                    ? (decimal?)null
                    : (lastOilOdometer ?? 0) + vehicle.OilChangeIntervalKm;
                var alert = BuildVehicleOilSummaryAlert(vehicle, lastOilOdometer, currentOdometer, kmSinceOilChange);

                return new Dictionary<string, object>
                {
                    ["رقم العربية"] = vehicle.PlateNumber,
                    ["آخر عداد غيار زيت"] = lastOilOdometer.HasValue ? lastOilOdometer.Value : string.Empty,
                    ["عداد اليوم"] = currentOdometer,
                    ["المقطوع من آخر غيار"] = kmSinceOilChange.HasValue ? kmSinceOilChange.Value : string.Empty,
                    ["تغيير الزيت كل كام كم"] = vehicle.OilChangeIntervalKm,
                    ["التغيير القادم"] = nextOilChangeOdometer.HasValue ? nextOilChangeOdometer.Value : string.Empty,
                    ["حالة الإنذار"] = alert
                };
            }).ToList()
        };
    }

    private static string BuildVehicleOilSummaryAlert(VehicleDto vehicle, decimal? lastOilOdometer, decimal currentOdometer, decimal? kmSinceOilChange)
    {
        if (vehicle.OilChangeIntervalKm <= 0)
        {
            return "تغيير الزيت كل كام كم غير مسجلة";
        }

        if (!lastOilOdometer.HasValue)
        {
            return "لا يوجد غيار زيت مسجل";
        }

        if (currentOdometer < lastOilOdometer.Value)
        {
            return "قراءة عداد اليوم أقل من آخر غيار زيت";
        }

        var remainingKm = vehicle.OilChangeIntervalKm - (kmSinceOilChange ?? 0);
        return BuildOilAlertText(remainingKm);
    }

    private ReportDataDto BuildTreasuryReportSnapshot(IEnumerable<TreasuryTransactionDto> treasuryTransactions)
    {
        var rows = treasuryTransactions.ToList();
        return new ReportDataDto
        {
            ReportTitle = "تقرير الخزينة",
            GeneratedDate = DateTime.UtcNow,
            GeneratedBy = _currentUser?.Username ?? "System",
            Columns = new List<string>
            {
                "رقم",
                "التاريخ",
                "نوع الحركة",
                "المبلغ",
                "الوصف",
                "مرتبط بـ",
                "رقم المرتبط",
                "طريقة الدفع",
                "ملاحظات"
            },
            Data = rows.Select(transaction => new Dictionary<string, object>
            {
                ["رقم"] = transaction.Id,
                ["التاريخ"] = transaction.TransactionDate,
                ["نوع الحركة"] = transaction.TransactionType,
                ["المبلغ"] = transaction.Amount,
                ["الوصف"] = transaction.Description,
                ["مرتبط بـ"] = transaction.RelatedEntityType,
                ["رقم المرتبط"] = transaction.RelatedEntityId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                ["طريقة الدفع"] = transaction.PaymentMethod,
                ["ملاحظات"] = transaction.Notes
            }).ToList()
        };
    }

    private ReportDataDto BuildDriverReportSnapshot(IEnumerable<DriverReportDto> driverReports)
    {
        var rows = driverReports.ToList();
        return new ReportDataDto
        {
            ReportTitle = "تقرير السائقين",
            GeneratedDate = DateTime.UtcNow,
            GeneratedBy = _currentUser?.Username ?? "System",
            Columns = new List<string>
            {
                "اسم السائق",
                "رقم قومي",
                "رقم الرخصة",
                "نوع الرخصة",
                "بداية الرخصة",
                "نهاية الرخصة",
                "متأمن عليه",
                "المحافظة",
                "العنوان بالكامل",
                "وحدة المرور",
                "موقع العمل",
                "أيام الحضور",
                "أيام الغياب",
                "أيام الإجازة",
                "جمعات عمل",
                "راحات مستحقة",
                "رصيد الراحة"
            },
            Data = rows.Select(row => new Dictionary<string, object>
            {
                ["اسم السائق"] = row.FullName,
                ["رقم قومي"] = row.NationalId,
                ["رقم الرخصة"] = row.LicenseNumber,
                ["نوع الرخصة"] = row.LicenseType,
                ["بداية الرخصة"] = row.LicenseStartDate,
                ["نهاية الرخصة"] = row.LicenseExpiryDate,
                ["متأمن عليه"] = row.IsCompanyInsured ? "نعم" : "لا",
                ["المحافظة"] = row.Governorate,
                ["العنوان بالكامل"] = row.FullAddress,
                ["وحدة المرور"] = row.TrafficUnit,
                ["موقع العمل"] = row.WorkLocation,
                ["أيام الحضور"] = row.PresentDays,
                ["أيام الغياب"] = row.AbsentDays,
                ["أيام الإجازة"] = row.LeaveDays,
                ["جمعات عمل"] = row.WorkedFridays,
                ["راحات مستحقة"] = row.EarnedRestDays,
                ["رصيد الراحة"] = row.RemainingRestDays
            }).ToList()
        };
    }

    private ReportDataDto BuildDriversReportSnapshot(IEnumerable<DriverDto> drivers)
    {
        var rows = drivers.ToList();
        return new ReportDataDto
        {
            ReportTitle = "تقرير بيانات السائقين",
            GeneratedDate = DateTime.UtcNow,
            GeneratedBy = _currentUser?.Username ?? "System",
            Columns = new List<string>
            {
                "مسلسل",
                "اسم السائق",
                "رقم قومي",
                "رقم الرخصة",
                "نوع الرخصة",
                "بداية الرخصة",
                "نهاية الرخصة",
                "متأمن عليه",
                "المحافظة",
                "العنوان بالكامل",
                "وحدة المرور",
                "موقع العمل",
                "مفعل",
                "إنذار الرخصة",
                "ملاحظات"
            },
            Data = rows.Select((driver, index) => new Dictionary<string, object>
            {
                ["مسلسل"] = index + 1,
                ["اسم السائق"] = driver.FullName,
                ["رقم قومي"] = driver.NationalId,
                ["رقم الرخصة"] = driver.LicenseNumber,
                ["نوع الرخصة"] = driver.LicenseType,
                ["بداية الرخصة"] = driver.LicenseStartDate,
                ["نهاية الرخصة"] = driver.LicenseExpiryDate,
                ["متأمن عليه"] = driver.IsCompanyInsured ? "نعم" : "لا",
                ["المحافظة"] = driver.Governorate,
                ["العنوان بالكامل"] = driver.FullAddress,
                ["وحدة المرور"] = driver.TrafficUnit,
                ["موقع العمل"] = driver.WorkLocation,
                ["مفعل"] = driver.IsActive ? "نعم" : "لا",
                ["إنذار الرخصة"] = driver.LicenseExpiryAlert,
                ["ملاحظات"] = driver.Notes
            }).ToList()
        };
    }

    private ReportDataDto BuildDriverAttendanceReportSnapshot(IEnumerable<DriverWeeklyAttendanceRow> attendanceRows, DateTime weekStart)
    {
        var rows = attendanceRows.ToList();
        return new ReportDataDto
        {
            ReportTitle = $"تقرير حضور وغياب السائقين - أسبوع يبدأ {weekStart:yyyy-MM-dd}",
            GeneratedDate = DateTime.UtcNow,
            GeneratedBy = _currentUser?.Username ?? "System",
            Columns = new List<string>
            {
                "مسلسل",
                "اسم السائق",
                "موقع العمل",
                "السبت",
                "الأحد",
                "الاثنين",
                "الثلاثاء",
                "الأربعاء",
                "الخميس",
                "الجمعة"
            },
            Data = rows.Select((row, index) => new Dictionary<string, object>
            {
                ["مسلسل"] = index + 1,
                ["اسم السائق"] = row.DriverName,
                ["موقع العمل"] = row.WorkLocation,
                ["السبت"] = row.SaturdayStatus,
                ["الأحد"] = row.SundayStatus,
                ["الاثنين"] = row.MondayStatus,
                ["الثلاثاء"] = row.TuesdayStatus,
                ["الأربعاء"] = row.WednesdayStatus,
                ["الخميس"] = row.ThursdayStatus,
                ["الجمعة"] = row.FridayStatus
            }).ToList()
        };
    }

    private static bool ReportDateMatches(DateTime date, ReportFilterDto filter) =>
        ReportDateMatches((DateTime?)date, filter);

    private static bool ReportDateMatches(DateTime? date, ReportFilterDto filter)
    {
        if (!filter.StartDate.HasValue && !filter.EndDate.HasValue)
        {
            return true;
        }

        if (!date.HasValue)
        {
            return false;
        }

        var value = date.Value.Date;
        return (!filter.StartDate.HasValue || value >= filter.StartDate.Value.Date) &&
               (!filter.EndDate.HasValue || value <= filter.EndDate.Value.Date);
    }

    private static bool ReportAnyDateMatches(DateTime? firstDate, DateTime? secondDate, ReportFilterDto filter)
    {
        if (!filter.StartDate.HasValue && !filter.EndDate.HasValue)
        {
            return true;
        }

        return ReportDateMatches(firstDate, filter) || ReportDateMatches(secondDate, filter);
    }

    private ReportDataDto BuildTripsReportSnapshot(IReadOnlyList<TripDto> trips, string title) =>
        new()
        {
            ReportTitle = title,
            GeneratedDate = DateTime.UtcNow,
            GeneratedBy = _currentUser?.Username ?? "System",
            Columns = new List<string>
            {
                "سيريال",
                "التاريخ",
                "النهاية",
                "رقم السيارة",
                "من",
                "إلى",
                "السائق",
                "الموصي",
                "المشرف",
                "الغرض",
                "الحالة",
                "المسافة",
                "ملاحظات"
            },
            Data = trips.Select((trip, index) => new Dictionary<string, object>
            {
                ["سيريال"] = trip.Serial > 0 ? trip.Serial : index + 1,
                ["التاريخ"] = trip.StartDate,
                ["النهاية"] = trip.EndDate ?? (object)string.Empty,
                ["رقم السيارة"] = trip.VehiclePlateNumber,
                ["من"] = trip.StartLocation,
                ["إلى"] = trip.EndLocation,
                ["السائق"] = trip.DriverName,
                ["الموصي"] = trip.RequesterName,
                ["المشرف"] = trip.SupervisorName,
                ["الغرض"] = trip.Purpose,
                ["الحالة"] = trip.Status,
                ["المسافة"] = trip.Distance,
                ["ملاحظات"] = trip.Notes
            }).ToList()
        };

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
            Year = licenseRow.Year > 0 ? licenseRow.Year : existingVehicle?.Year > 0 ? existingVehicle.Year : DateTime.Today.Year,
            Manufacturer = existingVehicle?.Manufacturer ?? string.Empty,
            Color = existingVehicle?.Color ?? string.Empty,
            ChassisNumber = string.IsNullOrWhiteSpace(licenseRow.ChassisNumber) ? existingVehicle?.ChassisNumber ?? string.Empty : licenseRow.ChassisNumber.Trim(),
            EngineNumber = string.IsNullOrWhiteSpace(licenseRow.EngineNumber) ? existingVehicle?.EngineNumber ?? string.Empty : licenseRow.EngineNumber.Trim(),
            CurrentMileage = existingVehicle?.CurrentMileage ?? 0,
            Status = string.IsNullOrWhiteSpace(existingVehicle?.Status) ? "Available" : existingVehicle.Status,
            AssignedTo = existingVehicle?.AssignedTo ?? string.Empty,
            PurchaseDate = existingVehicle is null || existingVehicle.PurchaseDate == default ? DateTime.Today : existingVehicle.PurchaseDate,
            PurchasePrice = existingVehicle?.PurchasePrice ?? 0,
            RegistrationType = "ترخيص",
            RegistrationStartDate = licenseRow.RegistrationStartDate,
            RegistrationExpiryDate = licenseRow.RegistrationExpiryDate,
            AccidentInsuranceDetails = existingVehicle?.AccidentInsuranceDetails ?? string.Empty,
            SocialInsuranceDetails = existingVehicle?.SocialInsuranceDetails ?? string.Empty,
            OilChangeIntervalKm = licenseRow.OilChangeIntervalKm > 0
                ? licenseRow.OilChangeIntervalKm
                : existingVehicle?.OilChangeIntervalKm > 0 ? existingVehicle.OilChangeIntervalKm : 10000,
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
        RegistrationType = "ترخيص",
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
        LicenseStartDate = dto.LicenseStartDate,
        LicenseExpiryDate = dto.LicenseExpiryDate,
        LicenseType = dto.LicenseType,
        IsCompanyInsured = dto.IsCompanyInsured,
        Governorate = dto.Governorate,
        FullAddress = dto.FullAddress,
        TrafficUnit = dto.TrafficUnit,
        WorkLocation = dto.WorkLocation,
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
        FuelConsumed = 0,
        TripCost = 0,
        Notes = dto.Notes
    };

    private FuelTransactionFormDto ToForm(FuelTransactionDto dto) => new()
    {
        Id = dto.Id,
        VehicleId = dto.VehicleId,
        TripId = dto.TripId,
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
        CurrentOdometer = dto.CurrentOdometer,
        CurrentOdometerDate = dto.CurrentOdometerDate,
        IsOilChanged = dto.IsOilChanged,
        ServiceItems = dto.ServiceItems,
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
        DriverId = dto.DriverId,
        EmployeeId = dto.EmployeeId,
        CustodyNumber = dto.CustodyNumber,
        Amount = dto.Amount,
        HandoverDate = dto.HandoverDate,
        ReturnDate = dto.ReturnDate,
        Status = dto.Status,
        PaidFromTreasury = dto.PaidFromTreasury,
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

        if (string.Equals(propertyName, "TripId", StringComparison.OrdinalIgnoreCase))
        {
            column = CreateLookupColumn(propertyName, "التشغيلة", _trips, "TripSummary", "Id");
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

        if (ReferenceEquals(sender, ReportsGrid))
        {
            e.Column.Header = CreateReportColumnHeader(e.PropertyName);
            e.Column.Width = new DataGridLength(DefaultGridColumnWidth);
            e.Column.MinWidth = CompactGridColumnWidth;
            return;
        }

        if (ReferenceEquals(sender, UsersGrid) && e.PropertyName.Equals("Role", StringComparison.OrdinalIgnoreCase))
        {
            e.Column = new DataGridComboBoxColumn
            {
                Header = ColumnHeaders.TryGetValue(e.PropertyName, out var roleHeader) ? roleHeader : "الدور",
                ItemsSource = _userRoleOptions,
                DisplayMemberPath = nameof(UserRoleOption.DisplayName),
                SelectedValuePath = nameof(UserRoleOption.Value),
                SelectedValueBinding = new Binding(e.PropertyName)
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                },
                Width = new DataGridLength(WideGridColumnWidth)
            };
            return;
        }

        if (ReferenceEquals(sender, UsersGrid) && e.PropertyName.Equals("EmployeeId", StringComparison.OrdinalIgnoreCase))
        {
            e.Column = CreateLookupColumn(e.PropertyName, "الموظف (أمين عهدة)", _employees, "FullName", "Id");
            ApplyReadableColumnWidth(e.Column, e.PropertyName);
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

        if (IsCalculatedColumn(e.PropertyName))
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

    private static bool IsCalculatedColumn(string propertyName) =>
        propertyName.Equals("NextOilChangeOdometer", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Equals("CurrentVehicleMileage", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Equals("RemainingKm", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Equals("OilAlert", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Equals("IsDue", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Equals("TreasuryTransactionId", StringComparison.OrdinalIgnoreCase);

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
        if (IsModuleTemporarilyLocked(tag))
        {
            return;
        }

        var tab = MainTabs.Items
            .OfType<TabItem>()
            .FirstOrDefault(item => string.Equals(item.Tag?.ToString(), tag, StringComparison.OrdinalIgnoreCase) && item.Visibility == Visibility.Visible);

        if (tab is not null)
        {
            MainTabs.SelectedItem = tab;
            UpdateShellForSelectedTab();
        }
    }

    private bool CanAccessModule(string module) =>
        !IsModuleTemporarilyLocked(module) &&
        _currentUser?.AllowedModules.Any(allowed => string.Equals(allowed, module, StringComparison.OrdinalIgnoreCase)) == true;

    private void QuickOpenTripsButton_Click(object sender, RoutedEventArgs e) => SelectTabByTag("Trips");
    private void QuickOpenDriversButton_Click(object sender, RoutedEventArgs e) => SelectTabByTag("Drivers");
    private void QuickOpenMaintenanceButton_Click(object sender, RoutedEventArgs e) => SelectTabByTag("Maintenance");
    private void QuickOpenLicensesAndInsuranceButton_Click(object sender, RoutedEventArgs e)
    {
        if (CanAccessModule("Licenses"))
        {
            SelectTabByTag("Licenses");
            return;
        }

        if (CanAccessModule("Insurance"))
        {
            SelectTabByTag("Insurance");
        }
    }
    private void QuickOpenCustodyButton_Click(object sender, RoutedEventArgs e) => SelectTabByTag("Custody");
    private void QuickOpenTreasuryButton_Click(object sender, RoutedEventArgs e) => SelectTabByTag("Treasury");
    private void QuickOpenReportsButton_Click(object sender, RoutedEventArgs e) => SelectTabByTag("Reports");
    private void BackToTripsButton_Click(object sender, RoutedEventArgs e) => SelectTabByTag("Trips");
    private void BackToLicensesButton_Click(object sender, RoutedEventArgs e) => SelectTabByTag("Licenses");

    private void DashboardAddUserButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.Equals(_currentUser?.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        SelectTabByTag("Users");
        AddUserButton_Click(sender, e);
    }

    private async void DashboardAddDriverButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.Equals(_currentUser?.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var window = new QuickNameEntryWindow(
            "إضافة سائق",
            "اكتب اسم السائق فقط. باقي البيانات الفنية يتم تجهيزها تلقائيًا حتى يظهر في التشغيلات.")
        {
            Owner = this
        };

        if (window.ShowDialog() != true)
        {
            return;
        }

        await RunSafeAsync(async () =>
        {
            var stamp = DateTime.Now.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
            await _driverService.SaveAsync(new DriverFormDto
            {
                FullName = window.EnteredName,
                LicenseNumber = $"AUTO-DRV-{stamp}",
                LicenseExpiryDate = DateTime.Today.AddYears(10),
                LicenseType = "غير مسجل",
                DateOfBirth = DateTime.Today.AddYears(-30),
                IsActive = true,
                Notes = "أضيف من الرئيسية بالاسم فقط."
            });

            await LoadDriversAsync();
            await LoadDashboardAsync();
            MessageBox.Show("تم حفظ السائق.", "تم", MessageBoxButton.OK, MessageBoxImage.Information);
        });
    }

    private async void DashboardAddSupervisorButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.Equals(_currentUser?.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var window = new QuickNameEntryWindow(
            "إضافة مشرف",
            "اكتب اسم المشرف فقط. سيتم حفظه كمشرف نشط ويظهر مباشرة في التشغيلات.")
        {
            Owner = this
        };

        if (window.ShowDialog() != true)
        {
            return;
        }

        await RunSafeAsync(async () =>
        {
            await _employeeService.SaveAsync(new EmployeeFormDto
            {
                FullName = window.EnteredName,
                Department = "التشغيل",
                Position = "مشرف",
                HireDate = DateTime.Today,
                Status = "Active",
                Notes = "أضيف من الرئيسية بالاسم فقط."
            });

            await LoadEmployeesAsync();
            await LoadDashboardAsync();
            MessageBox.Show("تم حفظ المشرف.", "تم", MessageBoxButton.OK, MessageBoxImage.Information);
        });
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
    private async void AddDriverButton_Click(object sender, RoutedEventArgs e) => await AddDriverFromPopupAsync();
    private async void EditDriverButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        var driver = SelectDriverFromSender(sender) ?? throw new InvalidOperationException("اختر سائقًا أولًا.");
        await EditDriverFromPopupAsync(driver);
    });

    private async Task AddDriverFromPopupAsync()
    {
        var form = CreateNewDriverForm();

        while (true)
        {
            var window = new DriverEntryWindow(form)
            {
                Owner = this
            };

            if (window.ShowDialog() != true || window.DriverForm is null)
            {
                return;
            }

            form = window.DriverForm;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                _isSavingDriver = true;
                var saved = await _driverService.SaveAsync(form);
                await LoadDriversAsync();
                var reselected = _drivers.FirstOrDefault(x => x.Id == saved.Id);
                if (reselected is not null)
                {
                    DriversGrid.SelectedItem = reselected;
                    DriversGrid.ScrollIntoView(reselected);
                }

                await LoadDriverAttendanceWeekAsync();
                await LoadDashboardAsync();
                await LoadNotificationsAsync();
                return;
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "تعذر حفظ السائق", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"لم يتم حفظ السائق:\n{ex.Message}", "تعذر حفظ السائق", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                _isSavingDriver = false;
                Mouse.OverrideCursor = null;
            }
        }
    }

    private async Task EditDriverFromPopupAsync(DriverDto driver)
    {
        var form = ToForm(driver);

        while (true)
        {
            var window = new DriverEntryWindow(form, true)
            {
                Owner = this
            };

            if (window.ShowDialog() != true || window.DriverForm is null)
            {
                return;
            }

            form = window.DriverForm;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                _isSavingDriver = true;
                var saved = await _driverService.SaveAsync(form);
                await LoadDriversAsync();
                var reselected = _drivers.FirstOrDefault(x => x.Id == saved.Id);
                if (reselected is not null)
                {
                    DriversGrid.SelectedItem = reselected;
                    DriversGrid.ScrollIntoView(reselected);
                }

                await LoadDriverAttendanceWeekAsync();
                await LoadDashboardAsync();
                await LoadNotificationsAsync();
                return;
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "تعذر تعديل السائق", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"لم يتم تعديل السائق:\n{ex.Message}", "تعذر تعديل السائق", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                _isSavingDriver = false;
                Mouse.OverrideCursor = null;
            }
        }
    }

    private DriverDto? SelectDriverFromSender(object sender)
    {
        var item = (sender as FrameworkElement)?.DataContext as DriverDto ?? Selected<DriverDto>(DriversGrid);
        if (item is not null)
        {
            DriversGrid.SelectedItem = item;
            DriversGrid.ScrollIntoView(item);
        }

        return item;
    }

    private static DriverFormDto CreateNewDriverForm() => new()
    {
        DateOfBirth = DateTime.Today.AddYears(-30),
        LicenseStartDate = DateTime.Today,
        LicenseExpiryDate = DateTime.Today.AddYears(1),
        LicenseType = "خاصة",
        IsActive = true,
        WorkLocation = "الموقع الرئيسي"
    };

    private async void SaveDriverButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        await SaveSelectedDriverSilentlyAsync();
        var item = Selected<DriverDto>(DriversGrid) ?? throw new InvalidOperationException("اختر سائقًا أولًا.");
        if (item.Id == 0)
        {
            throw new InvalidOperationException("أكمل بيانات السائق الأساسية قبل الحفظ.");
        }

        var saved = item;
        await LoadDriversAsync();
        var reselected = _drivers.FirstOrDefault(x => x.Id == saved.Id);
        if (reselected is not null)
        {
            DriversGrid.SelectedItem = reselected;
            DriversGrid.ScrollIntoView(reselected);
        }

        await LoadDriverAttendanceWeekAsync();
        await LoadDashboardAsync();
    });
    private async void DeleteDriverButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(() => DeleteSelectedAsync(DriversGrid, _drivers, x => x.Id, _driverService.DeleteAsync, LoadDriversAsync));

    private void DriversGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) => ScheduleDriverAutoSave();

    private void DriversGrid_CurrentCellChanged(object sender, EventArgs e) => ScheduleDriverAutoSave();

    private void DriversGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e) => ScheduleDriverAutoSave();

    private void ScheduleDriverAutoSave()
    {
        if (_isSavingDriver || !DriversGrid.IsKeyboardFocusWithin)
        {
            return;
        }

        _driverAutoSaveTimer.Stop();
        _driverAutoSaveTimer.Start();
    }

    private async Task SaveSelectedDriverSilentlyAsync()
    {
        if (_isSavingDriver)
        {
            return;
        }

        CommitGridEdit(DriversGrid);
        var item = Selected<DriverDto>(DriversGrid);
        if (!CanAutoSaveDriver(item))
        {
            return;
        }

        _isSavingDriver = true;
        try
        {
            var saved = await _driverService.SaveAsync(ToForm(item!));
            CopySavedDriverValues(item!, saved);
            ApplyDriverFilters();
            await LoadDriverAttendanceWeekAsync();
            await LoadDashboardAsync();
            await LoadNotificationsAsync();
        }
        finally
        {
            _isSavingDriver = false;
        }
    }

    private static bool CanAutoSaveDriver(DriverDto? driver) =>
        driver is not null
        && !string.IsNullOrWhiteSpace(driver.FullName)
        && !string.IsNullOrWhiteSpace(driver.LicenseNumber)
        && driver.LicenseStartDate != default
        && driver.LicenseExpiryDate != default
        && driver.LicenseExpiryDate.Date >= driver.LicenseStartDate.Date;

    private static void CopySavedDriverValues(DriverDto target, DriverDto saved)
    {
        target.Id = saved.Id;
        target.FullName = saved.FullName;
        target.NationalId = saved.NationalId;
        target.PhoneNumber = saved.PhoneNumber;
        target.Email = saved.Email;
        target.Address = saved.Address;
        target.DateOfBirth = saved.DateOfBirth;
        target.LicenseNumber = saved.LicenseNumber;
        target.LicenseStartDate = saved.LicenseStartDate;
        target.LicenseExpiryDate = saved.LicenseExpiryDate;
        target.LicenseType = saved.LicenseType;
        target.IsCompanyInsured = saved.IsCompanyInsured;
        target.Governorate = saved.Governorate;
        target.FullAddress = saved.FullAddress;
        target.TrafficUnit = saved.TrafficUnit;
        target.WorkLocation = saved.WorkLocation;
        target.LicenseExpiryAlert = saved.LicenseExpiryAlert;
        target.IsActive = saved.IsActive;
        target.Notes = saved.Notes;
    }

    private async void LoadDriverAttendanceWeekButton_Click(object sender, RoutedEventArgs e) =>
        await SetDriverAttendanceWeekAsync(DriverAttendanceWeekPicker.SelectedDate ?? DateTime.Today, true);

    private async void SaveDriverAttendanceWeekButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        await SaveDriverAttendanceWeekSilentlyAsync();
        await LoadDriverAttendanceWeekAsync();
        MessageBox.Show("تم حفظ حضور وغياب السائقين لهذا الأسبوع.", "تم", MessageBoxButton.OK, MessageBoxImage.Information);
    });

    private void DriverAttendanceInput_Changed(object sender, RoutedEventArgs e)
    {
        if (_isLoadingDriverAttendanceWeek || _isSavingDriverAttendanceWeek)
        {
            return;
        }

        if (sender is Control control && !control.IsKeyboardFocusWithin && !control.IsMouseOver)
        {
            return;
        }

        _driverAttendanceAutoSaveTimer.Stop();
        _driverAttendanceAutoSaveTimer.Start();
    }

    private async Task SaveDriverAttendanceWeekSilentlyAsync()
    {
        if (_isLoadingDriverAttendanceWeek || _isSavingDriverAttendanceWeek)
        {
            return;
        }
        _isSavingDriverAttendanceWeek = true;
        try
        {
            await SaveDailyAttendanceAsync();
        }
        finally
        {
            _isSavingDriverAttendanceWeek = false;
        }
    }

    private async Task LoadDailyAttendanceAsync()
    {
        var weekStart = StartOfDriverWeek(DateTime.Today);
        var records = await _driverAttendanceService.GetWeekAsync(weekStart);
        var todayRecords = records.Where(r => r.WorkDate.Date == DateTime.Today.Date).ToList();
        var isFriday = DateTime.Today.DayOfWeek == DayOfWeek.Friday;

        var rows = _drivers
            .Where(d => d.IsActive)
            .OrderBy(d => d.FullName, StringComparer.CurrentCultureIgnoreCase)
            .Select(driver =>
            {
                var record = todayRecords.FirstOrDefault(r => r.DriverId == driver.Id);
                string status;
                if (record is not null)
                {
                    // Map stored display status; empty = Rest day
                    status = string.IsNullOrEmpty(record.Status) ? "راحة" : record.Status;
                }
                else
                {
                    status = isFriday ? "راحة" : "حاضر";
                }

                return new DailyAttendanceRow
                {
                    DriverId = driver.Id,
                    DriverName = driver.FullName,
                    WorkLocation = record?.WorkLocation ?? driver.WorkLocation,
                    Status = status,
                    AbsenceReason = record?.AbsenceReason ?? string.Empty
                };
            })
            .ToList();
        ReplaceCollection(_dailyAttendanceRows, rows);
        DailyAttendanceDateTextBlock.Text = DateTime.Today.ToString(
            "dddd - yyyy/MM/dd", new System.Globalization.CultureInfo("ar-EG"));
    }

    private async Task SaveDailyAttendanceAsync()
    {
        CommitGridEdit(DailyAttendanceGrid);
        var weekStart = StartOfDriverWeek(DateTime.Today);
        var forms = _dailyAttendanceRows
            .Select(row => new DriverAttendanceFormDto
            {
                DriverId = row.DriverId,
                WorkDate = DateTime.Today,
                WorkLocation = row.WorkLocation,
                Status = row.Status,
                AbsenceReason = row.IsAbsent ? row.AbsenceReason : string.Empty
            })
            .ToList();
        await _driverAttendanceService.SaveWeekAsync(weekStart, forms);
    }

    private async Task LoadAbsenceHistoryAsync()
    {
        if (AbsenceHistoryDriverComboBox.SelectedValue is not int driverId || driverId <= 0)
        {
            MessageBox.Show("اختر سائقًا أولًا.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var from = AbsenceHistoryFromPicker.SelectedDate ?? DateTime.Today.AddMonths(-3);
        var to = AbsenceHistoryToPicker.SelectedDate ?? DateTime.Today;
        var absences = await _driverAttendanceService.GetDriverAbsencesAsync(driverId, from, to);
        ReplaceCollection(_absenceHistoryRows, absences);
    }

    private void DailyAttendanceInput_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is Control control && !control.IsKeyboardFocusWithin && !control.IsMouseOver)
            return;
        _driverAttendanceAutoSaveTimer.Stop();
        _driverAttendanceAutoSaveTimer.Start();
    }

    private async void SaveDailyAttendanceButton_Click(object sender, RoutedEventArgs e) =>
        await RunSafeAsync(async () =>
        {
            await SaveDailyAttendanceAsync();
            await LoadDailyAttendanceAsync();
            MessageBox.Show("تم حفظ حضور اليوم بنجاح.", "تم", MessageBoxButton.OK, MessageBoxImage.Information);
        });

    private async void LoadAbsenceHistoryButton_Click(object sender, RoutedEventArgs e) =>
        await RunSafeAsync(LoadAbsenceHistoryAsync);
    private async void GenerateDriverReportButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(GenerateDriverReportAsync);

    private async Task GenerateDriverReportAsync()
    {
        var start = (DriverReportStartDatePicker.SelectedDate ?? StartOfDriverWeek(DateTime.Today)).Date;
        var end = (DriverReportEndDatePicker.SelectedDate ?? start.AddDays(6)).Date;
        DriverReportStartDatePicker.SelectedDate = start;
        DriverReportEndDatePicker.SelectedDate = end;
        var driverId = DriverReportDriverComboBox.SelectedValue is int selectedDriverId && selectedDriverId > 0
            ? selectedDriverId
            : (int?)null;
        ReplaceCollection(_driverReportRows, await _driverAttendanceService.GenerateReportAsync(start, end, driverId));
        ApplyDriverReportFilters();
    }

    private void DriverFilter_Changed(object sender, RoutedEventArgs e) => ApplyDriverFilters();

    private void ApplyDriverFilters()
    {
        if (DriversGrid.ItemsSource is null)
        {
            return;
        }

        var view = CollectionViewSource.GetDefaultView(DriversGrid.ItemsSource);
        if (view is null)
        {
            return;
        }

        view.Filter = DriverRowMatchesFilters;
        view.Refresh();
    }

    private bool DriverRowMatchesFilters(object item)
    {
        if (item is not DriverDto driver)
        {
            return false;
        }

        if (driver.Id == 0)
        {
            return true;
        }

        return TextFilterMatches(DriverFilterName, driver.FullName)
            && TextFilterMatches(DriverFilterNationalId, driver.NationalId)
            && TextFilterMatches(DriverFilterLicense, driver.LicenseNumber)
            && TextFilterMatches(DriverFilterLicenseType, driver.LicenseType)
            && DateFilterMatches(DriverFilterLicenseStartDate, driver.LicenseStartDate)
            && DateFilterMatches(DriverFilterLicenseExpiryDate, driver.LicenseExpiryDate)
            && TextFilterMatches(DriverFilterInsurance, driver.IsCompanyInsured ? "نعم متأمن مؤمن true yes" : "لا غير متأمن غير مؤمن false no")
            && TextFilterMatches(DriverFilterGovernorate, driver.Governorate)
            && TextFilterMatches(DriverFilterFullAddress, driver.FullAddress)
            && TextFilterMatches(DriverFilterTrafficUnit, driver.TrafficUnit)
            && TextFilterMatches(DriverFilterWorkLocation, driver.WorkLocation)
            && TextFilterMatches(DriverFilterActive, driver.IsActive ? "نعم مفعل نشط active true yes" : "لا غير مفعل غير نشط inactive false no");
    }

    private async void PrintDriversButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(() =>
    {
        CommitGridEdit(DriversGrid);
        var rows = GetVisibleRows(DriversGrid, _drivers);
        OpenReportPreview(BuildDriversReportSnapshot(rows));
        return Task.CompletedTask;
    });

    private void DriverAttendanceFilter_Changed(object sender, RoutedEventArgs e) => ApplyDriverAttendanceFilters();

    private void ApplyDriverAttendanceFilters()
    {
        if (DriverAttendanceGrid.ItemsSource is null)
        {
            return;
        }

        var view = CollectionViewSource.GetDefaultView(DriverAttendanceGrid.ItemsSource);
        if (view is null)
        {
            return;
        }

        view.Filter = DriverAttendanceRowMatchesFilters;
        view.Refresh();
    }

    private bool DriverAttendanceRowMatchesFilters(object item)
    {
        if (item is not DriverWeeklyAttendanceRow row)
        {
            return false;
        }

        return TextFilterMatches(DriverAttendanceFilterName, row.DriverName)
            && TextFilterMatches(DriverAttendanceFilterWorkLocation, row.WorkLocation)
            && TextFilterMatches(DriverAttendanceFilterSaturday, row.SaturdayStatus)
            && TextFilterMatches(DriverAttendanceFilterSunday, row.SundayStatus)
            && TextFilterMatches(DriverAttendanceFilterMonday, row.MondayStatus)
            && TextFilterMatches(DriverAttendanceFilterTuesday, row.TuesdayStatus)
            && TextFilterMatches(DriverAttendanceFilterWednesday, row.WednesdayStatus)
            && TextFilterMatches(DriverAttendanceFilterThursday, row.ThursdayStatus)
            && TextFilterMatches(DriverAttendanceFilterFriday, row.FridayStatus);
    }

    private async void PrintDriverAttendanceButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        CommitGridEdit(DriverAttendanceGrid);
        await SaveDriverAttendanceWeekSilentlyAsync();
        var rows = GetVisibleRows(DriverAttendanceGrid, _driverAttendanceWeekRows);
        OpenReportPreview(BuildDriverAttendanceReportSnapshot(rows, GetSelectedDriverWeekStart()));
    });

    private void DriverReportFilter_Changed(object sender, RoutedEventArgs e) => ApplyDriverReportFilters();

    private void ApplyDriverReportFilters()
    {
        if (DriverReportsGrid.ItemsSource is null)
        {
            return;
        }

        var view = CollectionViewSource.GetDefaultView(DriverReportsGrid.ItemsSource);
        if (view is null)
        {
            return;
        }

        view.Filter = DriverReportRowMatchesFilters;
        view.Refresh();
    }

    private bool DriverReportRowMatchesFilters(object item)
    {
        if (item is not DriverReportDto row)
        {
            return false;
        }

        return TextFilterMatches(DriverReportFilterName, row.FullName)
            && TextFilterMatches(DriverReportFilterNationalId, row.NationalId)
            && TextFilterMatches(DriverReportFilterLicenseNumber, row.LicenseNumber)
            && TextFilterMatches(DriverReportFilterLicenseType, row.LicenseType)
            && DateFilterMatches(DriverReportFilterLicenseStartDate, row.LicenseStartDate)
            && DateFilterMatches(DriverReportFilterLicenseExpiryDate, row.LicenseExpiryDate)
            && TextFilterMatches(DriverReportFilterInsurance, row.IsCompanyInsured ? "نعم متأمن مؤمن true yes" : "لا غير متأمن غير مؤمن false no")
            && TextFilterMatches(DriverReportFilterGovernorate, row.Governorate)
            && TextFilterMatches(DriverReportFilterFullAddress, row.FullAddress)
            && TextFilterMatches(DriverReportFilterTrafficUnit, row.TrafficUnit)
            && TextFilterMatches(DriverReportFilterWorkLocation, row.WorkLocation)
            && TextFilterMatches(DriverReportFilterPresent, row.PresentDays.ToString(CultureInfo.InvariantCulture))
            && TextFilterMatches(DriverReportFilterAbsent, row.AbsentDays.ToString(CultureInfo.InvariantCulture))
            && TextFilterMatches(DriverReportFilterLeave, row.LeaveDays.ToString(CultureInfo.InvariantCulture))
            && TextFilterMatches(DriverReportFilterWorkedFridays, row.WorkedFridays.ToString(CultureInfo.InvariantCulture))
            && TextFilterMatches(DriverReportFilterEarnedRest, row.EarnedRestDays.ToString(CultureInfo.InvariantCulture))
            && TextFilterMatches(DriverReportFilterRemainingRest, row.RemainingRestDays.ToString(CultureInfo.InvariantCulture));
    }

    private async void PrintDriverReportButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        if (_driverReportRows.Count == 0)
        {
            await GenerateDriverReportAsync();
        }

        var rows = GetVisibleRows(DriverReportsGrid, _driverReportRows);
        OpenReportPreview(BuildDriverReportSnapshot(rows));
    });

    private static List<T> GetVisibleRows<T>(DataGrid grid, IEnumerable<T> fallback)
    {
        if (grid.ItemsSource is null)
        {
            return fallback.ToList();
        }

        var view = CollectionViewSource.GetDefaultView(grid.ItemsSource);
        return view is null
            ? fallback.ToList()
            : view.Cast<object>().OfType<T>().ToList();
    }

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

            if (TripFilterSerial != null && !string.IsNullOrWhiteSpace(TripFilterSerial.Text) && !trip.Serial.ToString().Contains(TripFilterSerial.Text.Trim(), StringComparison.OrdinalIgnoreCase))
                return false;

            if (TripFilterDate != null && TripFilterDate.SelectedDate.HasValue && trip.StartDate.Date != TripFilterDate.SelectedDate.Value.Date)
                return false;

            if (TripFilterEndDate != null && TripFilterEndDate.SelectedDate.HasValue && trip.EndDate?.Date != TripFilterEndDate.SelectedDate.Value.Date)
                return false;

            if (TripFilterPlate != null && !string.IsNullOrWhiteSpace(TripFilterPlate.Text) && (trip.VehiclePlateNumber == null || !trip.VehiclePlateNumber.Contains(TripFilterPlate.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                return false;

            if (TripFilterStartLocation != null && !string.IsNullOrWhiteSpace(TripFilterStartLocation.Text) && (trip.StartLocation == null || !trip.StartLocation.Contains(TripFilterStartLocation.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                return false;

            if (TripFilterEndLocation != null && !string.IsNullOrWhiteSpace(TripFilterEndLocation.Text) && (trip.EndLocation == null || !trip.EndLocation.Contains(TripFilterEndLocation.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                return false;

            if (TripFilterLocation != null && !string.IsNullOrWhiteSpace(TripFilterLocation.Text))
            {
                var route = $"{trip.StartLocation} {trip.EndLocation}";
                if (!route.Contains(TripFilterLocation.Text.Trim(), StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            if (TripFilterDriver != null && !string.IsNullOrWhiteSpace(TripFilterDriver.Text) && (trip.DriverName == null || !trip.DriverName.Contains(TripFilterDriver.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                return false;

            // استخدام TripFilterPerson لفلترة الموصي (RequesterName)
            if (TripFilterPerson != null && !string.IsNullOrWhiteSpace(TripFilterPerson.Text) && (trip.RequesterName == null || !trip.RequesterName.Contains(TripFilterPerson.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                return false;

            if (TripFilterSupervisor != null && !string.IsNullOrWhiteSpace(TripFilterSupervisor.Text) && (trip.SupervisorName == null || !trip.SupervisorName.Contains(TripFilterSupervisor.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                return false;

            if (TripFilterPurpose != null && !string.IsNullOrWhiteSpace(TripFilterPurpose.Text) && (trip.Purpose == null || !trip.Purpose.Contains(TripFilterPurpose.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                return false;

            if (TripFilterStatus != null && !string.IsNullOrWhiteSpace(TripFilterStatus.Text) && (trip.Status == null || !trip.Status.Contains(TripFilterStatus.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                return false;

            if (TripFilterDistance != null && !string.IsNullOrWhiteSpace(TripFilterDistance.Text) && !trip.Distance.ToString("0.##", CultureInfo.InvariantCulture).Contains(TripFilterDistance.Text.Trim(), StringComparison.OrdinalIgnoreCase))
                return false;

            if (TripFilterNotes != null && !string.IsNullOrWhiteSpace(TripFilterNotes.Text) && (trip.Notes == null || !trip.Notes.Contains(TripFilterNotes.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                return false;

            return true;
        };
    }

    private void FuelFilter_Changed(object sender, RoutedEventArgs e)
    {
        var view = CollectionViewSource.GetDefaultView(FuelGrid.ItemsSource);
        if (view is null)
        {
            return;
        }

        view.Filter = FuelRowMatchesFilters;
        view.Refresh();
    }

    private bool FuelRowMatchesFilters(object item)
    {
        if (item is not FuelTransactionDto fuel)
        {
            return false;
        }

        return TextFilterMatches(FuelFilterPlate, fuel.VehiclePlateNumber)
            && TextFilterMatches(FuelFilterTrip, fuel.TripId?.ToString(CultureInfo.InvariantCulture))
            && DateFilterMatches(FuelFilterDate, fuel.TransactionDate)
            && TextFilterMatches(FuelFilterType, fuel.FuelType)
            && TextFilterMatches(FuelFilterQuantity, fuel.Quantity.ToString("0.##", CultureInfo.InvariantCulture))
            && TextFilterMatches(FuelFilterUnitPrice, fuel.UnitPrice.ToString("0.##", CultureInfo.InvariantCulture))
            && TextFilterMatches(FuelFilterTotalCost, fuel.TotalCost.ToString("0.##", CultureInfo.InvariantCulture))
            && TextFilterMatches(FuelFilterStation, fuel.FuelStation)
            && TextFilterMatches(FuelFilterOdometer, fuel.Odometer.ToString("0.##", CultureInfo.InvariantCulture))
            && TextFilterMatches(FuelFilterTreasury, fuel.PaidFromTreasury ? "نعم true yes" : "لا false no")
            && TextFilterMatches(FuelFilterNotes, fuel.Notes);
    }

    private void OilFilter_Changed(object sender, RoutedEventArgs e)
    {
        var view = CollectionViewSource.GetDefaultView(OilChangesGrid.ItemsSource);
        if (view is null)
        {
            return;
        }

        view.Filter = OilRowMatchesFilters;
        view.Refresh();
    }

    private bool OilRowMatchesFilters(object item)
    {
        if (item is not OilChangeDto oil)
        {
            return false;
        }

        return TextFilterMatches(OilFilterPlate, oil.VehiclePlateNumber)
            && DateFilterMatches(OilFilterDate, oil.ChangeDate)
            && TextFilterMatches(OilFilterRecordType, oil.RecordType)
            && TextFilterMatches(OilFilterServiceItems, oil.ServiceItems)
            && TextFilterMatches(OilFilterOdometer, oil.OdometerAtChange.ToString("0.##", CultureInfo.InvariantCulture))
            && TextFilterMatches(OilFilterCurrentOdometer, oil.CurrentOdometer.ToString("0.##", CultureInfo.InvariantCulture))
            && TextFilterMatches(OilFilterType, oil.OilType)
            && TextFilterMatches(OilFilterQuantity, oil.QuantityDisplay)
            && TextFilterMatches(OilFilterCost, oil.CostDisplay)
            && TextFilterMatches(OilFilterNext, oil.NextOilChangeOdometer.ToString("0.##", CultureInfo.InvariantCulture))
            && TextFilterMatches(OilFilterInterval, oil.OilChangeIntervalKm.ToString("0.##", CultureInfo.InvariantCulture))
            && TextFilterMatches(OilFilterSince, oil.KmSinceOilChange.ToString("0.##", CultureInfo.InvariantCulture))
            && TextFilterMatches(OilFilterRemaining, oil.RemainingKm.ToString("0.##", CultureInfo.InvariantCulture))
            && TextFilterMatches(OilFilterAlert, oil.OilAlert)
            && TextFilterMatches(OilFilterStatus, oil.Status)
            && TextFilterMatches(OilFilterNotes, oil.Notes);
    }

    private void LicenseFilter_Changed(object sender, RoutedEventArgs e)
    {
        var view = CollectionViewSource.GetDefaultView(LicensesGrid.ItemsSource);
        if (view is null)
        {
            return;
        }

        view.Filter = LicenseRowMatchesFilters;
        view.Refresh();
    }

    private bool LicenseRowMatchesFilters(object item)
    {
        if (item is not VehicleLicenseRow license)
        {
            return false;
        }

        return TextFilterMatches(LicenseFilterPlate, license.PlateNumber)
            && TextFilterMatches(LicenseFilterModel, license.Model)
            && TextFilterMatches(LicenseFilterYear, license.Year.ToString(CultureInfo.InvariantCulture))
            && TextFilterMatches(LicenseFilterChassis, license.ChassisNumber)
            && TextFilterMatches(LicenseFilterEngine, license.EngineNumber)
            && TextFilterMatches(LicenseFilterOilInterval, license.OilChangeIntervalKm.ToString("0.##", CultureInfo.InvariantCulture))
            && DateFilterMatches(LicenseFilterStartDate, license.RegistrationStartDate)
            && DateFilterMatches(LicenseFilterEndDate, license.RegistrationExpiryDate)
            && TextFilterMatches(LicenseFilterStatus, license.Status)
            && TextFilterMatches(LicenseFilterAlert, license.ExpiryAlert);
    }

    private void InsuranceFilter_Changed(object sender, RoutedEventArgs e)
    {
        var view = CollectionViewSource.GetDefaultView(InsuranceGrid.ItemsSource);
        if (view is null)
        {
            return;
        }

        view.Filter = InsuranceRowMatchesFilters;
        view.Refresh();
    }

    private bool InsuranceRowMatchesFilters(object item)
    {
        if (item is not InsuranceDto insurance)
        {
            return false;
        }

        return TextFilterMatches(InsuranceFilterPlate, insurance.VehiclePlateNumber)
            && TextFilterMatches(InsuranceFilterModel, insurance.VehicleModel)
            && TextFilterMatches(InsuranceFilterYear, insurance.VehicleYear.ToString(CultureInfo.InvariantCulture))
            && TextFilterMatches(InsuranceFilterChassis, insurance.VehicleChassisNumber)
            && TextFilterMatches(InsuranceFilterEngine, insurance.VehicleEngineNumber)
            && TextFilterMatches(InsuranceFilterPolicy, insurance.PolicyNumber)
            && TextFilterMatches(InsuranceFilterCompany, insurance.InsuranceCompany)
            && TextFilterMatches(InsuranceFilterType, insurance.PolicyType)
            && DateFilterMatches(InsuranceFilterStartDate, insurance.StartDate)
            && DateFilterMatches(InsuranceFilterEndDate, insurance.ExpiryDate)
            && TextFilterMatches(InsuranceFilterPremium, insurance.PremiumAmount.ToString("0.##", CultureInfo.InvariantCulture))
            && TextFilterMatches(InsuranceFilterCoverage, insurance.CoverageAmount.ToString("0.##", CultureInfo.InvariantCulture))
            && TextFilterMatches(InsuranceFilterCoverageDetails, insurance.CoverageDetails)
            && TextFilterMatches(InsuranceFilterAgent, insurance.AgentName)
            && TextFilterMatches(InsuranceFilterAgentPhone, insurance.AgentPhoneNumber)
            && TextFilterMatches(InsuranceFilterNotes, insurance.Notes)
            && TextFilterMatches(InsuranceFilterStatus, insurance.Status)
            && TextFilterMatches(InsuranceFilterAlert, insurance.ExpiryAlert);
    }

    private void TreasuryFilter_Changed(object sender, RoutedEventArgs e)
    {
        var view = CollectionViewSource.GetDefaultView(TreasuryGrid.ItemsSource);
        if (view is null)
        {
            return;
        }

        view.Filter = TreasuryRowMatchesFilters;
        view.Refresh();
    }

    private bool TreasuryRowMatchesFilters(object item)
    {
        if (item is not TreasuryTransactionDto treasury)
        {
            return false;
        }

        return TextFilterMatches(TreasuryFilterId, treasury.Id.ToString(CultureInfo.InvariantCulture))
            && DateFilterMatches(TreasuryFilterDate, treasury.TransactionDate)
            && TextFilterMatches(TreasuryFilterType, treasury.TransactionType)
            && TextFilterMatches(TreasuryFilterAmount, treasury.Amount.ToString("0.##", CultureInfo.InvariantCulture))
            && TextFilterMatches(TreasuryFilterDescription, treasury.Description)
            && TextFilterMatches(TreasuryFilterRelatedType, treasury.RelatedEntityType)
            && TextFilterMatches(TreasuryFilterRelatedId, treasury.RelatedEntityId?.ToString(CultureInfo.InvariantCulture))
            && TextFilterMatches(TreasuryFilterPayment, treasury.PaymentMethod)
            && TextFilterMatches(TreasuryFilterNotes, treasury.Notes);
    }

    private void CustodyFilter_Changed(object sender, RoutedEventArgs e)
    {
        var view = CollectionViewSource.GetDefaultView(CustodyGrid.ItemsSource);
        if (view is null)
        {
            return;
        }

        view.Filter = CustodyRowMatchesFilters;
        view.Refresh();
    }

    private bool CustodyRowMatchesFilters(object item)
    {
        if (item is not CustodyDto custody)
        {
            return false;
        }

        return TextFilterMatches(CustodyFilterId, custody.Id.ToString(CultureInfo.InvariantCulture))
            && TextFilterMatches(CustodyFilterCustodianType, custody.CustodianType)
            && TextFilterMatches(CustodyFilterNumber, custody.CustodyNumber)
            && TextFilterMatches(CustodyFilterCustodian, custody.CustodianName)
            && TextFilterMatches(CustodyFilterAmount, custody.Amount.ToString("0.##", CultureInfo.InvariantCulture))
            && DateFilterMatches(CustodyFilterHandoverDate, custody.HandoverDate)
            && DateFilterMatches(CustodyFilterReturnDate, custody.ReturnDate)
            && TextFilterMatches(CustodyFilterStatus, custody.Status)
            && TextFilterMatches(CustodyFilterRemaining, custody.RemainingBalance.ToString("0.##", CultureInfo.InvariantCulture))
            && TextFilterMatches(CustodyFilterNotes, custody.Notes);
    }

    private static bool TextFilterMatches(TextBox? filter, string? value)
    {
        var text = filter?.Text?.Trim();
        return string.IsNullOrWhiteSpace(text)
            || (value ?? string.Empty).Contains(text, StringComparison.OrdinalIgnoreCase);
    }

    private static bool DateFilterMatches(DatePicker? filter, DateTime? value)
    {
        return filter?.SelectedDate is not DateTime selectedDate
            || value?.Date == selectedDate.Date;
    }

    private FrameworkElement CreateReportColumnHeader(string columnName)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        panel.Children.Add(new TextBlock
        {
            Text = columnName,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 8)
        });

        var filter = new TextBox
        {
            Tag = columnName,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Style = (Style)FindResource("FilterTextBoxStyle")
        };
        AutomationProperties.SetAutomationId(filter, $"ReportFilter_{columnName}");
        filter.TextChanged += ReportColumnFilter_Changed;
        panel.Children.Add(filter);

        return panel;
    }

    private void ReportColumnFilter_Changed(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox filter || filter.Tag is not string columnName)
        {
            return;
        }

        var value = filter.Text.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            _reportColumnFilters.Remove(columnName);
        }
        else
        {
            _reportColumnFilters[columnName] = value;
        }

        ApplyReportColumnFilters();
    }

    private void ApplyReportColumnFilters()
    {
        if (ReportsGrid.ItemsSource is not DataView view || view.Table is null)
        {
            return;
        }

        var filters = _reportColumnFilters
            .Where(filter => !string.IsNullOrWhiteSpace(filter.Value) && view.Table.Columns.Contains(filter.Key))
            .Select(filter => $"Convert({EscapeDataColumnName(filter.Key)}, 'System.String') LIKE '%{EscapeDataViewLikeValue(filter.Value)}%'")
            .ToList();

        try
        {
            view.RowFilter = string.Join(" AND ", filters);
        }
        catch (EvaluateException)
        {
            view.RowFilter = string.Empty;
        }
        catch (SyntaxErrorException)
        {
            view.RowFilter = string.Empty;
        }
    }

    private static string EscapeDataColumnName(string columnName) => $"[{columnName.Replace("]", "\\]")}]";

    private static string EscapeDataViewLikeValue(string value) => value
        .Replace("'", "''")
        .Replace("[", "[[]")
        .Replace("%", "[%]")
        .Replace("*", "[*]");

    private async void RefreshTripsButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(LoadTripsAsync);
    private async void AddTripButton_Click(object sender, RoutedEventArgs e)
    {
        var lookupDataLoaded = false;
        await RunSafeAsync(async () =>
        {
            await LoadVehiclesAsync();
            await LoadDriversAsync();
            await LoadEmployeesAsync();
            lookupDataLoaded = true;
        });

        if (!lookupDataLoaded)
        {
            return;
        }

        var tripDrivers = _drivers
            .Where(driver => !string.IsNullOrWhiteSpace(driver.FullName))
            .OrderBy(driver => driver.FullName)
            .ToList();
        if (tripDrivers.Count == 0)
        {
            MessageBox.Show(
                "لا يوجد سائقين مسجلين. أضف سائقًا من شاشة السائقين ثم افتح التشغيلات مرة أخرى.",
                "تنبيه واضح",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var window = new TripEntryWindow(_vehicles.ToList(), tripDrivers, _employees.ToList())
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
            await LoadOilChangesAsync();
            await LoadDashboardAsync();
            await LoadNotificationsAsync();
        });
    }

    private async void DeleteTripButton_Click(object sender, RoutedEventArgs e) =>
        await RunSafeAsync(() =>
        {
            SelectTripFromSender(sender);
            return DeleteSelectedAsync(TripsGrid, _trips, x => x.Id, _tripService.DeleteAsync, LoadTripsAsync);
        });

    private async void AddOilForSelectedTripVehicleButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(() =>
    {
        var trip = Selected<TripDto>(TripsGrid);
        var vehicle = trip is null
            ? _vehicles.FirstOrDefault()
            : _vehicles.FirstOrDefault(v => v.Id == trip.VehicleId)
                ?? throw new InvalidOperationException("العربية المرتبطة بالتشغيلة غير موجودة في بيانات المركبات.");

        if (vehicle is null)
        {
            throw new InvalidOperationException("لا توجد عربيات مسجلة. سجل عربية أولًا من صفحة التراخيص ثم افتح متابعة الزيت.");
        }

        var currentMileage = trip?.EndMileage ?? trip?.StartMileage ?? vehicle.CurrentMileage;
        if (currentMileage <= 0)
        {
            currentMileage = vehicle.CurrentMileage;
        }

        Dispatcher.BeginInvoke(new Action(() => OpenOilChangeEntry(BuildOilEntrySource(vehicle, currentMileage))));

        return Task.CompletedTask;
    });

    private async void PrintTripsButton_Click(object sender, RoutedEventArgs e)
    {
        ReportDataDto? report = null;
        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            await LoadTripsAsync();
            var view = CollectionViewSource.GetDefaultView(TripsGrid.ItemsSource);
            var trips = view is null
                ? _trips.ToList()
                : view.Cast<object>().OfType<TripDto>().ToList();

            report = BuildTripsReportSnapshot(trips, "تقرير التشغيلات");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"لم يتم تنفيذ العملية:\n{ex.Message}", "تنبيه واضح", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }

        if (report is not null)
        {
            OpenReportPreview(report);
        }
    }

    private async void CloseTripButton_Click(object sender, RoutedEventArgs e)
    {
        var trip = SelectTripFromSender(sender);
        if (trip is null)
        {
            MessageBox.Show("اختر تشغيلة أولًا.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var vehicle = _vehicles.FirstOrDefault(v => v.Id == trip.VehicleId);
        var window = new TripCloseWindow(trip, vehicle)
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
            await LoadOilChangesAsync();
            await LoadDashboardAsync();
            await LoadNotificationsAsync();
        });
    }

    private void ShowTripFormButton_Click(object sender, RoutedEventArgs e)
    {
        var trip = SelectTripFromSender(sender);
        if (trip is null)
        {
            MessageBox.Show("اختر تشغيلة أولًا.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        OpenReportPreview(BuildTripsReportSnapshot(new[] { trip }, $"تقرير التشغيلة رقم {trip.Serial}"));
    }

    private TripDto? SelectTripFromSender(object sender)
    {
        var trip = (sender as FrameworkElement)?.DataContext as TripDto ?? Selected<TripDto>(TripsGrid);
        if (trip is null)
        {
            return null;
        }

        TripsGrid.SelectedItem = trip;
        TripsGrid.ScrollIntoView(trip);
        return trip;
    }

    private FuelTransactionDto? SelectFuelFromSender(object sender)
    {
        var item = (sender as FrameworkElement)?.DataContext as FuelTransactionDto ?? Selected<FuelTransactionDto>(FuelGrid);
        if (item is null)
        {
            return null;
        }

        FuelGrid.SelectedItem = item;
        FuelGrid.ScrollIntoView(item);
        return item;
    }

    private OilChangeDto? SelectOilChangeFromSender(object sender)
    {
        var item = (sender as FrameworkElement)?.DataContext as OilChangeDto ?? Selected<OilChangeDto>(OilChangesGrid);
        if (item is null)
        {
            return null;
        }

        OilChangesGrid.SelectedItem = item;
        OilChangesGrid.ScrollIntoView(item);
        return item;
    }

    private VehicleLicenseRow? SelectVehicleLicenseFromSender(object sender)
    {
        var item = (sender as FrameworkElement)?.DataContext as VehicleLicenseRow ?? Selected<VehicleLicenseRow>(LicensesGrid);
        if (item is null)
        {
            return null;
        }

        LicensesGrid.SelectedItem = item;
        LicensesGrid.ScrollIntoView(item);
        return item;
    }

    private InsuranceDto? SelectInsuranceFromSender(object sender)
    {
        var item = (sender as FrameworkElement)?.DataContext as InsuranceDto ?? Selected<InsuranceDto>(InsuranceGrid);
        if (item is null)
        {
            return null;
        }

        InsuranceGrid.SelectedItem = item;
        InsuranceGrid.ScrollIntoView(item);
        return item;
    }

    private TreasuryTransactionDto? SelectTreasuryFromSender(object sender)
    {
        var item = (sender as FrameworkElement)?.DataContext as TreasuryTransactionDto ?? Selected<TreasuryTransactionDto>(TreasuryGrid);
        if (item is null)
        {
            return null;
        }

        TreasuryGrid.SelectedItem = item;
        TreasuryGrid.ScrollIntoView(item);
        return item;
    }

    private CustodyDto? SelectCustodyFromSender(object sender)
    {
        var item = (sender as FrameworkElement)?.DataContext as CustodyDto ?? Selected<CustodyDto>(CustodyGrid);
        if (item is null)
        {
            return null;
        }

        CustodyGrid.SelectedItem = item;
        CustodyGrid.ScrollIntoView(item);
        return item;
    }

    private void ShowVehicleLicenseFormButton_Click(object sender, RoutedEventArgs e)
    {
        var item = SelectVehicleLicenseFromSender(sender);
        if (item is null)
        {
            MessageBox.Show("اختر ترخيص عربية أولًا.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        OpenReportPreview(BuildVehicleLicensesReportSnapshot(new[] { item }, $"ترخيص العربية {ValueOrDash(item.PlateNumber)}"));
    }

    private void PrintAllLicensesButton_Click(object sender, RoutedEventArgs e) =>
        OpenReportPreview(BuildVehicleLicensesReportSnapshot(GetVisibleLicenseRows(), "تقرير التراخيص"));

    private void ShowInsuranceFormButton_Click(object sender, RoutedEventArgs e)
    {
        var item = SelectInsuranceFromSender(sender);
        if (item is null)
        {
            MessageBox.Show("اختر وثيقة تأمين أولًا.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        OpenReportPreview(BuildInsuranceReportSnapshot(new[] { item }, $"تأمين العربية {ValueOrDash(item.VehiclePlateNumber)}"));
    }

    private void PrintAllInsuranceButton_Click(object sender, RoutedEventArgs e) =>
        OpenReportPreview(BuildInsuranceReportSnapshot(GetVisibleInsuranceRows(), "تقرير التأمينات"));

    private async void RefreshFuelButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(LoadFuelAsync);
    private void AddFuelButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFuelEntry(BuildFuelDraft());
    }

    private FuelTransactionDto BuildFuelDraft()
    {
        var vehicle = _vehicles.FirstOrDefault();
        return new FuelTransactionDto
        {
            VehicleId = vehicle?.Id ?? 0,
            VehiclePlateNumber = vehicle?.PlateNumber ?? string.Empty,
            TransactionDate = DateTime.Today,
            FuelType = "بنزين"
        };
    }

    private FuelTransactionDto BuildFuelDraftForTrip(TripDto trip)
    {
        SelectTabByTag("Fuel");
        return new FuelTransactionDto
        {
            VehicleId = trip.VehicleId,
            VehiclePlateNumber = trip.VehiclePlateNumber,
            TripId = trip.Id,
            TransactionDate = (trip.EndDate ?? trip.StartDate).Date,
            FuelType = "بنزين",
            Odometer = trip.EndMileage ?? trip.StartMileage
        };
    }

    private async void OpenFuelEntry(FuelTransactionDto source)
    {
        SelectTabByTag("Fuel");
        var window = new FuelEntryWindow(_vehicles, _trips, source)
        {
            Owner = this
        };

        if (window.ShowDialog() != true || window.FuelForm is null)
        {
            return;
        }

        await RunSafeAsync(async () =>
        {
            await _fuelService.SaveAsync(window.FuelForm);
            await LoadFuelAsync();
            await LoadTripsAsync();
            await LoadVehiclesAsync();
            await LoadOilChangesAsync();
            await LoadTreasuryIfUnlockedAsync();
            await LoadDashboardAsync();
            await LoadNotificationsAsync();
        });
    }

    private void SaveFuelButton_Click(object sender, RoutedEventArgs e)
    {
        var item = SelectFuelFromSender(sender);
        if (item is null)
        {
            MessageBox.Show("اختر سجل وقود أولًا.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        OpenFuelEntry(item);
    }

    private async void DeleteFuelButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        SelectFuelFromSender(sender);
        await DeleteSelectedAsync(FuelGrid, _fuel, x => x.Id, _fuelService.DeleteAsync, LoadFuelAsync);
        await LoadTripsAsync();
        await LoadTreasuryIfUnlockedAsync();
        await LoadDashboardAsync();
    });

    private void PrintAllFuelButton_Click(object sender, RoutedEventArgs e)
    {
        var rows = GetVisibleFuelRows();
        if (rows.Count == 0)
        {
            MessageBox.Show("لا توجد سجلات بنزين ظاهرة للطباعة.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        OpenReportPreview(BuildFuelReportSnapshot(rows, "تقرير بنزين كل العربيات"));
    }

    private void PrintSelectedVehicleFuelButton_Click(object sender, RoutedEventArgs e)
    {
        var selected = Selected<FuelTransactionDto>(FuelGrid);
        if (selected is null)
        {
            MessageBox.Show("اختر سجل بنزين أولًا لطباعة بنزين العربية الخاصة به.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var rows = GetVisibleFuelRows()
            .Where(fuel => fuel.VehicleId == selected.VehicleId)
            .ToList();

        if (rows.Count == 0)
        {
            MessageBox.Show("لا توجد سجلات بنزين ظاهرة لهذه العربية.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        OpenReportPreview(BuildFuelReportSnapshot(rows, $"تقرير بنزين العربية {selected.VehiclePlateNumber}"));
    }

    private async void RefreshExpensesButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(LoadExpensesAsync);
    private void AddExpenseButton_Click(object sender, RoutedEventArgs e) => AddNewItem(_expenses, ExpensesGrid, new ExpenseDto { VehicleId = _vehicles.FirstOrDefault()?.Id ?? 0, ExpenseDate = DateTime.Today, Status = "Pending" });
    private async void SaveExpenseButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () => { var item = Selected<ExpenseDto>(ExpensesGrid) ?? throw new InvalidOperationException("اختر مصروفًا أولًا."); await _expenseService.SaveAsync(ToForm(item)); await LoadExpensesAsync(); await LoadTreasuryIfUnlockedAsync(); await LoadDashboardAsync(); });
    private async void DeleteExpenseButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () => { await DeleteSelectedAsync(ExpensesGrid, _expenses, x => x.Id, _expenseService.DeleteAsync, LoadExpensesAsync); await LoadTreasuryIfUnlockedAsync(); });

    private async void RefreshOilChangesButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(LoadOilChangesAsync);
    private void AddOilChangeButton_Click(object sender, RoutedEventArgs e)
    {
        var vehicle = _vehicles.FirstOrDefault();
        OpenOilChangeEntry(BuildOilEntrySource(vehicle, vehicle?.CurrentMileage ?? 0));
    }

    private OilChangeDto BuildOilEntrySource(VehicleDto? vehicle, decimal currentMileage)
    {
        var interval = vehicle?.OilChangeIntervalKm ?? 0;
        var latestOilChange = vehicle is null
            ? null
            : _oilChanges
                .Where(oil => oil.VehicleId == vehicle.Id && oil.IsOilChanged)
                .OrderByDescending(oil => oil.OdometerAtChange)
                .ThenByDescending(oil => oil.ChangeDate)
                .FirstOrDefault();
        var isFirstActualChange = latestOilChange is null;
        var odometerAtChange = latestOilChange?.OdometerAtChange ?? currentMileage;
        var nextOdometer = interval > 0 ? odometerAtChange + interval : 0;
        var remainingKm = interval > 0 ? nextOdometer - currentMileage : 0;

        return new OilChangeDto
        {
            VehicleId = vehicle?.Id ?? 0,
            VehiclePlateNumber = vehicle?.PlateNumber ?? string.Empty,
            ChangeDate = DateTime.Today,
            OdometerAtChange = odometerAtChange,
            CurrentOdometer = currentMileage,
            CurrentOdometerDate = DateTime.Today,
            IsOilChanged = isFirstActualChange,
            RecordType = isFirstActualChange ? "تغيير زيت" : "متابعة يومية",
            ServiceItems = latestOilChange?.ServiceItems ?? "زيت فقط",
            OilType = latestOilChange?.OilType ?? string.Empty,
            OilChangeIntervalKm = interval,
            CurrentVehicleMileage = currentMileage,
            NextOilChangeOdometer = nextOdometer,
            RemainingKm = remainingKm,
            OilAlert = interval > 0 ? BuildOilAlertText(remainingKm) : "تغيير الزيت كل كام كم غير مسجلة",
            Status = isFirstActualChange ? "Completed" : "DailyCheck"
        };
    }

    private const decimal OilAlertThresholdKm = 500;

    private static string BuildOilAlertText(decimal remainingKm)
    {
        if (remainingKm < 0)
        {
            return $"تغيير الزيت متأخر بـ {Math.Abs(remainingKm):0} كم";
        }

        return remainingKm <= OilAlertThresholdKm
            ? $"متبقي {remainingKm:0} كم على تغيير الزيت"
            : "لا يوجد إنذار";
    }

    private async void OpenOilChangeEntry(OilChangeDto source)
    {
        SelectTabByTag("OilChanges");
        var window = new OilChangeEntryWindow(_vehicles, source, _oilChanges)
        {
            Owner = this
        };

        if (window.ShowDialog() != true || window.OilChangeForm is null)
        {
            return;
        }

        await RunSafeAsync(async () =>
        {
            await _oilChangeService.SaveAsync(window.OilChangeForm);
            if (window.OilChangeForm.IsOilChanged && window.OilChangeForm.Cost > 0)
            {
                var vehicle = _vehicles.FirstOrDefault(v => v.Id == window.OilChangeForm.VehicleId);
                await _treasuryService.SaveAsync(new TreasuryTransactionFormDto
                {
                    TransactionDate = window.OilChangeForm.ChangeDate,
                    TransactionType = "صرف",
                    Amount = window.OilChangeForm.Cost,
                    Description = $"تغيير زيت - {vehicle?.PlateNumber ?? string.Empty}",
                    RelatedEntityType = "زيت",
                    PaymentMethod = "نقدي"
                });
                await LoadTreasuryIfUnlockedAsync();
            }
            await LoadOilChangesAsync();
            await LoadVehiclesAsync();
            await LoadDashboardAsync();
            await LoadNotificationsAsync();
        });
    }

    private void SaveOilChangeButton_Click(object sender, RoutedEventArgs e)
    {
        var item = SelectOilChangeFromSender(sender);
        if (item is null)
        {
            MessageBox.Show("اختر سجل زيت أولًا.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        OpenOilChangeEntry(item);
    }

    private async void DeleteOilChangeButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(() =>
    {
        SelectOilChangeFromSender(sender);
        return DeleteSelectedAsync(OilChangesGrid, _oilChanges, x => x.Id, _oilChangeService.DeleteAsync, async () =>
        {
            await LoadOilChangesAsync();
            await LoadDashboardAsync();
            await LoadNotificationsAsync();
        });
    });

    private async void RefreshTreasuryButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(LoadTreasuryIfUnlockedAsync);
    private async void AddTreasuryButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsModuleTemporarilyLocked("Treasury"))
        {
            return;
        }

        var window = new TreasuryEntryWindow(new TreasuryTransactionDto
        {
            TransactionDate = DateTime.Today,
            TransactionType = "إيراد",
            PaymentMethod = "نقدي"
        })
        {
            Owner = this
        };

        if (window.ShowDialog() != true || window.TreasuryForm is null)
        {
            return;
        }

        await RunSafeAsync(async () =>
        {
            await _treasuryService.SaveAsync(window.TreasuryForm);
            await LoadTreasuryIfUnlockedAsync();
            await LoadDashboardAsync();
            await LoadNotificationsAsync();
        });
    }

    private async void SaveTreasuryButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsModuleTemporarilyLocked("Treasury"))
        {
            return;
        }

        var item = SelectTreasuryFromSender(sender);
        if (item is null)
        {
            MessageBox.Show("اختر حركة خزينة أولًا.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var window = new TreasuryEntryWindow(item)
        {
            Owner = this
        };

        if (window.ShowDialog() != true || window.TreasuryForm is null)
        {
            return;
        }

        await RunSafeAsync(async () =>
        {
            await _treasuryService.SaveAsync(window.TreasuryForm);
            await LoadTreasuryIfUnlockedAsync();
            await LoadDashboardAsync();
            await LoadNotificationsAsync();
        });
    }

    private async void DeleteTreasuryButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(() =>
    {
        if (IsModuleTemporarilyLocked("Treasury"))
        {
            return Task.CompletedTask;
        }

        SelectTreasuryFromSender(sender);
        return DeleteSelectedAsync(TreasuryGrid, _treasuryTransactions, x => x.Id, _treasuryService.DeleteAsync, LoadTreasuryIfUnlockedAsync);
    });

    private async void RefreshLicensesButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        await LoadVehiclesAsync();
        await LoadLicensesAsync();
    });

    private async void AddVehicleFromLicenseButton_Click(object sender, RoutedEventArgs e)
    {
        var draft = new VehicleLicenseRow
        {
            Year = DateTime.Today.Year,
            OilChangeIntervalKm = 10000,
            RegistrationType = "ترخيص",
            RegistrationStartDate = DateTime.Today,
            RegistrationExpiryDate = DateTime.Today.AddYears(1)
        };

        var window = new VehicleLicenseEntryWindow(draft, RegistrationTypes)
        {
            Owner = this
        };

        if (window.ShowDialog() != true)
        {
            return;
        }

        await RunSafeAsync(async () => await SaveVehicleLicenseRowAsync(window.LicenseRow));
    }

    private async void SaveVehicleLicenseButton_Click(object sender, RoutedEventArgs e)
    {
        var item = SelectVehicleLicenseFromSender(sender);
        if (item is null)
        {
            MessageBox.Show("اختر ترخيص عربية أولًا.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var window = new VehicleLicenseEntryWindow(item, RegistrationTypes)
        {
            Owner = this
        };

        if (window.ShowDialog() != true)
        {
            return;
        }

        await RunSafeAsync(async () => await SaveVehicleLicenseRowAsync(window.LicenseRow));
    }

    private async Task SaveVehicleLicenseRowAsync(VehicleLicenseRow item)
    {
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

        if (item.Year is < 1900 or > 2100)
        {
            throw new InvalidOperationException("سنة الصنع يجب أن تكون رقمًا صحيحًا بين 1900 و2100.");
        }

        if (item.OilChangeIntervalKm <= 0)
        {
            throw new InvalidOperationException("قيمة تغيير الزيت كل كام كم يجب أن تكون أكبر من صفر.");
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
    }

    private async void DeleteVehicleFromLicenseButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        var item = SelectVehicleLicenseFromSender(sender)
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
        var item = SelectVehicleLicenseFromSender(sender)
            ?? throw new InvalidOperationException("اختر ترخيص عربية أولًا.");

        var vehicle = _vehicles.FirstOrDefault(v => v.Id == item.VehicleId)
            ?? throw new InvalidOperationException("العربية غير موجودة.");

        vehicle.RegistrationStartDate = null;
        vehicle.RegistrationExpiryDate = null;
        vehicle.RegistrationType = "ترخيص";
        await _vehicleService.SaveAsync(ToForm(vehicle));
        await LoadVehiclesAsync();
        await LoadLicensesAsync();
        await LoadDashboardAsync();
        await LoadNotificationsAsync();
    });

    private void OpenInsuranceTabButton_Click(object sender, RoutedEventArgs e) => SelectTabByTag("Insurance");

    private async void AddInsuranceForSelectedLicenseButton_Click(object sender, RoutedEventArgs e)
    {
        var item = Selected<VehicleLicenseRow>(LicensesGrid);
        if (item is null)
        {
            MessageBox.Show("اختر عربية من جدول التراخيص قبل إضافة التأمين.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var vehicle = _vehicles.FirstOrDefault(v => v.Id == item.VehicleId);
        if (vehicle is null)
        {
            MessageBox.Show("احفظ العربية أولًا قبل إضافة وثيقة التأمين.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var window = new InsuranceEntryWindow(_vehicles, BuildInsuranceDraft(vehicle))
        {
            Owner = this
        };

        if (window.ShowDialog() != true)
        {
            return;
        }

        await RunSafeAsync(async () =>
        {
            await _insuranceService.SaveAsync(ToForm(window.Insurance));
            await LoadInsuranceAsync();
            await LoadDashboardAsync();
            await LoadNotificationsAsync();
            SelectTabByTag("Insurance");
        });
    }

    private async void RefreshInsuranceButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(LoadInsuranceAsync);
    private async void AddInsuranceButton_Click(object sender, RoutedEventArgs e)
    {
        var vehicle = _vehicles.FirstOrDefault();
        var window = new InsuranceEntryWindow(_vehicles, BuildInsuranceDraft(vehicle))
        {
            Owner = this
        };

        if (window.ShowDialog() != true)
        {
            return;
        }

        await RunSafeAsync(async () =>
        {
            await _insuranceService.SaveAsync(ToForm(window.Insurance));
            await LoadInsuranceAsync();
            await LoadDashboardAsync();
            await LoadNotificationsAsync();
        });
    }

    private static InsuranceDto BuildInsuranceDraft(VehicleDto? vehicle) => new()
    {
        VehicleId = vehicle?.Id ?? 0,
        VehiclePlateNumber = vehicle?.PlateNumber ?? string.Empty,
        VehicleModel = vehicle?.Model ?? string.Empty,
        VehicleYear = vehicle?.Year ?? 0,
        VehicleChassisNumber = vehicle?.ChassisNumber ?? string.Empty,
        VehicleEngineNumber = vehicle?.EngineNumber ?? string.Empty,
        PolicyType = "تأمين حوادث",
        StartDate = DateTime.Today,
        ExpiryDate = DateTime.Today.AddYears(1),
        Status = "Active"
    };

    private async void SaveInsuranceButton_Click(object sender, RoutedEventArgs e)
    {
        var item = SelectInsuranceFromSender(sender);
        if (item is null)
        {
            MessageBox.Show("اختر وثيقة تأمين أولًا.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var window = new InsuranceEntryWindow(_vehicles, item)
        {
            Owner = this
        };

        if (window.ShowDialog() != true)
        {
            return;
        }

        await RunSafeAsync(async () =>
        {
            EnsureVehicleExists(window.Insurance.VehicleId, "وثيقة التأمين");
            await _insuranceService.SaveAsync(ToForm(window.Insurance));
            await LoadInsuranceAsync();
            await LoadDashboardAsync();
            await LoadNotificationsAsync();
        });
    }
    private async void DeleteInsuranceButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(() =>
    {
        SelectInsuranceFromSender(sender);
        return DeleteSelectedAsync(InsuranceGrid, _insurance, x => x.Id, _insuranceService.DeleteAsync, LoadInsuranceAsync);
    });
    private async void AttachInsuranceDocumentButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        var item = SelectInsuranceFromSender(sender) ?? throw new InvalidOperationException("اختر وثيقة تأمين أولًا.");
        var filePath = PickDocumentPath();
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        item.DocumentUrl = filePath;
        await _insuranceService.SaveAsync(ToForm(item));
        await LoadInsuranceAsync();
    });

    private async void RefreshCustodyButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(LoadCustodyIfUnlockedAsync);
    private async void AddCustodyButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsModuleTemporarilyLocked("Custody"))
        {
            return;
        }

        var window = new CustodyEntryWindow(_drivers, _employees)
        {
            Owner = this
        };

        if (window.ShowDialog() != true || window.CustodyForm is null)
        {
            return;
        }

        await RunSafeAsync(async () =>
        {
            // The service posts the treasury disbursement inside the same transaction.
            await _custodyService.SaveAsync(window.CustodyForm);
            await LoadTreasuryIfUnlockedAsync();
            await LoadCustodyIfUnlockedAsync();
            await LoadDashboardAsync();
            await LoadNotificationsAsync();
        });
    }

    private async void SaveCustodyButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsModuleTemporarilyLocked("Custody"))
        {
            return;
        }

        var item = SelectCustodyFromSender(sender);
        if (item is null)
        {
            MessageBox.Show("اختر سجل عهدة أولًا.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var window = new CustodyEntryWindow(_drivers, _employees, item)
        {
            Owner = this
        };

        if (window.ShowDialog() != true || window.CustodyForm is null)
        {
            return;
        }

        await RunSafeAsync(async () =>
        {
            // The service keeps the treasury movement in sync inside the same transaction.
            await _custodyService.SaveAsync(window.CustodyForm);
            await LoadTreasuryIfUnlockedAsync();
            await LoadCustodyIfUnlockedAsync();
            await LoadDashboardAsync();
            await LoadNotificationsAsync();
        });
    }

    private async void ApproveCustodyButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        if (IsModuleTemporarilyLocked("Custody"))
        {
            return;
        }

        var item = SelectCustodyFromSender(sender) ?? throw new InvalidOperationException("اختر سجل عهدة أولًا.");
        var confirmed = MessageBox.Show(
            $"اعتماد تصفية العهدة {item.CustodyNumber} الخاصة بـ {item.CustodianName}؟\n" +
            "سيتم قفل العهدة وتسجيل مصاريفها في الخزينة منسوبة لصاحبها.",
            "اعتماد تصفية العهدة",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmed != MessageBoxResult.Yes)
        {
            return;
        }

        var approver = _currentUser?.FullName;
        if (string.IsNullOrWhiteSpace(approver))
        {
            approver = _currentUser?.Username ?? string.Empty;
        }

        await _custodyService.ApproveClosureAsync(item.Id, approver);
        await LoadCustodyIfUnlockedAsync();
        await LoadTreasuryIfUnlockedAsync();
        await LoadDashboardAsync();
    });

    private async void RejectCustodyButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        if (IsModuleTemporarilyLocked("Custody"))
        {
            return;
        }

        var item = SelectCustodyFromSender(sender) ?? throw new InvalidOperationException("اختر سجل عهدة أولًا.");
        var reasonWindow = new QuickNameEntryWindow(
            "رفض تصفية العهدة",
            $"اكتب سبب رفض تصفية العهدة {item.CustodyNumber} حتى يظهر لأمين العهدة:")
        {
            Owner = this
        };

        if (reasonWindow.ShowDialog() != true || string.IsNullOrWhiteSpace(reasonWindow.EnteredName))
        {
            return;
        }

        var supervisor = _currentUser?.FullName;
        if (string.IsNullOrWhiteSpace(supervisor))
        {
            supervisor = _currentUser?.Username ?? string.Empty;
        }

        await _custodyService.RejectClosureAsync(item.Id, supervisor, reasonWindow.EnteredName.Trim());
        await LoadCustodyIfUnlockedAsync();
    });

    private async void DeleteCustodyButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(() =>
    {
        if (IsModuleTemporarilyLocked("Custody"))
        {
            return Task.CompletedTask;
        }

        SelectCustodyFromSender(sender);
        return DeleteSelectedAsync(CustodyGrid, _custodies, x => x.Id, _custodyService.DeleteAsync, LoadCustodyIfUnlockedAsync);
    });

    private async void AttachCustodyDocumentButton_Click(object sender, RoutedEventArgs e) => await RunSafeAsync(async () =>
    {
        if (IsModuleTemporarilyLocked("Custody"))
        {
            return;
        }

        var item = SelectCustodyFromSender(sender) ?? throw new InvalidOperationException("اختر سجل عهدة أولًا.");
        var filePath = PickDocumentPath();
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        item.DocumentUrl = filePath;
        await _custodyService.SaveAsync(ToForm(item));
        await LoadCustodyIfUnlockedAsync();
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

    private async void ShowReportA5Button_Click(object sender, RoutedEventArgs e) => await OpenCurrentReportPreviewAsync();

    private async void ShowReportRowA5Button_Click(object sender, RoutedEventArgs e) => await OpenSelectedReportRowPreviewAsync(sender);

    private void DeleteReportButton_Click(object sender, RoutedEventArgs e)
    {
        if (ReportsGrid.ItemsSource is not DataView view)
        {
            MessageBox.Show("لا يوجد تقرير معروض للحذف.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if ((sender as FrameworkElement)?.DataContext is DataRowView rowFromAction)
        {
            ReportsGrid.SelectedItem = rowFromAction;
            ReportsGrid.ScrollIntoView(rowFromAction);
        }

        if (ReportsGrid.SelectedItem is DataRowView selectedRow)
        {
            var table = selectedRow.Row.Table;
            selectedRow.Row.Delete();
            table?.AcceptChanges();
            ReportsGrid.SelectedItem = null;
        }
        else
        {
            ReportsGrid.ItemsSource = null;
            _currentReport = null;
            _reportColumnFilters.Clear();
        }
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

    private async void PrintReportPdfButton_Click(object sender, RoutedEventArgs e) => await OpenCurrentReportPreviewAsync();

    private async Task OpenCurrentReportPreviewAsync()
    {
        ReportDataDto? report = null;
        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            if (_currentReport is null || ReportsGrid.ItemsSource is not DataView)
            {
                await GenerateCurrentReportAsync();
            }

            report = BuildDisplayedReportSnapshot(_currentReport!);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"لم يتم تنفيذ العملية:\n{ex.Message}", "تنبيه واضح", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }

        if (report is not null)
        {
            OpenReportPreview(report);
        }
    }

    private async Task OpenSelectedReportRowPreviewAsync(object sender)
    {
        ReportDataDto? report = null;
        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            if (_currentReport is null || ReportsGrid.ItemsSource is not DataView)
            {
                await GenerateCurrentReportAsync();
            }

            var selectedRow = (sender as FrameworkElement)?.DataContext as DataRowView ?? ReportsGrid.SelectedItem as DataRowView;
            if (selectedRow is null)
            {
                throw new InvalidOperationException("اختر سجلًا من التقرير أولًا.");
            }

            report = BuildSelectedReportRowSnapshot(_currentReport!, selectedRow);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"لم يتم تنفيذ العملية:\n{ex.Message}", "تنبيه واضح", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }

        if (report is not null)
        {
            OpenReportPreview(report);
        }
    }

    private void OpenReportPreview(ReportDataDto report)
    {
        Mouse.OverrideCursor = null;
        var window = new ReportPreviewWindow(report.ReportTitle, BuildReportDocument(report))
        {
            Owner = this
        };
        window.ShowDialog();
    }

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
        _reportColumnFilters.Clear();
        var filter = BuildCurrentReportFilter();
        await RefreshReportSourceAsync(filter.ReportType);
        var report = BuildCurrentTableReportSnapshot(filter);
        _currentReport = report;
        ReportsGrid.ItemsSource = ToDataTable(report).DefaultView;
        return report;
    }

    private async Task RefreshReportSourceAsync(string reportType)
    {
        var normalized = (reportType ?? string.Empty).Trim().ToLowerInvariant();
        switch (normalized)
        {
            case "alltrips":
                await LoadTripsAsync();
                break;
            case "fuel":
                await LoadFuelAsync();
                break;
            case "vehiclelicenses":
                await LoadVehiclesAsync();
                await LoadLicensesAsync();
                break;
            case "insurance":
                await LoadInsuranceAsync();
                break;
            case "oilchanges":
                await LoadVehiclesAsync();
                await LoadOilChangesAsync();
                break;
            case "vehicles":
            default:
                await LoadVehiclesAsync();
                await LoadInsuranceAsync();
                break;
        }
    }

    private ReportDataDto BuildCurrentTableReportSnapshot(ReportFilterDto filter)
    {
        var reportType = (filter.ReportType ?? string.Empty).Trim().ToLowerInvariant();
        return reportType switch
        {
            "alltrips" => BuildTripsReportSnapshot(
                _trips.Where(trip =>
                        (!filter.VehicleId.HasValue || trip.VehicleId == filter.VehicleId.Value) &&
                        ReportDateMatches(trip.StartDate, filter))
                    .ToList(),
                GetSelectedTripsReportTitle(filter.VehicleId)),
            "fuel" => BuildFuelReportSnapshot(_fuel.Where(fuel =>
                    (!filter.VehicleId.HasValue || fuel.VehicleId == filter.VehicleId.Value) &&
                    ReportDateMatches(fuel.TransactionDate, filter))),
            "vehiclelicenses" => BuildVehicleLicensesReportSnapshot(_vehicleLicenses.Where(license =>
                ReportAnyDateMatches(license.RegistrationStartDate, license.RegistrationExpiryDate, filter)), "تقرير تراخيص العربيات"),
            "insurance" => BuildInsuranceReportSnapshot(_insurance.Where(insurance =>
                ReportAnyDateMatches(insurance.StartDate, insurance.ExpiryDate, filter)), "تقرير التأمينات"),
            "oilchanges" => BuildOilChangesReportSnapshot(_oilChanges),
            "vehicles" or _ => BuildVehiclesReportSnapshot(_vehicles.Where(vehicle =>
                ReportAnyDateMatches(vehicle.RegistrationStartDate, vehicle.RegistrationExpiryDate, filter)))
        };
    }

    private string GetSelectedTripsReportTitle(int? vehicleId)
    {
        var vehicleName = vehicleId.HasValue
            ? _vehicles.FirstOrDefault(vehicle => vehicle.Id == vehicleId.Value)?.PlateNumber
            : string.Empty;

        return string.IsNullOrWhiteSpace(vehicleName)
            ? "تقرير جميع التشغيلات"
            : $"تقرير تشغيلات العربية {vehicleName}";
    }

    private ReportFilterDto BuildCurrentReportFilter()
    {
        var selectedItem = ReportTypeComboBox.SelectedItem as ComboBoxItem;
        var reportType = selectedItem?.Tag?.ToString() ?? "vehicles";
        var usesVehicleFilter = string.Equals(reportType, "fuel", StringComparison.OrdinalIgnoreCase);
        var usesDateFilter = !string.Equals(reportType, "oilchanges", StringComparison.OrdinalIgnoreCase);
        int? vehicleId = usesVehicleFilter && ReportVehicleComboBox.SelectedValue is int selectedVehicleId
            ? selectedVehicleId
            : null;

        return new ReportFilterDto
        {
            ReportType = reportType,
            VehicleId = vehicleId,
            StartDate = usesDateFilter ? ReportStartDatePicker.SelectedDate : null,
            EndDate = usesDateFilter ? ReportEndDatePicker.SelectedDate : null
        };
    }

    private void ReportTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _currentReport = null;
        _reportColumnFilters.Clear();
        UpdateReportVehicleFilterVisibility();
    }

    private void ClearReportVehicleFilterButton_Click(object sender, RoutedEventArgs e)
    {
        ReportVehicleComboBox.SelectedItem = null;
        ReportVehicleComboBox.SelectedValue = null;
    }

    private void UpdateReportVehicleFilterVisibility()
    {
        if (ReportTypeComboBox is null || ReportVehicleFilterPanel is null)
        {
            return;
        }

        var selectedItem = ReportTypeComboBox.SelectedItem as ComboBoxItem;
        var reportType = selectedItem?.Tag?.ToString();
        ReportVehicleFilterPanel.Visibility =
            string.Equals(reportType, "fuel", StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible
            : Visibility.Collapsed;

        var dateFilterVisibility = string.Equals(reportType, "oilchanges", StringComparison.OrdinalIgnoreCase)
            ? Visibility.Collapsed
            : Visibility.Visible;
        ReportStartDatePanel.Visibility = dateFilterVisibility;
        ReportEndDatePanel.Visibility = dateFilterVisibility;

        var dateFilterPrefix = reportType?.ToLowerInvariant() switch
        {
            "vehicles" or "vehiclelicenses" => "تاريخ الترخيص",
            "insurance" => "تاريخ التأمين",
            "alltrips" => "تاريخ التشغيل",
            "fuel" => "تاريخ البنزين",
            _ => "التاريخ"
        };

        if (ReportStartDateLabel is not null)
        {
            ReportStartDateLabel.Text = $"{dateFilterPrefix} من";
        }

        if (ReportEndDateLabel is not null)
        {
            ReportEndDateLabel.Text = $"{dateFilterPrefix} إلى";
        }
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

    private sealed record UserRoleOption(string Value, string DisplayName);
}
