namespace FleetManagementSystem.Tests;

public class SmokeTests
{
    [Fact]
    public async Task BootstrapService_Seeds_DefaultData()
    {
        await using var harness = await TestHarness.CreateAsync();

        var users = await harness.Context.Users.ToListAsync();
        var vehicleTypes = await harness.Context.VehicleTypes.ToListAsync();
        var contractStatuses = await harness.Context.ContractStatuses.ToListAsync();
        var company = await harness.Context.CompanySettings.FirstOrDefaultAsync();

        Assert.Contains(users, u => u.Username == "admin");
        Assert.Contains(users, u => u.Username == "mahmoud" && u.Role == UserRole.Admin);
        Assert.Contains(users, u => u.Username == "amr" && u.Role == UserRole.TreasuryOfficer);
        Assert.Contains(users, u => u.Username == "abdelrahman" && u.Role == UserRole.TripsLicensesOfficer);
        Assert.Contains(users, u => u.Username == "osama" && u.Role == UserRole.InsuranceOfficer);
        Assert.NotEmpty(vehicleTypes);
        Assert.NotEmpty(contractStatuses);
        Assert.NotNull(company);
    }

    [Fact]
    public async Task BootstrapService_Seeds_RequestedOperationalUsersWithExactPermissions()
    {
        await using var harness = await TestHarness.CreateAsync();

        var mahmoud = await harness.AuthenticationService.AuthenticateAsync(new UserLoginDto { Username = "mahmoud", Password = "mahmoud" });
        var amr = await harness.AuthenticationService.AuthenticateAsync(new UserLoginDto { Username = "amr", Password = "amr" });
        var abdelrahman = await harness.AuthenticationService.AuthenticateAsync(new UserLoginDto { Username = "abdelrahman", Password = "abdelrahman" });
        var osama = await harness.AuthenticationService.AuthenticateAsync(new UserLoginDto { Username = "osama", Password = "osama" });

        Assert.NotNull(mahmoud);
        Assert.NotNull(amr);
        Assert.NotNull(abdelrahman);
        Assert.NotNull(osama);

        Assert.Equal("Admin", mahmoud!.Role);
        Assert.Contains("Users", mahmoud.AllowedModules);
        Assert.Contains("Reports", mahmoud.AllowedModules);

        Assert.Equal("TreasuryOfficer", amr!.Role);
        Assert.Contains("Treasury", amr.AllowedModules);
        Assert.DoesNotContain("Trips", amr.AllowedModules);
        Assert.DoesNotContain("Licenses", amr.AllowedModules);
        Assert.DoesNotContain("Insurance", amr.AllowedModules);

        Assert.Equal("TripsLicensesOfficer", abdelrahman!.Role);
        Assert.Contains("Trips", abdelrahman.AllowedModules);
        Assert.Contains("Licenses", abdelrahman.AllowedModules);
        Assert.DoesNotContain("Insurance", abdelrahman.AllowedModules);
        Assert.DoesNotContain("Treasury", abdelrahman.AllowedModules);

        Assert.Equal("InsuranceOfficer", osama!.Role);
        Assert.Contains("Insurance", osama.AllowedModules);
        Assert.DoesNotContain("Licenses", osama.AllowedModules);
        Assert.DoesNotContain("Trips", osama.AllowedModules);
        Assert.DoesNotContain("Treasury", osama.AllowedModules);
    }

    [Fact]
    public async Task DriverAttendanceService_SavesWeeklyAttendance_ReportsRestBalance_AndAlertsExpiringLicense()
    {
        await using var harness = await TestHarness.CreateAsync();

        var driver = await harness.DriverService.SaveAsync(new DriverFormDto
        {
            FullName = "سائق حضور",
            NationalId = "2990101000100",
            LicenseNumber = "DRV-ATT-001",
            LicenseStartDate = DateTime.Today.AddYears(-1),
            LicenseExpiryDate = DateTime.Today.AddDays(20),
            LicenseType = "خاصة",
            IsCompanyInsured = true,
            Governorate = "القاهرة",
            FullAddress = "مدينة نصر - شارع الاختبار",
            TrafficUnit = "مرور مدينة نصر",
            WorkLocation = "محطة التجمع",
            IsActive = true
        });

        var weekStart = DateTime.Today;
        while (weekStart.DayOfWeek != DayOfWeek.Saturday)
        {
            weekStart = weekStart.AddDays(-1);
        }

        await harness.DriverAttendanceService.SaveWeekAsync(weekStart, new[]
        {
            new DriverAttendanceFormDto { DriverId = driver.Id, WorkDate = weekStart, WorkLocation = "محطة التجمع", Status = "غائب", AbsenceReason = "ظرف مرضي" },
            new DriverAttendanceFormDto { DriverId = driver.Id, WorkDate = weekStart.AddDays(1), WorkLocation = "محطة التجمع", Status = "إجازة", AbsenceReason = "إجازة سنوية" },
            new DriverAttendanceFormDto { DriverId = driver.Id, WorkDate = weekStart.AddDays(2), WorkLocation = "محطة التجمع", Status = "حاضر" },
            new DriverAttendanceFormDto { DriverId = driver.Id, WorkDate = weekStart.AddDays(6), WorkLocation = "محطة التجمع", Status = "حاضر" }
        });

        var weeklyRows = await harness.DriverAttendanceService.GetWeekAsync(weekStart);
        var report = Assert.Single(await harness.DriverAttendanceService.GenerateReportAsync(weekStart, weekStart.AddDays(6), driver.Id));
        var alerts = await harness.ReportingService.GetAlertsAsync();

        Assert.Contains(weeklyRows, x => x.DriverId == driver.Id && x.DayName == "الجمعة" && x.Status == "حاضر");
        Assert.Equal(driver.FullName, report.FullName);
        Assert.Equal(driver.LicenseNumber, report.LicenseNumber);
        Assert.Equal("محطة التجمع", report.WorkLocation);
        Assert.Equal(2, report.PresentDays);
        Assert.Equal(1, report.AbsentDays);
        Assert.Equal(1, report.LeaveDays);
        Assert.Equal(1, report.WorkedFridays);
        Assert.Equal(1, report.EarnedRestDays);
        Assert.Equal(0, report.RemainingRestDays);
        Assert.Empty(report.AbsenceReasons);
        Assert.All(weeklyRows.Where(x => x.DriverId == driver.Id), row => Assert.Empty(row.AbsenceReason));
        Assert.Contains(alerts, a => a.RelatedEntityType == "Driver" && a.Title.Contains("رخصة سائق"));
    }

    [Fact]
    public async Task DriverAttendanceService_GeneratesMultiWeekDriverReport()
    {
        await using var harness = await TestHarness.CreateAsync();

        var driver = await harness.DriverService.SaveAsync(new DriverFormDto
        {
            FullName = "سائق تقرير فترة",
            NationalId = "2990101000101",
            LicenseNumber = "DRV-ATT-002",
            LicenseStartDate = DateTime.Today.AddYears(-1),
            LicenseExpiryDate = DateTime.Today.AddYears(1),
            LicenseType = "خاصة",
            IsCompanyInsured = true,
            Governorate = "القاهرة",
            FullAddress = "مدينة نصر - شارع التقرير",
            TrafficUnit = "مرور مدينة نصر",
            WorkLocation = "محطة التجمع",
            IsActive = true
        });

        var weekStart = DateTime.Today;
        while (weekStart.DayOfWeek != DayOfWeek.Saturday)
        {
            weekStart = weekStart.AddDays(-1);
        }

        await harness.DriverAttendanceService.SaveWeekAsync(weekStart, new[]
        {
            new DriverAttendanceFormDto { DriverId = driver.Id, WorkDate = weekStart.AddDays(1), WorkLocation = "محطة التجمع", Status = "إجازة" },
            new DriverAttendanceFormDto { DriverId = driver.Id, WorkDate = weekStart.AddDays(6), WorkLocation = "محطة التجمع", Status = "حاضر" }
        });

        await harness.DriverAttendanceService.SaveWeekAsync(weekStart.AddDays(7), new[]
        {
            new DriverAttendanceFormDto { DriverId = driver.Id, WorkDate = weekStart.AddDays(9), WorkLocation = "محطة التجمع", Status = "حاضر" },
            new DriverAttendanceFormDto { DriverId = driver.Id, WorkDate = weekStart.AddDays(10), WorkLocation = "محطة التجمع", Status = "غائب" },
            new DriverAttendanceFormDto { DriverId = driver.Id, WorkDate = weekStart.AddDays(11), WorkLocation = "محطة التجمع", Status = "إجازة" },
            new DriverAttendanceFormDto { DriverId = driver.Id, WorkDate = weekStart.AddDays(13), WorkLocation = "محطة التجمع", Status = "حاضر" }
        });

        var report = Assert.Single(await harness.DriverAttendanceService.GenerateReportAsync(weekStart, weekStart.AddDays(13), driver.Id));

        Assert.Equal(3, report.PresentDays);
        Assert.Equal(1, report.AbsentDays);
        Assert.Equal(2, report.LeaveDays);
        Assert.Equal(2, report.WorkedFridays);
        Assert.Equal(2, report.EarnedRestDays);
        Assert.Equal(0, report.RemainingRestDays);
        Assert.Empty(report.AbsenceReasons);
    }

    [Fact]
    public async Task VehicleService_SaveAsync_PersistsVehicleAndAudit()
    {
        await using var harness = await TestHarness.CreateAsync();
        var vehicleTypeId = await harness.Context.VehicleTypes.Select(x => x.Id).FirstAsync();

        var saved = await harness.VehicleService.SaveAsync(new VehicleFormDto
        {
            PlateNumber = "TEST-100",
            VehicleTypeId = vehicleTypeId,
            Model = "Toyota Corolla",
            Year = 2024,
            Manufacturer = "Toyota",
            Status = "Active",
            PurchaseDate = DateTime.Today,
            CurrentMileage = 1200
        });

        var allVehicles = await harness.VehicleService.GetAllAsync();
        var auditLogs = await harness.AuditService.GetRecentAsync();

        Assert.True(saved.Id > 0);
        Assert.Contains(allVehicles, v => v.PlateNumber == "TEST-100");
        Assert.Contains(auditLogs, a => a.EntityName == "Vehicle" || a.EntityName == "Vehicle");
    }

    [Fact]
    public async Task ReportingService_ReturnsDashboardMetricsFromSavedRecords()
    {
        await using var harness = await TestHarness.CreateAsync();
        var vehicleTypeId = await harness.Context.VehicleTypes.Select(x => x.Id).FirstAsync();
        var contractStatusId = await harness.Context.ContractStatuses.Select(x => x.Id).FirstAsync();
        var maintenanceTypeId = await harness.Context.MaintenanceTypes.Select(x => x.Id).FirstAsync();

        var vehicle = await harness.VehicleService.SaveAsync(new VehicleFormDto
        {
            PlateNumber = "REP-001",
            VehicleTypeId = vehicleTypeId,
            Model = "Hyundai Elantra",
            Year = 2023,
            Manufacturer = "Hyundai",
            Status = "Active",
            PurchaseDate = DateTime.Today.AddMonths(-2),
            CurrentMileage = 5000
        });

        await harness.ContractService.SaveAsync(new ContractFormDto
        {
            ContractNumber = "CNT-001",
            VehicleId = vehicle.Id,
            ContractStatusId = contractStatusId,
            ClientName = "Test Client",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(20),
            ContractValue = 10000,
            PaidAmount = 4000
        });

        await harness.MaintenanceService.SaveAsync(new MaintenanceRequestFormDto
        {
            VehicleId = vehicle.Id,
            MaintenanceTypeId = maintenanceTypeId,
            RequestDate = DateTime.Today,
            Status = "Open",
            EstimatedCost = 1500
        });

        var metrics = await harness.ReportingService.GetDashboardMetricsAsync();

        Assert.True(metrics.TotalVehicles >= 1);
        Assert.True(metrics.ActiveContracts >= 1);
        Assert.True(metrics.OpenMaintenanceRequests >= 1);
        Assert.NotEmpty(metrics.Alerts);
    }

    [Fact]
    public async Task TripService_SaveAsync_UpdatesVehicleStatusAndMileage()
    {
        await using var harness = await TestHarness.CreateAsync();
        var vehicleTypeId = await harness.Context.VehicleTypes.Select(x => x.Id).FirstAsync();

        var vehicle = await harness.VehicleService.SaveAsync(new VehicleFormDto
        {
            PlateNumber = "TRP-001",
            VehicleTypeId = vehicleTypeId,
            Model = "Nissan Sunny",
            Year = 2024,
            Manufacturer = "Nissan",
            Status = "Available",
            PurchaseDate = DateTime.Today,
            CurrentMileage = 1000,
            RegistrationStartDate = DateTime.Today.AddMonths(-6),
            RegistrationExpiryDate = DateTime.Today.AddMonths(6),
            AccidentInsuranceDetails = "وثيقة حوادث سارية",
            SocialInsuranceDetails = "تأمين اجتماعي ساري",
            OilChangeIntervalKm = 10000,
            MaintenanceIntervalKm = 15000
        });
        await EnsureActiveInsuranceAsync(harness, vehicle.Id, "TRP-001");

        var driver = await harness.DriverService.SaveAsync(new DriverFormDto
        {
            FullName = "Test Driver",
            LicenseNumber = "DRV-100",
            LicenseStartDate = DateTime.Today.AddYears(-1),
            LicenseExpiryDate = DateTime.Today.AddYears(1),
            WorkLocation = "الموقع الرئيسي",
            IsActive = true
        });

        var supervisor = await harness.EmployeeService.SaveAsync(new EmployeeFormDto
        {
            FullName = "المشرف",
            EmployeeId = "EMP-SUP-1",
            Status = "Active"
        });

        var openedTrip = await harness.TripService.SaveAsync(new TripFormDto
        {
            VehicleId = vehicle.Id,
            DriverId = driver.Id,
            RequesterNameText = "محمد أحمد",
            SupervisorEmployeeId = supervisor.Id,
            StartDate = DateTime.Today,
            StartLocation = "المقر",
            EndLocation = "العميل",
            Purpose = "تنفيذ مأمورية",
            Status = "Open"
        });

        var vehicleAfterOpen = await harness.VehicleService.GetByIdAsync(vehicle.Id);
        Assert.Equal("InTrip", vehicleAfterOpen?.Status);

        await harness.TripService.SaveAsync(new TripFormDto
        {
            Id = openedTrip.Id,
            VehicleId = vehicle.Id,
            DriverId = driver.Id,
            RequesterNameText = "محمد أحمد",
            SupervisorEmployeeId = supervisor.Id,
            StartDate = openedTrip.StartDate,
            EndDate = DateTime.Today.AddHours(2),
            StartLocation = openedTrip.StartLocation,
            EndLocation = openedTrip.EndLocation,
            Purpose = openedTrip.Purpose,
            StartMileage = 1000,
            EndMileage = 1080,
            Status = "Closed"
        });

        var vehicleAfterClose = await harness.VehicleService.GetByIdAsync(vehicle.Id);
        Assert.Equal("Available", vehicleAfterClose?.Status);
        Assert.Equal(1080, vehicleAfterClose?.CurrentMileage);
    }

    [Fact]
    public async Task TripService_GetAllAsync_FiltersByVehicle_AndReturnsRequesterSupervisorNames()
    {
        await using var harness = await TestHarness.CreateAsync();
        var vehicleTypeId = await harness.Context.VehicleTypes.Select(x => x.Id).FirstAsync();

        var firstVehicle = await harness.VehicleService.SaveAsync(new VehicleFormDto
        {
            PlateNumber = "OPS-001",
            VehicleTypeId = vehicleTypeId,
            Model = "Toyota Corolla",
            Year = 2024,
            Manufacturer = "Toyota",
            Status = "Available",
            CurrentMileage = 7000,
            RegistrationStartDate = DateTime.Today.AddMonths(-4),
            RegistrationExpiryDate = DateTime.Today.AddMonths(8),
            AccidentInsuranceDetails = "تأمين حوادث 1",
            SocialInsuranceDetails = "تأمين اجتماعي 1"
        });
        await EnsureActiveInsuranceAsync(harness, firstVehicle.Id, "OPS-001");

        var secondVehicle = await harness.VehicleService.SaveAsync(new VehicleFormDto
        {
            PlateNumber = "OPS-002",
            VehicleTypeId = vehicleTypeId,
            Model = "Hyundai Elantra",
            Year = 2023,
            Manufacturer = "Hyundai",
            Status = "Available",
            CurrentMileage = 9000,
            RegistrationStartDate = DateTime.Today.AddMonths(-5),
            RegistrationExpiryDate = DateTime.Today.AddMonths(7),
            AccidentInsuranceDetails = "تأمين حوادث 2",
            SocialInsuranceDetails = "تأمين اجتماعي 2"
        });
        await EnsureActiveInsuranceAsync(harness, secondVehicle.Id, "OPS-002");

        var driver = await harness.DriverService.SaveAsync(new DriverFormDto
        {
            FullName = "سائق التشغيل",
            LicenseNumber = "DRV-200",
            LicenseStartDate = DateTime.Today.AddYears(-1),
            LicenseExpiryDate = DateTime.Today.AddYears(1),
            WorkLocation = "الموقع الرئيسي",
            IsActive = true
        });

        var supervisor = await harness.EmployeeService.SaveAsync(new EmployeeFormDto
        {
            FullName = "المشرف المناوب",
            EmployeeId = "EMP-OPS-2",
            Status = "Active"
        });

        await harness.TripService.SaveAsync(new TripFormDto
        {
            VehicleId = firstVehicle.Id,
            DriverId = driver.Id,
            RequesterNameText = "الموصي",
            SupervisorEmployeeId = supervisor.Id,
            StartDate = DateTime.Today,
            StartLocation = "مدينة نصر",
            EndLocation = "المعادى",
            Purpose = "توصيل مستندات",
            EndDate = DateTime.Today.AddHours(2),
            EndMileage = firstVehicle.CurrentMileage + 30,
            Status = "Closed"
        });

        await harness.TripService.SaveAsync(new TripFormDto
        {
            VehicleId = secondVehicle.Id,
            DriverId = driver.Id,
            RequesterNameText = "الموصي",
            SupervisorEmployeeId = supervisor.Id,
            StartDate = DateTime.Today.AddDays(-1),
            StartLocation = "الجيزة",
            EndLocation = "6 أكتوبر",
            Purpose = "مأمورية خارجية",
            EndDate = DateTime.Today.AddDays(-1).AddHours(3),
            EndMileage = secondVehicle.CurrentMileage + 45,
            Status = "Closed"
        });

        var filteredTrips = await harness.TripService.GetAllAsync(firstVehicle.Id);

        var trip = Assert.Single(filteredTrips);
        Assert.Equal(firstVehicle.Id, trip.VehicleId);
        Assert.Equal("الموصي", trip.RequesterName);
        Assert.Equal("المشرف المناوب", trip.SupervisorName);
    }

    [Fact]
    public async Task TripService_SaveAsync_AllowsStaleInTripVehicle_WhenNoOpenTripExists()
    {
        await using var harness = await TestHarness.CreateAsync();
        var vehicleTypeId = await harness.Context.VehicleTypes.Select(x => x.Id).FirstAsync();

        var vehicle = await harness.VehicleService.SaveAsync(new VehicleFormDto
        {
            PlateNumber = "STALE-TRIP",
            VehicleTypeId = vehicleTypeId,
            Model = "Toyota Hiace",
            Year = 2024,
            Manufacturer = "Toyota",
            Status = "InTrip",
            CurrentMileage = 5000,
            RegistrationStartDate = DateTime.Today.AddMonths(-2),
            RegistrationExpiryDate = DateTime.Today.AddMonths(10),
            AccidentInsuranceDetails = "تأمين حوادث ساري",
            SocialInsuranceDetails = "تأمين اجتماعي ساري",
            OilChangeIntervalKm = 10000,
            MaintenanceIntervalKm = 15000
        });
        await EnsureActiveInsuranceAsync(harness, vehicle.Id, "STALE-TRIP");

        var driver = await harness.DriverService.SaveAsync(new DriverFormDto
        {
            FullName = "سائق حالة معلقة",
            LicenseNumber = "DRV-STALE",
            LicenseStartDate = DateTime.Today.AddYears(-1),
            LicenseExpiryDate = DateTime.Today.AddYears(1),
            WorkLocation = "الموقع الرئيسي",
            IsActive = true
        });

        var supervisor = await harness.EmployeeService.SaveAsync(new EmployeeFormDto
        {
            FullName = "مشرف الحالة المعلقة",
            EmployeeId = "EMP-STALE",
            Status = "Active"
        });

        var trip = await harness.TripService.SaveAsync(new TripFormDto
        {
            VehicleId = vehicle.Id,
            DriverId = driver.Id,
            RequesterNameText = "طلب تشغيل",
            SupervisorEmployeeId = supervisor.Id,
            StartDate = DateTime.Now,
            StartLocation = "المقر",
            EndLocation = "الموقع",
            Purpose = "تشغيل فعلي",
            Status = "Open"
        });

        Assert.True(trip.Id > 0);
    }

    [Fact]
    public async Task TripService_SaveAsync_ComputesDistanceFromOdometer_WhenManualDistanceConflicts()
    {
        await using var harness = await TestHarness.CreateAsync();
        var vehicleTypeId = await harness.Context.VehicleTypes.Select(x => x.Id).FirstAsync();

        var vehicle = await harness.VehicleService.SaveAsync(new VehicleFormDto
        {
            PlateNumber = "DIST-001",
            VehicleTypeId = vehicleTypeId,
            Model = "Toyota Corolla",
            Year = 2024,
            Manufacturer = "Toyota",
            Status = "Available",
            CurrentMileage = 1000,
            RegistrationStartDate = DateTime.Today.AddMonths(-1),
            RegistrationExpiryDate = DateTime.Today.AddMonths(11),
            AccidentInsuranceDetails = "وثيقة حوادث سارية",
            SocialInsuranceDetails = "تأمين اجتماعي ساري",
            OilChangeIntervalKm = 10000,
            MaintenanceIntervalKm = 15000
        });
        await EnsureActiveInsuranceAsync(harness, vehicle.Id, "DIST-001");

        var driver = await harness.DriverService.SaveAsync(new DriverFormDto
        {
            FullName = "سائق المسافة",
            LicenseNumber = "DRV-DIST",
            LicenseStartDate = DateTime.Today.AddYears(-1),
            LicenseExpiryDate = DateTime.Today.AddYears(1),
            WorkLocation = "الموقع الرئيسي",
            IsActive = true
        });

        var supervisor = await harness.EmployeeService.SaveAsync(new EmployeeFormDto
        {
            FullName = "مشرف المسافة",
            EmployeeId = "EMP-DIST",
            Status = "Active"
        });

        var trip = await harness.TripService.SaveAsync(new TripFormDto
        {
            VehicleId = vehicle.Id,
            DriverId = driver.Id,
            RequesterNameText = "طالب تشغيل",
            SupervisorEmployeeId = supervisor.Id,
            StartDate = DateTime.Today.AddHours(9),
            EndDate = DateTime.Today.AddHours(11),
            StartLocation = "المقر",
            EndLocation = "الفرع",
            Purpose = "اختبار حساب المسافة",
            StartMileage = 1000,
            EndMileage = 1080,
            Distance = 999,
            Status = "Closed"
        });

        Assert.Equal(80, trip.Distance);
    }

    [Fact]
    public async Task FuelService_SaveAsync_CreatesTreasuryTransaction_WhenPaidFromTreasury()
    {
        await using var harness = await TestHarness.CreateAsync();
        var vehicleTypeId = await harness.Context.VehicleTypes.Select(x => x.Id).FirstAsync();

        var vehicle = await harness.VehicleService.SaveAsync(new VehicleFormDto
        {
            PlateNumber = "FUEL-001",
            VehicleTypeId = vehicleTypeId,
            Model = "Hyundai Accent",
            Year = 2023,
            Manufacturer = "Hyundai",
            Status = "Available",
            PurchaseDate = DateTime.Today,
            CurrentMileage = 4000,
            OilChangeIntervalKm = 10000,
            MaintenanceIntervalKm = 15000
        });

        var fuel = await harness.FuelService.SaveAsync(new FuelTransactionFormDto
        {
            VehicleId = vehicle.Id,
            TransactionDate = DateTime.Today,
            FuelType = "Diesel",
            Quantity = 40,
            UnitPrice = 15,
            PaidFromTreasury = true,
            Odometer = 4200
        });

        var treasuryRows = await harness.TreasuryService.GetAllAsync();
        Assert.NotNull(fuel.TreasuryTransactionId);
        Assert.Contains(treasuryRows, t => t.Id == fuel.TreasuryTransactionId && t.TransactionType == "صرف" && t.Amount == fuel.TotalCost);
    }

    [Fact]
    public async Task OilChangeService_SaveAsync_ShowsDueMetric_WhenVehicleApproachesThreshold()
    {
        await using var harness = await TestHarness.CreateAsync();
        var vehicleTypeId = await harness.Context.VehicleTypes.Select(x => x.Id).FirstAsync();

        var vehicle = await harness.VehicleService.SaveAsync(new VehicleFormDto
        {
            PlateNumber = "OIL-001",
            VehicleTypeId = vehicleTypeId,
            Model = "Toyota Hilux",
            Year = 2022,
            Manufacturer = "Toyota",
            Status = "Available",
            PurchaseDate = DateTime.Today.AddMonths(-2),
            CurrentMileage = 5000,
            OilChangeIntervalKm = 10000,
            MaintenanceIntervalKm = 15000
        });

        await harness.OilChangeService.SaveAsync(new OilChangeFormDto
        {
            VehicleId = vehicle.Id,
            ChangeDate = DateTime.Today.AddMonths(-1),
            OdometerAtChange = 5000,
            OilType = "5W30",
            Quantity = 5,
            Cost = 800,
            NextOilChangeOdometer = 10000
        });

        var vehicleAfterOil = await harness.VehicleService.GetByIdAsync(vehicle.Id);
        await harness.VehicleService.SaveAsync(new VehicleFormDto
        {
            Id = vehicle.Id,
            PlateNumber = vehicle.PlateNumber,
            VehicleTypeId = vehicle.VehicleTypeId,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Manufacturer = vehicle.Manufacturer,
            Status = vehicleAfterOil?.Status ?? "Available",
            PurchaseDate = vehicle.PurchaseDate,
            CurrentMileage = 14600,
            OilChangeIntervalKm = vehicle.OilChangeIntervalKm,
            MaintenanceIntervalKm = vehicle.MaintenanceIntervalKm
        });

        var metrics = await harness.ReportingService.GetDashboardMetricsAsync();
        Assert.True(metrics.OilChangesDue >= 1);
    }

    [Fact]
    public async Task OilChangeService_SaveAsync_DailyFollowUp_UsesLastActualOilChangeAndUpdatesMileage()
    {
        await using var harness = await TestHarness.CreateAsync();
        var vehicleTypeId = await harness.Context.VehicleTypes.Select(x => x.Id).FirstAsync();

        var vehicle = await harness.VehicleService.SaveAsync(new VehicleFormDto
        {
            PlateNumber = "OIL-DAILY-001",
            VehicleTypeId = vehicleTypeId,
            Model = "Toyota Hiace",
            Year = 2023,
            Manufacturer = "Toyota",
            Status = "Available",
            PurchaseDate = DateTime.Today.AddMonths(-3),
            CurrentMileage = 10000,
            OilChangeIntervalKm = 5000,
            MaintenanceIntervalKm = 15000
        });

        await harness.OilChangeService.SaveAsync(new OilChangeFormDto
        {
            VehicleId = vehicle.Id,
            ChangeDate = DateTime.Today.AddDays(-1),
            OdometerAtChange = 10000,
            CurrentOdometer = 10000,
            CurrentOdometerDate = DateTime.Today.AddDays(-1),
            IsOilChanged = true,
            ServiceItems = "زيت وفلتر",
            OilType = "5W30",
            Quantity = 5,
            Cost = 850
        });

        var dailyFollowUp = await harness.OilChangeService.SaveAsync(new OilChangeFormDto
        {
            VehicleId = vehicle.Id,
            ChangeDate = DateTime.Today,
            CurrentOdometer = 10300,
            CurrentOdometerDate = DateTime.Today,
            IsOilChanged = false,
            Notes = "متابعة يومية"
        });

        var vehicleAfterFollowUp = await harness.VehicleService.GetByIdAsync(vehicle.Id);

        Assert.False(dailyFollowUp.IsOilChanged);
        Assert.Equal(10000, dailyFollowUp.OdometerAtChange);
        Assert.Equal(15000, dailyFollowUp.NextOilChangeOdometer);
        Assert.Equal(10300, dailyFollowUp.CurrentOdometer);
        Assert.Equal(4700, dailyFollowUp.RemainingKm);
        Assert.Equal("متابعة يومية", dailyFollowUp.Status);
        Assert.Equal(string.Empty, dailyFollowUp.ServiceItems);
        Assert.Equal(string.Empty, dailyFollowUp.OilType);
        Assert.Equal(10300, vehicleAfterFollowUp?.CurrentMileage);
    }

    [Fact]
    public async Task OilChangeService_SaveAsync_OilAndFilterChange_PersistsServiceItems()
    {
        await using var harness = await TestHarness.CreateAsync();
        var vehicleTypeId = await harness.Context.VehicleTypes.Select(x => x.Id).FirstAsync();

        var vehicle = await harness.VehicleService.SaveAsync(new VehicleFormDto
        {
            PlateNumber = "OIL-FILTER-001",
            VehicleTypeId = vehicleTypeId,
            Model = "Hyundai HD",
            Year = 2023,
            Manufacturer = "Hyundai",
            Status = "Available",
            PurchaseDate = DateTime.Today.AddMonths(-3),
            CurrentMileage = 12000,
            OilChangeIntervalKm = 5000,
            MaintenanceIntervalKm = 15000
        });

        var oilChange = await harness.OilChangeService.SaveAsync(new OilChangeFormDto
        {
            VehicleId = vehicle.Id,
            ChangeDate = DateTime.Today,
            OdometerAtChange = 12000,
            CurrentOdometer = 12000,
            CurrentOdometerDate = DateTime.Today,
            IsOilChanged = true,
            ServiceItems = "زيت وفلتر",
            OilType = "15W40",
            Quantity = 8,
            Cost = 1300
        });

        Assert.True(oilChange.IsOilChanged);
        Assert.Equal("زيت وفلتر", oilChange.ServiceItems);
        Assert.Equal("تغيير زيت", oilChange.RecordType);
        Assert.Equal(17000, oilChange.NextOilChangeOdometer);
    }

    [Fact]
    public async Task OilChangeService_SaveAsync_DailyFollowUpBeforeFirstOilChange_IsRejected()
    {
        await using var harness = await TestHarness.CreateAsync();
        var vehicleTypeId = await harness.Context.VehicleTypes.Select(x => x.Id).FirstAsync();

        var vehicle = await harness.VehicleService.SaveAsync(new VehicleFormDto
        {
            PlateNumber = "OIL-DAILY-002",
            VehicleTypeId = vehicleTypeId,
            Model = "Hyundai H1",
            Year = 2022,
            Manufacturer = "Hyundai",
            Status = "Available",
            PurchaseDate = DateTime.Today.AddMonths(-3),
            CurrentMileage = 7000,
            OilChangeIntervalKm = 5000,
            MaintenanceIntervalKm = 15000
        });

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => harness.OilChangeService.SaveAsync(new OilChangeFormDto
        {
            VehicleId = vehicle.Id,
            ChangeDate = DateTime.Today,
            CurrentOdometer = 7000,
            CurrentOdometerDate = DateTime.Today,
            IsOilChanged = false
        }));

        Assert.Contains("أول تغيير زيت فعلي", error.Message);
    }

    [Fact]
    public async Task ReportingService_InsuranceReport_AppliesExpiryDateFilter()
    {
        await using var harness = await TestHarness.CreateAsync();
        var vehicleTypeId = await harness.Context.VehicleTypes.Select(x => x.Id).FirstAsync();

        var vehicle = await harness.VehicleService.SaveAsync(new VehicleFormDto
        {
            PlateNumber = "INS-REP-001",
            VehicleTypeId = vehicleTypeId,
            Model = "Toyota Corolla",
            Year = 2024,
            Manufacturer = "Toyota",
            Status = "Available",
            CurrentMileage = 1000
        });

        await harness.InsuranceService.SaveAsync(new InsuranceFormDto
        {
            VehicleId = vehicle.Id,
            PolicyNumber = "INS-IN-RANGE",
            InsuranceCompany = "Company A",
            PolicyType = "Comprehensive",
            StartDate = DateTime.Today.AddMonths(-1),
            ExpiryDate = DateTime.Today.AddDays(10),
            PremiumAmount = 1000
        });

        await harness.InsuranceService.SaveAsync(new InsuranceFormDto
        {
            VehicleId = vehicle.Id,
            PolicyNumber = "INS-OUT-RANGE",
            InsuranceCompany = "Company B",
            PolicyType = "Comprehensive",
            StartDate = DateTime.Today.AddMonths(-1),
            ExpiryDate = DateTime.Today.AddMonths(4),
            PremiumAmount = 1000
        });

        var report = await harness.ReportingService.GenerateReportAsync(new ReportFilterDto
        {
            ReportType = "insurance",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(30)
        });

        Assert.Equal("تقرير التأمينات", report.ReportTitle);
        Assert.Contains(report.Data, row => row["رقم الوثيقة"].ToString() == "INS-IN-RANGE");
        Assert.DoesNotContain(report.Data, row => row["رقم الوثيقة"].ToString() == "INS-OUT-RANGE");
    }

    [Fact]
    public async Task ReportingService_VehicleLicenseReport_IncludesLicensesOverlappingSelectedDateRange()
    {
        await using var harness = await TestHarness.CreateAsync();
        var vehicleTypeId = await harness.Context.VehicleTypes.Select(x => x.Id).FirstAsync();

        await harness.VehicleService.SaveAsync(new VehicleFormDto
        {
            PlateNumber = "LIC-SPAN-RANGE",
            VehicleTypeId = vehicleTypeId,
            Model = "Toyota Hiace",
            Year = 2024,
            Manufacturer = "Toyota",
            Status = "Available",
            CurrentMileage = 1000,
            RegistrationStartDate = DateTime.Today.AddDays(-60),
            RegistrationExpiryDate = DateTime.Today.AddDays(60)
        });

        await harness.VehicleService.SaveAsync(new VehicleFormDto
        {
            PlateNumber = "LIC-FUTURE-RANGE",
            VehicleTypeId = vehicleTypeId,
            Model = "Toyota Hiace",
            Year = 2024,
            Manufacturer = "Toyota",
            Status = "Available",
            CurrentMileage = 1000,
            RegistrationStartDate = DateTime.Today.AddDays(40),
            RegistrationExpiryDate = DateTime.Today.AddDays(80)
        });

        var report = await harness.ReportingService.GenerateReportAsync(new ReportFilterDto
        {
            ReportType = "vehiclelicenses",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(7)
        });

        Assert.Contains(report.Data, row => row["رقم السيارة"].ToString() == "LIC-SPAN-RANGE");
        Assert.DoesNotContain(report.Data, row => row["رقم السيارة"].ToString() == "LIC-FUTURE-RANGE");
    }

    private static Task EnsureActiveInsuranceAsync(TestHarness harness, int vehicleId, string suffix) =>
        harness.InsuranceService.SaveAsync(new InsuranceFormDto
        {
            VehicleId = vehicleId,
            PolicyNumber = $"INS-{suffix}",
            InsuranceCompany = "شركة تأمين الاختبار",
            PolicyType = "تأمين حوادث",
            StartDate = DateTime.Today.AddMonths(-1),
            ExpiryDate = DateTime.Today.AddYears(1),
            PremiumAmount = 1000,
            CoverageAmount = 100000,
            CoverageDetails = "تغطية اختبار",
            AgentName = "مندوب اختبار",
            AgentPhoneNumber = "01000000000"
        });

    [Fact]
    public async Task AuthenticationService_LocksUserAfterRepeatedBadPasswords()
    {
        await using var harness = await TestHarness.CreateAsync();
        var username = $"lockout-{Guid.NewGuid():N}";

        await harness.AuthenticationService.SaveUserAsync(new UserFormDto
        {
            Username = username,
            Email = $"{username}@fleet.local",
            FullName = "Lockout User",
            Role = "Staff",
            IsActive = true,
            Password = "Strong@123",
            ConfirmPassword = "Strong@123"
        });

        var badLogin = new UserLoginDto
        {
            Username = username,
            Password = "Wrong@123"
        };

        for (var attempt = 0; attempt < 4; attempt++)
        {
            Assert.Null(await harness.AuthenticationService.AuthenticateAsync(badLogin));
        }

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            harness.AuthenticationService.AuthenticateAsync(badLogin));

        Assert.Contains("إيقاف", exception.Message);
    }

    private sealed class TestHarness : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        public FleetDbContext Context { get; }
        public IAuditService AuditService { get; }
        public INotificationService NotificationService { get; }
        public IVehicleService VehicleService { get; }
        public IContractService ContractService { get; }
        public IMaintenanceService MaintenanceService { get; }
        public IDriverService DriverService { get; }
        public IDriverAttendanceService DriverAttendanceService { get; }
        public IEmployeeService EmployeeService { get; }
        public ITripService TripService { get; }
        public IInsuranceService InsuranceService { get; }
        public IFuelService FuelService { get; }
        public IOilChangeService OilChangeService { get; }
        public ITreasuryService TreasuryService { get; }
        public IReportingService ReportingService { get; }
        public IAuthenticationService AuthenticationService { get; }

        private TestHarness(
            SqliteConnection connection,
            FleetDbContext context,
            IAuditService auditService,
            INotificationService notificationService,
            IVehicleService vehicleService,
            IContractService contractService,
            IMaintenanceService maintenanceService,
            IDriverService driverService,
            IDriverAttendanceService driverAttendanceService,
            IEmployeeService employeeService,
            ITripService tripService,
            IInsuranceService insuranceService,
            IFuelService fuelService,
            IOilChangeService oilChangeService,
            ITreasuryService treasuryService,
            IReportingService reportingService,
            IAuthenticationService authenticationService)
        {
            _connection = connection;
            Context = context;
            AuditService = auditService;
            NotificationService = notificationService;
            VehicleService = vehicleService;
            ContractService = contractService;
            MaintenanceService = maintenanceService;
            DriverService = driverService;
            DriverAttendanceService = driverAttendanceService;
            EmployeeService = employeeService;
            TripService = tripService;
            InsuranceService = insuranceService;
            FuelService = fuelService;
            OilChangeService = oilChangeService;
            TreasuryService = treasuryService;
            ReportingService = reportingService;
            AuthenticationService = authenticationService;
        }

        public static async Task<TestHarness> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<FleetDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new FleetDbContext(options);
            var bootstrap = new DataBootstrapService(context);
            await bootstrap.InitializeAsync();

            var auditService = new AuditService(context);
            var notificationService = new NotificationService(context);
            var vehicleService = new VehicleService(context, auditService, notificationService);
            var contractService = new ContractService(context, auditService, notificationService);
            var maintenanceService = new MaintenanceService(context, auditService, notificationService);
            var driverService = new DriverService(context, auditService);
            var driverAttendanceService = new DriverAttendanceService(context, auditService);
            var employeeService = new EmployeeService(context, auditService);
            var tripService = new TripService(context, auditService);
            var insuranceService = new InsuranceService(context, auditService);
            var fuelService = new FuelService(context, auditService);
            var oilChangeService = new OilChangeService(context, auditService);
            var treasuryService = new TreasuryService(context, auditService);
            var reportingService = new ReportingService(context);
            var authenticationService = new AuthenticationService(context, auditService);

            return new TestHarness(
                connection,
                context,
                auditService,
                notificationService,
                vehicleService,
                contractService,
                maintenanceService,
                driverService,
                driverAttendanceService,
                employeeService,
                tripService,
                insuranceService,
                fuelService,
                oilChangeService,
                treasuryService,
                reportingService,
                authenticationService);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
