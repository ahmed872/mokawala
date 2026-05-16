// ============================================================================
// CORE PROJECT DTO CLASSES
// LOCATION: FleetManagementSystem.Core/DTOs/
// ============================================================================

using System;
using System.Collections.Generic;

namespace FleetManagementSystem.Core.DTOs
{
    // ========================================================================
    // COMMON / INFRASTRUCTURE DTOs
    // ========================================================================

    /// <summary>
    /// Generic paginated result DTO for list operations.
    /// </summary>
    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// Generic filter DTO for list operations.
    /// </summary>
    public class FilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchText { get; set; } = string.Empty;
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }

    /// <summary>
    /// API response wrapper DTO.
    /// </summary>
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    // ========================================================================
    // USER DTOs
    // ========================================================================

    /// <summary>
    /// User DTO for login and authentication.
    /// </summary>
    public class UserLoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// User DTO for authentication response.
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = "Staff";
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> AllowedModules { get; set; } = new List<string>();
    }

    /// <summary>
    /// User DTO for list view.
    /// </summary>
    public class UserListItemDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    /// <summary>
    /// User DTO for create/edit form.
    /// </summary>
    public class UserFormDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string Role { get; set; } = "Staff";
        public bool IsActive { get; set; } = true;
    }

    // ========================================================================
    // COMPANY SETTINGS DTOs
    // ========================================================================

    /// <summary>
    /// Company settings DTO.
    /// </summary>
    public class CompanySettingsDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyNameEn { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string CommercialRegistration { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = "ر.س";
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string TimeFormat { get; set; } = "HH:mm:ss";
        public int DecimalPlaces { get; set; } = 2;
        public string DefaultLanguage { get; set; } = "ar";
        public string DefaultTheme { get; set; } = "Light";
    }

    // ========================================================================
    // AUDIT LOG DTOs
    // ========================================================================

    /// <summary>
    /// Audit log DTO for list view.
    /// </summary>
    public class AuditLogDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string OldValues { get; set; } = string.Empty;
        public string NewValues { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    // ========================================================================
    // NOTIFICATION DTOs
    // ========================================================================

    /// <summary>
    /// Notification DTO.
    /// </summary>
    public class NotificationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "Info";
        public string RelatedEntityType { get; set; } = string.Empty;
        public int? RelatedEntityId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ========================================================================
    // MASTER DATA DTOs (LOOKUP)
    // ========================================================================

    /// <summary>
    /// Vehicle type lookup DTO for dropdowns.
    /// </summary>
    public class VehicleTypeLookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
    }

    /// <summary>
    /// Vehicle type DTO for list/form.
    /// </summary>
    public class VehicleTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Contract status lookup DTO for dropdowns.
    /// </summary>
    public class ContractStatusLookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string Color { get; set; } = "#000000";
    }

    /// <summary>
    /// Contract status DTO for list/form.
    /// </summary>
    public class ContractStatusDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = "#000000";
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Maintenance type lookup DTO for dropdowns.
    /// </summary>
    public class MaintenanceTypeLookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
    }

    /// <summary>
    /// Maintenance type DTO for list/form.
    /// </summary>
    public class MaintenanceTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public int EstimatedDurationDays { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Service provider lookup DTO for dropdowns.
    /// </summary>
    public class ServiceProviderLookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
    }

    /// <summary>
    /// Service provider DTO for list/form.
    /// </summary>
    public class ServiceProviderDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public decimal AverageRating { get; set; }
        public bool IsActive { get; set; }
    }

    // ========================================================================
    // VEHICLE DTOs
    // ========================================================================

    /// <summary>
    /// Vehicle lookup DTO for dropdowns.
    /// </summary>
    public class VehicleLookupDto
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
    }

    /// <summary>
    /// Vehicle DTO for list view.
    /// </summary>
    public class VehicleListItemDto
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Status { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public decimal CurrentMileage { get; set; }
        public DateTime? RegistrationExpiryDate { get; set; }
    }

    /// <summary>
    /// Vehicle DTO for detail view.
    /// </summary>
    public class VehicleDto
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public int VehicleTypeId { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Manufacturer { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string ChassisNumber { get; set; } = string.Empty;
        public string EngineNumber { get; set; } = string.Empty;
        public decimal CurrentMileage { get; set; }
        public string Status { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime? RegistrationStartDate { get; set; }
        public DateTime? RegistrationExpiryDate { get; set; }
        public string AccidentInsuranceDetails { get; set; } = string.Empty;
        public string SocialInsuranceDetails { get; set; } = string.Empty;
        public decimal OilChangeIntervalKm { get; set; }
        public decimal MaintenanceIntervalKm { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Vehicle DTO for create/edit form.
    /// </summary>
    public class VehicleFormDto
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public int VehicleTypeId { get; set; }
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Manufacturer { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string ChassisNumber { get; set; } = string.Empty;
        public string EngineNumber { get; set; } = string.Empty;
        public decimal CurrentMileage { get; set; }
        public string Status { get; set; } = "Active";
        public string AssignedTo { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime? RegistrationStartDate { get; set; }
        public DateTime? RegistrationExpiryDate { get; set; }
        public string AccidentInsuranceDetails { get; set; } = string.Empty;
        public string SocialInsuranceDetails { get; set; } = string.Empty;
        public decimal OilChangeIntervalKm { get; set; } = 10000;
        public decimal MaintenanceIntervalKm { get; set; } = 15000;
        public string Notes { get; set; } = string.Empty;
    }

    // ========================================================================
    // CONTRACT DTOs
    // ========================================================================

    /// <summary>
    /// Contract DTO for list view.
    /// </summary>
    public class ContractListItemDto
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string VehiclePlateNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal ContractValue { get; set; }
        public decimal PaidAmount { get; set; }
        public bool IsExpiring { get; set; }
    }

    /// <summary>
    /// Contract DTO for detail view.
    /// </summary>
    public class ContractDto
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public string VehiclePlateNumber { get; set; } = string.Empty;
        public int ContractStatusId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientPhoneNumber { get; set; } = string.Empty;
        public string ClientAddress { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal ContractValue { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentTerms { get; set; } = string.Empty;
        public string ContractTerms { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Contract DTO for create/edit form.
    /// </summary>
    public class ContractFormDto
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public int ContractStatusId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientPhoneNumber { get; set; } = string.Empty;
        public string ClientAddress { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal ContractValue { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentTerms { get; set; } = string.Empty;
        public string ContractTerms { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    // ========================================================================
    // MAINTENANCE REQUEST DTOs
    // ========================================================================

    /// <summary>
    /// Maintenance request DTO for list view.
    /// </summary>
    public class MaintenanceRequestListItemDto
    {
        public int Id { get; set; }
        public string VehiclePlateNumber { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal ActualCost { get; set; }
        public string ServiceProvider { get; set; } = string.Empty;
    }

    /// <summary>
    /// Maintenance request DTO for detail view.
    /// </summary>
    public class MaintenanceRequestDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string VehiclePlateNumber { get; set; } = string.Empty;
        public int MaintenanceTypeId { get; set; }
        public string MaintenanceType { get; set; } = string.Empty;
        public int? ServiceProviderId { get; set; }
        public string ServiceProvider { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public decimal ActualCost { get; set; }
        public string WorkPerformed { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Maintenance request DTO for create/edit form.
    /// </summary>
    public class MaintenanceRequestFormDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int MaintenanceTypeId { get; set; }
        public int? ServiceProviderId { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string Status { get; set; } = "Open";
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public decimal ActualCost { get; set; }
        public string WorkPerformed { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
    }

    // ========================================================================
    // DRIVER DTOs
    // ========================================================================

    /// <summary>
    /// Driver lookup DTO for dropdowns.
    /// </summary>
    public class DriverLookupDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// Driver DTO for list view.
    /// </summary>
    public class DriverListItemDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime LicenseExpiryDate { get; set; }
        public bool IsLicenseExpiring { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Driver DTO for detail view.
    /// </summary>
    public class DriverDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime LicenseExpiryDate { get; set; }
        public string LicenseType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Driver DTO for create/edit form.
    /// </summary>
    public class DriverFormDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime LicenseExpiryDate { get; set; }
        public string LicenseType { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string Notes { get; set; } = string.Empty;
    }

    // ========================================================================
    // EMPLOYEE DTOs
    // ========================================================================

    /// <summary>
    /// Employee lookup DTO for dropdowns.
    /// </summary>
    public class EmployeeLookupDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
    }

    /// <summary>
    /// Employee DTO for list view.
    /// </summary>
    public class EmployeeListItemDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
    }

    /// <summary>
    /// Employee DTO for detail view.
    /// </summary>
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Employee DTO for create/edit form.
    /// </summary>
    public class EmployeeFormDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string Status { get; set; } = "Active";
        public string Notes { get; set; } = string.Empty;
    }

    // ========================================================================
    // TRIP DTOs
    // ========================================================================

    /// <summary>
    /// Trip DTO for list view.
    /// </summary>
    public class TripListItemDto
    {
        public int Id { get; set; }
        public string VehiclePlateNumber { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Trip DTO for detail view.
    /// </summary>
    public class TripDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string VehiclePlateNumber { get; set; } = string.Empty;
        public int? DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public int? RequesterEmployeeId { get; set; }
        public string RequesterNameText { get; set; } = string.Empty;
        public string RequesterName { get; set; } = string.Empty;
        public int? SupervisorEmployeeId { get; set; }
        public string SupervisorName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;
        public decimal StartMileage { get; set; }
        public decimal? EndMileage { get; set; }
        public decimal Distance { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal FuelConsumed { get; set; }
        public decimal TripCost { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Trip DTO for create/edit form.
    /// </summary>
    public class TripFormDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int? DriverId { get; set; }
        public int? RequesterEmployeeId { get; set; }
        public string RequesterNameText { get; set; } = string.Empty;
        public int? SupervisorEmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;
        public decimal StartMileage { get; set; }
        public decimal? EndMileage { get; set; }
        public decimal Distance { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string Status { get; set; } = "Planned";
        public decimal FuelConsumed { get; set; }
        public decimal TripCost { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    // ========================================================================
    // FUEL TRANSACTION DTOs
    // ========================================================================

    /// <summary>
    /// Fuel transaction DTO for list view.
    /// </summary>
    public class FuelTransactionListItemDto
    {
        public int Id { get; set; }
        public string VehiclePlateNumber { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal TotalCost { get; set; }
        public string FuelStation { get; set; } = string.Empty;
    }

    /// <summary>
    /// Fuel transaction DTO for detail view.
    /// </summary>
    public class FuelTransactionDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string VehiclePlateNumber { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalCost { get; set; }
        public string FuelStation { get; set; } = string.Empty;
        public decimal Odometer { get; set; }
        public bool PaidFromTreasury { get; set; }
        public int? TreasuryTransactionId { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Fuel transaction DTO for create/edit form.
    /// </summary>
    public class FuelTransactionFormDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalCost { get; set; }
        public string FuelStation { get; set; } = string.Empty;
        public decimal Odometer { get; set; }
        public bool PaidFromTreasury { get; set; }
        public int? TreasuryTransactionId { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    // ========================================================================
    // EXPENSE DTOs
    // ========================================================================

    /// <summary>
    /// Expense DTO for list view.
    /// </summary>
    public class ExpenseListItemDto
    {
        public int Id { get; set; }
        public string VehiclePlateNumber { get; set; } = string.Empty;
        public DateTime ExpenseDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Vendor { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Expense DTO for detail view.
    /// </summary>
    public class ExpenseDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string VehiclePlateNumber { get; set; } = string.Empty;
        public DateTime ExpenseDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string ReceiptUrl { get; set; } = string.Empty;
        public bool PaidFromTreasury { get; set; }
        public int? TreasuryTransactionId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Expense DTO for create/edit form.
    /// </summary>
    public class ExpenseFormDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string ReceiptUrl { get; set; } = string.Empty;
        public bool PaidFromTreasury { get; set; }
        public int? TreasuryTransactionId { get; set; }
        public string Status { get; set; } = "Pending";
        public string Notes { get; set; } = string.Empty;
    }

    // ========================================================================
    // OIL CHANGE DTOs
    // ========================================================================

    public class OilChangeDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string VehiclePlateNumber { get; set; } = string.Empty;
        public DateTime ChangeDate { get; set; }
        public decimal OdometerAtChange { get; set; }
        public string OilType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Cost { get; set; }
        public decimal NextOilChangeOdometer { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsDue { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class OilChangeFormDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime ChangeDate { get; set; }
        public decimal OdometerAtChange { get; set; }
        public string OilType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Cost { get; set; }
        public decimal NextOilChangeOdometer { get; set; }
        public string Status { get; set; } = "Scheduled";
        public string Notes { get; set; } = string.Empty;
    }

    // ========================================================================
    // TREASURY DTOs
    // ========================================================================

    public class TreasuryTransactionDto
    {
        public int Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string RelatedEntityType { get; set; } = string.Empty;
        public int? RelatedEntityId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class TreasuryTransactionFormDto
    {
        public int Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } = "Out";
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string RelatedEntityType { get; set; } = string.Empty;
        public int? RelatedEntityId { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string Notes { get; set; } = string.Empty;
    }

    // ========================================================================
    // LICENSE DTOs
    // ========================================================================

    /// <summary>
    /// License DTO for list view.
    /// </summary>
    public class LicenseListItemDto
    {
        public int Id { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string LicenseType { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsExpiring { get; set; }
    }

    /// <summary>
    /// License DTO for detail view.
    /// </summary>
    public class LicenseDto
    {
        public int Id { get; set; }
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string LicenseType { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string IssuingAuthority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// License DTO for create/edit form.
    /// </summary>
    public class LicenseFormDto
    {
        public int Id { get; set; }
        public int DriverId { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public string LicenseType { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string IssuingAuthority { get; set; } = string.Empty;
        public string Status { get; set; } = "Valid";
        public string Notes { get; set; } = string.Empty;
    }

    // ========================================================================
    // INSURANCE DTOs
    // ========================================================================

    /// <summary>
    /// Insurance DTO for list view.
    /// </summary>
    public class InsuranceListItemDto
    {
        public int Id { get; set; }
        public string VehiclePlateNumber { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public string InsuranceCompany { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public decimal PremiumAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsExpiring { get; set; }
    }

    /// <summary>
    /// Insurance DTO for detail view.
    /// </summary>
    public class InsuranceDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string VehiclePlateNumber { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public string InsuranceCompany { get; set; } = string.Empty;
        public string PolicyType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal CoverageAmount { get; set; }
        public string CoverageDetails { get; set; } = string.Empty;
        public string AgentName { get; set; } = string.Empty;
        public string AgentPhoneNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public int DaysUntilExpiry => (ExpiryDate.Date - DateTime.Today).Days;
        public bool IsExpiringSoon => DaysUntilExpiry >= 0 && DaysUntilExpiry <= 30;
        public string ExpiryAlert => DaysUntilExpiry switch
        {
            < 0 => "منتهي",
            <= 30 => $"ينتهي خلال {DaysUntilExpiry} يوم",
            _ => "ساري"
        };
    }

    /// <summary>
    /// Insurance DTO for create/edit form.
    /// </summary>
    public class InsuranceFormDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string InsuranceCompany { get; set; } = string.Empty;
        public string PolicyType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal CoverageAmount { get; set; }
        public string CoverageDetails { get; set; } = string.Empty;
        public string AgentName { get; set; } = string.Empty;
        public string AgentPhoneNumber { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public string DocumentUrl { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    // ========================================================================
    // CUSTODY DTOs
    // ========================================================================

    /// <summary>
    /// Custody DTO for list view.
    /// </summary>
    public class CustodyListItemDto
    {
        public int Id { get; set; }
        public string CustodyNumber { get; set; } = string.Empty;
        public string VehiclePlateNumber { get; set; } = string.Empty;
        public string CustodianName { get; set; } = string.Empty;
        public DateTime HandoverDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Custody DTO for detail view.
    /// </summary>
    public class CustodyDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string VehiclePlateNumber { get; set; } = string.Empty;
        public string CustodyNumber { get; set; } = string.Empty;
        public string CustodianName { get; set; } = string.Empty;
        public string CustodianPosition { get; set; } = string.Empty;
        public DateTime HandoverDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal VehicleConditionRating { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public List<CustodyItemDto> Items { get; set; } = new List<CustodyItemDto>();
    }

    /// <summary>
    /// Custody DTO for create/edit form.
    /// </summary>
    public class CustodyFormDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string CustodyNumber { get; set; } = string.Empty;
        public string CustodianName { get; set; } = string.Empty;
        public string CustodianPosition { get; set; } = string.Empty;
        public DateTime HandoverDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = "Active";
        public decimal VehicleConditionRating { get; set; } = 5;
        public string Notes { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Custody item DTO.
    /// </summary>
    public class CustodyItemDto
    {
        public int Id { get; set; }
        public int CustodyId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemDescription { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public bool IsReturned { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Condition { get; set; } = "Good";
    }

    // ========================================================================
    // DASHBOARD AND REPORTING DTOs
    // ========================================================================

    /// <summary>
    /// Dashboard metrics DTO.
    /// </summary>
    public class DashboardMetricsDto
    {
        public int TotalVehicles { get; set; }
        public int AvailableVehicles { get; set; }
        public int ActiveVehicles { get; set; }
        public int VehiclesInTrip { get; set; }
        public int VehiclesInMaintenance { get; set; }
        public int TotalContracts { get; set; }
        public int ActiveContracts { get; set; }
        public int ExpiringContracts { get; set; }
        public int OpenTrips { get; set; }
        public int TripsToday { get; set; }
        public int OpenMaintenanceRequests { get; set; }
        public int CompletedMaintenanceRequests { get; set; }
        public int VehicleLicensesExpiring { get; set; }
        public int DriversWithExpiringLicenses { get; set; }
        public int InsurancePoliciesExpiring { get; set; }
        public int OilChangesDue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalFuelCost { get; set; }
        public decimal TreasuryBalance { get; set; }
        public List<RecentActivityDto> RecentActivities { get; set; } = new List<RecentActivityDto>();
        public List<AlertDto> Alerts { get; set; } = new List<AlertDto>();
    }

    /// <summary>
    /// Recent activity DTO for dashboard.
    /// </summary>
    public class RecentActivityDto
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Alert DTO for dashboard.
    /// </summary>
    public class AlertDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // Warning, Error, Info
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string RelatedEntityType { get; set; } = string.Empty;
        public int? RelatedEntityId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Report filter DTO.
    /// </summary>
    public class ReportFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 100;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReportType { get; set; } = string.Empty; // Vehicles, Contracts, Maintenance, etc.
        public string Status { get; set; } = string.Empty;
        public int? VehicleId { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }

    /// <summary>
    /// Report data DTO for export.
    /// </summary>
    public class ReportDataDto
    {
        public string ReportTitle { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
        public List<Dictionary<string, object>> Data { get; set; } = new List<Dictionary<string, object>>();
        public List<string> Columns { get; set; } = new List<string>();
    }
}
