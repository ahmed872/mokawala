using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Core.Enums;
using FleetManagementSystem.Data.Entities;
using ServiceProviderEntity = FleetManagementSystem.Data.Entities.ServiceProvider;

namespace FleetManagementSystem.Services;

internal static class ServiceHelpers
{
    public const decimal DefaultOilAlertThresholdKm = 500;

    public static string Clean(string? value) => value?.Trim() ?? string.Empty;

    public static DateTime OrToday(DateTime value) => value == default ? DateTime.Today : value;

    public static DateTime? OrNull(DateTime? value) => value is null || value == default ? null : value;

    public static decimal Distance(decimal startMileage, decimal? endMileage, decimal distance)
    {
        if (endMileage.HasValue && endMileage.Value >= startMileage)
        {
            return endMileage.Value - startMileage;
        }

        return distance > 0 ? distance : 0;
    }

    public static string LicenseStatus(DateTime expiryDate)
    {
        if (expiryDate.Date < DateTime.Today)
        {
            return "Expired";
        }

        if (expiryDate.Date <= DateTime.Today.AddDays(30))
        {
            return "Expiring";
        }

        return "Valid";
    }

    public static string InsuranceStatus(DateTime expiryDate)
    {
        if (expiryDate.Date < DateTime.Today)
        {
            return "Expired";
        }

        return "Active";
    }

    public static string ContractHealth(DateTime endDate)
    {
        if (endDate.Date < DateTime.Today)
        {
            return "Expired";
        }

        if (endDate.Date <= DateTime.Today.AddDays(30))
        {
            return "Expiring";
        }

        return "Active";
    }

    public static bool IsOpenTrip(string? status, DateTime? endDate)
    {
        if (!endDate.HasValue)
        {
            return true;
        }

        var normalized = Clean(status).ToLowerInvariant();
        return normalized is "open" or "planned" or "inprogress" or "in trip" or "جارية" or "مفتوحة";
    }

    public static bool IsVehicleAvailable(string? status)
    {
        var normalized = Clean(status).ToLowerInvariant();
        return normalized is "" or "active" or "available" or "متاح" or "نشط";
    }

    public static bool IsVehicleInactive(string? status)
    {
        var normalized = Clean(status).ToLowerInvariant();
        return normalized is "inactive" or "disabled" or "sold" or "outofservice" or "out of service" or "غير نشط" or "موقوف" or "مباع" or "خارج الخدمة";
    }

    public static bool IsMaintenanceOpen(string? status)
    {
        var normalized = Clean(status).ToLowerInvariant();
        return normalized is "open" or "inprogress" or "مفتوحة" or "جارية";
    }

    public static bool IsTreasuryIncome(string? transactionType)
    {
        var normalized = Clean(transactionType).ToLowerInvariant();
        return normalized is "in" or "income" or "deposit" or "إيراد" or "ايراد" or "وارد";
    }

    public static string TreasuryTypeDisplay(string? transactionType) =>
        IsTreasuryIncome(transactionType) ? "إيراد" : "صرف";

    public static string PaymentMethodDisplay(string? paymentMethod)
    {
        var normalized = Clean(paymentMethod).ToLowerInvariant();
        return normalized switch
        {
            "" or "cash" or "نقدي" => "نقدي",
            "bank" or "transfer" or "banktransfer" or "تحويل" or "تحويل بنكي" => "تحويل بنكي",
            "card" or "visa" or "بطاقة" => "بطاقة",
            _ => Clean(paymentMethod)
        };
    }

    public static string StatusDisplay(string? status)
    {
        var normalized = Clean(status).ToLowerInvariant();
        return normalized switch
        {
            "" => string.Empty,
            "active" or "available" => "نشط",
            "inactive" => "غير نشط",
            "open" => "مفتوح",
            "closed" or "completed" => "مغلق",
            "pending" => "معلق",
            "scheduled" or "planned" => "مجدول",
            "dailycheck" or "daily check" => "متابعة يومية",
            "duesoon" => "قريب",
            "due" => "مستحق",
            "overdue" => "متأخر",
            "inprogress" or "intrip" or "in trip" => "جاري",
            "maintenance" => "صيانة",
            "returned" => "مرتجع",
            _ => Clean(status)
        };
    }

    public static string OilStatusDisplay(string? status, bool isOilChanged)
    {
        var normalized = Clean(status).ToLowerInvariant();
        return normalized switch
        {
            "oilchanged" => "تغيير زيت",
            "completed" when isOilChanged => "تغيير زيت",
            "dailycheck" or "daily check" => "متابعة يومية",
            "duesoon" => "قريب",
            "overdue" => "متأخر",
            "" when isOilChanged => "تغيير زيت",
            "" => "متابعة يومية",
            _ => StatusDisplay(status)
        };
    }

    public static List<string> AllowedModulesForRole(UserRole role)
    {
        return role switch
        {
            UserRole.Admin => new List<string>
            {
                "Dashboard", "Vehicles", "Drivers", "Employees", "Trips", "Fuel", "Expenses", "OilChanges",
                "Maintenance", "Licenses", "Insurance", "Custody", "Treasury", "Reports", "MasterData", "Settings", "Users"
            },
            UserRole.OperationsManager => new List<string>
            {
                "Dashboard", "Vehicles", "Contracts", "Drivers", "Employees", "Trips", "Fuel", "Expenses",
                "Licenses", "Insurance", "Reports", "Notifications"
            },
            UserRole.OperationsDataEntry => new List<string>
            {
                "Dashboard", "Vehicles", "Drivers", "Employees", "Trips", "Fuel", "Expenses",
                "Licenses", "Insurance", "Notifications"
            },
            UserRole.MaintenanceOfficer => new List<string>
            {
                "Dashboard", "Vehicles", "Maintenance", "OilChanges", "Expenses", "Reports", "Notifications", "Insurance"
            },
            UserRole.TreasuryOfficer => new List<string>
            {
                "Dashboard", "Treasury"
            },
            UserRole.TripsLicensesOfficer => new List<string>
            {
                "Dashboard", "Trips", "Licenses"
            },
            UserRole.InsuranceOfficer => new List<string>
            {
                "Dashboard", "Insurance"
            },
            UserRole.Viewer => new List<string>
            {
                "Dashboard", "Reports", "Notifications"
            },
            _ => new List<string> { "Dashboard", "Trips", "Vehicles" }
        };
    }

    public static VehicleDto ToDto(this Vehicle entity)
    {
        var latestInsurance = entity.InsurancePolicies?
            .OrderByDescending(policy => policy.ExpiryDate)
            .FirstOrDefault(policy => !string.IsNullOrWhiteSpace(policy.PolicyNumber));

        return new VehicleDto
        {
            Id = entity.Id,
            PlateNumber = entity.PlateNumber,
            VehicleTypeId = entity.VehicleTypeId,
            VehicleType = entity.VehicleType?.Name ?? string.Empty,
            Model = entity.Model,
            Year = entity.Year,
            Manufacturer = entity.Manufacturer,
            Color = entity.Color,
            ChassisNumber = entity.ChassisNumber,
            EngineNumber = entity.EngineNumber,
            CurrentMileage = entity.Mileage,
            Status = entity.Status,
            AssignedTo = entity.AssignedTo,
            PurchaseDate = entity.PurchaseDate ?? DateTime.Today,
            PurchasePrice = entity.PurchasePrice,
            RegistrationType = string.IsNullOrWhiteSpace(entity.RegistrationType) ? "ترخيص" : entity.RegistrationType,
            RegistrationStartDate = entity.RegistrationStartDate,
            RegistrationExpiryDate = entity.RegistrationExpiryDate,
            AccidentInsuranceDetails = latestInsurance is null
                ? entity.AccidentInsuranceDetails
                : $"{latestInsurance.PolicyType} رقم {latestInsurance.PolicyNumber} - {latestInsurance.InsuranceCompany} حتى {latestInsurance.ExpiryDate:yyyy-MM-dd}",
            SocialInsuranceDetails = entity.SocialInsuranceDetails,
            OilChangeIntervalKm = entity.OilChangeIntervalKm,
            MaintenanceIntervalKm = entity.MaintenanceIntervalKm,
            Notes = entity.Notes
        };
    }

    public static ContractDto ToDto(this Contract entity) =>
        new()
        {
            Id = entity.Id,
            ContractNumber = entity.ContractNumber,
            VehicleId = entity.VehicleId,
            VehiclePlateNumber = entity.Vehicle?.PlateNumber ?? string.Empty,
            ContractStatusId = entity.ContractStatusId,
            Status = entity.ContractStatus?.Name ?? ContractHealth(entity.EndDate),
            ClientName = entity.ClientName,
            ClientEmail = entity.ClientEmail,
            ClientPhoneNumber = entity.ClientPhoneNumber,
            ClientAddress = entity.ClientAddress,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            ContractValue = entity.ContractValue,
            PaidAmount = entity.PaidAmount,
            PaymentTerms = entity.PaymentTerms,
            ContractTerms = entity.ContractTerms,
            DocumentUrl = entity.DocumentUrl,
            Notes = entity.Notes
        };

    public static MaintenanceRequestDto ToDto(this MaintenanceRequest entity) =>
        new()
        {
            Id = entity.Id,
            VehicleId = entity.VehicleId,
            VehiclePlateNumber = entity.Vehicle?.PlateNumber ?? string.Empty,
            MaintenanceTypeId = entity.MaintenanceTypeId,
            MaintenanceType = entity.MaintenanceType?.Name ?? string.Empty,
            ServiceProviderId = entity.ServiceProviderId,
            ServiceProvider = entity.ServiceProvider?.Name ?? string.Empty,
            RequestDate = entity.RequestDate,
            CompletionDate = entity.CompletionDate,
            Status = entity.Status,
            Description = entity.Description,
            EstimatedCost = entity.EstimatedCost,
            ActualCost = entity.ActualCost,
            WorkPerformed = entity.WorkPerformed,
            Notes = entity.Notes,
            DocumentUrl = entity.DocumentUrl
        };

    public static DriverDto ToDto(this Driver entity) =>
        new()
        {
            Id = entity.Id,
            FullName = entity.FullName,
            NationalId = entity.NationalId,
            PhoneNumber = entity.PhoneNumber,
            Email = entity.Email,
            Address = entity.Address,
            DateOfBirth = entity.DateOfBirth ?? DateTime.Today,
            LicenseNumber = entity.LicenseNumber,
            LicenseStartDate = entity.LicenseStartDate ?? DateTime.Today,
            LicenseExpiryDate = entity.LicenseExpiryDate ?? DateTime.Today,
            LicenseType = entity.LicenseType,
            IsCompanyInsured = entity.IsCompanyInsured,
            Governorate = entity.Governorate,
            FullAddress = string.IsNullOrWhiteSpace(entity.FullAddress) ? entity.Address : entity.FullAddress,
            TrafficUnit = entity.TrafficUnit,
            WorkLocation = entity.WorkLocation,
            LicenseExpiryAlert = DriverLicenseAlert(entity.LicenseExpiryDate),
            IsActive = entity.IsActive,
            Notes = entity.Notes
        };

    public static DriverAttendanceDto ToDto(this DriverAttendance entity) =>
        new()
        {
            Id = entity.Id,
            DriverId = entity.DriverId,
            DriverName = entity.Driver?.FullName ?? string.Empty,
            DriverWorkLocation = entity.Driver?.WorkLocation ?? string.Empty,
            WorkDate = entity.WorkDate,
            DayName = ArabicDayName(entity.WorkDate.DayOfWeek),
            WorkLocation = string.IsNullOrWhiteSpace(entity.WorkLocation) ? entity.Driver?.WorkLocation ?? string.Empty : entity.WorkLocation,
            Status = AttendanceStatusDisplay(entity.Status),
            AbsenceReason = entity.AbsenceReason,
            Notes = entity.Notes
        };

    public static string DriverLicenseAlert(DateTime? expiryDate)
    {
        if (!expiryDate.HasValue)
        {
            return "تاريخ انتهاء الرخصة غير مسجل";
        }

        var days = (expiryDate.Value.Date - DateTime.Today).Days;
        return days switch
        {
            < 0 => "رخصة منتهية",
            <= 30 => $"الرخصة تنتهي خلال {days} يوم",
            _ => "لا يوجد إنذار"
        };
    }

    public static string AttendanceStatusStorage(string status)
    {
        var value = Clean(status);
        return value.Trim().ToLowerInvariant() switch
        {
            "present" or "حاضر" or "حضور" => "Present",
            "absent" or "غائب" or "غياب" => "Absent",
            "leave" or "اجازة" or "إجازة" or "أجازة" => "Leave",
            "compensatoryrest" or "compensatory rest" or "راحة مستحقة" or "راحة تعويضية" => "CompensatoryRest",
            "rest" or "راحة" or "راحة جمعة" or "راحة أسبوعية" or "راحة اسبوعية" => "Rest",
            _ => string.IsNullOrWhiteSpace(value) ? "Present" : value
        };
    }

    public static string AttendanceStatusDisplay(string status) =>
        Clean(status).Trim().ToLowerInvariant() switch
        {
            "present" => "حاضر",
            "absent" => "غائب",
            "leave" => "إجازة",
            "compensatoryrest" => "إجازة",
            "rest" => string.Empty,
            "" => "حاضر",
            var value => value
        };

    public static string ArabicDayName(DayOfWeek day) =>
        day switch
        {
            DayOfWeek.Saturday => "السبت",
            DayOfWeek.Sunday => "الأحد",
            DayOfWeek.Monday => "الاثنين",
            DayOfWeek.Tuesday => "الثلاثاء",
            DayOfWeek.Wednesday => "الأربعاء",
            DayOfWeek.Thursday => "الخميس",
            DayOfWeek.Friday => "الجمعة",
            _ => string.Empty
        };

    public static EmployeeDto ToDto(this Employee entity) =>
        new()
        {
            Id = entity.Id,
            FullName = entity.FullName,
            EmployeeId = entity.EmployeeId,
            PhoneNumber = entity.PhoneNumber,
            Email = entity.Email,
            Department = entity.Department,
            Position = entity.Position,
            HireDate = entity.HireDate ?? DateTime.Today,
            TerminationDate = entity.TerminationDate,
            Status = entity.Status,
            Notes = entity.Notes
        };

    public static TripDto ToDto(this Trip entity) =>
        new()
        {
            Id = entity.Id,
            VehicleId = entity.VehicleId,
            VehiclePlateNumber = entity.Vehicle?.PlateNumber ?? string.Empty,
            DriverId = entity.DriverId,
            DriverName = entity.Driver?.FullName ?? string.Empty,
            RequesterEmployeeId = entity.RequesterEmployeeId,
            RequesterNameText = entity.RequesterNameText,
            RequesterName = string.IsNullOrWhiteSpace(entity.RequesterNameText)
                ? entity.RequesterEmployee?.FullName ?? string.Empty
                : entity.RequesterNameText,
            SupervisorEmployeeId = entity.SupervisorEmployeeId,
            SupervisorName = entity.SupervisorEmployee?.FullName ?? string.Empty,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            StartLocation = entity.StartLocation,
            EndLocation = entity.EndLocation,
            StartMileage = entity.StartMileage,
            EndMileage = entity.EndMileage,
            Distance = entity.Distance,
            Purpose = entity.Purpose,
            Status = entity.Status,
            FuelConsumed = 0,
            TripCost = 0,
            Notes = entity.Notes
        };

    public static FuelTransactionDto ToDto(this FuelTransaction entity) =>
        new()
        {
            Id = entity.Id,
            VehicleId = entity.VehicleId,
            VehiclePlateNumber = entity.Vehicle?.PlateNumber ?? string.Empty,
            TripId = entity.TripId,
            TripSummary = entity.Trip is null
                ? string.Empty
                : $"{entity.Trip.Id} - {entity.Trip.Vehicle?.PlateNumber ?? entity.Vehicle?.PlateNumber ?? string.Empty} - {entity.Trip.StartDate:yyyy-MM-dd HH:mm}",
            TransactionDate = entity.TransactionDate,
            FuelType = entity.FuelType,
            Quantity = entity.Quantity,
            UnitPrice = entity.UnitPrice,
            TotalCost = entity.TotalCost,
            FuelStation = entity.FuelStation,
            Odometer = entity.Odometer ?? 0,
            PaidFromTreasury = entity.PaidFromTreasury,
            TreasuryTransactionId = entity.TreasuryTransactionId,
            Notes = entity.Notes
        };

    public static ExpenseDto ToDto(this Expense entity) =>
        new()
        {
            Id = entity.Id,
            VehicleId = entity.VehicleId,
            VehiclePlateNumber = entity.Vehicle?.PlateNumber ?? string.Empty,
            ExpenseDate = entity.ExpenseDate,
            Category = entity.Category,
            Amount = entity.Amount,
            Description = entity.Description,
            Vendor = entity.Vendor,
            ReceiptUrl = entity.ReceiptUrl,
            PaidFromTreasury = entity.PaidFromTreasury,
            TreasuryTransactionId = entity.TreasuryTransactionId,
            Status = entity.Status,
            Notes = entity.Notes
        };

    public static OilChangeDto ToDto(this OilChange entity)
    {
        var currentVehicleMileage = entity.Vehicle?.Mileage ?? entity.CurrentOdometer ?? 0;
        var currentOdometer = entity.CurrentOdometer ?? currentVehicleMileage;
        var kmSinceOilChange = Math.Max(0, currentOdometer - entity.OdometerAtChange);
        var remainingKm = entity.NextOilChangeOdometer - currentOdometer;
        var isDue = remainingKm <= DefaultOilAlertThresholdKm;

        return new OilChangeDto
        {
            Id = entity.Id,
            VehicleId = entity.VehicleId,
            VehiclePlateNumber = entity.Vehicle?.PlateNumber ?? string.Empty,
            ChangeDate = entity.ChangeDate,
            OdometerAtChange = entity.OdometerAtChange,
            OilType = entity.OilType,
            Quantity = entity.Quantity,
            Cost = entity.Cost,
            NextOilChangeOdometer = entity.NextOilChangeOdometer,
            CurrentOdometer = currentOdometer,
            CurrentOdometerDate = entity.CurrentOdometerDate,
            IsOilChanged = entity.IsOilChanged,
            RecordType = entity.IsOilChanged ? "تغيير زيت" : "متابعة يومية",
            ServiceItems = entity.ServiceItems,
            CurrentVehicleMileage = currentVehicleMileage,
            OilChangeIntervalKm = entity.Vehicle?.OilChangeIntervalKm ?? 0,
            KmSinceOilChange = kmSinceOilChange,
            RemainingKm = remainingKm,
            OilAlert = BuildOilAlert(remainingKm),
            Status = OilStatusDisplay(entity.Status, entity.IsOilChanged),
            IsDue = isDue,
            Notes = entity.Notes
        };
    }

    public static string BuildOilAlert(decimal remainingKm)
    {
        if (remainingKm < 0)
        {
            return $"تغيير الزيت متأخر بـ {Math.Abs(remainingKm):0} كم";
        }

        if (remainingKm <= DefaultOilAlertThresholdKm)
        {
            return $"متبقي {remainingKm:0} كم على تغيير الزيت";
        }

        return "لا يوجد إنذار";
    }

    public static TreasuryTransactionDto ToDto(this TreasuryTransaction entity) =>
        new()
        {
            Id = entity.Id,
            TransactionDate = entity.TransactionDate,
            TransactionType = TreasuryTypeDisplay(entity.TransactionType),
            Amount = entity.Amount,
            Description = entity.Description,
            RelatedEntityType = entity.RelatedEntityType,
            RelatedEntityId = entity.RelatedEntityId,
            PaymentMethod = PaymentMethodDisplay(entity.PaymentMethod),
            Notes = entity.Notes
        };

    public static LicenseDto ToDto(this License entity) =>
        new()
        {
            Id = entity.Id,
            DriverId = entity.DriverId,
            DriverName = entity.Driver?.FullName ?? string.Empty,
            LicenseNumber = entity.LicenseNumber,
            LicenseType = entity.LicenseType,
            IssueDate = entity.IssueDate,
            ExpiryDate = entity.ExpiryDate,
            IssuingAuthority = entity.IssuingAuthority,
            Status = string.IsNullOrWhiteSpace(entity.Status) ? LicenseStatus(entity.ExpiryDate) : entity.Status,
            Notes = entity.Notes
        };

    public static InsuranceDto ToDto(this Insurance entity) =>
        new()
        {
            Id = entity.Id,
            VehicleId = entity.VehicleId,
            VehiclePlateNumber = entity.Vehicle?.PlateNumber ?? string.Empty,
            VehicleModel = entity.Vehicle?.Model ?? string.Empty,
            VehicleYear = entity.Vehicle?.Year ?? 0,
            VehicleChassisNumber = entity.Vehicle?.ChassisNumber ?? string.Empty,
            VehicleEngineNumber = entity.Vehicle?.EngineNumber ?? string.Empty,
            PolicyNumber = entity.PolicyNumber,
            InsuranceCompany = entity.InsuranceCompany,
            PolicyType = entity.PolicyType,
            StartDate = entity.StartDate,
            ExpiryDate = entity.ExpiryDate,
            PremiumAmount = entity.PremiumAmount,
            CoverageAmount = entity.CoverageAmount,
            CoverageDetails = entity.CoverageDetails,
            AgentName = entity.AgentName,
            AgentPhoneNumber = entity.AgentPhoneNumber,
            Status = InsuranceStatus(entity.ExpiryDate),
            DocumentUrl = entity.DocumentUrl,
            Notes = entity.Notes
        };

    public static CustodyDto ToDto(this Custody entity) =>
        new()
        {
            Id = entity.Id,
            VehicleId = entity.VehicleId,
            VehiclePlateNumber = entity.Vehicle?.PlateNumber ?? string.Empty,
            CustodyNumber = entity.CustodyNumber,
            CustodianName = entity.CustodianName,
            CustodianPosition = entity.CustodianPosition,
            HandoverDate = entity.HandoverDate,
            ReturnDate = entity.ReturnDate,
            Status = StatusDisplay(entity.Status),
            VehicleConditionRating = entity.VehicleConditionRating,
            Notes = entity.Notes,
            DocumentUrl = entity.DocumentUrl,
            Items = new List<CustodyItemDto>()
        };

    public static VehicleTypeDto ToDto(this VehicleType entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            NameEn = entity.NameEn,
            Description = entity.Description,
            IsActive = entity.IsActive
        };

    public static ContractStatusDto ToDto(this ContractStatus entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            NameEn = entity.NameEn,
            Description = entity.Description,
            Color = entity.Color,
            IsActive = entity.IsActive
        };

    public static MaintenanceTypeDto ToDto(this MaintenanceType entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            NameEn = entity.NameEn,
            Description = entity.Description,
            EstimatedCost = entity.EstimatedCost,
            EstimatedDurationDays = entity.EstimatedDurationDays,
            IsActive = entity.IsActive
        };

    public static ServiceProviderDto ToDto(this ServiceProviderEntity entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            NameEn = entity.NameEn,
            ContactPerson = entity.ContactPerson,
            PhoneNumber = entity.PhoneNumber,
            Email = entity.Email,
            Address = entity.Address,
            Specialization = entity.Specialization,
            AverageRating = entity.AverageRating,
            IsActive = entity.IsActive
        };

    public static CompanySettingsDto ToDto(this CompanySettings entity) =>
        new()
        {
            Id = entity.Id,
            CompanyName = entity.CompanyName,
            CompanyNameEn = entity.CompanyNameEn,
            LogoUrl = entity.LogoUrl,
            Address = entity.Address,
            PhoneNumber = entity.PhoneNumber,
            Email = entity.Email,
            Website = entity.Website,
            TaxId = entity.TaxId,
            CommercialRegistration = entity.CommercialRegistration,
            CurrencySymbol = entity.CurrencySymbol,
            DateFormat = entity.DateFormat,
            TimeFormat = entity.TimeFormat,
            DecimalPlaces = entity.DecimalPlaces,
            DefaultLanguage = entity.DefaultLanguage,
            DefaultTheme = entity.DefaultTheme
        };

    public static UserDto ToDto(this User entity) =>
        new()
        {
            Id = entity.Id,
            Username = entity.Username,
            Email = entity.Email,
            FullName = entity.FullName,
            PhoneNumber = entity.PhoneNumber,
            Role = entity.Role.ToString(),
            IsActive = entity.IsActive,
            LastLoginAt = entity.LastLogin,
            AllowedModules = AllowedModulesForRole(entity.Role)
        };

    public static UserListItemDto ToListItemDto(this User entity) =>
        new()
        {
            Id = entity.Id,
            Username = entity.Username,
            FullName = entity.FullName,
            Email = entity.Email,
            Role = entity.Role.ToString(),
            IsActive = entity.IsActive,
            LastLoginAt = entity.LastLogin
        };

    public static AuditLogDto ToDto(this AuditLog entity) =>
        new()
        {
            Id = entity.Id,
            UserId = entity.UserId ?? 0,
            UserName = entity.UserName,
            Action = entity.Action,
            EntityName = string.IsNullOrWhiteSpace(entity.EntityName) ? entity.EntityType : entity.EntityName,
            EntityId = entity.EntityId,
            OldValues = entity.OldValues,
            NewValues = entity.NewValues,
            IpAddress = entity.IpAddress,
            Timestamp = entity.Timestamp
        };

    public static NotificationDto ToDto(this Notification entity) =>
        new()
        {
            Id = entity.Id,
            UserId = entity.UserId ?? 0,
            Title = entity.Title,
            Message = entity.Message,
            Type = entity.Type,
            RelatedEntityType = entity.RelatedEntityType,
            RelatedEntityId = entity.RelatedEntityId,
            IsRead = entity.IsRead,
            ReadAt = entity.ReadAt,
            ExpiresAt = entity.ExpiresAt ?? entity.CreatedAt.AddDays(7),
            CreatedAt = entity.CreatedAt
        };
}
