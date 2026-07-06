using BCrypt.Net;
using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Core.Enums;
using FleetManagementSystem.Data;
using FleetManagementSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Data;

namespace FleetManagementSystem.Services;

public sealed class DataBootstrapService(FleetDbContext context) : IDataBootstrapService
{
    private const string DefaultCompanyName = "شركة جوميكس للحركة والمعدات";
    private const string DefaultCompanyNameEn = "Gomix Movement and Equipment";
    private const string DefaultLogoUrl = "pack://application:,,,/Resources/gomix-logo.png";

    private readonly FleetDbContext _context = context;

    public async Task InitializeAsync()
    {
        await _context.Database.EnsureCreatedAsync();
        await EnsureOperationalSchemaAsync();

        if (!await _context.VehicleTypes.AnyAsync())
        {
            _context.VehicleTypes.AddRange(
                new VehicleType { Name = "سيارة ركاب", NameEn = "Sedan", Description = "سيارات التشغيل اليومية" },
                new VehicleType { Name = "شاحنة", NameEn = "Truck", Description = "شاحنات النقل الثقيل" },
                new VehicleType { Name = "حافلة", NameEn = "Bus", Description = "حافلات نقل الموظفين" });
        }

        if (!await _context.ContractStatuses.AnyAsync())
        {
            _context.ContractStatuses.AddRange(
                new ContractStatus { Name = "مسودة", NameEn = "Draft", Color = "#6B7280" },
                new ContractStatus { Name = "نشط", NameEn = "Active", Color = "#10B981" },
                new ContractStatus { Name = "منتهي", NameEn = "Expired", Color = "#EF4444" },
                new ContractStatus { Name = "ملغي", NameEn = "Cancelled", Color = "#F59E0B" });
        }

        if (!await _context.MaintenanceTypes.AnyAsync())
        {
            _context.MaintenanceTypes.AddRange(
                new MaintenanceType { Name = "صيانة دورية", NameEn = "Routine", EstimatedCost = 1500, EstimatedDurationDays = 1 },
                new MaintenanceType { Name = "إصلاح", NameEn = "Repair", EstimatedCost = 3000, EstimatedDurationDays = 3 },
                new MaintenanceType { Name = "فحص شامل", NameEn = "Inspection", EstimatedCost = 800, EstimatedDurationDays = 1 });
        }

        if (!await _context.ServiceProviders.AnyAsync())
        {
            _context.ServiceProviders.AddRange(
                new FleetManagementSystem.Data.Entities.ServiceProvider
                {
                    Name = "مركز الصيانة المتكامل",
                    NameEn = "Integrated Service Center",
                    ContactPerson = "قسم الاستقبال",
                    PhoneNumber = "01000000001",
                    Email = "service1@example.com",
                    Address = "القاهرة",
                    Specialization = "ميكانيكا وكهرباء",
                    AverageRating = 4.5m
                },
                new FleetManagementSystem.Data.Entities.ServiceProvider
                {
                    Name = "ورشة الأسطول الحديثة",
                    NameEn = "Modern Fleet Garage",
                    ContactPerson = "مدير الورشة",
                    PhoneNumber = "01000000002",
                    Email = "service2@example.com",
                    Address = "الجيزة",
                    Specialization = "سمكرة ودهان",
                    AverageRating = 4.2m
                });
        }

        if (!await _context.CompanySettings.AnyAsync())
        {
            _context.CompanySettings.Add(new CompanySettings
            {
                CompanyName = DefaultCompanyName,
                CompanyNameEn = DefaultCompanyNameEn,
                LogoUrl = DefaultLogoUrl,
                Address = "القاهرة",
                PhoneNumber = "0220000000",
                Email = "info@fleet.local",
                Website = "https://fleet.local",
                TaxId = "000000000",
                CommercialRegistration = "000000",
                CurrencySymbol = "EGP",
                DefaultLanguage = "ar",
                DefaultTheme = "Light"
            });
        }
        else
        {
            var settings = await _context.CompanySettings.OrderBy(x => x.Id).FirstAsync();
            var shouldApplyGomixBranding =
                string.IsNullOrWhiteSpace(settings.CompanyName) ||
                settings.CompanyName.Contains("عبد", StringComparison.OrdinalIgnoreCase) ||
                settings.CompanyName.Contains("الأسطول", StringComparison.OrdinalIgnoreCase) ||
                settings.CompanyName.Contains("للحركه", StringComparison.OrdinalIgnoreCase) ||
                settings.CompanyName.Contains("شركه جوميكس", StringComparison.OrdinalIgnoreCase);

            if (shouldApplyGomixBranding)
            {
                settings.CompanyName = DefaultCompanyName;
            }

            if (string.IsNullOrWhiteSpace(settings.CompanyNameEn) ||
                settings.CompanyNameEn.Contains("Fleet", StringComparison.OrdinalIgnoreCase))
            {
                settings.CompanyNameEn = DefaultCompanyNameEn;
            }

            if (shouldApplyGomixBranding || string.IsNullOrWhiteSpace(settings.LogoUrl))
            {
                settings.LogoUrl = DefaultLogoUrl;
            }
            else if (!settings.LogoUrl.StartsWith("pack://", StringComparison.OrdinalIgnoreCase) &&
                     !File.Exists(settings.LogoUrl))
            {
                // مسار لوجو قديم لم يعد موجودًا على الجهاز؛ نرجع للوجو المدمج.
                settings.LogoUrl = DefaultLogoUrl;
            }

            settings.UpdatedAt = DateTime.UtcNow;
        }

        if (!await _context.Users.AnyAsync())
        {
            _context.Users.Add(new User
            {
                Username = "admin",
                Email = "admin@fleet.local",
                PhoneNumber = "01000000000",
                FullName = "مدير النظام",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin,
                IsActive = true
            });
        }

        await EnsureRequiredOperationalUserAsync("mahmoud", "محمود مرعي", "mahmoud", UserRole.Admin);
        await EnsureRequiredOperationalUserAsync("amr", "عمرو جاد", "amr", UserRole.TreasuryOfficer);
        await EnsureRequiredOperationalUserAsync("abdelrahman", "عبد الرحمن", "abdelrahman", UserRole.TripsLicensesOfficer);
        await EnsureRequiredOperationalUserAsync("osama", "أسامة", "osama", UserRole.InsuranceOfficer);

        // إصلاح أي حساب سُجّل بكلمة مرور فارغة (كان يسبب "Invalid salt" عند الدخول):
        // نعطيه كلمة المرور الافتراضية = اسم المستخدم ونطلب تغييرها بعد الدخول.
        var brokenAccounts = await _context.Users
            .Where(u => u.PasswordHash == null || u.PasswordHash == "")
            .ToListAsync();
        foreach (var brokenUser in brokenAccounts)
        {
            brokenUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(brokenUser.Username);
            brokenUser.MustChangePassword = true;
            brokenUser.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        // Keep production/client databases clean; operational data must be entered by the user.
    }

    private async Task EnsureRequiredOperationalUserAsync(string username, string fullName, string password, UserRole role)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
        if (user is null)
        {
            user = new User
            {
                Username = username,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
        }

        user.FullName = fullName;
        user.Email = $"{username}@gomix.local";
        user.PhoneNumber = string.Empty;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        user.Role = role;
        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
    }

    private async Task EnsureOperationalSchemaAsync()
    {
        await EnsureColumnAsync("Vehicles", "RegistrationStartDate", GetNullableDateColumnDefinition());
        await EnsureColumnAsync("Vehicles", "RegistrationType", GetRegistrationTypeColumnDefinition());
        await EnsureColumnAsync("Vehicles", "AccidentInsuranceDetails", GetInsuranceDetailsColumnDefinition());
        await EnsureColumnAsync("Vehicles", "SocialInsuranceDetails", GetInsuranceDetailsColumnDefinition());
        await EnsureColumnAsync("Trips", "RequesterEmployeeId", GetNullableIntColumnDefinition());
        await EnsureColumnAsync("Trips", "RequesterNameText", GetRequesterNameColumnDefinition());
        await EnsureColumnAsync("Trips", "SupervisorEmployeeId", GetNullableIntColumnDefinition());
        await EnsureColumnAsync("FuelTransactions", "TripId", GetNullableIntColumnDefinition());
        await EnsureColumnAsync("OilChanges", "CurrentOdometer", GetNullableDecimalColumnDefinition());
        await EnsureColumnAsync("OilChanges", "CurrentOdometerDate", GetNullableDateColumnDefinition());
        await EnsureColumnAsync("OilChanges", "IsOilChanged", GetBooleanColumnDefinition(defaultValue: true));
        await EnsureColumnAsync("OilChanges", "ServiceItems", GetShortTextColumnDefinition(defaultValue: string.Empty));
        await EnsureColumnAsync("Drivers", "LicenseStartDate", GetNullableDateColumnDefinition());
        await EnsureColumnAsync("Drivers", "IsCompanyInsured", GetBooleanColumnDefinition(defaultValue: false));
        await EnsureColumnAsync("Drivers", "Governorate", GetShortTextColumnDefinition(defaultValue: string.Empty));
        await EnsureColumnAsync("Drivers", "FullAddress", GetInsuranceDetailsColumnDefinition());
        await EnsureColumnAsync("Drivers", "TrafficUnit", GetShortTextColumnDefinition(defaultValue: string.Empty));
        await EnsureColumnAsync("Drivers", "WorkLocation", GetShortTextColumnDefinition(defaultValue: string.Empty));
        await EnsureCustodyTableAsync();
        // بعض قواعد البيانات القديمة فيها جدول عهد ناقص أعمدة أساسية (مثل CustodianName) —
        // نتحقق من كل الأعمدة عمودًا عمودًا قبل أي استخدام حتى لا يفشل التحميل أو الحفظ.
        await EnsureColumnAsync("Custody", "VehicleId", GetNullableIntColumnDefinition());
        await EnsureColumnAsync("Custody", "EmployeeId", GetNullableIntColumnDefinition());
        await EnsureColumnAsync("Custody", "CustodyNumber", GetShortTextColumnDefinition(defaultValue: string.Empty));
        await EnsureColumnAsync("Custody", "CustodianName", GetShortTextColumnDefinition(defaultValue: string.Empty));
        await EnsureColumnAsync("Custody", "CustodianPosition", GetShortTextColumnDefinition(defaultValue: string.Empty));
        await EnsureColumnAsync("Custody", "HandoverDate", GetDateTimeColumnDefinition());
        await EnsureColumnAsync("Custody", "ReturnDate", GetNullableDateColumnDefinition());
        await EnsureColumnAsync("Custody", "Status", GetShortTextColumnDefinition(defaultValue: "Active"));
        await EnsureColumnAsync("Custody", "VehicleConditionRating", GetRatingColumnDefinition());
        await EnsureColumnAsync("Custody", "Notes", GetShortTextColumnDefinition(defaultValue: string.Empty));
        await EnsureColumnAsync("Custody", "DocumentUrl", GetShortTextColumnDefinition(defaultValue: string.Empty));
        await EnsureColumnAsync("Custody", "CreatedAt", GetDateTimeColumnDefinition());
        await EnsureColumnAsync("Custody", "UpdatedAt", GetDateTimeColumnDefinition());
        await EnsureColumnAsync("Custody", "Amount", GetMoneyColumnDefinition());
        await EnsureColumnAsync("Custody", "SettledAmount", GetMoneyColumnDefinition());
        await EnsureColumnAsync("Custody", "SettlementDate", GetNullableDateColumnDefinition());
        await EnsureColumnAsync("Custody", "SettlementNotes", GetShortTextColumnDefinition(defaultValue: string.Empty));
        await EnsureColumnAsync("Custody", "UserId", GetNullableIntColumnDefinition());
        await EnsureColumnAsync("Users", "MustChangePassword", GetBooleanColumnDefinition(defaultValue: false));
        await EnsureColumnAsync("TreasuryTransactions", "Status", GetShortTextColumnDefinition(defaultValue: "معتمد"));
        await EnsureCustodyVehicleOptionalAsync();
        await EnsureDriverAttendanceTableAsync();

        if (await HasColumnAsync("Vehicles", "RegistrationType"))
        {
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Vehicles SET RegistrationType = 'ترخيص' WHERE RegistrationType IS NULL OR TRIM(RegistrationType) = '' OR RegistrationType <> 'ترخيص'");
        }

        if (await HasColumnAsync("Trips", "EmployeeId") && await HasColumnAsync("Trips", "RequesterEmployeeId"))
        {
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Trips SET RequesterEmployeeId = COALESCE(RequesterEmployeeId, EmployeeId) WHERE EmployeeId IS NOT NULL");
        }

        var requestersToBackfill = await _context.Trips
            .Include(t => t.RequesterEmployee)
            .Where(t => string.IsNullOrWhiteSpace(t.RequesterNameText) && t.RequesterEmployeeId.HasValue)
            .ToListAsync();

        foreach (var trip in requestersToBackfill)
        {
            trip.RequesterNameText = trip.RequesterEmployee?.FullName ?? string.Empty;
        }

        if (requestersToBackfill.Count > 0)
        {
            await _context.SaveChangesAsync();
        }
    }

    private async Task EnsureDriverAttendanceTableAsync()
    {
        if (await HasTableAsync("DriverAttendances"))
        {
            return;
        }

#pragma warning disable EF1002
        if (_context.Database.IsSqlite())
        {
            await _context.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS DriverAttendances (
                    Id INTEGER NOT NULL CONSTRAINT PK_DriverAttendances PRIMARY KEY AUTOINCREMENT,
                    DriverId INTEGER NOT NULL,
                    WorkDate TEXT NOT NULL,
                    WorkLocation TEXT NOT NULL DEFAULT '',
                    Status TEXT NOT NULL DEFAULT 'Present',
                    AbsenceReason TEXT NOT NULL DEFAULT '',
                    Notes TEXT NOT NULL DEFAULT '',
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL,
                    CONSTRAINT FK_DriverAttendances_Drivers_DriverId FOREIGN KEY (DriverId) REFERENCES Drivers (Id) ON DELETE CASCADE
                );
                """);
            await _context.Database.ExecuteSqlRawAsync("CREATE UNIQUE INDEX IF NOT EXISTS IX_DriverAttendances_DriverId_WorkDate ON DriverAttendances (DriverId, WorkDate)");
            return;
        }

        if (_context.Database.IsMySql())
        {
            await _context.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS DriverAttendances (
                    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    DriverId INT NOT NULL,
                    WorkDate DATETIME NOT NULL,
                    WorkLocation VARCHAR(255) NOT NULL DEFAULT '',
                    Status VARCHAR(40) NOT NULL DEFAULT 'Present',
                    AbsenceReason VARCHAR(500) NOT NULL DEFAULT '',
                    Notes VARCHAR(500) NOT NULL DEFAULT '',
                    CreatedAt DATETIME NOT NULL,
                    UpdatedAt DATETIME NOT NULL,
                    UNIQUE KEY IX_DriverAttendances_DriverId_WorkDate (DriverId, WorkDate),
                    CONSTRAINT FK_DriverAttendances_Drivers_DriverId FOREIGN KEY (DriverId) REFERENCES Drivers (Id) ON DELETE CASCADE
                );
                """);
        }
#pragma warning restore EF1002
    }

    private async Task EnsureCustodyTableAsync()
    {
        if (await HasTableAsync("Custody"))
        {
            return;
        }

#pragma warning disable EF1002
        if (_context.Database.IsSqlite())
        {
            await _context.Database.ExecuteSqlRawAsync(SqliteCustodyTableDdl);
            return;
        }

        if (_context.Database.IsMySql())
        {
            await _context.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS Custody (
                    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    VehicleId INT NULL,
                    EmployeeId INT NULL,
                    UserId INT NULL,
                    CustodyNumber VARCHAR(255) NOT NULL DEFAULT '',
                    CustodianName VARCHAR(255) NOT NULL DEFAULT '',
                    CustodianPosition VARCHAR(255) NOT NULL DEFAULT '',
                    HandoverDate DATETIME NOT NULL DEFAULT '2000-01-01 00:00:00',
                    ReturnDate DATETIME NULL,
                    Status VARCHAR(255) NOT NULL DEFAULT 'Active',
                    VehicleConditionRating DECIMAL(18,2) NOT NULL DEFAULT 5,
                    Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
                    SettledAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
                    SettlementDate DATETIME NULL,
                    SettlementNotes VARCHAR(500) NOT NULL DEFAULT '',
                    Notes VARCHAR(500) NOT NULL DEFAULT '',
                    DocumentUrl VARCHAR(500) NOT NULL DEFAULT '',
                    CreatedAt DATETIME NOT NULL DEFAULT '2000-01-01 00:00:00',
                    UpdatedAt DATETIME NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """);
        }
#pragma warning restore EF1002
    }

    private const string SqliteCustodyTableDdl = """
        CREATE TABLE Custody (
            Id INTEGER NOT NULL CONSTRAINT PK_Custody PRIMARY KEY AUTOINCREMENT,
            VehicleId INTEGER NULL,
            EmployeeId INTEGER NULL,
            UserId INTEGER NULL,
            CustodyNumber TEXT NOT NULL DEFAULT '',
            CustodianName TEXT NOT NULL DEFAULT '',
            CustodianPosition TEXT NOT NULL DEFAULT '',
            HandoverDate TEXT NOT NULL DEFAULT '2000-01-01 00:00:00',
            ReturnDate TEXT NULL,
            Status TEXT NOT NULL DEFAULT 'Active',
            VehicleConditionRating TEXT NOT NULL DEFAULT '5.0',
            Amount TEXT NOT NULL DEFAULT '0',
            SettledAmount TEXT NOT NULL DEFAULT '0',
            SettlementDate TEXT NULL,
            SettlementNotes TEXT NOT NULL DEFAULT '',
            Notes TEXT NOT NULL DEFAULT '',
            DocumentUrl TEXT NOT NULL DEFAULT '',
            CreatedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00',
            UpdatedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
        )
        """;

    /// <summary>
    /// العهدة كانت مربوطة إجباريًا بمركبة؛ الآن الربط اختياري، فنعدّل قواعد البيانات القديمة
    /// حتى يقبل عمود VehicleId قيمة فارغة.
    /// </summary>
    private async Task EnsureCustodyVehicleOptionalAsync()
    {
#pragma warning disable EF1002
        if (_context.Database.IsMySql())
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Custody MODIFY COLUMN VehicleId INT NULL");
            }
            catch
            {
                // لا نوقف تشغيل التطبيق لو فشل التعديل؛ الإنشاء الجديد للجداول أصلاً يجعل العمود اختياريًا.
            }

            return;
        }

        if (!_context.Database.IsSqlite())
        {
            return;
        }

        var connection = _context.Database.GetDbConnection();
        var closeWhenDone = connection.State != ConnectionState.Open;
        if (closeWhenDone)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using (var check = connection.CreateCommand())
            {
                check.CommandText = "PRAGMA table_info('Custody')";
                await using var reader = await check.ExecuteReaderAsync();
                var vehicleIdNotNull = false;
                while (await reader.ReadAsync())
                {
                    if (string.Equals(reader["name"]?.ToString(), "VehicleId", StringComparison.OrdinalIgnoreCase))
                    {
                        vehicleIdNotNull = Convert.ToInt32(reader["notnull"]) == 1;
                    }
                }

                if (!vehicleIdNotNull)
                {
                    return;
                }
            }

            // SQLite لا يدعم تعديل العمود مباشرة؛ نعيد بناء الجدول بنفس البيانات.
            var columns = "Id, VehicleId, EmployeeId, UserId, CustodyNumber, CustodianName, CustodianPosition, " +
                          "HandoverDate, ReturnDate, Status, VehicleConditionRating, Amount, SettledAmount, " +
                          "SettlementDate, SettlementNotes, Notes, DocumentUrl, CreatedAt, UpdatedAt";
            var statements = new[]
            {
                "PRAGMA foreign_keys = OFF",
                "ALTER TABLE Custody RENAME TO __Custody_old",
                SqliteCustodyTableDdl,
                $"INSERT INTO Custody ({columns}) SELECT {columns} FROM __Custody_old",
                "DROP TABLE __Custody_old",
                "PRAGMA foreign_keys = ON"
            };

            foreach (var sql in statements)
            {
                await using var command = connection.CreateCommand();
                command.CommandText = sql;
                await command.ExecuteNonQueryAsync();
            }
        }
        finally
        {
            if (closeWhenDone)
            {
                await connection.CloseAsync();
            }
        }
#pragma warning restore EF1002
    }

    private async Task EnsureColumnAsync(string tableName, string columnName, string columnDefinition)
    {
        if (await HasColumnAsync(tableName, columnName))
        {
            return;
        }

#pragma warning disable EF1002
        await _context.Database.ExecuteSqlRawAsync(
            $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition}");
#pragma warning restore EF1002
    }

    private async Task<bool> HasTableAsync(string tableName)
    {
        var connection = _context.Database.GetDbConnection();
        var closeWhenDone = connection.State != ConnectionState.Open;
        if (closeWhenDone)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var command = connection.CreateCommand();
            if (_context.Database.IsSqlite())
            {
                command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @tableName";
            }
            else if (_context.Database.IsMySql())
            {
                command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @tableName";
            }
            else
            {
                command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";
            }

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
        finally
        {
            if (closeWhenDone)
            {
                await connection.CloseAsync();
            }
        }
    }

    private async Task<bool> HasColumnAsync(string tableName, string columnName)
    {
        var connection = _context.Database.GetDbConnection();
        var closeWhenDone = connection.State != ConnectionState.Open;
        if (closeWhenDone)
        {
            await connection.OpenAsync();
        }

        try
        {
            if (_context.Database.IsSqlite())
            {
                await using var command = connection.CreateCommand();
                command.CommandText = $"PRAGMA table_info('{tableName}')";
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (string.Equals(reader["name"]?.ToString(), columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }

            if (_context.Database.IsMySql())
            {
                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @tableName AND COLUMN_NAME = @columnName";

                var tableParameter = command.CreateParameter();
                tableParameter.ParameterName = "@tableName";
                tableParameter.Value = tableName;
                command.Parameters.Add(tableParameter);

                var columnParameter = command.CreateParameter();
                columnParameter.ParameterName = "@columnName";
                columnParameter.Value = columnName;
                command.Parameters.Add(columnParameter);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }

            await using var fallbackCommand = connection.CreateCommand();
            fallbackCommand.CommandText = $"SELECT * FROM {tableName} WHERE 1 = 0";
            await using var fallbackReader = await fallbackCommand.ExecuteReaderAsync(CommandBehavior.SchemaOnly);
            var schemaTable = fallbackReader.GetSchemaTable();
            if (schemaTable is null)
            {
                return false;
            }

            foreach (DataRow row in schemaTable.Rows)
            {
                if (string.Equals(row["ColumnName"]?.ToString(), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
        finally
        {
            if (closeWhenDone)
            {
                await connection.CloseAsync();
            }
        }
    }

    private string GetNullableDateColumnDefinition() =>
        _context.Database.IsSqlite() ? "TEXT NULL" : "DATETIME NULL";

    private string GetInsuranceDetailsColumnDefinition() =>
        _context.Database.IsSqlite() ? "TEXT NOT NULL DEFAULT ''" : "VARCHAR(500) NOT NULL DEFAULT ''";

    private string GetNullableIntColumnDefinition() =>
        _context.Database.IsSqlite() ? "INTEGER NULL" : "INT NULL";

    private string GetMoneyColumnDefinition() =>
        _context.Database.IsSqlite() ? "TEXT NOT NULL DEFAULT '0'" : "DECIMAL(18,2) NOT NULL DEFAULT 0";

    private string GetRatingColumnDefinition() =>
        _context.Database.IsSqlite() ? "TEXT NOT NULL DEFAULT '5.0'" : "DECIMAL(18,2) NOT NULL DEFAULT 5";

    private string GetDateTimeColumnDefinition() =>
        _context.Database.IsSqlite()
            ? "TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'"
            : "DATETIME NOT NULL DEFAULT '2000-01-01 00:00:00'";

    private string GetNullableDecimalColumnDefinition() =>
        _context.Database.IsSqlite() ? "TEXT NULL" : "DECIMAL(18,2) NULL";

    private string GetBooleanColumnDefinition(bool defaultValue) =>
        _context.Database.IsSqlite()
            ? $"INTEGER NOT NULL DEFAULT {(defaultValue ? 1 : 0)}"
            : $"BIT NOT NULL DEFAULT {(defaultValue ? 1 : 0)}";

    private string GetRequesterNameColumnDefinition() =>
        _context.Database.IsSqlite() ? "TEXT NOT NULL DEFAULT ''" : "VARCHAR(255) NOT NULL DEFAULT ''";

    private string GetShortTextColumnDefinition(string defaultValue) =>
        _context.Database.IsSqlite()
            ? $"TEXT NOT NULL DEFAULT '{defaultValue.Replace("'", "''")}'"
            : $"VARCHAR(255) NOT NULL DEFAULT '{defaultValue.Replace("'", "''")}'";

    private string GetRegistrationTypeColumnDefinition() =>
        _context.Database.IsSqlite() ? "TEXT NOT NULL DEFAULT 'ترخيص'" : "VARCHAR(30) NOT NULL DEFAULT 'ترخيص'";

    private async Task SeedDemoOperationsAsync()
    {
        if (!await _context.Vehicles.AnyAsync())
        {
            var vehicleTypes = await _context.VehicleTypes
                .OrderBy(v => v.Id)
                .ToListAsync();

            var defaultVehicleTypeId = vehicleTypes.FirstOrDefault()?.Id ?? 1;

            _context.Vehicles.AddRange(
                new Vehicle
                {
                    PlateNumber = "س ي 2415",
                    VehicleTypeId = defaultVehicleTypeId,
                    Model = "Toyota Corolla",
                    Year = 2023,
                    Manufacturer = "Toyota",
                    Color = "أبيض",
                    ChassisNumber = "CHS-OPS-1001",
                    EngineNumber = "ENG-OPS-1001",
                    Status = "Available",
                    Mileage = 24500,
                    RegistrationStartDate = DateTime.Today.AddMonths(-8),
                    RegistrationExpiryDate = DateTime.Today.AddMonths(4),
                    AccidentInsuranceDetails = "وثيقة حوادث رقم AH-1001",
                    SocialInsuranceDetails = "تأمين اجتماعي ساري حتى نهاية العام",
                    OilChangeIntervalKm = 10000,
                    MaintenanceIntervalKm = 15000,
                    Notes = "سيارة تشغيل داخلية"
                },
                new Vehicle
                {
                    PlateNumber = "س ف 7782",
                    VehicleTypeId = defaultVehicleTypeId,
                    Model = "Hyundai Elantra",
                    Year = 2022,
                    Manufacturer = "Hyundai",
                    Color = "فضي",
                    ChassisNumber = "CHS-OPS-1002",
                    EngineNumber = "ENG-OPS-1002",
                    Status = "Available",
                    Mileage = 31800,
                    RegistrationStartDate = DateTime.Today.AddMonths(-10),
                    RegistrationExpiryDate = DateTime.Today.AddMonths(2),
                    AccidentInsuranceDetails = "وثيقة حوادث رقم AH-1002",
                    SocialInsuranceDetails = "تأمين اجتماعي مجدد",
                    OilChangeIntervalKm = 10000,
                    MaintenanceIntervalKm = 15000,
                    Notes = "تخدم تنقلات الإدارة"
                },
                new Vehicle
                {
                    PlateNumber = "ب ر 9051",
                    VehicleTypeId = defaultVehicleTypeId,
                    Model = "Nissan Sunny",
                    Year = 2024,
                    Manufacturer = "Nissan",
                    Color = "أسود",
                    ChassisNumber = "CHS-OPS-1003",
                    EngineNumber = "ENG-OPS-1003",
                    Status = "Available",
                    Mileage = 12400,
                    RegistrationStartDate = DateTime.Today.AddMonths(-5),
                    RegistrationExpiryDate = DateTime.Today.AddMonths(7),
                    AccidentInsuranceDetails = "وثيقة حوادث رقم AH-1003",
                    SocialInsuranceDetails = "تأمين اجتماعي ساري",
                    OilChangeIntervalKm = 10000,
                    MaintenanceIntervalKm = 15000,
                    Notes = "سيارة مخصصة للمأموريات"
                });
        }

        if (!await _context.Drivers.AnyAsync())
        {
            _context.Drivers.AddRange(
                new Driver
                {
                    FullName = "أحمد محمود علي",
                    NationalId = "29801011234567",
                    LicenseNumber = "DRV-DEMO-001",
                    LicenseStartDate = DateTime.Today.AddYears(-1),
                    LicenseExpiryDate = DateTime.Today.AddYears(1),
                    LicenseType = "مهنية",
                    IsCompanyInsured = true,
                    Governorate = "القاهرة",
                    FullAddress = "مدينة نصر",
                    TrafficUnit = "مرور مدينة نصر",
                    WorkLocation = "الموقع الرئيسي",
                    PhoneNumber = "01000000011",
                    IsActive = true,
                    Address = "مدينة نصر"
                },
                new Driver
                {
                    FullName = "محمد السيد حسن",
                    NationalId = "29605021234567",
                    LicenseNumber = "DRV-DEMO-002",
                    LicenseStartDate = DateTime.Today.AddYears(-1),
                    LicenseExpiryDate = DateTime.Today.AddYears(2),
                    LicenseType = "مهنية",
                    IsCompanyInsured = true,
                    Governorate = "القاهرة",
                    FullAddress = "المعادى",
                    TrafficUnit = "مرور المعادى",
                    WorkLocation = "الموقع الرئيسي",
                    PhoneNumber = "01000000012",
                    IsActive = true,
                    Address = "المعادى"
                },
                new Driver
                {
                    FullName = "خالد إبراهيم سعد",
                    NationalId = "29507151234567",
                    LicenseNumber = "DRV-DEMO-003",
                    LicenseStartDate = DateTime.Today.AddYears(-1),
                    LicenseExpiryDate = DateTime.Today.AddMonths(18),
                    LicenseType = "خاصة",
                    IsCompanyInsured = false,
                    Governorate = "الجيزة",
                    FullAddress = "الهرم",
                    TrafficUnit = "مرور الهرم",
                    WorkLocation = "الموقع الرئيسي",
                    PhoneNumber = "01000000013",
                    IsActive = true,
                    Address = "الهرم"
                });
        }

        if (!await _context.Employees.AnyAsync())
        {
            _context.Employees.AddRange(
                new Employee
                {
                    FullName = "سارة أحمد",
                    EmployeeId = "EMP-DEMO-001",
                    Department = "التشغيل",
                    Position = "موصي",
                    PhoneNumber = "01000000021",
                    Status = "Active"
                },
                new Employee
                {
                    FullName = "محمود فتحي",
                    EmployeeId = "EMP-DEMO-002",
                    Department = "الإدارة",
                    Position = "مشرف",
                    PhoneNumber = "01000000022",
                    Status = "Active"
                },
                new Employee
                {
                    FullName = "ندى سمير",
                    EmployeeId = "EMP-DEMO-003",
                    Department = "المشروعات",
                    Position = "موصي",
                    PhoneNumber = "01000000023",
                    Status = "Active"
                },
                new Employee
                {
                    FullName = "هاني عبد الله",
                    EmployeeId = "EMP-DEMO-004",
                    Department = "التشغيل",
                    Position = "مشرف",
                    PhoneNumber = "01000000024",
                    Status = "Active"
                });
        }

        var vehiclesWithoutRegistrationType = await _context.Vehicles
            .Where(vehicle => string.IsNullOrWhiteSpace(vehicle.RegistrationType))
            .ToListAsync();
        foreach (var vehicle in vehiclesWithoutRegistrationType)
        {
            vehicle.RegistrationType = "ترخيص";
        }

        await _context.SaveChangesAsync();

        var seededVehicles = await _context.Vehicles
            .OrderBy(v => v.Id)
            .Take(3)
            .ToListAsync();
        var seededEmployees = await _context.Employees
            .OrderBy(e => e.Id)
            .Take(4)
            .ToListAsync();

        if (!await _context.Insurances.AnyAsync() && seededVehicles.Count > 0)
        {
            var policies = seededVehicles.Select((vehicle, index) =>
            {
                var policyNumber = $"AH-DEMO-{index + 1:000}";
                var expiryDate = index switch
                {
                    0 => DateTime.Today.AddDays(20),
                    1 => DateTime.Today.AddMonths(3),
                    _ => DateTime.Today.AddDays(-5)
                };

                vehicle.AccidentInsuranceDetails = $"تأمين حوادث رقم {policyNumber} حتى {expiryDate:yyyy-MM-dd}";

                return new Insurance
                {
                    VehicleId = vehicle.Id,
                    PolicyNumber = policyNumber,
                    InsuranceCompany = index switch
                    {
                        0 => "مصر للتأمين",
                        1 => "قناة السويس للتأمين",
                        _ => "المهندس للتأمين"
                    },
                    PolicyType = "تأمين حوادث",
                    StartDate = DateTime.Today.AddMonths(-11 + index),
                    ExpiryDate = expiryDate,
                    PremiumAmount = 3500 + (index * 450),
                    CoverageAmount = 250000,
                    CoverageDetails = "تغطية حوادث ومسؤولية مدنية",
                    AgentName = "مسؤول التأمين",
                    AgentPhoneNumber = $"0100000003{index + 1}",
                    Status = ServiceHelpers.InsuranceStatus(expiryDate),
                    Notes = "بيانات تجريبية قابلة للتعديل"
                };
            });

            _context.Insurances.AddRange(policies);
        }

        if (!await _context.Custodies.AnyAsync() && seededVehicles.Count > 0 && seededEmployees.Count > 0)
        {
            _context.Custodies.AddRange(
                new Custody
                {
                    VehicleId = seededVehicles[0].Id,
                    EmployeeId = seededEmployees[0].Id,
                    CustodyNumber = "CU-DEMO-001",
                    CustodianName = seededEmployees[0].FullName,
                    CustodianPosition = seededEmployees[0].Position,
                    HandoverDate = DateTime.Today.AddDays(-14),
                    Status = "Active",
                    VehicleConditionRating = 8,
                    Notes = "عهدة تشغيل يومية"
                },
                new Custody
                {
                    VehicleId = seededVehicles[Math.Min(1, seededVehicles.Count - 1)].Id,
                    EmployeeId = seededEmployees[Math.Min(1, seededEmployees.Count - 1)].Id,
                    CustodyNumber = "CU-DEMO-002",
                    CustodianName = seededEmployees[Math.Min(1, seededEmployees.Count - 1)].FullName,
                    CustodianPosition = seededEmployees[Math.Min(1, seededEmployees.Count - 1)].Position,
                    HandoverDate = DateTime.Today.AddDays(-30),
                    ReturnDate = DateTime.Today.AddDays(-3),
                    Status = "Returned",
                    VehicleConditionRating = 7,
                    Notes = "عهدة مرتجعة بعد مأمورية"
                });
        }

        await _context.SaveChangesAsync();

        if (await _context.Trips.AnyAsync())
        {
            return;
        }

        var vehicles = await _context.Vehicles
            .OrderBy(v => v.Id)
            .Take(3)
            .ToListAsync();
        var drivers = await _context.Drivers
            .OrderBy(d => d.Id)
            .Take(3)
            .ToListAsync();
        var employees = await _context.Employees
            .OrderBy(e => e.Id)
            .Take(4)
            .ToListAsync();

        if (vehicles.Count < 3 || drivers.Count < 2 || employees.Count < 4)
        {
            return;
        }

        _context.Trips.AddRange(
            new Trip
            {
                VehicleId = vehicles[0].Id,
                DriverId = drivers[0].Id,
                RequesterEmployeeId = employees[0].Id,
                RequesterNameText = employees[0].FullName,
                SupervisorEmployeeId = employees[1].Id,
                StartDate = DateTime.Today.AddDays(-2).AddHours(9),
                EndDate = DateTime.Today.AddDays(-2).AddHours(13),
                StartLocation = "المقر الرئيسي",
                EndLocation = "فرع مدينة نصر",
                StartMileage = 24420,
                EndMileage = 24500,
                Distance = 80,
                Purpose = "توصيل مستندات وتشغيل يومي",
                Status = "Closed",
                Notes = "بيانات تجريبية"
            },
            new Trip
            {
                VehicleId = vehicles[1].Id,
                DriverId = drivers[1].Id,
                RequesterEmployeeId = employees[2].Id,
                RequesterNameText = employees[2].FullName,
                SupervisorEmployeeId = employees[3].Id,
                StartDate = DateTime.Today.AddDays(-1).AddHours(8),
                EndDate = DateTime.Today.AddDays(-1).AddHours(12),
                StartLocation = "الجيزة",
                EndLocation = "6 أكتوبر",
                StartMileage = 31720,
                EndMileage = 31800,
                Distance = 80,
                Purpose = "مأمورية متابعة موقع",
                Status = "Closed",
                Notes = "بيانات تجريبية"
            });

        await _context.SaveChangesAsync();
    }
}

public sealed class AuthenticationService(FleetDbContext context, IAuditService auditService) : IAuthenticationService
{
    private const int MaxFailedLoginAttempts = 5;
    private static readonly TimeSpan LoginLockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly ConcurrentDictionary<string, LoginFailureState> LoginFailures = new(StringComparer.OrdinalIgnoreCase);

    private readonly FleetDbContext _context = context;
    private readonly IAuditService _auditService = auditService;

    public async Task<UserDto?> AuthenticateAsync(UserLoginDto dto)
    {
        var username = ServiceHelpers.Clean(dto.Username);
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return null;
        }

        ThrowIfLoginLocked(username);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.IsActive && (u.Username == username || u.Email == username));
        if (user is null)
        {
            RegisterFailedLogin(username);
            return null;
        }

        // حماية من حسابات بلا كلمة مرور صالحة (مثلاً حساب أُنشئ بهاش فارغ):
        // نقبل الدخول بكلمة المرور الافتراضية (نفس اسم المستخدم) ونطلب تغييرها، بدل أن يرمي BCrypt استثناء "Invalid salt".
        bool passwordOk;
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            passwordOk = string.Equals(dto.Password, user.Username, StringComparison.Ordinal);
            if (passwordOk)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Username);
                user.MustChangePassword = true;
            }
        }
        else
        {
            try
            {
                passwordOk = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            }
            catch
            {
                // هاش تالف/غير صالح — نعامله كحساب بلا كلمة مرور.
                passwordOk = string.Equals(dto.Password, user.Username, StringComparison.Ordinal);
                if (passwordOk)
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Username);
                    user.MustChangePassword = true;
                }
            }
        }

        if (!passwordOk)
        {
            RegisterFailedLogin(username);
            ThrowIfLoginLocked(username);
            return null;
        }

        LoginFailures.TryRemove(username, out _);

        user.LastLogin = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogActionAsync("Login", "User", user.Id, null, user.Username, user.Id, user.Username);
        return user.ToDto();
    }

    public async Task<List<UserListItemDto>> GetUsersAsync() =>
        (await _context.Users
            .OrderBy(u => u.Username)
            .ToListAsync())
        .Select(u => u.ToListItemDto())
        .ToList();

    public async Task<UserDto> SaveUserAsync(UserFormDto dto)
    {
        var username = ServiceHelpers.Clean(dto.Username);
        var email = ServiceHelpers.Clean(dto.Email);
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidOperationException("اسم المستخدم مطلوب.");
        }

        var duplicate = await _context.Users.FirstOrDefaultAsync(u =>
            u.Id != dto.Id &&
            (u.Username == username || (email != string.Empty && u.Email == email)));

        if (duplicate is not null)
        {
            throw new InvalidOperationException("اسم المستخدم أو البريد الإلكتروني مستخدم بالفعل.");
        }

        User entity;
        var action = dto.Id == 0 ? "Create" : "Update";
        if (dto.Id == 0)
        {
            entity = new User();
            _context.Users.Add(entity);
        }
        else
        {
            entity = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.Id)
                ?? throw new InvalidOperationException("المستخدم غير موجود.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            ValidatePasswordPolicy(dto.Password, dto.ConfirmPassword, username);
        }

        entity.Username = username;
        entity.Email = email;
        entity.FullName = ServiceHelpers.Clean(dto.FullName);
        entity.PhoneNumber = ServiceHelpers.Clean(dto.PhoneNumber);
        entity.Role = Enum.TryParse<UserRole>(dto.Role, true, out var role) ? role : UserRole.Staff;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        if (dto.Id == 0)
        {
            entity.CreatedAt = DateTime.UtcNow;
        }

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            entity.MustChangePassword = false;
        }
        else if (dto.Id == 0)
        {
            // كلمة المرور الافتراضية للمستخدم الجديد = اسم المستخدم، ويُطلب منه تغييرها بعد الدخول.
            entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(username);
            entity.MustChangePassword = true;
        }

        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync(action, "User", entity.Id, null, entity.Username, entity.Id, entity.Username);
        return entity.ToDto();
    }

    public async Task ToggleUserStatusAsync(int id, bool isActive)
    {
        var entity = await _context.Users.FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new InvalidOperationException("المستخدم غير موجود.");

        entity.IsActive = isActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync("Status", "User", entity.Id, null, isActive.ToString(), entity.Id, entity.Username);
    }

    public async Task ChangePasswordAsync(ChangePasswordDto dto)
    {
        var entity = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId)
            ?? throw new InvalidOperationException("المستخدم غير موجود.");

        if (string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
            !BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, entity.PasswordHash))
        {
            throw new InvalidOperationException("كلمة المرور الحالية غير صحيحة.");
        }

        ValidatePasswordPolicy(dto.NewPassword, dto.ConfirmPassword, entity.Username);

        entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        entity.MustChangePassword = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync("ChangePassword", "User", entity.Id, null, entity.Username, entity.Id, entity.Username);
    }

    private static void ThrowIfLoginLocked(string username)
    {
        if (!LoginFailures.TryGetValue(username, out var state) || state.LockedUntilUtc is null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        if (state.LockedUntilUtc <= now)
        {
            LoginFailures.TryRemove(username, out _);
            return;
        }

        var remainingMinutes = Math.Max(1, (int)Math.Ceiling((state.LockedUntilUtc.Value - now).TotalMinutes));
        throw new InvalidOperationException($"تم إيقاف محاولات الدخول لهذا المستخدم مؤقتًا بسبب تكرار كلمة مرور خاطئة. حاول بعد {remainingMinutes} دقيقة.");
    }

    private static void RegisterFailedLogin(string username)
    {
        var now = DateTime.UtcNow;
        LoginFailures.AddOrUpdate(
            username,
            _ => new LoginFailureState(1, null),
            (_, state) =>
            {
                if (state.LockedUntilUtc.HasValue && state.LockedUntilUtc.Value > now)
                {
                    return state;
                }

                var nextCount = state.LockedUntilUtc.HasValue && state.LockedUntilUtc.Value <= now
                    ? 1
                    : state.Count + 1;

                return nextCount >= MaxFailedLoginAttempts
                    ? new LoginFailureState(nextCount, now.Add(LoginLockoutDuration))
                    : new LoginFailureState(nextCount, null);
            });
    }

    private static void ValidatePasswordPolicy(string password, string confirmPassword, string username)
    {
        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("تأكيد كلمة المرور غير مطابق.");
        }

        if (password.Length < 8 ||
            !password.Any(char.IsLetter) ||
            !password.Any(char.IsDigit) ||
            !password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            throw new InvalidOperationException("كلمة المرور يجب ألا تقل عن 8 أحرف وتحتوي على حرف ورقم ورمز.");
        }

        if (string.Equals(password, username, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("كلمة المرور لا يمكن أن تكون نفس اسم المستخدم.");
        }
    }

    private sealed record LoginFailureState(int Count, DateTime? LockedUntilUtc);
}

public sealed class AuditService(FleetDbContext context) : IAuditService
{
    private readonly FleetDbContext _context = context;

    public async Task LogActionAsync(string action, string entityType, int entityId, string? oldValues = null, string? newValues = null, int? userId = null, string? userName = null, string? ipAddress = null)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityName = entityType,
            EntityId = entityId,
            Description = $"{action} {entityType} #{entityId}",
            UserId = userId,
            UserName = userName ?? string.Empty,
            OldValues = oldValues ?? string.Empty,
            NewValues = newValues ?? string.Empty,
            IpAddress = ipAddress ?? string.Empty,
            Timestamp = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    public async Task<List<AuditLogDto>> GetRecentAsync(int count = 50) =>
        (await _context.AuditLogs
            .OrderByDescending(x => x.Timestamp)
            .Take(Math.Max(count, 1))
            .ToListAsync())
        .Select(x => x.ToDto())
        .ToList();
}

public sealed class NotificationService(FleetDbContext context) : INotificationService
{
    private readonly FleetDbContext _context = context;

    public async Task CreateNotificationAsync(string title, string message, string type, string? relatedEntityType = null, int? relatedEntityId = null, int? userId = null, DateTime? expiresAt = null)
    {
        _context.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = ServiceHelpers.Clean(title),
            Message = ServiceHelpers.Clean(message),
            Type = string.IsNullOrWhiteSpace(type) ? "Info" : type,
            RelatedEntityType = relatedEntityType ?? string.Empty,
            RelatedEntityId = relatedEntityId,
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    public async Task<List<NotificationDto>> GetActiveNotificationsAsync() =>
        (await _context.Notifications
            .Where(n => !n.ExpiresAt.HasValue || n.ExpiresAt >= DateTime.UtcNow)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync())
        .Select(n => n.ToDto())
        .ToList();

    public async Task MarkAsReadAsync(int id)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id)
            ?? throw new InvalidOperationException("الإشعار غير موجود.");

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}

public sealed class SettingsService(FleetDbContext context, IAuditService auditService) : ISettingsService
{
    private readonly FleetDbContext _context = context;
    private readonly IAuditService _auditService = auditService;

    public async Task<CompanySettingsDto> GetSettingsAsync()
    {
        var settings = await _context.CompanySettings.OrderBy(x => x.Id).FirstOrDefaultAsync();
        if (settings is null)
        {
            settings = new CompanySettings();
            _context.CompanySettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return settings.ToDto();
    }

    public async Task<CompanySettingsDto> SaveSettingsAsync(CompanySettingsDto dto)
    {
        var settings = await _context.CompanySettings.OrderBy(x => x.Id).FirstOrDefaultAsync();
        var action = settings is null ? "Create" : "Update";
        settings ??= new CompanySettings();

        settings.CompanyName = ServiceHelpers.Clean(dto.CompanyName);
        settings.CompanyNameEn = ServiceHelpers.Clean(dto.CompanyNameEn);
        settings.LogoUrl = ServiceHelpers.Clean(dto.LogoUrl);
        settings.Address = ServiceHelpers.Clean(dto.Address);
        settings.PhoneNumber = ServiceHelpers.Clean(dto.PhoneNumber);
        settings.Email = ServiceHelpers.Clean(dto.Email);
        settings.Website = ServiceHelpers.Clean(dto.Website);
        settings.TaxId = ServiceHelpers.Clean(dto.TaxId);
        settings.CommercialRegistration = ServiceHelpers.Clean(dto.CommercialRegistration);
        settings.CurrencySymbol = ServiceHelpers.Clean(dto.CurrencySymbol);
        settings.DateFormat = ServiceHelpers.Clean(dto.DateFormat);
        settings.TimeFormat = ServiceHelpers.Clean(dto.TimeFormat);
        settings.DecimalPlaces = dto.DecimalPlaces;
        settings.DefaultLanguage = ServiceHelpers.Clean(dto.DefaultLanguage);
        settings.DefaultTheme = ServiceHelpers.Clean(dto.DefaultTheme);
        settings.UpdatedAt = DateTime.UtcNow;

        if (settings.Id == 0)
        {
            settings.CreatedAt = DateTime.UtcNow;
            _context.CompanySettings.Add(settings);
        }

        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync(action, "CompanySettings", settings.Id, null, settings.CompanyName);
        return settings.ToDto();
    }
}

public sealed class ReportingService(FleetDbContext context) : IReportingService
{
    private const int VehicleRegistrationAlertDays = 60;
    private readonly FleetDbContext _context = context;

    public async Task<DashboardMetricsDto> GetDashboardMetricsAsync()
    {
        var totalExpenses = (await _context.Expenses.Select(x => x.Amount).ToListAsync()).Sum();
        var totalFuelCost = (await _context.FuelTransactions.Select(x => x.TotalCost).ToListAsync()).Sum();
        var treasuryBalance = (await _context.TreasuryTransactions
            .Select(x => new { x.TransactionType, x.Amount, x.Status })
            .ToListAsync())
            .Where(x => !ServiceHelpers.IsTreasuryPending(x.Status))
            .Sum(x => ServiceHelpers.IsTreasuryIncome(x.TransactionType) ? x.Amount : -x.Amount);
        var today = DateTime.Today;
        var oilChangesDue = await CountOilAlertsAsync();

        var metrics = new DashboardMetricsDto
        {
            TotalVehicles = await _context.Vehicles.CountAsync(),
            AvailableVehicles = await _context.Vehicles.CountAsync(v => v.Status == null || v.Status == "" || v.Status == "Active" || v.Status == "Available" || v.Status == "متاح" || v.Status == "نشط"),
            ActiveVehicles = await _context.Vehicles.CountAsync(v => v.Status == null || v.Status == "" || v.Status == "Active" || v.Status == "Available" || v.Status == "متاح" || v.Status == "نشط"),
            VehiclesInTrip = await _context.Vehicles.CountAsync(v => v.Status == "InTrip" || v.Status == "جارية"),
            VehiclesInMaintenance = await _context.Vehicles.CountAsync(v => v.Status == "Maintenance" || v.Status.Contains("صيانة")),
            TotalContracts = await _context.Contracts.CountAsync(),
            ActiveContracts = await _context.Contracts.CountAsync(c => c.EndDate >= DateTime.Today),
            ExpiringContracts = await _context.Contracts.CountAsync(c => c.EndDate >= DateTime.Today && c.EndDate <= DateTime.Today.AddDays(30)),
            OpenTrips = await _context.Trips.CountAsync(t => t.EndDate == null || t.Status == "Open" || t.Status == "Planned" || t.Status == "InProgress" || t.Status == "InTrip" || t.Status == "مفتوحة" || t.Status == "جارية"),
            TripsToday = await _context.Trips.CountAsync(t => t.StartDate.Date == today),
            OpenMaintenanceRequests = await _context.MaintenanceRequests.CountAsync(m => m.Status != "Completed" && m.Status != "مكتمل"),
            CompletedMaintenanceRequests = await _context.MaintenanceRequests.CountAsync(m => m.Status == "Completed" || m.Status == "مكتمل"),
            VehicleLicensesExpiring = await _context.Vehicles.CountAsync(v => v.RegistrationExpiryDate.HasValue && v.RegistrationExpiryDate.Value <= today.AddDays(VehicleRegistrationAlertDays)),
            DriversWithExpiringLicenses = await _context.Drivers.CountAsync(d => d.LicenseExpiryDate.HasValue && d.LicenseExpiryDate.Value >= DateTime.Today && d.LicenseExpiryDate.Value <= DateTime.Today.AddDays(30)),
            InsurancePoliciesExpiring = await _context.Insurances.CountAsync(i => i.ExpiryDate <= DateTime.Today.AddDays(60)),
            OilChangesDue = oilChangesDue,
            TotalExpenses = totalExpenses,
            TotalFuelCost = totalFuelCost,
            TreasuryBalance = treasuryBalance
        };

        metrics.RecentActivities = await GetRecentActivityAsync(10);
        metrics.Alerts = await GetAlertsAsync();
        return metrics;
    }

    public async Task<List<AlertDto>> GetAlertsAsync()
    {
        var alerts = new List<AlertDto>();
        var now = DateTime.Today;

        var expiringContracts = await _context.Contracts
            .Include(c => c.Vehicle)
            .Where(c => c.EndDate >= now && c.EndDate <= now.AddDays(30))
            .OrderBy(c => c.EndDate)
            .Take(10)
            .ToListAsync();

        alerts.AddRange(expiringContracts.Select(c => new AlertDto
        {
            Id = c.Id,
            Type = "Warning",
            Title = "عقد يقترب من الانتهاء",
            Message = $"العقد {c.ContractNumber} للمركبة {c.Vehicle?.PlateNumber} ينتهي بتاريخ {c.EndDate:yyyy-MM-dd}.",
            RelatedEntityType = "Contract",
            RelatedEntityId = c.Id,
            DueDate = c.EndDate,
            CreatedAt = c.UpdatedAt
        }));

        var expiringLicenses = await _context.Licenses
            .Include(l => l.Driver)
            .Where(l => l.ExpiryDate >= now && l.ExpiryDate <= now.AddDays(30))
            .OrderBy(l => l.ExpiryDate)
            .Take(10)
            .ToListAsync();

        alerts.AddRange(expiringLicenses.Select(l => new AlertDto
        {
            Id = l.Id,
            Type = "Warning",
            Title = "رخصة سائق تقترب من الانتهاء",
            Message = $"رخصة السائق {l.Driver?.FullName} تنتهي بتاريخ {l.ExpiryDate:yyyy-MM-dd}.",
            RelatedEntityType = "License",
            RelatedEntityId = l.Id,
            DueDate = l.ExpiryDate,
            CreatedAt = l.UpdatedAt
        }));

        var expiringDrivers = await _context.Drivers
            .Where(d => d.LicenseExpiryDate.HasValue && d.LicenseExpiryDate.Value <= now.AddDays(30))
            .OrderBy(d => d.LicenseExpiryDate)
            .Take(10)
            .ToListAsync();

        alerts.AddRange(expiringDrivers.Select(d => new AlertDto
        {
            Id = d.Id,
            Type = "Warning",
            Title = d.LicenseExpiryDate!.Value.Date < now ? "رخصة سائق منتهية" : "رخصة سائق تقترب من الانتهاء",
            Message = d.LicenseExpiryDate!.Value.Date < now
                ? $"رخصة السائق {d.FullName} منتهية منذ {d.LicenseExpiryDate:yyyy-MM-dd}."
                : $"رخصة السائق {d.FullName} تنتهي بتاريخ {d.LicenseExpiryDate:yyyy-MM-dd}.",
            RelatedEntityType = "Driver",
            RelatedEntityId = d.Id,
            DueDate = d.LicenseExpiryDate,
            CreatedAt = d.UpdatedAt
        }));

        var expiringVehicleRegistrations = await _context.Vehicles
            .Where(v => v.RegistrationExpiryDate.HasValue && v.RegistrationExpiryDate.Value <= now.AddDays(VehicleRegistrationAlertDays))
            .OrderBy(v => v.RegistrationExpiryDate)
            .Take(10)
            .ToListAsync();

        alerts.AddRange(expiringVehicleRegistrations.Select(v => new AlertDto
        {
            Id = v.Id,
            Type = "Warning",
            Title = v.RegistrationExpiryDate!.Value.Date < now ? "ترخيص عربية منتهي" : "ترخيص عربية يحتاج تجديد",
            Message = v.RegistrationExpiryDate!.Value.Date < now
                ? $"ترخيص العربية {v.PlateNumber} منتهي منذ {v.RegistrationExpiryDate:yyyy-MM-dd} ويحتاج تجديد."
                : $"العربية {v.PlateNumber} محتاجة تجديد ترخيص قبل {v.RegistrationExpiryDate:yyyy-MM-dd}.",
            RelatedEntityType = "Vehicle",
            RelatedEntityId = v.Id,
            DueDate = v.RegistrationExpiryDate,
            CreatedAt = v.UpdatedAt
        }));

        var expiringInsurance = await _context.Insurances
            .Include(i => i.Vehicle)
            .Where(i => i.ExpiryDate <= now.AddDays(60))
            .OrderBy(i => i.ExpiryDate)
            .Take(10)
            .ToListAsync();

        alerts.AddRange(expiringInsurance.Select(i => new AlertDto
        {
            Id = i.Id,
            Type = "Warning",
            Title = i.ExpiryDate.Date < now ? "تأمين عربية منتهي" : "تأمين عربية يحتاج تجديد",
            Message = i.ExpiryDate.Date < now
                ? $"تأمين العربية {i.Vehicle?.PlateNumber} منتهي منذ {i.ExpiryDate:yyyy-MM-dd} ويحتاج تجديد."
                : $"العربية {i.Vehicle?.PlateNumber} محتاجة تجديد تأمين قبل {i.ExpiryDate:yyyy-MM-dd}.",
            RelatedEntityType = "Insurance",
            RelatedEntityId = i.Id,
            DueDate = i.ExpiryDate,
            CreatedAt = i.UpdatedAt
        }));

        var dueOilChanges = await GetLatestOilChangesWithVehiclesAsync();
        alerts.AddRange(dueOilChanges
            .Where(x => x.Vehicle is not null && GetRemainingOilKm(x) <= ServiceHelpers.DefaultOilAlertThresholdKm)
            .OrderBy(GetRemainingOilKm)
            .Take(10)
            .Select(BuildOilChangeAlert));

        var vehiclesMissingOilChange = await _context.Vehicles
            .Include(v => v.OilChanges)
            .Where(v => !v.OilChanges.Any(x => x.IsOilChanged) && v.OilChangeIntervalKm > 0)
            .ToListAsync();

        alerts.AddRange(vehiclesMissingOilChange
            .Where(v => v.OilChangeIntervalKm - v.Mileage <= ServiceHelpers.DefaultOilAlertThresholdKm)
            .OrderBy(v => v.OilChangeIntervalKm - v.Mileage)
            .Take(10)
            .Select(BuildMissingOilChangeAlert));

        var openMaintenance = await _context.MaintenanceRequests
            .Include(m => m.Vehicle)
            .Where(m => m.Status != "Completed" && m.Status != "مكتمل" && m.RequestDate <= now.AddDays(-7))
            .OrderBy(m => m.RequestDate)
            .Take(10)
            .ToListAsync();

        alerts.AddRange(openMaintenance.Select(m => new AlertDto
        {
            Id = m.Id,
            Type = "Info",
            Title = "طلب صيانة مفتوح منذ فترة",
            Message = $"طلب الصيانة للمركبة {m.Vehicle?.PlateNumber} ما زال مفتوحًا منذ {m.RequestDate:yyyy-MM-dd}.",
            RelatedEntityType = "Maintenance",
            RelatedEntityId = m.Id,
            CreatedAt = m.UpdatedAt
        }));

        var overdueTrips = await _context.Trips
            .Include(t => t.Vehicle)
            .Where(t => (t.EndDate == null || t.Status == "Open" || t.Status == "Planned" || t.Status == "InProgress" || t.Status == "InTrip" || t.Status == "مفتوحة" || t.Status == "جارية") && t.StartDate <= now.AddDays(-1))
            .OrderBy(t => t.StartDate)
            .Take(10)
            .ToListAsync();

        alerts.AddRange(overdueTrips.Select(t => new AlertDto
        {
            Id = t.Id,
            Type = "Info",
            Title = "رحلة مفتوحة تحتاج إغلاق",
            Message = $"رحلة المركبة {t.Vehicle?.PlateNumber} ما زالت مفتوحة منذ {t.StartDate:yyyy-MM-dd HH:mm}.",
            RelatedEntityType = "Trip",
            RelatedEntityId = t.Id,
            CreatedAt = t.UpdatedAt
        }));

        return alerts
            .OrderBy(a => GetAlertPriority(a, now))
            .ThenBy(a => a.DueDate.HasValue ? Math.Abs((a.DueDate.Value.Date - now).Days) : int.MaxValue)
            .ThenByDescending(a => a.CreatedAt)
            .Take(20)
            .ToList();
    }

    private static int GetAlertPriority(AlertDto alert, DateTime today)
    {
        if (!alert.DueDate.HasValue)
        {
            return alert.Type switch
            {
                "Critical" or "Error" => 0,
                "Warning" => 1,
                _ => 3
            };
        }

        var days = (alert.DueDate.Value.Date - today).Days;
        return days switch
        {
            < 0 => 0,
            <= 7 => 1,
            _ => 2
        };
    }

    private async Task<List<OilChange>> GetLatestOilChangesWithVehiclesAsync()
    {
        var oilChanges = await _context.OilChanges
            .Include(x => x.Vehicle)
            .Where(x => x.IsOilChanged)
            .ToListAsync();

        return oilChanges
            .Where(x => x.Vehicle is not null)
            .GroupBy(x => x.VehicleId)
            .Select(group => group
                .OrderByDescending(x => x.ChangeDate)
                .ThenByDescending(x => x.OdometerAtChange)
                .ThenByDescending(x => x.Id)
                .First())
            .ToList();
    }

    private async Task<int> CountOilAlertsAsync()
    {
        var latestOilChanges = await GetLatestOilChangesWithVehiclesAsync();
        var dueFromRecordedOil = latestOilChanges.Count(x => GetRemainingOilKm(x) <= ServiceHelpers.DefaultOilAlertThresholdKm);

        var vehiclesWithoutOilChange = await _context.Vehicles
            .Include(v => v.OilChanges)
            .Where(v => !v.OilChanges.Any(x => x.IsOilChanged) && v.OilChangeIntervalKm > 0)
            .ToListAsync();

        return dueFromRecordedOil + vehiclesWithoutOilChange.Count(v => v.OilChangeIntervalKm - v.Mileage <= ServiceHelpers.DefaultOilAlertThresholdKm);
    }

    private async Task<Dictionary<int, (decimal Quantity, decimal TotalCost)>> GetFuelTotalsByVehicleAsync(DateTime? startDate, DateTime? endExclusive)
    {
        var query = _context.FuelTransactions.AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(f => f.TransactionDate >= startDate.Value);
        }

        if (endExclusive.HasValue)
        {
            query = query.Where(f => f.TransactionDate < endExclusive.Value);
        }

        var fuelRows = await query
            .Select(f => new { f.VehicleId, f.Quantity, f.TotalCost })
            .ToListAsync();

        return fuelRows
            .GroupBy(f => f.VehicleId)
            .ToDictionary(
                group => group.Key,
                group => (Quantity: group.Sum(f => f.Quantity), TotalCost: group.Sum(f => f.TotalCost)));
    }

    private static decimal GetRemainingOilKm(OilChange oilChange) =>
        oilChange.Vehicle is null ? decimal.MaxValue : oilChange.NextOilChangeOdometer - oilChange.Vehicle.Mileage;

    private static AlertDto BuildOilChangeAlert(OilChange oilChange)
    {
        var remainingKm = GetRemainingOilKm(oilChange);
        return new AlertDto
        {
            Id = oilChange.Id,
            Type = remainingKm < 0 ? "Critical" : "Warning",
            Title = remainingKm < 0 ? "تغيير زيت متأخر" : "تغيير زيت قريب",
            Message = remainingKm < 0
                ? $"العربية {oilChange.Vehicle?.PlateNumber} تعدت موعد تغيير الزيت بـ {Math.Abs(remainingKm):0} كم. آخر تغيير كان عند {oilChange.OdometerAtChange:0} كم."
                : $"العربية {oilChange.Vehicle?.PlateNumber} متبقي لها {remainingKm:0} كم على تغيير الزيت. آخر تغيير كان عند {oilChange.OdometerAtChange:0} كم.",
            RelatedEntityType = "OilChange",
            RelatedEntityId = oilChange.Id,
            DueDate = DateTime.Today,
            CreatedAt = oilChange.UpdatedAt
        };
    }

    private static AlertDto BuildMissingOilChangeAlert(Vehicle vehicle)
    {
        var remainingKm = vehicle.OilChangeIntervalKm - vehicle.Mileage;
        return new AlertDto
        {
            Id = vehicle.Id,
            Type = remainingKm < 0 ? "Critical" : "Warning",
            Title = remainingKm < 0 ? "لا يوجد سجل زيت والسيارة تعدت الدورية" : "لا يوجد سجل تغيير زيت",
            Message = remainingKm < 0
                ? $"العربية {vehicle.PlateNumber} لا يوجد لها سجل تغيير زيت وتعدت الدورية المسجلة بـ {Math.Abs(remainingKm):0} كم."
                : $"العربية {vehicle.PlateNumber} لا يوجد لها سجل تغيير زيت ومتبقي لها {remainingKm:0} كم حسب الدورية المسجلة.",
            RelatedEntityType = "OilChange",
            RelatedEntityId = vehicle.Id,
            DueDate = DateTime.Today,
            CreatedAt = vehicle.UpdatedAt
        };
    }

    public async Task<List<RecentActivityDto>> GetRecentActivityAsync(int count = 10) =>
        await _context.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(Math.Max(count, 1))
            .Select(a => new RecentActivityDto
            {
                Id = a.Id,
                EntityType = a.EntityType,
                EntityName = string.IsNullOrWhiteSpace(a.EntityName) ? a.EntityType : a.EntityName,
                Action = a.Action,
                UserName = a.UserName,
                Timestamp = a.Timestamp
            })
            .ToListAsync();

    public async Task<ReportDataDto> GenerateReportAsync(ReportFilterDto filter)
    {
        var reportType = ServiceHelpers.Clean(filter.ReportType).ToLowerInvariant();
        var startDate = filter.StartDate?.Date;
        var endExclusive = filter.EndDate?.Date.AddDays(1);

        if (startDate.HasValue && filter.EndDate.HasValue && filter.EndDate.Value.Date < startDate.Value)
        {
            throw new InvalidOperationException("تاريخ نهاية التقرير يجب أن يكون بعد تاريخ البداية.");
        }

        if (reportType == "vehicletrips" || reportType == "alltrips")
        {
            return await GenerateVehicleTripsReportAsync(filter);
        }

        if (reportType == "fuel")
        {
            var query = _context.FuelTransactions
                .Include(f => f.Vehicle)
                .Include(f => f.Trip)
                .AsQueryable();

            if (filter.VehicleId.HasValue)
            {
                query = query.Where(f => f.VehicleId == filter.VehicleId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(f => f.TransactionDate >= startDate.Value);
            }

            if (endExclusive.HasValue)
            {
                query = query.Where(f => f.TransactionDate < endExclusive.Value);
            }

            var fuelTransactions = await query
                .OrderByDescending(f => f.TransactionDate)
                .ThenByDescending(f => f.Id)
                .ToListAsync();

            return new ReportDataDto
            {
                ReportTitle = "تقرير البنزين",
                GeneratedDate = DateTime.UtcNow,
                GeneratedBy = "System",
                Columns = new List<string> { "التاريخ", "رقم السيارة", "رقم التشغيلة", "نوع الوقود", "عدد اللترات", "سعر اللتر", "إجمالي البنزين", "محطة البنزين", "عداد التموين", "من الخزينة" },
                Data = fuelTransactions.Select(f => new Dictionary<string, object>
                {
                    ["التاريخ"] = f.TransactionDate,
                    ["رقم السيارة"] = f.Vehicle?.PlateNumber ?? string.Empty,
                    ["رقم التشغيلة"] = f.TripId?.ToString() ?? string.Empty,
                    ["نوع الوقود"] = f.FuelType,
                    ["عدد اللترات"] = f.Quantity,
                    ["سعر اللتر"] = f.UnitPrice,
                    ["إجمالي البنزين"] = f.TotalCost,
                    ["محطة البنزين"] = f.FuelStation,
                    ["عداد التموين"] = f.Odometer ?? 0,
                    ["من الخزينة"] = f.PaidFromTreasury ? "نعم" : "لا"
                }).ToList()
            };
        }

        if (reportType == "vehiclelicenses")
        {
            var query = ApplyVehicleRegistrationOverlapDateFilter(_context.Vehicles.AsQueryable(), startDate, endExclusive);

            var vehicles = await query
                .OrderBy(v => v.PlateNumber)
                .ToListAsync();

            return new ReportDataDto
            {
                ReportTitle = "تقرير تراخيص العربيات",
                GeneratedDate = DateTime.UtcNow,
                GeneratedBy = "System",
                Columns = new List<string> { "رقم السيارة", "الموديل", "سنة الصنع", "رقم الشاسيه", "رقم الموتور", "تغيير الزيت كل كام كم", "بداية الترخيص", "نهاية الترخيص", "الحالة", "الإنذار" },
                Data = vehicles.Select(v => new Dictionary<string, object>
                {
                    ["رقم السيارة"] = v.PlateNumber,
                    ["الموديل"] = v.Model,
                    ["سنة الصنع"] = v.Year,
                    ["رقم الشاسيه"] = v.ChassisNumber,
                    ["رقم الموتور"] = v.EngineNumber,
                    ["تغيير الزيت كل كام كم"] = v.OilChangeIntervalKm,
                    ["بداية الترخيص"] = DateOnly(v.RegistrationStartDate),
                    ["نهاية الترخيص"] = DateOnly(v.RegistrationExpiryDate),
                    ["الحالة"] = VehicleLicenseStatus(v.RegistrationExpiryDate),
                    ["الإنذار"] = VehicleLicenseAlert(v.RegistrationExpiryDate)
                }).ToList()
            };
        }

        if (reportType == "insurance")
        {
            var query = _context.Insurances
                .Include(i => i.Vehicle)
                .AsQueryable();

            if (startDate.HasValue || endExclusive.HasValue)
            {
                query = query.Where(i =>
                    ((!startDate.HasValue || i.StartDate >= startDate.Value) &&
                     (!endExclusive.HasValue || i.StartDate < endExclusive.Value)) ||
                    ((!startDate.HasValue || i.ExpiryDate >= startDate.Value) &&
                     (!endExclusive.HasValue || i.ExpiryDate < endExclusive.Value)));
            }

            var insurance = await query
                .OrderBy(i => i.ExpiryDate)
                .ToListAsync();

            return new ReportDataDto
            {
                ReportTitle = "تقرير التأمينات",
                GeneratedDate = DateTime.UtcNow,
                GeneratedBy = "System",
                Columns = new List<string> { "رقم السيارة", "الموديل", "سنة الصنع", "رقم الشاسيه", "رقم الموتور", "رقم الوثيقة", "شركة التأمين", "نوع الوثيقة", "بداية التأمين", "نهاية التأمين", "القسط", "مبلغ التغطية", "تفاصيل التغطية", "مندوب التأمين", "هاتف المندوب", "الحالة", "الإنذار", "ملاحظات" },
                Data = insurance.Select(i => new Dictionary<string, object>
                {
                    ["رقم السيارة"] = i.Vehicle?.PlateNumber ?? string.Empty,
                    ["الموديل"] = i.Vehicle?.Model ?? string.Empty,
                    ["سنة الصنع"] = i.Vehicle?.Year ?? 0,
                    ["رقم الشاسيه"] = i.Vehicle?.ChassisNumber ?? string.Empty,
                    ["رقم الموتور"] = i.Vehicle?.EngineNumber ?? string.Empty,
                    ["رقم الوثيقة"] = i.PolicyNumber,
                    ["شركة التأمين"] = i.InsuranceCompany,
                    ["نوع الوثيقة"] = i.PolicyType,
                    ["بداية التأمين"] = DateOnly(i.StartDate),
                    ["نهاية التأمين"] = DateOnly(i.ExpiryDate),
                    ["القسط"] = i.PremiumAmount,
                    ["مبلغ التغطية"] = i.CoverageAmount,
                    ["تفاصيل التغطية"] = i.CoverageDetails,
                    ["مندوب التأمين"] = i.AgentName,
                    ["هاتف المندوب"] = i.AgentPhoneNumber,
                    ["الحالة"] = ServiceHelpers.InsuranceStatus(i.ExpiryDate),
                    ["الإنذار"] = InsuranceAlert(i.ExpiryDate),
                    ["ملاحظات"] = i.Notes
                }).ToList()
            };
        }

        if (reportType == "contracts")
        {
            var query = _context.Contracts
                .Include(c => c.Vehicle)
                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(c => c.EndDate >= startDate.Value);
            }

            if (endExclusive.HasValue)
            {
                query = query.Where(c => c.StartDate < endExclusive.Value);
            }

            var contracts = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return new ReportDataDto
            {
                ReportTitle = "تقرير العقود",
                GeneratedDate = DateTime.UtcNow,
                GeneratedBy = "System",
                Columns = new List<string> { "رقم العقد", "رقم السيارة", "العميل", "بداية العقد", "نهاية العقد", "قيمة العقد", "المدفوع" },
                Data = contracts.Select(c => new Dictionary<string, object>
                {
                    ["رقم العقد"] = c.ContractNumber,
                    ["رقم السيارة"] = c.Vehicle?.PlateNumber ?? string.Empty,
                    ["العميل"] = c.ClientName,
                    ["بداية العقد"] = c.StartDate,
                    ["نهاية العقد"] = c.EndDate,
                    ["قيمة العقد"] = c.ContractValue,
                    ["المدفوع"] = c.PaidAmount
                }).ToList()
            };
        }

        if (reportType == "maintenance")
        {
            var query = _context.MaintenanceRequests
                .Include(m => m.Vehicle)
                .Include(m => m.MaintenanceType)
                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(m => m.RequestDate >= startDate.Value);
            }

            if (endExclusive.HasValue)
            {
                query = query.Where(m => m.RequestDate < endExclusive.Value);
            }

            var maintenance = await query
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return new ReportDataDto
            {
                ReportTitle = "تقرير الصيانة",
                GeneratedDate = DateTime.UtcNow,
                GeneratedBy = "System",
                Columns = new List<string> { "رقم السيارة", "نوع الصيانة", "تاريخ الطلب", "الحالة", "التكلفة الفعلية" },
                Data = maintenance.Select(m => new Dictionary<string, object>
                {
                    ["رقم السيارة"] = m.Vehicle?.PlateNumber ?? string.Empty,
                    ["نوع الصيانة"] = m.MaintenanceType?.Name ?? string.Empty,
                    ["تاريخ الطلب"] = m.RequestDate,
                    ["الحالة"] = ServiceHelpers.StatusDisplay(m.Status),
                    ["التكلفة الفعلية"] = m.ActualCost
                }).ToList()
            };
        }

        if (reportType == "oilchanges")
        {
            var query = _context.Vehicles
                .Include(x => x.OilChanges)
                .AsQueryable();

            var vehicles = await query
                .OrderBy(x => x.PlateNumber)
                .ToListAsync();

            var rows = vehicles
                .Select(vehicle =>
                {
                    var latestOilChange = vehicle.OilChanges
                        .Where(oil => oil.IsOilChanged)
                        .OrderByDescending(oil => oil.OdometerAtChange)
                        .ThenByDescending(oil => oil.ChangeDate)
                        .ThenByDescending(oil => oil.Id)
                        .FirstOrDefault();
                    var latestOdometerReading = vehicle.OilChanges
                        .Where(oil => oil.CurrentOdometer.HasValue && oil.CurrentOdometer.Value > 0)
                        .OrderByDescending(oil => oil.CurrentOdometerDate ?? oil.ChangeDate.Date)
                        .ThenByDescending(oil => oil.ChangeDate)
                        .ThenByDescending(oil => oil.Id)
                        .FirstOrDefault();
                    var summaryDate = latestOdometerReading?.CurrentOdometerDate?.Date
                        ?? latestOdometerReading?.ChangeDate.Date
                        ?? latestOilChange?.ChangeDate.Date;
                    var currentOdometer = latestOdometerReading?.CurrentOdometer > 0
                        ? latestOdometerReading.CurrentOdometer.Value
                        : vehicle.Mileage;
                    var lastOilOdometer = latestOilChange?.OdometerAtChange;
                    var kmSinceOilChange = lastOilOdometer.HasValue
                        ? Math.Max(0, currentOdometer - lastOilOdometer.Value)
                        : (decimal?)null;
                    var nextOilChangeOdometer = vehicle.OilChangeIntervalKm <= 0
                        ? (decimal?)null
                        : (lastOilOdometer ?? 0) + vehicle.OilChangeIntervalKm;
                    var alert = BuildVehicleOilSummaryAlert(vehicle.OilChangeIntervalKm, lastOilOdometer, currentOdometer, kmSinceOilChange);

                    return new
                    {
                        SummaryDate = summaryDate,
                        Data = new Dictionary<string, object>
                        {
                            ["رقم العربية"] = vehicle.PlateNumber,
                            ["آخر عداد غيار زيت"] = lastOilOdometer.HasValue ? lastOilOdometer.Value : string.Empty,
                            ["عداد اليوم"] = currentOdometer,
                            ["المقطوع من آخر غيار"] = kmSinceOilChange.HasValue ? kmSinceOilChange.Value : string.Empty,
                            ["تغيير الزيت كل كام كم"] = vehicle.OilChangeIntervalKm,
                            ["التغيير القادم"] = nextOilChangeOdometer.HasValue ? nextOilChangeOdometer.Value : string.Empty,
                            ["حالة الإنذار"] = alert
                        }
                    };
                })
                .Where(row =>
                    (!startDate.HasValue && !endExclusive.HasValue) ||
                    (row.SummaryDate.HasValue &&
                     (!startDate.HasValue || row.SummaryDate.Value >= startDate.Value) &&
                     (!endExclusive.HasValue || row.SummaryDate.Value < endExclusive.Value)))
                .Select(row => row.Data)
                .ToList();

            return new ReportDataDto
            {
                ReportTitle = "تقرير الزيوت",
                GeneratedDate = DateTime.UtcNow,
                GeneratedBy = "System",
                Columns = new List<string> { "رقم العربية", "آخر عداد غيار زيت", "عداد اليوم", "المقطوع من آخر غيار", "تغيير الزيت كل كام كم", "التغيير القادم", "حالة الإنذار" },
                Data = rows
            };
        }

        if (reportType == "treasury")
        {
            var query = _context.TreasuryTransactions.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(x => x.TransactionDate >= startDate.Value);
            }

            if (endExclusive.HasValue)
            {
                query = query.Where(x => x.TransactionDate < endExclusive.Value);
            }

            var treasury = await query
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();

            return new ReportDataDto
            {
                ReportTitle = "تقرير الخزينة",
                GeneratedDate = DateTime.UtcNow,
                GeneratedBy = "System",
                Columns = new List<string> { "التاريخ", "نوع الحركة", "المبلغ", "الوصف", "مرتبط بـ" },
                Data = treasury.Select(x => new Dictionary<string, object>
                {
                    ["التاريخ"] = x.TransactionDate,
                    ["نوع الحركة"] = ServiceHelpers.TreasuryTypeDisplay(x.TransactionType),
                    ["المبلغ"] = x.Amount,
                    ["الوصف"] = x.Description,
                    ["مرتبط بـ"] = x.RelatedEntityType
                }).ToList()
            };
        }

        var vehicleQuery = ApplyVehicleRegistrationDateFilter(
            _context.Vehicles
                .Include(v => v.InsurancePolicies)
                .AsQueryable(),
            startDate,
            endExclusive);

        var allVehicles = await vehicleQuery
            .OrderBy(v => v.PlateNumber)
            .ToListAsync();

        return new ReportDataDto
        {
            ReportTitle = "تقرير المركبات",
            GeneratedDate = DateTime.UtcNow,
            GeneratedBy = "System",
            Columns = new List<string>
            {
                "رقم السيارة",
                "الموديل",
                "سنة الصنع",
                "رقم الشاسيه",
                "رقم الموتور",
                "العداد",
                "الحالة",
                "بداية الترخيص",
                "نهاية الترخيص",
                "شركة التأمين",
                "رقم وثيقة التأمين",
                "نهاية التأمين",
                "مبلغ التغطية",
                "تغيير الزيت كل كام كم"
            },
            Data = allVehicles.Select(v =>
            {
                var insurance = v.InsurancePolicies
                    .OrderByDescending(i => i.ExpiryDate)
                    .ThenByDescending(i => i.Id)
                    .FirstOrDefault();

                return new Dictionary<string, object>
                {
                    ["رقم السيارة"] = v.PlateNumber,
                    ["الموديل"] = v.Model,
                    ["سنة الصنع"] = v.Year,
                    ["رقم الشاسيه"] = v.ChassisNumber,
                    ["رقم الموتور"] = v.EngineNumber,
                    ["العداد"] = v.Mileage,
                    ["الحالة"] = ServiceHelpers.StatusDisplay(v.Status),
                    ["بداية الترخيص"] = DateOnly(v.RegistrationStartDate),
                    ["نهاية الترخيص"] = DateOnly(v.RegistrationExpiryDate),
                    ["شركة التأمين"] = insurance?.InsuranceCompany ?? string.Empty,
                    ["رقم وثيقة التأمين"] = insurance?.PolicyNumber ?? string.Empty,
                    ["نهاية التأمين"] = DateOnly(insurance?.ExpiryDate),
                    ["مبلغ التغطية"] = insurance?.CoverageAmount ?? 0,
                    ["تغيير الزيت كل كام كم"] = v.OilChangeIntervalKm
                };
            }).ToList()
        };

        static IQueryable<Vehicle> ApplyVehicleRegistrationDateFilter(IQueryable<Vehicle> query, DateTime? startDate, DateTime? endExclusive)
        {
            if (!startDate.HasValue && !endExclusive.HasValue)
            {
                return query;
            }

            return query.Where(v =>
                (v.RegistrationStartDate.HasValue &&
                 (!startDate.HasValue || v.RegistrationStartDate.Value >= startDate.Value) &&
                 (!endExclusive.HasValue || v.RegistrationStartDate.Value < endExclusive.Value)) ||
                (v.RegistrationExpiryDate.HasValue &&
                 (!startDate.HasValue || v.RegistrationExpiryDate.Value >= startDate.Value) &&
                 (!endExclusive.HasValue || v.RegistrationExpiryDate.Value < endExclusive.Value)));
        }

        static IQueryable<Vehicle> ApplyVehicleRegistrationOverlapDateFilter(IQueryable<Vehicle> query, DateTime? startDate, DateTime? endExclusive)
        {
            if (!startDate.HasValue && !endExclusive.HasValue)
            {
                return query;
            }

            // Match licenses whose validity period overlaps the selected report range.
            return query.Where(v =>
                (v.RegistrationStartDate.HasValue || v.RegistrationExpiryDate.HasValue) &&
                (!startDate.HasValue || (v.RegistrationExpiryDate ?? v.RegistrationStartDate) >= startDate.Value) &&
                (!endExclusive.HasValue || (v.RegistrationStartDate ?? v.RegistrationExpiryDate) < endExclusive.Value));
        }

        static string DateOnly(DateTime? date) => date?.ToString("yyyy-MM-dd") ?? string.Empty;

        static string VehicleLicenseStatus(DateTime? expiryDate)
        {
            if (!expiryDate.HasValue)
            {
                return "غير مسجل";
            }

            if (expiryDate.Value.Date < DateTime.Today)
            {
                return "منتهي";
            }

            return expiryDate.Value.Date <= DateTime.Today.AddDays(VehicleRegistrationAlertDays) ? "قارب الانتهاء" : "ساري";
        }

        static string VehicleLicenseAlert(DateTime? expiryDate)
        {
            if (!expiryDate.HasValue)
            {
                return "أدخل بداية ونهاية الترخيص";
            }

            var days = (expiryDate.Value.Date - DateTime.Today).Days;
            return days switch
            {
                < 0 => "الترخيص منتهي",
                <= VehicleRegistrationAlertDays => $"يحتاج تجديد خلال {days} يوم",
                _ => "لا يوجد إنذار"
            };
        }

        static string InsuranceAlert(DateTime expiryDate)
        {
            var days = (expiryDate.Date - DateTime.Today).Days;
            return days switch
            {
                < 0 => "التأمين منتهي",
                <= 60 => $"يحتاج تجديد خلال {days} يوم",
                _ => "لا يوجد إنذار"
            };
        }

        static string BuildVehicleOilSummaryAlert(decimal oilChangeIntervalKm, decimal? lastOilOdometer, decimal currentOdometer, decimal? kmSinceOilChange)
        {
            if (oilChangeIntervalKm <= 0)
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

            var remainingKm = oilChangeIntervalKm - (kmSinceOilChange ?? 0);
            return ServiceHelpers.BuildOilAlert(remainingKm);
        }
    }

    private async Task<ReportDataDto> GenerateVehicleTripsReportAsync(ReportFilterDto filter)
    {
        var isAllTripsReport = string.Equals(filter.ReportType, "alltrips", StringComparison.OrdinalIgnoreCase);
        var query = _context.Trips
            .Include(t => t.Vehicle)
            .Include(t => t.Driver)
            .Include(t => t.RequesterEmployee)
            .Include(t => t.SupervisorEmployee)
            .AsQueryable();

        if (filter.VehicleId.HasValue && !isAllTripsReport)
        {
            query = query.Where(t => t.VehicleId == filter.VehicleId.Value);
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(t => t.StartDate >= filter.StartDate.Value.Date);
        }

        if (filter.EndDate.HasValue)
        {
            var endExclusive = filter.EndDate.Value.Date.AddDays(1);
            query = query.Where(t => t.StartDate < endExclusive);
        }

        var selectedVehiclePlateNumber = filter.VehicleId.HasValue
            ? await _context.Vehicles
                .Where(v => v.Id == filter.VehicleId.Value)
                .Select(v => v.PlateNumber)
                .FirstOrDefaultAsync()
            : null;

        var trips = await query
            .OrderByDescending(t => t.StartDate)
            .ToListAsync();

        var vehicleName = selectedVehiclePlateNumber ?? trips.FirstOrDefault()?.Vehicle?.PlateNumber;
        return new ReportDataDto
        {
            ReportTitle = isAllTripsReport
                ? "تقرير جميع التشغيلات"
                : string.IsNullOrWhiteSpace(vehicleName)
                ? "تقرير تشغيلات العربيات"
                : $"تقرير تشغيلات العربية {vehicleName}",
            GeneratedDate = DateTime.UtcNow,
            GeneratedBy = "System",
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
            Data = trips.Select((t, index) => new Dictionary<string, object>
            {
                ["سيريال"] = index + 1,
                ["التاريخ"] = t.StartDate,
                ["النهاية"] = t.EndDate ?? (object)string.Empty,
                ["رقم السيارة"] = t.Vehicle?.PlateNumber ?? string.Empty,
                ["من"] = t.StartLocation,
                ["إلى"] = t.EndLocation,
                ["السائق"] = t.Driver?.FullName ?? string.Empty,
                ["الموصي"] = string.IsNullOrWhiteSpace(t.RequesterNameText)
                    ? t.RequesterEmployee?.FullName ?? string.Empty
                    : t.RequesterNameText,
                ["المشرف"] = t.SupervisorEmployee?.FullName ?? string.Empty,
                ["الغرض"] = t.Purpose,
                ["الحالة"] = t.Status,
                ["المسافة"] = t.Distance,
                ["ملاحظات"] = t.Notes
            }).ToList()
        };
    }
}
