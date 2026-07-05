using FleetManagementSystem.Core.DTOs;

namespace FleetManagementSystem.Services;

public interface IDataBootstrapService
{
    Task InitializeAsync();
}

public interface IAuthenticationService
{
    Task<UserDto?> AuthenticateAsync(UserLoginDto dto);
    Task<List<UserListItemDto>> GetUsersAsync();
    Task<UserDto> SaveUserAsync(UserFormDto dto);
    Task ToggleUserStatusAsync(int id, bool isActive);
}

public interface IVehicleService
{
    Task<List<VehicleDto>> GetAllAsync(string? search = null);
    Task<VehicleDto?> GetByIdAsync(int id);
    Task<VehicleDto> SaveAsync(VehicleFormDto dto);
    Task DeleteAsync(int id);
    Task<List<VehicleLookupDto>> GetLookupAsync();
}

public interface IContractService
{
    Task<List<ContractDto>> GetAllAsync(string? status = null);
    Task<ContractDto?> GetByIdAsync(int id);
    Task<ContractDto> SaveAsync(ContractFormDto dto);
    Task DeleteAsync(int id);
}

public interface IMaintenanceService
{
    Task<List<MaintenanceRequestDto>> GetAllAsync(string? status = null);
    Task<MaintenanceRequestDto?> GetByIdAsync(int id);
    Task<MaintenanceRequestDto> SaveAsync(MaintenanceRequestFormDto dto);
    Task DeleteAsync(int id);
}

public interface IDriverService
{
    Task<List<DriverDto>> GetAllAsync(string? search = null);
    Task<DriverDto?> GetByIdAsync(int id);
    Task<DriverDto> SaveAsync(DriverFormDto dto);
    Task DeleteAsync(int id);
    Task<List<DriverLookupDto>> GetLookupAsync();
}

public interface IDriverAttendanceService
{
    Task<List<DriverAttendanceDto>> GetWeekAsync(DateTime weekStart);
    Task<List<DriverAttendanceDto>> SaveWeekAsync(DateTime weekStart, IEnumerable<DriverAttendanceFormDto> records);
    Task<List<DriverReportDto>> GenerateReportAsync(DateTime startDate, DateTime endDate, int? driverId = null);
    Task<List<DriverAttendanceDto>> GetDriverAbsencesAsync(int driverId, DateTime from, DateTime to);
}

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetAllAsync(string? search = null);
    Task<EmployeeDto?> GetByIdAsync(int id);
    Task<EmployeeDto> SaveAsync(EmployeeFormDto dto);
    Task DeleteAsync(int id);
    Task<List<EmployeeLookupDto>> GetLookupAsync();
}

public interface ITripService
{
    Task<List<TripDto>> GetAllAsync(int? vehicleId = null);
    Task<TripDto?> GetByIdAsync(int id);
    Task<TripDto> SaveAsync(TripFormDto dto);
    Task DeleteAsync(int id);
}

public interface IFuelService
{
    Task<List<FuelTransactionDto>> GetAllAsync();
    Task<FuelTransactionDto?> GetByIdAsync(int id);
    Task<FuelTransactionDto> SaveAsync(FuelTransactionFormDto dto);
    Task DeleteAsync(int id);
}

public interface IExpenseService
{
    Task<List<ExpenseDto>> GetAllAsync();
    Task<ExpenseDto?> GetByIdAsync(int id);
    Task<ExpenseDto> SaveAsync(ExpenseFormDto dto);
    Task DeleteAsync(int id);
}

public interface IOilChangeService
{
    Task<List<OilChangeDto>> GetAllAsync();
    Task<OilChangeDto?> GetByIdAsync(int id);
    Task<OilChangeDto> SaveAsync(OilChangeFormDto dto);
    Task DeleteAsync(int id);
}

public interface ITreasuryService
{
    Task<List<TreasuryTransactionDto>> GetAllAsync();
    Task<TreasuryTransactionDto?> GetByIdAsync(int id);
    Task<TreasuryTransactionDto> SaveAsync(TreasuryTransactionFormDto dto);
    Task DeleteAsync(int id);
    Task<decimal> GetCurrentBalanceAsync();
}

public interface ILicenseService
{
    Task<List<LicenseDto>> GetAllAsync();
    Task<LicenseDto?> GetByIdAsync(int id);
    Task<LicenseDto> SaveAsync(LicenseFormDto dto);
    Task DeleteAsync(int id);
}

public interface IInsuranceService
{
    Task<List<InsuranceDto>> GetAllAsync();
    Task<InsuranceDto?> GetByIdAsync(int id);
    Task<InsuranceDto> SaveAsync(InsuranceFormDto dto);
    Task DeleteAsync(int id);
}

public interface ICustodyService
{
    Task<List<CustodyDto>> GetAllAsync();
    Task<CustodyDto?> GetByIdAsync(int id);
    Task<CustodyDto> SaveAsync(CustodyFormDto dto);
    Task DeleteAsync(int id);

    /// <summary>
    /// "عهدتي" self-service query: active custodies held by the given custodian.
    /// Exactly one of <paramref name="driverId"/>/<paramref name="employeeId"/> must be provided.
    /// </summary>
    Task<List<CustodyDto>> GetActiveForCustodianAsync(int? driverId, int? employeeId);

    /// <summary>
    /// Posts an expense settlement (تسوية عهدة) against a custody's remaining balance.
    /// Requires a verified scanned receipt; rejects amounts exceeding the remaining balance.
    /// </summary>
    Task<CustodyDto> SettleAsync(CustodySettlementFormDto dto);

    /// <summary>Removes an erroneous settlement and restores the settled amount to the custody balance.</summary>
    Task DeleteSettlementAsync(int settlementId);

    /// <summary>Custodies fully consumed by settlements, awaiting the treasury supervisor's decision.</summary>
    Task<List<CustodyDto>> GetPendingApprovalAsync();

    /// <summary>
    /// Treasury supervisor approval: closes the custody as Settled and, for treasury-funded
    /// custodies, posts the liquidation to the treasury — one reversal income plus one expense
    /// entry per settlement attributed to the custodian — atomically.
    /// </summary>
    Task<CustodyDto> ApproveClosureAsync(int custodyId, string approvedBy);

    /// <summary>Treasury supervisor rejection: reopens the custody with the reason recorded in its notes.</summary>
    Task<CustodyDto> RejectClosureAsync(int custodyId, string rejectedBy, string reason);
}

/// <summary>
/// Stores scanned financial documents (invoices/receipts) under the local Uploads/Receipts
/// directory and returns the stored relative path. Validates type (PNG/JPEG/PDF by extension
/// and file signature), non-empty content, and size before accepting the payload.
/// </summary>
public interface IReceiptFileService
{
    Task<string> SaveReceiptAsync(string sourceFilePath, CancellationToken cancellationToken = default);

    /// <summary>Resolves a stored relative receipt path to an absolute path on disk.</summary>
    string ResolveAbsolutePath(string storedReceiptPath);
}

public interface IMasterDataService
{
    Task<List<VehicleTypeDto>> GetVehicleTypesAsync();
    Task<VehicleTypeDto> SaveVehicleTypeAsync(VehicleTypeDto dto);
    Task DeleteVehicleTypeAsync(int id);

    Task<List<ContractStatusDto>> GetContractStatusesAsync();
    Task<ContractStatusDto> SaveContractStatusAsync(ContractStatusDto dto);
    Task DeleteContractStatusAsync(int id);

    Task<List<MaintenanceTypeDto>> GetMaintenanceTypesAsync();
    Task<MaintenanceTypeDto> SaveMaintenanceTypeAsync(MaintenanceTypeDto dto);
    Task DeleteMaintenanceTypeAsync(int id);

    Task<List<ServiceProviderDto>> GetServiceProvidersAsync();
    Task<ServiceProviderDto> SaveServiceProviderAsync(ServiceProviderDto dto);
    Task DeleteServiceProviderAsync(int id);
}

public interface IAuditService
{
    Task LogActionAsync(string action, string entityType, int entityId, string? oldValues = null, string? newValues = null, int? userId = null, string? userName = null, string? ipAddress = null);
    Task<List<AuditLogDto>> GetRecentAsync(int count = 50);
}

public interface INotificationService
{
    Task CreateNotificationAsync(string title, string message, string type, string? relatedEntityType = null, int? relatedEntityId = null, int? userId = null, DateTime? expiresAt = null);
    Task<List<NotificationDto>> GetActiveNotificationsAsync();
    Task MarkAsReadAsync(int id);
}

public interface IReportingService
{
    Task<DashboardMetricsDto> GetDashboardMetricsAsync();
    Task<List<AlertDto>> GetAlertsAsync();
    Task<List<RecentActivityDto>> GetRecentActivityAsync(int count = 10);
    Task<ReportDataDto> GenerateReportAsync(ReportFilterDto filter);
}

public interface ISettingsService
{
    Task<CompanySettingsDto> GetSettingsAsync();
    Task<CompanySettingsDto> SaveSettingsAsync(CompanySettingsDto dto);
}
