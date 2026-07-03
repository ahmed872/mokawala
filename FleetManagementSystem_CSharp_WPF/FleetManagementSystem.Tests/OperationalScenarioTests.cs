namespace FleetManagementSystem.Tests;

public class OperationalScenarioTests
{
    [Fact]
    public async Task FullSystem_OneHundredOperationalScenarios_Pass()
    {
        var scenarios = BuildScenarios();
        Assert.Equal(100, scenarios.Count);

        for (var index = 0; index < scenarios.Count; index++)
        {
            var scenario = scenarios[index];
            await using var harness = await ScenarioHarness.CreateAsync();

            try
            {
                await scenario.Execute(harness, index + 1);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Scenario {index + 1:000} failed: {scenario.Name}", ex);
            }
        }
    }

    private static IReadOnlyList<Scenario> BuildScenarios() =>
        new List<Scenario>
        {
            new("Bootstrap seeds admin and lookup data", async (h, n) =>
            {
                var users = await h.Context.Users.ToListAsync();
                Assert.Contains(users, u => u.Username == "admin");
                Assert.Contains(users, u => u.Username == "mahmoud" && u.Role == UserRole.Admin);
                Assert.Contains(users, u => u.Username == "amr" && u.Role == UserRole.TreasuryOfficer);
                Assert.Contains(users, u => u.Username == "abdelrahman" && u.Role == UserRole.TripsLicensesOfficer);
                Assert.Contains(users, u => u.Username == "osama" && u.Role == UserRole.InsuranceOfficer);
                Assert.NotEmpty(await h.MasterDataService.GetVehicleTypesAsync());
                Assert.NotEmpty(await h.MasterDataService.GetContractStatusesAsync());
            }),
            new("Company settings update persists", async (h, n) =>
            {
                var saved = await h.SettingsService.SaveSettingsAsync(new CompanySettingsDto
                {
                    CompanyName = $"شركة اختبار {n}",
                    CompanyNameEn = "Scenario Fleet",
                    Address = "القاهرة",
                    PhoneNumber = "0200000000",
                    Email = "scenario@fleet.local",
                    CurrencySymbol = "EGP",
                    DefaultLanguage = "ar",
                    DefaultTheme = "Light"
                });

                Assert.Equal($"شركة اختبار {n}", saved.CompanyName);
                Assert.Equal(saved.CompanyName, (await h.SettingsService.GetSettingsAsync()).CompanyName);
            }),
            new("Vehicle type create update delete", async (h, n) =>
            {
                var created = await h.MasterDataService.SaveVehicleTypeAsync(new VehicleTypeDto { Name = $"نوع {n}", NameEn = $"Type {n}", IsActive = true });
                created.Description = "تم التحديث";
                var updated = await h.MasterDataService.SaveVehicleTypeAsync(created);
                Assert.Equal("تم التحديث", updated.Description);
                await h.MasterDataService.DeleteVehicleTypeAsync(updated.Id);
                Assert.DoesNotContain(await h.MasterDataService.GetVehicleTypesAsync(), x => x.Id == updated.Id);
            }),
            new("Contract status create update delete", async (h, n) =>
            {
                var created = await h.MasterDataService.SaveContractStatusAsync(new ContractStatusDto { Name = $"حالة {n}", NameEn = $"Status {n}", Color = "#1F2937", IsActive = true });
                created.Color = "#0F4C75";
                var updated = await h.MasterDataService.SaveContractStatusAsync(created);
                Assert.Equal("#0F4C75", updated.Color);
                await h.MasterDataService.DeleteContractStatusAsync(updated.Id);
                Assert.DoesNotContain(await h.MasterDataService.GetContractStatusesAsync(), x => x.Id == updated.Id);
            }),
            new("Maintenance type create update delete", async (h, n) =>
            {
                var created = await h.MasterDataService.SaveMaintenanceTypeAsync(new MaintenanceTypeDto { Name = $"صيانة {n}", NameEn = $"Maint {n}", EstimatedCost = 500, EstimatedDurationDays = 1, IsActive = true });
                created.EstimatedCost = 750;
                var updated = await h.MasterDataService.SaveMaintenanceTypeAsync(created);
                Assert.Equal(750, updated.EstimatedCost);
                await h.MasterDataService.DeleteMaintenanceTypeAsync(updated.Id);
                Assert.DoesNotContain(await h.MasterDataService.GetMaintenanceTypesAsync(), x => x.Id == updated.Id);
            }),
            new("Service provider create update delete", async (h, n) =>
            {
                var created = await h.MasterDataService.SaveServiceProviderAsync(new ServiceProviderDto { Name = $"مزود {n}", NameEn = $"Provider {n}", ContactPerson = "مسؤول", PhoneNumber = "01000000000", AverageRating = 4, IsActive = true });
                created.AverageRating = 4.8m;
                var updated = await h.MasterDataService.SaveServiceProviderAsync(created);
                Assert.Equal(4.8m, updated.AverageRating);
                await h.MasterDataService.DeleteServiceProviderAsync(updated.Id);
                Assert.DoesNotContain(await h.MasterDataService.GetServiceProvidersAsync(), x => x.Id == updated.Id);
            }),
            new("Vehicle create persists", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n);
                Assert.True(vehicle.Id > 0);
                Assert.Equal($"SC{n:000}-V", vehicle.PlateNumber);
            }),
            new("Vehicle search returns matching plate", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n, "SEARCH");
                var results = await h.VehicleService.GetAllAsync(vehicle.PlateNumber);
                Assert.Contains(results, x => x.Id == vehicle.Id);
            }),
            new("Vehicle lookup contains saved vehicle", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n, "LOOK");
                var lookup = await h.VehicleService.GetLookupAsync();
                Assert.Contains(lookup, x => x.Id == vehicle.Id && x.PlateNumber == vehicle.PlateNumber);
            }),
            new("Vehicle update mileage persists", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n, currentMileage: 1200);
                var form = VehicleForm(vehicle);
                form.CurrentMileage = 2200;
                var updated = await h.VehicleService.SaveAsync(form);
                Assert.Equal(2200, updated.CurrentMileage);
            }),
            new("Vehicle duplicate plate rejected", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n);
                var duplicate = NewVehicleForm(h, n, "DUP");
                duplicate.PlateNumber = vehicle.PlateNumber;
                await AssertInvalidOperationAsync(() => h.VehicleService.SaveAsync(duplicate), "رقم اللوحة");
            }),
            new("Vehicle duplicate chassis rejected", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n);
                var duplicate = NewVehicleForm(h, n, "DUPCH");
                duplicate.ChassisNumber = vehicle.ChassisNumber;
                await AssertInvalidOperationAsync(() => h.VehicleService.SaveAsync(duplicate), "الشاسيه");
            }),
            new("Vehicle duplicate engine rejected", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n);
                var duplicate = NewVehicleForm(h, n, "DUPEN");
                duplicate.EngineNumber = vehicle.EngineNumber;
                await AssertInvalidOperationAsync(() => h.VehicleService.SaveAsync(duplicate), "الموتور");
            }),
            new("Vehicle negative mileage rejected", async (h, n) =>
            {
                var form = NewVehicleForm(h, n);
                form.CurrentMileage = -1;
                await AssertInvalidOperationAsync(() => h.VehicleService.SaveAsync(form), "العداد");
            }),
            new("Vehicle inverted registration dates rejected", async (h, n) =>
            {
                var form = NewVehicleForm(h, n);
                form.RegistrationStartDate = DateTime.Today;
                form.RegistrationExpiryDate = DateTime.Today.AddDays(-1);
                await AssertInvalidOperationAsync(() => h.VehicleService.SaveAsync(form), "انتهاء الترخيص");
            }),
            new("Unused vehicle can be deleted", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n, withInsuranceDetails: false);
                await h.VehicleService.DeleteAsync(vehicle.Id);
                Assert.Null(await h.VehicleService.GetByIdAsync(vehicle.Id));
            }),
            new("Vehicle delete is blocked by trip dependency", async (h, n) =>
            {
                var bundle = await CreateTripAsync(h, n, closed: true);
                await AssertInvalidOperationAsync(() => h.VehicleService.DeleteAsync(bundle.Vehicle.Id), "ارتباطها");
            }),
            new("Vehicle registration expiry appears in alerts", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n, registrationExpiry: DateTime.Today.AddDays(12));
                var alerts = await h.ReportingService.GetAlertsAsync();
                Assert.Contains(alerts, x => x.RelatedEntityType == "Vehicle" && x.RelatedEntityId == vehicle.Id);
            }),
            new("Vehicle license report includes saved vehicle", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n, "LICREP");
                var report = await h.ReportingService.GenerateReportAsync(new ReportFilterDto { ReportType = "vehiclelicenses" });
                Assert.Contains(report.Data, row => row["رقم السيارة"].ToString() == vehicle.PlateNumber);
            }),
            new("Vehicle report future date filter excludes current records", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n, "FUTURE");
                var report = await h.ReportingService.GenerateReportAsync(new ReportFilterDto { ReportType = "vehicles", StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(2) });
                Assert.DoesNotContain(report.Data, row => row["رقم السيارة"].ToString() == vehicle.PlateNumber);
            }),
            new("Driver create persists", async (h, n) =>
            {
                var driver = await CreateDriverAsync(h, n);
                Assert.True(driver.Id > 0);
                Assert.Equal($"سائق {n}-D", driver.FullName);
            }),
            new("Driver search returns matching driver", async (h, n) =>
            {
                var driver = await CreateDriverAsync(h, n, "SEARCH");
                Assert.Contains(await h.DriverService.GetAllAsync(driver.FullName), x => x.Id == driver.Id);
            }),
            new("Driver lookup contains saved driver", async (h, n) =>
            {
                var driver = await CreateDriverAsync(h, n, "LOOK");
                Assert.Contains(await h.DriverService.GetLookupAsync(), x => x.Id == driver.Id);
            }),
            new("Driver update inactive persists", async (h, n) =>
            {
                var driver = await CreateDriverAsync(h, n);
                var updated = await h.DriverService.SaveAsync(new DriverFormDto
                {
                    Id = driver.Id,
                    FullName = driver.FullName,
                    LicenseNumber = driver.LicenseNumber,
                    LicenseStartDate = driver.LicenseStartDate,
                    LicenseExpiryDate = driver.LicenseExpiryDate,
                    WorkLocation = driver.WorkLocation,
                    IsActive = false
                });

                Assert.False(updated.IsActive);
            }),
            new("Driver duplicate license rejected", async (h, n) =>
            {
                var driver = await CreateDriverAsync(h, n);
                await AssertInvalidOperationAsync(() => h.DriverService.SaveAsync(new DriverFormDto
                {
                    FullName = "سائق مكرر",
                    LicenseNumber = driver.LicenseNumber,
                    LicenseStartDate = DateTime.Today.AddYears(-1),
                    LicenseExpiryDate = DateTime.Today.AddYears(1),
                    WorkLocation = "الموقع الرئيسي",
                    IsActive = true
                }), "الرخصة");
            }),
            new("Unused driver can be deleted", async (h, n) =>
            {
                var driver = await CreateDriverAsync(h, n);
                await h.DriverService.DeleteAsync(driver.Id);
                Assert.Null(await h.DriverService.GetByIdAsync(driver.Id));
            }),
            new("Driver delete is blocked by trip dependency", async (h, n) =>
            {
                var bundle = await CreateTripAsync(h, n, closed: true);
                await AssertInvalidOperationAsync(() => h.DriverService.DeleteAsync(bundle.Driver.Id), "ارتباطه");
            }),
            new("Expired driver license blocks trip", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n);
                var driver = await CreateDriverAsync(h, n, licenseExpiry: DateTime.Today.AddDays(-1));
                var supervisor = await CreateEmployeeAsync(h, n, "SUP");
                await AssertInvalidOperationAsync(() => h.TripService.SaveAsync(NewTripForm(vehicle, driver, supervisor, n)), "رخصته منتهية");
            }),
            new("Inactive driver blocks trip", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n);
                var driver = await CreateDriverAsync(h, n, active: false);
                var supervisor = await CreateEmployeeAsync(h, n, "SUP");
                await AssertInvalidOperationAsync(() => h.TripService.SaveAsync(NewTripForm(vehicle, driver, supervisor, n)), "غير مفعل");
            }),
            new("Employee create persists", async (h, n) =>
            {
                var employee = await CreateEmployeeAsync(h, n);
                Assert.True(employee.Id > 0);
            }),
            new("Employee search returns matching employee", async (h, n) =>
            {
                var employee = await CreateEmployeeAsync(h, n, "SEARCH");
                Assert.Contains(await h.EmployeeService.GetAllAsync(employee.FullName), x => x.Id == employee.Id);
            }),
            new("Employee lookup contains saved employee", async (h, n) =>
            {
                var employee = await CreateEmployeeAsync(h, n, "LOOK");
                Assert.Contains(await h.EmployeeService.GetLookupAsync(), x => x.Id == employee.Id);
            }),
            new("Employee update termination date persists", async (h, n) =>
            {
                var employee = await CreateEmployeeAsync(h, n);
                var updated = await h.EmployeeService.SaveAsync(new EmployeeFormDto
                {
                    Id = employee.Id,
                    FullName = employee.FullName,
                    EmployeeId = employee.EmployeeId,
                    Department = employee.Department,
                    Position = employee.Position,
                    Status = "Inactive",
                    TerminationDate = DateTime.Today
                });

                Assert.Equal(DateTime.Today, updated.TerminationDate);
            }),
            new("Unused employee can be deleted", async (h, n) =>
            {
                var employee = await CreateEmployeeAsync(h, n);
                await h.EmployeeService.DeleteAsync(employee.Id);
                Assert.Null(await h.EmployeeService.GetByIdAsync(employee.Id));
            }),
            new("Employee delete is blocked by trip dependency", async (h, n) =>
            {
                var bundle = await CreateTripAsync(h, n, closed: true);
                await AssertInvalidOperationAsync(() => h.EmployeeService.DeleteAsync(bundle.Supervisor.Id), "ارتباطه");
            }),
            new("Default admin can authenticate", async (h, n) =>
            {
                var user = await h.AuthenticationService.AuthenticateAsync(new UserLoginDto { Username = "admin", Password = "Admin@123" });
                Assert.NotNull(user);
                Assert.Equal("Admin", user!.Role);
            }),
            new("Wrong password returns null before lockout", async (h, n) =>
            {
                var user = await h.AuthenticationService.AuthenticateAsync(new UserLoginDto { Username = $"unknown-{n}", Password = "Wrong@123" });
                Assert.Null(user);
            }),
            new("Strong user password saves and authenticates", async (h, n) =>
            {
                var username = $"user{n}";
                await h.AuthenticationService.SaveUserAsync(NewUser(username, "Strong@123"));
                Assert.NotNull(await h.AuthenticationService.AuthenticateAsync(new UserLoginDto { Username = username, Password = "Strong@123" }));
            }),
            new("Weak password is rejected", async (h, n) =>
            {
                await AssertInvalidOperationAsync(() => h.AuthenticationService.SaveUserAsync(NewUser($"weak{n}", "123")), "كلمة المرور");
            }),
            new("Password confirmation mismatch is rejected", async (h, n) =>
            {
                var user = NewUser($"mismatch{n}", "Strong@123");
                user.ConfirmPassword = "Strong@124";
                await AssertInvalidOperationAsync(() => h.AuthenticationService.SaveUserAsync(user), "تأكيد");
            }),
            new("Duplicate username is rejected", async (h, n) =>
            {
                var user = NewUser($"duplicate{n}", "Strong@123");
                await h.AuthenticationService.SaveUserAsync(user);
                await AssertInvalidOperationAsync(() => h.AuthenticationService.SaveUserAsync(NewUser(user.Username, "Other@123")), "مستخدم");
            }),
            new("Inactive user cannot authenticate", async (h, n) =>
            {
                var username = $"inactive{n}";
                var saved = await h.AuthenticationService.SaveUserAsync(NewUser(username, "Strong@123"));
                await h.AuthenticationService.ToggleUserStatusAsync(saved.Id, false);
                Assert.Null(await h.AuthenticationService.AuthenticateAsync(new UserLoginDto { Username = username, Password = "Strong@123" }));
            }),
            new("Repeated bad passwords lock user", async (h, n) =>
            {
                var username = $"lock{Guid.NewGuid():N}";
                await h.AuthenticationService.SaveUserAsync(NewUser(username, "Strong@123"));
                for (var i = 0; i < 4; i++)
                {
                    Assert.Null(await h.AuthenticationService.AuthenticateAsync(new UserLoginDto { Username = username, Password = "Wrong@123" }));
                }

                await AssertInvalidOperationAsync(() => h.AuthenticationService.AuthenticateAsync(new UserLoginDto { Username = username, Password = "Wrong@123" }), "إيقاف");
            }),
            new("User list contains saved user", async (h, n) =>
            {
                var username = $"listed{n}";
                await h.AuthenticationService.SaveUserAsync(NewUser(username, "Strong@123"));
                Assert.Contains(await h.AuthenticationService.GetUsersAsync(), x => x.Username == username);
            }),
            new("Audit log records saved vehicle", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n);
                Assert.Contains(await h.AuditService.GetRecentAsync(), x => x.EntityName == "Vehicle" && x.EntityId == vehicle.Id);
            }),
            new("Open trip moves vehicle to InTrip", async (h, n) =>
            {
                var bundle = await CreateTripAsync(h, n, closed: false);
                Assert.Equal("InTrip", (await h.VehicleService.GetByIdAsync(bundle.Vehicle.Id))?.Status);
            }),
            new("Closing trip restores vehicle and mileage", async (h, n) =>
            {
                var bundle = await CreateTripAsync(h, n, closed: false);
                var closeForm = TripForm(bundle.Trip);
                closeForm.EndDate = bundle.Trip.StartDate.AddHours(2);
                closeForm.EndMileage = bundle.Trip.StartMileage + 75;
                closeForm.Status = "Closed";
                await h.TripService.SaveAsync(closeForm);
                var vehicle = await h.VehicleService.GetByIdAsync(bundle.Vehicle.Id);
                Assert.Equal("Available", vehicle?.Status);
                Assert.Equal(bundle.Trip.StartMileage + 75, vehicle?.CurrentMileage);
            }),
            new("Second open trip for same vehicle is blocked", async (h, n) =>
            {
                var bundle = await CreateTripAsync(h, n, closed: false);
                var secondDriver = await CreateDriverAsync(h, n, "D2");
                await AssertInvalidOperationAsync(() => h.TripService.SaveAsync(NewTripForm(bundle.Vehicle, secondDriver, bundle.Supervisor, n, "SECOND")), "تشغيلة مفتوحة");
            }),
            new("Second open trip for same driver is blocked", async (h, n) =>
            {
                var first = await CreateTripAsync(h, n, closed: false);
                var secondVehicle = await CreateVehicleAsync(h, n, "V2");
                await AssertInvalidOperationAsync(() => h.TripService.SaveAsync(NewTripForm(secondVehicle, first.Driver, first.Supervisor, n, "DRIVER")), "السائق");
            }),
            new("Trip requester is required", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n);
                var driver = await CreateDriverAsync(h, n);
                var supervisor = await CreateEmployeeAsync(h, n, "SUP");
                var form = NewTripForm(vehicle, driver, supervisor, n);
                form.RequesterNameText = string.Empty;
                form.RequesterEmployeeId = null;
                await AssertInvalidOperationAsync(() => h.TripService.SaveAsync(form), "طالب التشغيل");
            }),
            new("Trip supervisor is required", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n);
                var driver = await CreateDriverAsync(h, n);
                var form = NewTripForm(vehicle, driver, null, n);
                await AssertInvalidOperationAsync(() => h.TripService.SaveAsync(form), "المشرف");
            }),
            new("Trip start location is required", async (h, n) =>
            {
                var bundle = await CreateTripPrerequisitesAsync(h, n);
                var form = NewTripForm(bundle.Vehicle, bundle.Driver, bundle.Supervisor, n);
                form.StartLocation = string.Empty;
                await AssertInvalidOperationAsync(() => h.TripService.SaveAsync(form), "التحرك");
            }),
            new("Trip end location is required", async (h, n) =>
            {
                var bundle = await CreateTripPrerequisitesAsync(h, n);
                var form = NewTripForm(bundle.Vehicle, bundle.Driver, bundle.Supervisor, n);
                form.EndLocation = string.Empty;
                await AssertInvalidOperationAsync(() => h.TripService.SaveAsync(form), "الوصول");
            }),
            new("Trip purpose is required", async (h, n) =>
            {
                var bundle = await CreateTripPrerequisitesAsync(h, n);
                var form = NewTripForm(bundle.Vehicle, bundle.Driver, bundle.Supervisor, n);
                form.Purpose = string.Empty;
                await AssertInvalidOperationAsync(() => h.TripService.SaveAsync(form), "الغرض");
            }),
            new("Trip end date before start is rejected", async (h, n) =>
            {
                var bundle = await CreateTripPrerequisitesAsync(h, n);
                var form = NewTripForm(bundle.Vehicle, bundle.Driver, bundle.Supervisor, n);
                form.EndDate = form.StartDate.AddHours(-1);
                await AssertInvalidOperationAsync(() => h.TripService.SaveAsync(form), "نهاية الرحلة");
            }),
            new("Trip end mileage before start is rejected", async (h, n) =>
            {
                var bundle = await CreateTripPrerequisitesAsync(h, n);
                var form = NewTripForm(bundle.Vehicle, bundle.Driver, bundle.Supervisor, n);
                form.StartMileage = bundle.Vehicle.CurrentMileage;
                form.EndMileage = bundle.Vehicle.CurrentMileage - 10;
                await AssertInvalidOperationAsync(() => h.TripService.SaveAsync(form), "العداد النهائية");
            }),
            new("Expired vehicle registration blocks trip", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n, registrationExpiry: DateTime.Today.AddDays(-1));
                var driver = await CreateDriverAsync(h, n);
                var supervisor = await CreateEmployeeAsync(h, n, "SUP");
                await AssertInvalidOperationAsync(() => h.TripService.SaveAsync(NewTripForm(vehicle, driver, supervisor, n)), "الترخيص منتهي");
            }),
            new("Vehicle without insurance data blocks trip", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n, withInsuranceDetails: false);
                var driver = await CreateDriverAsync(h, n);
                var supervisor = await CreateEmployeeAsync(h, n, "SUP");
                await AssertInvalidOperationAsync(() => h.TripService.SaveAsync(NewTripForm(vehicle, driver, supervisor, n)), "تأمين ساري");
            }),
            new("Stale InTrip vehicle without open trip can start", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n, status: "InTrip");
                var driver = await CreateDriverAsync(h, n);
                var supervisor = await CreateEmployeeAsync(h, n, "SUP");
                var trip = await h.TripService.SaveAsync(NewTripForm(vehicle, driver, supervisor, n));
                Assert.True(trip.Id > 0);
            }),
            new("Deleting open trip restores vehicle availability", async (h, n) =>
            {
                var bundle = await CreateTripAsync(h, n, closed: false);
                await h.TripService.DeleteAsync(bundle.Trip.Id);
                Assert.Equal("Available", (await h.VehicleService.GetByIdAsync(bundle.Vehicle.Id))?.Status);
            }),
            new("Trip list filters by vehicle", async (h, n) =>
            {
                var first = await CreateTripAsync(h, n, "A", closed: true);
                await CreateTripAsync(h, n, "B", closed: true);
                var filtered = await h.TripService.GetAllAsync(first.Vehicle.Id);
                Assert.All(filtered, x => Assert.Equal(first.Vehicle.Id, x.VehicleId));
            }),
            new("Trip report filters by date", async (h, n) =>
            {
                var trip = await CreateTripAsync(h, n, closed: true, startDate: DateTime.Today.AddDays(-3));
                var report = await h.ReportingService.GenerateReportAsync(new ReportFilterDto { ReportType = "vehicletrips", StartDate = DateTime.Today.AddDays(-3), EndDate = DateTime.Today.AddDays(-3) });
                Assert.Contains(report.Data, row => row["رقم السيارة"].ToString() == trip.Vehicle.PlateNumber);
            }),
            new("Trip requester employee name flows to DTO", async (h, n) =>
            {
                var bundle = await CreateTripAsync(h, n, closed: true);
                var loaded = await h.TripService.GetByIdAsync(bundle.Trip.Id);
                Assert.Equal(bundle.Requester.FullName, loaded?.RequesterName);
            }),
            new("Trip supervisor employee name flows to DTO", async (h, n) =>
            {
                var bundle = await CreateTripAsync(h, n, closed: true);
                var loaded = await h.TripService.GetByIdAsync(bundle.Trip.Id);
                Assert.Equal(bundle.Supervisor.FullName, loaded?.SupervisorName);
            }),
            new("Closed trip distance is computed from odometer", async (h, n) =>
            {
                var bundle = await CreateTripAsync(h, n, closed: true, distance: 65);
                Assert.Equal(65, bundle.Trip.Distance);
            }),
            new("Contract create persists", async (h, n) =>
            {
                var contract = await CreateContractAsync(h, n);
                Assert.True(contract.Id > 0);
            }),
            new("Contract end before start rejected", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n);
                await AssertInvalidOperationAsync(() => h.ContractService.SaveAsync(NewContractForm(h, n, vehicle.Id, start: DateTime.Today, end: DateTime.Today.AddDays(-1))), "نهاية العقد");
            }),
            new("Contract paid amount above value rejected", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n);
                var form = NewContractForm(h, n, vehicle.Id);
                form.ContractValue = 100;
                form.PaidAmount = 200;
                await AssertInvalidOperationAsync(() => h.ContractService.SaveAsync(form), "المدفوع");
            }),
            new("Contract duplicate number rejected", async (h, n) =>
            {
                var contract = await CreateContractAsync(h, n);
                var vehicle = await CreateVehicleAsync(h, n, "C2");
                var duplicate = NewContractForm(h, n, vehicle.Id, suffix: "DUP");
                duplicate.ContractNumber = contract.ContractNumber;
                await AssertInvalidOperationAsync(() => h.ContractService.SaveAsync(duplicate), "العقد");
            }),
            new("Contract report uses Arabic columns and date filter", async (h, n) =>
            {
                var contract = await CreateContractAsync(h, n, start: DateTime.Today.AddDays(-1), end: DateTime.Today.AddDays(10));
                var report = await h.ReportingService.GenerateReportAsync(new ReportFilterDto { ReportType = "contracts", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(30) });
                Assert.Contains("رقم العقد", report.Columns);
                Assert.Contains(report.Data, row => row["رقم العقد"].ToString() == contract.ContractNumber);
            }),
            new("Contract delete removes unused contract", async (h, n) =>
            {
                var contract = await CreateContractAsync(h, n);
                await h.ContractService.DeleteAsync(contract.Id);
                Assert.Null(await h.ContractService.GetByIdAsync(contract.Id));
            }),
            new("Maintenance create with provider persists", async (h, n) =>
            {
                var request = await CreateMaintenanceAsync(h, n, status: "Open");
                Assert.True(request.Id > 0);
                Assert.NotEmpty(request.ServiceProvider);
            }),
            new("Maintenance invalid vehicle rejected", async (h, n) =>
            {
                await AssertInvalidOperationAsync(() => h.MaintenanceService.SaveAsync(NewMaintenanceForm(h, n, vehicleId: 999999)), "المركبة");
            }),
            new("Maintenance invalid type rejected", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n);
                var form = NewMaintenanceForm(h, n, vehicle.Id);
                form.MaintenanceTypeId = 999999;
                await AssertInvalidOperationAsync(() => h.MaintenanceService.SaveAsync(form), "نوع الصيانة");
            }),
            new("Maintenance completion before request rejected", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n);
                var form = NewMaintenanceForm(h, n, vehicle.Id);
                form.RequestDate = DateTime.Today;
                form.CompletionDate = DateTime.Today.AddDays(-1);
                await AssertInvalidOperationAsync(() => h.MaintenanceService.SaveAsync(form), "الإكمال");
            }),
            new("Open maintenance does not block trip", async (h, n) =>
            {
                var prereq = await CreateTripPrerequisitesAsync(h, n);
                await h.MaintenanceService.SaveAsync(NewMaintenanceForm(h, n, prereq.Vehicle.Id, status: "Open"));
                var trip = await h.TripService.SaveAsync(NewTripForm(prereq.Vehicle, prereq.Driver, prereq.Supervisor, n));
                Assert.True(trip.Id > 0);
            }),
            new("Closed maintenance allows trip", async (h, n) =>
            {
                var prereq = await CreateTripPrerequisitesAsync(h, n);
                await h.MaintenanceService.SaveAsync(NewMaintenanceForm(h, n, prereq.Vehicle.Id, status: "Completed"));
                var trip = await h.TripService.SaveAsync(NewTripForm(prereq.Vehicle, prereq.Driver, prereq.Supervisor, n));
                Assert.True(trip.Id > 0);
            }),
            new("Maintenance report filters by request date", async (h, n) =>
            {
                var request = await CreateMaintenanceAsync(h, n, requestDate: DateTime.Today.AddDays(-2), status: "Completed");
                var report = await h.ReportingService.GenerateReportAsync(new ReportFilterDto { ReportType = "maintenance", StartDate = DateTime.Today.AddDays(-2), EndDate = DateTime.Today.AddDays(-2) });
                Assert.Contains(report.Data, row => row["رقم السيارة"].ToString() == request.VehiclePlateNumber);
            }),
            new("Oil change updates vehicle mileage", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n, currentMileage: 1000);
                var oil = await h.OilChangeService.SaveAsync(new OilChangeFormDto { VehicleId = vehicle.Id, ChangeDate = DateTime.Today, OdometerAtChange = 1800, OilType = "5W30", Quantity = 5, Cost = 800, NextOilChangeOdometer = 10000 });
                Assert.True(oil.Id > 0);
                Assert.Equal(1800, (await h.VehicleService.GetByIdAsync(vehicle.Id))?.CurrentMileage);
            }),
            new("Oil change next odometer is calculated from vehicle interval", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n);
                var oil = await h.OilChangeService.SaveAsync(new OilChangeFormDto { VehicleId = vehicle.Id, ChangeDate = DateTime.Today, OdometerAtChange = 2000, OilType = "5W30", Quantity = 5, Cost = 800, NextOilChangeOdometer = 1999 });
                Assert.Equal(12000, oil.NextOilChangeOdometer);
            }),
            new("Oil due metric counts approaching vehicle", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n, currentMileage: 5000);
                await h.OilChangeService.SaveAsync(new OilChangeFormDto { VehicleId = vehicle.Id, ChangeDate = DateTime.Today, OdometerAtChange = 5000, OilType = "5W30", Quantity = 5, Cost = 800, NextOilChangeOdometer = 10000 });
                var vehicleAfterOil = (await h.VehicleService.GetByIdAsync(vehicle.Id))!;
                var form = NewVehicleForm(h, n, currentMileage: 14600);
                form.Id = vehicleAfterOil.Id;
                form.PlateNumber = vehicleAfterOil.PlateNumber;
                form.VehicleTypeId = vehicleAfterOil.VehicleTypeId;
                form.Model = vehicleAfterOil.Model;
                form.Year = vehicleAfterOil.Year;
                form.ChassisNumber = vehicleAfterOil.ChassisNumber;
                form.EngineNumber = vehicleAfterOil.EngineNumber;
                form.RegistrationStartDate = vehicleAfterOil.RegistrationStartDate;
                form.RegistrationExpiryDate = vehicleAfterOil.RegistrationExpiryDate;
                await h.VehicleService.SaveAsync(form);
                Assert.True((await h.ReportingService.GetDashboardMetricsAsync()).OilChangesDue >= 1);
            }),
            new("Oil report filters by change date", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n);
                await h.OilChangeService.SaveAsync(new OilChangeFormDto { VehicleId = vehicle.Id, ChangeDate = DateTime.Today, OdometerAtChange = 1500, OilType = "5W30", Quantity = 5, Cost = 800, NextOilChangeOdometer = 10000 });
                var report = await h.ReportingService.GenerateReportAsync(new ReportFilterDto { ReportType = "oilchanges", StartDate = DateTime.Today, EndDate = DateTime.Today });
                Assert.Contains("المقطوع من آخر غيار", report.Columns);
                Assert.Contains(report.Data, row => row["رقم العربية"].ToString() == vehicle.PlateNumber);
            }),
            new("Insurance create persists", async (h, n) =>
            {
                var insurance = await CreateInsuranceAsync(h, n);
                Assert.True(insurance.Id > 0);
                Assert.Equal("Active", insurance.Status);
            }),
            new("Insurance expiry before start rejected", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n);
                await AssertInvalidOperationAsync(() => h.InsuranceService.SaveAsync(NewInsuranceForm(n, vehicle.Id, start: DateTime.Today, expiry: DateTime.Today.AddDays(-1))), "انتهاء التأمين");
            }),
            new("Insurance negative premium rejected", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n);
                var form = NewInsuranceForm(n, vehicle.Id);
                form.PremiumAmount = -1;
                await AssertInvalidOperationAsync(() => h.InsuranceService.SaveAsync(form), "سالبة");
            }),
            new("Insurance expiring appears in alerts", async (h, n) =>
            {
                var insurance = await CreateInsuranceAsync(h, n, expiry: DateTime.Today.AddDays(8));
                var alerts = await h.ReportingService.GetAlertsAsync();
                Assert.Contains(alerts, x => x.RelatedEntityType == "Insurance" && x.RelatedEntityId == insurance.Id);
            }),
            new("Insurance report filters by expiry", async (h, n) =>
            {
                var insurance = await CreateInsuranceAsync(h, n, expiry: DateTime.Today.AddDays(9));
                var report = await h.ReportingService.GenerateReportAsync(new ReportFilterDto { ReportType = "insurance", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(30) });
                Assert.Contains(report.Data, row => row["رقم الوثيقة"].ToString() == insurance.PolicyNumber);
            }),
            new("Driver license create persists", async (h, n) =>
            {
                var license = await CreateLicenseAsync(h, n);
                Assert.True(license.Id > 0);
            }),
            new("Driver license expiry before issue rejected", async (h, n) =>
            {
                var driver = await CreateDriverAsync(h, n);
                await AssertInvalidOperationAsync(() => h.LicenseService.SaveAsync(NewLicenseForm(n, driver.Id, issue: DateTime.Today, expiry: DateTime.Today.AddDays(-1))), "انتهاء الرخصة");
            }),
            new("Driver license duplicate number rejected", async (h, n) =>
            {
                var license = await CreateLicenseAsync(h, n);
                var driver = await CreateDriverAsync(h, n, "D2");
                var duplicate = NewLicenseForm(n, driver.Id, suffix: "DUP");
                duplicate.LicenseNumber = license.LicenseNumber;
                await AssertInvalidOperationAsync(() => h.LicenseService.SaveAsync(duplicate), "الرخصة");
            }),
            new("Driver license expiring appears in alerts", async (h, n) =>
            {
                var license = await CreateLicenseAsync(h, n, expiry: DateTime.Today.AddDays(11));
                var alerts = await h.ReportingService.GetAlertsAsync();
                Assert.Contains(alerts, x => x.RelatedEntityType == "License" && x.RelatedEntityId == license.Id);
            }),
            new("Custody create persists", async (h, n) =>
            {
                var custody = await CreateCustodyAsync(h, n);
                Assert.True(custody.Id > 0);
            }),
            new("Custody links employee by custodian name", async (h, n) =>
            {
                var employee = await CreateEmployeeAsync(h, n);
                var vehicle = await CreateVehicleAsync(h, n);
                var custody = await h.CustodyService.SaveAsync(NewCustodyForm(n, vehicle.Id, employee.FullName));
                var entity = await h.Context.Custodies.FirstAsync(x => x.Id == custody.Id);
                Assert.Equal(employee.Id, entity.EmployeeId);
            }),
            new("Custody delete removes record", async (h, n) =>
            {
                var custody = await CreateCustodyAsync(h, n);
                await h.CustodyService.DeleteAsync(custody.Id);
                Assert.Null(await h.CustodyService.GetByIdAsync(custody.Id));
            }),
            new("Custody blocks linked employee delete", async (h, n) =>
            {
                var employee = await CreateEmployeeAsync(h, n);
                var vehicle = await CreateVehicleAsync(h, n);
                await h.CustodyService.SaveAsync(NewCustodyForm(n, vehicle.Id, employee.FullName));
                await AssertInvalidOperationAsync(() => h.EmployeeService.DeleteAsync(employee.Id), "ارتباطه");
            }),
            new("Treasury income increases balance", async (h, n) =>
            {
                await h.TreasuryService.SaveAsync(new TreasuryTransactionFormDto { TransactionDate = DateTime.Today, TransactionType = "إيراد", Amount = 1000, Description = "تحصيل" });
                Assert.Equal(1000, await h.TreasuryService.GetCurrentBalanceAsync());
            }),
            new("Treasury expense decreases balance", async (h, n) =>
            {
                await h.TreasuryService.SaveAsync(new TreasuryTransactionFormDto { TransactionDate = DateTime.Today, TransactionType = "صرف", Amount = 350, Description = "مصروف" });
                Assert.Equal(-350, await h.TreasuryService.GetCurrentBalanceAsync());
            }),
            new("Fuel paid from treasury creates linked transaction", async (h, n) =>
            {
                var bundle = await CreateTripAsync(h, n, closed: true);
                var fuel = await h.FuelService.SaveAsync(new FuelTransactionFormDto { VehicleId = bundle.Vehicle.Id, TripId = bundle.Trip.Id, TransactionDate = DateTime.Today, FuelType = "بنزين", Quantity = 40, UnitPrice = 15, Odometer = 1300, PaidFromTreasury = true });
                Assert.NotNull(fuel.TreasuryTransactionId);
                Assert.Contains(await h.TreasuryService.GetAllAsync(), x => x.Id == fuel.TreasuryTransactionId && x.TransactionType == "صرف");
                var fuelReport = await h.ReportingService.GenerateReportAsync(new ReportFilterDto { ReportType = "fuel", StartDate = DateTime.Today, EndDate = DateTime.Today, VehicleId = bundle.Vehicle.Id });
                Assert.Contains(fuelReport.Data, row => row["إجمالي البنزين"].ToString() == "600");
                var tripsReport = await h.ReportingService.GenerateReportAsync(new ReportFilterDto { ReportType = "alltrips", StartDate = DateTime.Today, EndDate = DateTime.Today });
                Assert.DoesNotContain("تكلفة البنزين", tripsReport.Columns);
                await h.TripService.DeleteAsync(bundle.Trip.Id);
                Assert.Null(await h.TripService.GetByIdAsync(bundle.Trip.Id));
                Assert.Null((await h.FuelService.GetByIdAsync(fuel.Id))?.TripId);
            }),
            new("Fuel delete removes linked treasury transaction", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n);
                var fuel = await h.FuelService.SaveAsync(new FuelTransactionFormDto { VehicleId = vehicle.Id, TransactionDate = DateTime.Today, FuelType = "بنزين", Quantity = 20, UnitPrice = 10, Odometer = 1200, PaidFromTreasury = true });
                var treasuryId = fuel.TreasuryTransactionId;
                await h.FuelService.DeleteAsync(fuel.Id);
                Assert.DoesNotContain(await h.TreasuryService.GetAllAsync(), x => x.Id == treasuryId);
            }),
            new("Expense paid from treasury links transaction and appears in treasury report", async (h, n) =>
            {
                var vehicle = await CreateVehicleAsync(h, n);
                var expense = await h.ExpenseService.SaveAsync(new ExpenseFormDto { VehicleId = vehicle.Id, ExpenseDate = DateTime.Today, Category = "غسيل", Amount = 150, Description = "غسيل سيارة", Vendor = "مركز خدمة", PaidFromTreasury = true });
                Assert.NotNull(expense.TreasuryTransactionId);
                var report = await h.ReportingService.GenerateReportAsync(new ReportFilterDto { ReportType = "treasury", StartDate = DateTime.Today, EndDate = DateTime.Today });
                Assert.Contains(report.Data, row => row["مرتبط بـ"].ToString() == "Expense");
            })
        };

    private static async Task AssertInvalidOperationAsync(Func<Task> action, string expectedMessagePart)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(action);
        Assert.Contains(expectedMessagePart, exception.Message);
    }

    private static VehicleFormDto NewVehicleForm(ScenarioHarness h, int scenario, string suffix = "V", string status = "Available", decimal currentMileage = 1000, DateTime? registrationExpiry = null, bool withInsuranceDetails = true) =>
        new()
        {
            PlateNumber = $"SC{scenario:000}-{suffix}",
            VehicleTypeId = h.DefaultVehicleTypeId,
            Model = "Toyota Corolla",
            Year = 2024,
            Manufacturer = "Toyota",
            Color = "أبيض",
            ChassisNumber = $"CH{scenario:000}{suffix}",
            EngineNumber = $"EN{scenario:000}{suffix}",
            Status = status,
            CurrentMileage = currentMileage,
            PurchaseDate = DateTime.Today.AddMonths(-3),
            RegistrationStartDate = DateTime.Today.AddMonths(-2),
            RegistrationExpiryDate = registrationExpiry ?? DateTime.Today.AddMonths(10),
            AccidentInsuranceDetails = withInsuranceDetails ? $"حوادث {scenario}-{suffix}" : string.Empty,
            SocialInsuranceDetails = withInsuranceDetails ? $"اجتماعي {scenario}-{suffix}" : string.Empty,
            OilChangeIntervalKm = 10000,
            MaintenanceIntervalKm = 15000,
            Notes = "سيناريو اختبار"
        };

    private static async Task<VehicleDto> CreateVehicleAsync(ScenarioHarness h, int scenario, string suffix = "V", string status = "Available", decimal currentMileage = 1000, DateTime? registrationExpiry = null, bool withInsuranceDetails = true)
    {
        var vehicle = await h.VehicleService.SaveAsync(NewVehicleForm(h, scenario, suffix, status, currentMileage, registrationExpiry, withInsuranceDetails));
        if (withInsuranceDetails)
        {
            await h.InsuranceService.SaveAsync(NewInsuranceForm(scenario, vehicle.Id, $"AUTO-{suffix}"));
        }

        return vehicle;
    }

    private static VehicleFormDto VehicleForm(VehicleDto dto) =>
        new()
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
            Status = dto.Status,
            CurrentMileage = dto.CurrentMileage,
            PurchaseDate = dto.PurchaseDate,
            RegistrationStartDate = dto.RegistrationStartDate,
            RegistrationExpiryDate = dto.RegistrationExpiryDate,
            AccidentInsuranceDetails = dto.AccidentInsuranceDetails,
            SocialInsuranceDetails = dto.SocialInsuranceDetails,
            OilChangeIntervalKm = dto.OilChangeIntervalKm,
            MaintenanceIntervalKm = dto.MaintenanceIntervalKm,
            Notes = dto.Notes
        };

    private static async Task<DriverDto> CreateDriverAsync(ScenarioHarness h, int scenario, string suffix = "D", bool active = true, DateTime? licenseExpiry = null) =>
        await h.DriverService.SaveAsync(new DriverFormDto
        {
            FullName = $"سائق {scenario}-{suffix}",
            NationalId = $"2990101{scenario:000}{suffix.Length}",
            PhoneNumber = "01000000000",
            LicenseNumber = $"DRV-{scenario:000}-{suffix}",
            LicenseStartDate = DateTime.Today.AddYears(-1),
            LicenseExpiryDate = licenseExpiry ?? DateTime.Today.AddYears(1),
            LicenseType = "خاصة",
            WorkLocation = "الموقع الرئيسي",
            IsActive = active,
            Address = "القاهرة"
        });

    private static async Task<EmployeeDto> CreateEmployeeAsync(ScenarioHarness h, int scenario, string suffix = "E") =>
        await h.EmployeeService.SaveAsync(new EmployeeFormDto
        {
            FullName = $"موظف {scenario}-{suffix}",
            EmployeeId = $"EMP-{scenario:000}-{suffix}",
            Department = "التشغيل",
            Position = suffix.Contains("SUP", StringComparison.OrdinalIgnoreCase) ? "مشرف" : "موصي",
            HireDate = DateTime.Today.AddMonths(-6),
            Status = "Active",
            PhoneNumber = "01000000000"
        });

    private static async Task<TripBundle> CreateTripPrerequisitesAsync(ScenarioHarness h, int scenario, string suffix = "T")
    {
        var vehicle = await CreateVehicleAsync(h, scenario, $"V{suffix}");
        var driver = await CreateDriverAsync(h, scenario, $"D{suffix}");
        var requester = await CreateEmployeeAsync(h, scenario, $"REQ{suffix}");
        var supervisor = await CreateEmployeeAsync(h, scenario, $"SUP{suffix}");
        return new TripBundle(vehicle, driver, requester, supervisor, new TripDto());
    }

    private static async Task<TripBundle> CreateTripAsync(ScenarioHarness h, int scenario, string suffix = "T", bool closed = false, DateTime? startDate = null, decimal distance = 40)
    {
        var prereq = await CreateTripPrerequisitesAsync(h, scenario, suffix);
        var form = NewTripForm(prereq.Vehicle, prereq.Driver, prereq.Supervisor, scenario, suffix);
        form.RequesterEmployeeId = prereq.Requester.Id;
        form.RequesterNameText = prereq.Requester.FullName;
        form.StartDate = startDate ?? DateTime.Today.AddHours(9);
        form.StartMileage = prereq.Vehicle.CurrentMileage;

        if (closed)
        {
            form.EndDate = form.StartDate.AddHours(2);
            form.EndMileage = form.StartMileage + distance;
            form.Status = "Closed";
        }

        var trip = await h.TripService.SaveAsync(form);
        return prereq with { Trip = trip };
    }

    private static TripFormDto NewTripForm(VehicleDto vehicle, DriverDto driver, EmployeeDto? supervisor, int scenario, string suffix = "T") =>
        new()
        {
            VehicleId = vehicle.Id,
            DriverId = driver.Id,
            RequesterNameText = $"طالب تشغيل {scenario}-{suffix}",
            SupervisorEmployeeId = supervisor?.Id,
            StartDate = DateTime.Today.AddHours(9),
            StartLocation = "المقر",
            EndLocation = "العميل",
            StartMileage = vehicle.CurrentMileage,
            Purpose = $"مأمورية {scenario}-{suffix}",
            Status = "Open"
        };

    private static TripFormDto TripForm(TripDto trip) =>
        new()
        {
            Id = trip.Id,
            VehicleId = trip.VehicleId,
            DriverId = trip.DriverId,
            RequesterEmployeeId = trip.RequesterEmployeeId,
            RequesterNameText = trip.RequesterNameText,
            SupervisorEmployeeId = trip.SupervisorEmployeeId,
            StartDate = trip.StartDate,
            EndDate = trip.EndDate,
            StartLocation = trip.StartLocation,
            EndLocation = trip.EndLocation,
            StartMileage = trip.StartMileage,
            EndMileage = trip.EndMileage,
            Distance = trip.Distance,
            Purpose = trip.Purpose,
            Status = trip.Status,
            FuelConsumed = trip.FuelConsumed,
            TripCost = trip.TripCost,
            Notes = trip.Notes
        };

    private static ContractFormDto NewContractForm(ScenarioHarness h, int scenario, int vehicleId, string suffix = "C", DateTime? start = null, DateTime? end = null) =>
        new()
        {
            ContractNumber = $"CON-{scenario:000}-{suffix}",
            VehicleId = vehicleId,
            ContractStatusId = h.DefaultContractStatusId,
            ClientName = $"عميل {scenario}",
            StartDate = start ?? DateTime.Today,
            EndDate = end ?? DateTime.Today.AddMonths(1),
            ContractValue = 10000,
            PaidAmount = 2500,
            PaymentTerms = "شهري"
        };

    private static async Task<ContractDto> CreateContractAsync(ScenarioHarness h, int scenario, DateTime? start = null, DateTime? end = null)
    {
        var vehicle = await CreateVehicleAsync(h, scenario);
        return await h.ContractService.SaveAsync(NewContractForm(h, scenario, vehicle.Id, start: start, end: end));
    }

    private static MaintenanceRequestFormDto NewMaintenanceForm(ScenarioHarness h, int scenario, int vehicleId, string status = "Open", DateTime? requestDate = null) =>
        new()
        {
            VehicleId = vehicleId,
            MaintenanceTypeId = h.DefaultMaintenanceTypeId,
            ServiceProviderId = h.DefaultServiceProviderId,
            RequestDate = requestDate ?? DateTime.Today,
            Status = status,
            Description = $"طلب صيانة {scenario}",
            EstimatedCost = 1000,
            ActualCost = status == "Completed" ? 900 : 0
        };

    private static async Task<MaintenanceRequestDto> CreateMaintenanceAsync(ScenarioHarness h, int scenario, string status = "Open", DateTime? requestDate = null)
    {
        var vehicle = await CreateVehicleAsync(h, scenario);
        return await h.MaintenanceService.SaveAsync(NewMaintenanceForm(h, scenario, vehicle.Id, status, requestDate));
    }

    private static InsuranceFormDto NewInsuranceForm(int scenario, int vehicleId, string suffix = "I", DateTime? start = null, DateTime? expiry = null) =>
        new()
        {
            VehicleId = vehicleId,
            PolicyNumber = $"INS-{scenario:000}-{suffix}",
            InsuranceCompany = "شركة التأمين",
            PolicyType = "شامل",
            StartDate = start ?? DateTime.Today.AddMonths(-1),
            ExpiryDate = expiry ?? DateTime.Today.AddMonths(8),
            PremiumAmount = 1200,
            CoverageAmount = 100000,
            CoverageDetails = "تغطية تشغيل",
            AgentName = "مندوب",
            AgentPhoneNumber = "01000000000"
        };

    private static async Task<InsuranceDto> CreateInsuranceAsync(ScenarioHarness h, int scenario, DateTime? expiry = null)
    {
        var vehicle = await CreateVehicleAsync(h, scenario, withInsuranceDetails: false);
        return await h.InsuranceService.SaveAsync(NewInsuranceForm(scenario, vehicle.Id, expiry: expiry));
    }

    private static LicenseFormDto NewLicenseForm(int scenario, int driverId, string suffix = "L", DateTime? issue = null, DateTime? expiry = null) =>
        new()
        {
            DriverId = driverId,
            LicenseNumber = $"LIC-{scenario:000}-{suffix}",
            LicenseType = "خاصة",
            IssueDate = issue ?? DateTime.Today.AddMonths(-6),
            ExpiryDate = expiry ?? DateTime.Today.AddYears(1),
            IssuingAuthority = "المرور"
        };

    private static async Task<LicenseDto> CreateLicenseAsync(ScenarioHarness h, int scenario, DateTime? expiry = null)
    {
        var driver = await CreateDriverAsync(h, scenario);
        return await h.LicenseService.SaveAsync(NewLicenseForm(scenario, driver.Id, expiry: expiry));
    }

    private static CustodyFormDto NewCustodyForm(int scenario, int vehicleId, string custodianName = "مستلم عهدة") =>
        new()
        {
            VehicleId = vehicleId,
            CustodyNumber = $"CUS-{scenario:000}",
            CustodianName = custodianName,
            CustodianPosition = "مشرف",
            HandoverDate = DateTime.Today,
            Status = "Active",
            VehicleConditionRating = 5,
            Notes = "عهدة اختبار"
        };

    private static async Task<CustodyDto> CreateCustodyAsync(ScenarioHarness h, int scenario)
    {
        var vehicle = await CreateVehicleAsync(h, scenario);
        return await h.CustodyService.SaveAsync(NewCustodyForm(scenario, vehicle.Id));
    }

    private static UserFormDto NewUser(string username, string password) =>
        new()
        {
            Username = username,
            Email = $"{username}@fleet.local",
            FullName = $"مستخدم {username}",
            Role = "Staff",
            IsActive = true,
            Password = password,
            ConfirmPassword = password
        };

    private sealed record Scenario(string Name, Func<ScenarioHarness, int, Task> Execute);

    private sealed record TripBundle(VehicleDto Vehicle, DriverDto Driver, EmployeeDto Requester, EmployeeDto Supervisor, TripDto Trip);

    private sealed class ScenarioHarness : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        public FleetDbContext Context { get; }
        public IAuditService AuditService { get; }
        public INotificationService NotificationService { get; }
        public IVehicleService VehicleService { get; }
        public IContractService ContractService { get; }
        public IMaintenanceService MaintenanceService { get; }
        public IDriverService DriverService { get; }
        public IEmployeeService EmployeeService { get; }
        public ITripService TripService { get; }
        public IFuelService FuelService { get; }
        public IExpenseService ExpenseService { get; }
        public IOilChangeService OilChangeService { get; }
        public ITreasuryService TreasuryService { get; }
        public ILicenseService LicenseService { get; }
        public IInsuranceService InsuranceService { get; }
        public ICustodyService CustodyService { get; }
        public IMasterDataService MasterDataService { get; }
        public IAuthenticationService AuthenticationService { get; }
        public IReportingService ReportingService { get; }
        public ISettingsService SettingsService { get; }
        public int DefaultVehicleTypeId { get; private set; }
        public int DefaultContractStatusId { get; private set; }
        public int DefaultMaintenanceTypeId { get; private set; }
        public int DefaultServiceProviderId { get; private set; }

        private ScenarioHarness(SqliteConnection connection, FleetDbContext context)
        {
            _connection = connection;
            Context = context;
            AuditService = new AuditService(context);
            NotificationService = new NotificationService(context);
            VehicleService = new VehicleService(context, AuditService, NotificationService);
            ContractService = new ContractService(context, AuditService, NotificationService);
            MaintenanceService = new MaintenanceService(context, AuditService, NotificationService);
            DriverService = new DriverService(context, AuditService);
            EmployeeService = new EmployeeService(context, AuditService);
            TripService = new TripService(context, AuditService);
            FuelService = new FuelService(context, AuditService);
            ExpenseService = new ExpenseService(context, AuditService);
            OilChangeService = new OilChangeService(context, AuditService);
            TreasuryService = new TreasuryService(context, AuditService);
            LicenseService = new LicenseService(context, AuditService);
            InsuranceService = new InsuranceService(context, AuditService);
            CustodyService = new CustodyService(context, AuditService);
            MasterDataService = new MasterDataService(context, AuditService);
            AuthenticationService = new AuthenticationService(context, AuditService);
            ReportingService = new ReportingService(context);
            SettingsService = new SettingsService(context, AuditService);
        }

        public static async Task<ScenarioHarness> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<FleetDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new FleetDbContext(options);
            await new DataBootstrapService(context).InitializeAsync();

            var harness = new ScenarioHarness(connection, context)
            {
                DefaultVehicleTypeId = await context.VehicleTypes.Select(x => x.Id).FirstAsync(),
                DefaultContractStatusId = await context.ContractStatuses.Select(x => x.Id).FirstAsync(),
                DefaultMaintenanceTypeId = await context.MaintenanceTypes.Select(x => x.Id).FirstAsync(),
                DefaultServiceProviderId = await context.ServiceProviders.Select(x => x.Id).FirstAsync()
            };

            return harness;
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
