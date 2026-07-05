namespace FleetManagementSystem.Core.Enums;

public enum UserRole
{
    Admin,
    OperationsManager,
    OperationsDataEntry,
    MaintenanceOfficer,
    TreasuryOfficer,
    TripsLicensesOfficer,
    InsuranceOfficer,
    Viewer,
    Staff,

    /// <summary>
    /// A driver or employee who holds financial custodies: sees only their own custodies
    /// ("عهدتي") and records receipt-backed settlements against them.
    /// </summary>
    Custodian
}

public enum VehicleStatus
{
    Active,
    Inactive,
    Maintenance,
    Retired
}

public enum ContractStatusEnum
{
    Draft,
    Active,
    Expired,
    Terminated
}

public enum MaintenanceStatusEnum
{
    Open,
    InProgress,
    Completed,
    Cancelled
}

public enum TripStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled
}

public enum NotificationType
{
    Info,
    Warning,
    Error,
    Success
}

public enum ExpenseCategory
{
    Fuel,
    Maintenance,
    Insurance,
    Registration,
    Other
}

public enum LicenseStatus
{
    Valid,
    Expiring,
    Expired
}

public enum InsuranceStatus
{
    Active,
    Expiring,
    Expired
}

public enum CustodyStatus
{
    Assigned,
    Returned,
    Lost,
    Damaged
}
