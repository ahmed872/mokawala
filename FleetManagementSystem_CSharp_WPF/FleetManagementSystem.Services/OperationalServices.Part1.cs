using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Data;
using FleetManagementSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementSystem.Services;

public sealed class VehicleService(FleetDbContext context, IAuditService auditService, INotificationService notificationService) : IVehicleService
{
    private readonly FleetDbContext _context = context;
    private readonly IAuditService _auditService = auditService;
    private readonly INotificationService _notificationService = notificationService;

    public async Task<List<VehicleDto>> GetAllAsync(string? search = null)
    {
        var query = _context.Vehicles.Include(v => v.VehicleType).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(v =>
                v.PlateNumber.Contains(term) ||
                v.Model.Contains(term) ||
                v.AssignedTo.Contains(term));
        }

        var items = await query.OrderByDescending(v => v.CreatedAt).ToListAsync();
        return items.Select(v => v.ToDto()).ToList();
    }

    public async Task<VehicleDto?> GetByIdAsync(int id)
    {
        var entity = await _context.Vehicles
            .Include(v => v.VehicleType)
            .FirstOrDefaultAsync(v => v.Id == id);

        return entity?.ToDto();
    }

    public async Task<VehicleDto> SaveAsync(VehicleFormDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.PlateNumber))
        {
            throw new InvalidOperationException("رقم اللوحة مطلوب.");
        }

        if (dto.CurrentMileage < 0)
        {
            throw new InvalidOperationException("قراءة العداد الحالية لا يمكن أن تكون سالبة.");
        }

        if (dto.OilChangeIntervalKm <= 0 || dto.MaintenanceIntervalKm <= 0)
        {
            throw new InvalidOperationException("دورية الزيت والصيانة يجب أن تكون أكبر من صفر.");
        }

        if (dto.RegistrationStartDate.HasValue &&
            dto.RegistrationExpiryDate.HasValue &&
            dto.RegistrationExpiryDate.Value.Date < dto.RegistrationStartDate.Value.Date)
        {
            throw new InvalidOperationException("تاريخ انتهاء الترخيص يجب أن يكون بعد بداية الترخيص.");
        }

        var vehicleTypeExists = await _context.VehicleTypes.AnyAsync(vt => vt.Id == dto.VehicleTypeId);
        if (!vehicleTypeExists)
        {
            throw new InvalidOperationException("نوع المركبة غير موجود.");
        }

        var duplicate = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id != dto.Id && v.PlateNumber == dto.PlateNumber.Trim());
        if (duplicate is not null)
        {
            throw new InvalidOperationException("رقم اللوحة مستخدم بالفعل.");
        }

        if (!string.IsNullOrWhiteSpace(dto.ChassisNumber))
        {
            var duplicateChassis = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id != dto.Id && v.ChassisNumber == dto.ChassisNumber.Trim());
            if (duplicateChassis is not null)
            {
                throw new InvalidOperationException("رقم الشاسيه مستخدم بالفعل.");
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.EngineNumber))
        {
            var duplicateEngine = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id != dto.Id && v.EngineNumber == dto.EngineNumber.Trim());
            if (duplicateEngine is not null)
            {
                throw new InvalidOperationException("رقم الموتور مستخدم بالفعل.");
            }
        }

        Vehicle entity;
        var action = dto.Id == 0 ? "Create" : "Update";
        if (dto.Id == 0)
        {
            entity = new Vehicle { CreatedAt = DateTime.UtcNow };
            _context.Vehicles.Add(entity);
        }
        else
        {
            entity = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == dto.Id)
                ?? throw new InvalidOperationException("المركبة غير موجودة.");
        }

        var oldValues = dto.Id == 0 ? null : $"{entity.PlateNumber}|{entity.Status}|{entity.Mileage}";

        entity.PlateNumber = dto.PlateNumber.Trim();
        entity.VehicleTypeId = dto.VehicleTypeId;
        entity.Model = ServiceHelpers.Clean(dto.Model);
        entity.Year = dto.Year;
        entity.Manufacturer = ServiceHelpers.Clean(dto.Manufacturer);
        entity.Color = ServiceHelpers.Clean(dto.Color);
        entity.ChassisNumber = ServiceHelpers.Clean(dto.ChassisNumber);
        entity.EngineNumber = ServiceHelpers.Clean(dto.EngineNumber);
        entity.Mileage = dto.CurrentMileage;
        entity.Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status;
        entity.AssignedTo = ServiceHelpers.Clean(dto.AssignedTo);
        entity.PurchaseDate = dto.PurchaseDate == default ? null : dto.PurchaseDate;
        entity.PurchasePrice = dto.PurchasePrice;
        entity.RegistrationStartDate = ServiceHelpers.OrNull(dto.RegistrationStartDate);
        entity.RegistrationExpiryDate = ServiceHelpers.OrNull(dto.RegistrationExpiryDate);
        entity.AccidentInsuranceDetails = ServiceHelpers.Clean(dto.AccidentInsuranceDetails);
        entity.SocialInsuranceDetails = ServiceHelpers.Clean(dto.SocialInsuranceDetails);
        entity.OilChangeIntervalKm = dto.OilChangeIntervalKm;
        entity.MaintenanceIntervalKm = dto.MaintenanceIntervalKm;
        entity.Notes = ServiceHelpers.Clean(dto.Notes);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogActionAsync(action, "Vehicle", entity.Id, oldValues, $"{entity.PlateNumber}|{entity.Status}|{entity.Mileage}");
        await _notificationService.CreateNotificationAsync(
            "Vehicle Saved",
            $"Vehicle {entity.PlateNumber} has been saved successfully.",
            "Info",
            "Vehicle",
            entity.Id);

        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new InvalidOperationException("المركبة غير موجودة.");

        var hasDependents =
            await _context.Contracts.AnyAsync(x => x.VehicleId == id) ||
            await _context.MaintenanceRequests.AnyAsync(x => x.VehicleId == id) ||
            await _context.Trips.AnyAsync(x => x.VehicleId == id) ||
            await _context.FuelTransactions.AnyAsync(x => x.VehicleId == id) ||
            await _context.Expenses.AnyAsync(x => x.VehicleId == id) ||
            await _context.Insurances.AnyAsync(x => x.VehicleId == id) ||
            await _context.Custodies.AnyAsync(x => x.VehicleId == id);

        if (hasDependents)
        {
            throw new InvalidOperationException("لا يمكن حذف المركبة لارتباطها بسجلات أخرى.");
        }

        _context.Vehicles.Remove(entity);
        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync("Delete", "Vehicle", entity.Id, entity.PlateNumber, null);
    }

    public async Task<List<VehicleLookupDto>> GetLookupAsync() =>
        await _context.Vehicles
            .OrderBy(v => v.PlateNumber)
            .Select(v => new VehicleLookupDto
            {
                Id = v.Id,
                PlateNumber = v.PlateNumber,
                Model = v.Model
            })
            .ToListAsync();
}

public sealed class ContractService(FleetDbContext context, IAuditService auditService, INotificationService notificationService) : IContractService
{
    private readonly FleetDbContext _context = context;
    private readonly IAuditService _auditService = auditService;
    private readonly INotificationService _notificationService = notificationService;

    public async Task<List<ContractDto>> GetAllAsync(string? status = null)
    {
        var query = _context.Contracts
            .Include(c => c.Vehicle)
            .Include(c => c.ContractStatus)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(c => c.ContractStatus != null && c.ContractStatus.Name == status);
        }

        var items = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return items.Select(c => c.ToDto()).ToList();
    }

    public async Task<ContractDto?> GetByIdAsync(int id)
    {
        var entity = await _context.Contracts
            .Include(c => c.Vehicle)
            .Include(c => c.ContractStatus)
            .FirstOrDefaultAsync(c => c.Id == id);

        return entity?.ToDto();
    }

    public async Task<ContractDto> SaveAsync(ContractFormDto dto)
    {
        if (dto.EndDate <= dto.StartDate)
        {
            throw new InvalidOperationException("تاريخ نهاية العقد يجب أن يكون بعد تاريخ البداية.");
        }

        if (dto.PaidAmount > dto.ContractValue)
        {
            throw new InvalidOperationException("المبلغ المدفوع لا يمكن أن يتجاوز قيمة العقد.");
        }

        if (!await _context.Vehicles.AnyAsync(v => v.Id == dto.VehicleId))
        {
            throw new InvalidOperationException("المركبة غير موجودة.");
        }

        if (!await _context.ContractStatuses.AnyAsync(s => s.Id == dto.ContractStatusId))
        {
            throw new InvalidOperationException("حالة العقد غير موجودة.");
        }

        var duplicate = await _context.Contracts.FirstOrDefaultAsync(c => c.Id != dto.Id && c.ContractNumber == dto.ContractNumber.Trim());
        if (duplicate is not null)
        {
            throw new InvalidOperationException("رقم العقد مستخدم بالفعل.");
        }

        Contract entity;
        var action = dto.Id == 0 ? "Create" : "Update";
        if (dto.Id == 0)
        {
            entity = new Contract { CreatedAt = DateTime.UtcNow };
            _context.Contracts.Add(entity);
        }
        else
        {
            entity = await _context.Contracts.FirstOrDefaultAsync(c => c.Id == dto.Id)
                ?? throw new InvalidOperationException("العقد غير موجود.");
        }

        entity.ContractNumber = dto.ContractNumber.Trim();
        entity.VehicleId = dto.VehicleId;
        entity.ContractStatusId = dto.ContractStatusId;
        entity.ClientName = ServiceHelpers.Clean(dto.ClientName);
        entity.ClientEmail = ServiceHelpers.Clean(dto.ClientEmail);
        entity.ClientPhoneNumber = ServiceHelpers.Clean(dto.ClientPhoneNumber);
        entity.ClientAddress = ServiceHelpers.Clean(dto.ClientAddress);
        entity.StartDate = ServiceHelpers.OrToday(dto.StartDate);
        entity.EndDate = ServiceHelpers.OrToday(dto.EndDate);
        entity.ContractValue = dto.ContractValue;
        entity.PaidAmount = dto.PaidAmount;
        entity.PaymentTerms = ServiceHelpers.Clean(dto.PaymentTerms);
        entity.ContractTerms = ServiceHelpers.Clean(dto.ContractTerms);
        entity.DocumentUrl = ServiceHelpers.Clean(dto.DocumentUrl);
        entity.Notes = ServiceHelpers.Clean(dto.Notes);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync(action, "Contract", entity.Id, null, entity.ContractNumber);
        await _notificationService.CreateNotificationAsync("Contract Saved", $"Contract {entity.ContractNumber} was saved.", "Info", "Contract", entity.Id);

        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Contracts.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new InvalidOperationException("العقد غير موجود.");

        _context.Contracts.Remove(entity);
        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync("Delete", "Contract", entity.Id, entity.ContractNumber, null);
    }
}

public sealed class MaintenanceService(FleetDbContext context, IAuditService auditService, INotificationService notificationService) : IMaintenanceService
{
    private readonly FleetDbContext _context = context;
    private readonly IAuditService _auditService = auditService;
    private readonly INotificationService _notificationService = notificationService;

    public async Task<List<MaintenanceRequestDto>> GetAllAsync(string? status = null)
    {
        var query = _context.MaintenanceRequests
            .Include(m => m.Vehicle)
            .Include(m => m.MaintenanceType)
            .Include(m => m.ServiceProvider)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(m => m.Status == status);
        }

        var items = await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
        return items.Select(m => m.ToDto()).ToList();
    }

    public async Task<MaintenanceRequestDto?> GetByIdAsync(int id)
    {
        var entity = await _context.MaintenanceRequests
            .Include(m => m.Vehicle)
            .Include(m => m.MaintenanceType)
            .Include(m => m.ServiceProvider)
            .FirstOrDefaultAsync(m => m.Id == id);

        return entity?.ToDto();
    }

    public async Task<MaintenanceRequestDto> SaveAsync(MaintenanceRequestFormDto dto)
    {
        if (!await _context.Vehicles.AnyAsync(v => v.Id == dto.VehicleId))
        {
            throw new InvalidOperationException("المركبة غير موجودة.");
        }

        if (!await _context.MaintenanceTypes.AnyAsync(mt => mt.Id == dto.MaintenanceTypeId))
        {
            throw new InvalidOperationException("نوع الصيانة غير موجود.");
        }

        if (dto.ServiceProviderId.HasValue && !await _context.ServiceProviders.AnyAsync(sp => sp.Id == dto.ServiceProviderId.Value))
        {
            throw new InvalidOperationException("مزود الخدمة غير موجود.");
        }

        if (dto.CompletionDate.HasValue && dto.CompletionDate.Value < dto.RequestDate)
        {
            throw new InvalidOperationException("تاريخ الإكمال لا يمكن أن يكون قبل تاريخ الطلب.");
        }

        MaintenanceRequest entity;
        var action = dto.Id == 0 ? "Create" : "Update";
        if (dto.Id == 0)
        {
            entity = new MaintenanceRequest { CreatedAt = DateTime.UtcNow };
            _context.MaintenanceRequests.Add(entity);
        }
        else
        {
            entity = await _context.MaintenanceRequests.FirstOrDefaultAsync(m => m.Id == dto.Id)
                ?? throw new InvalidOperationException("طلب الصيانة غير موجود.");
        }

        entity.VehicleId = dto.VehicleId;
        entity.MaintenanceTypeId = dto.MaintenanceTypeId;
        entity.ServiceProviderId = dto.ServiceProviderId;
        entity.RequestDate = ServiceHelpers.OrToday(dto.RequestDate);
        entity.CompletionDate = ServiceHelpers.OrNull(dto.CompletionDate);
        entity.Status = string.IsNullOrWhiteSpace(dto.Status) ? "Open" : dto.Status;
        entity.Description = ServiceHelpers.Clean(dto.Description);
        entity.EstimatedCost = dto.EstimatedCost;
        entity.ActualCost = dto.ActualCost;
        entity.WorkPerformed = ServiceHelpers.Clean(dto.WorkPerformed);
        entity.Notes = ServiceHelpers.Clean(dto.Notes);
        entity.DocumentUrl = ServiceHelpers.Clean(dto.DocumentUrl);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync(action, "Maintenance", entity.Id, null, entity.Status);
        await _notificationService.CreateNotificationAsync("Maintenance Saved", $"Maintenance request #{entity.Id} was saved.", "Info", "Maintenance", entity.Id);

        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.MaintenanceRequests.FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new InvalidOperationException("طلب الصيانة غير موجود.");

        _context.MaintenanceRequests.Remove(entity);
        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync("Delete", "Maintenance", entity.Id, entity.Status, null);
    }
}

public sealed class DriverService(FleetDbContext context, IAuditService auditService) : IDriverService
{
    private readonly FleetDbContext _context = context;
    private readonly IAuditService _auditService = auditService;

    public async Task<List<DriverDto>> GetAllAsync(string? search = null)
    {
        var query = _context.Drivers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(d => d.FullName.Contains(term) || d.LicenseNumber.Contains(term) || d.NationalId.Contains(term));
        }

        var items = await query.OrderBy(d => d.FullName).ToListAsync();
        return items.Select(d => d.ToDto()).ToList();
    }

    public async Task<DriverDto?> GetByIdAsync(int id)
    {
        var entity = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == id);
        return entity?.ToDto();
    }

    public async Task<DriverDto> SaveAsync(DriverFormDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
        {
            throw new InvalidOperationException("اسم السائق مطلوب.");
        }

        var duplicate = await _context.Drivers.FirstOrDefaultAsync(d => d.Id != dto.Id && d.LicenseNumber == dto.LicenseNumber.Trim());
        if (duplicate is not null)
        {
            throw new InvalidOperationException("رقم الرخصة مستخدم بالفعل.");
        }

        Driver entity;
        var action = dto.Id == 0 ? "Create" : "Update";
        if (dto.Id == 0)
        {
            entity = new Driver { CreatedAt = DateTime.UtcNow };
            _context.Drivers.Add(entity);
        }
        else
        {
            entity = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == dto.Id)
                ?? throw new InvalidOperationException("السائق غير موجود.");
        }

        entity.FullName = ServiceHelpers.Clean(dto.FullName);
        entity.NationalId = ServiceHelpers.Clean(dto.NationalId);
        entity.PhoneNumber = ServiceHelpers.Clean(dto.PhoneNumber);
        entity.Email = ServiceHelpers.Clean(dto.Email);
        entity.Address = ServiceHelpers.Clean(dto.Address);
        entity.DateOfBirth = dto.DateOfBirth == default ? null : dto.DateOfBirth;
        entity.LicenseNumber = ServiceHelpers.Clean(dto.LicenseNumber);
        entity.LicenseExpiryDate = dto.LicenseExpiryDate == default ? null : dto.LicenseExpiryDate;
        entity.LicenseType = ServiceHelpers.Clean(dto.LicenseType);
        entity.IsActive = dto.IsActive;
        entity.Notes = ServiceHelpers.Clean(dto.Notes);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync(action, "Driver", entity.Id, null, entity.FullName);
        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new InvalidOperationException("السائق غير موجود.");

        var hasDependents =
            await _context.Trips.AnyAsync(t => t.DriverId == id) ||
            await _context.Licenses.AnyAsync(l => l.DriverId == id);

        if (hasDependents)
        {
            throw new InvalidOperationException("لا يمكن حذف السائق لارتباطه برحلات أو رخص.");
        }

        _context.Drivers.Remove(entity);
        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync("Delete", "Driver", entity.Id, entity.FullName, null);
    }

    public async Task<List<DriverLookupDto>> GetLookupAsync() =>
        await _context.Drivers
            .OrderBy(d => d.FullName)
            .Select(d => new DriverLookupDto
            {
                Id = d.Id,
                FullName = d.FullName,
                LicenseNumber = d.LicenseNumber
            })
            .ToListAsync();
}

public sealed class EmployeeService(FleetDbContext context, IAuditService auditService) : IEmployeeService
{
    private readonly FleetDbContext _context = context;
    private readonly IAuditService _auditService = auditService;

    public async Task<List<EmployeeDto>> GetAllAsync(string? search = null)
    {
        var query = _context.Employees.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(e => e.FullName.Contains(term) || e.EmployeeId.Contains(term) || e.Department.Contains(term));
        }

        var items = await query.OrderBy(e => e.FullName).ToListAsync();
        return items.Select(e => e.ToDto()).ToList();
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var entity = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
        return entity?.ToDto();
    }

    public async Task<EmployeeDto> SaveAsync(EmployeeFormDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
        {
            throw new InvalidOperationException("اسم الموظف مطلوب.");
        }

        if (!string.IsNullOrWhiteSpace(dto.EmployeeId))
        {
            var duplicate = await _context.Employees.FirstOrDefaultAsync(e => e.Id != dto.Id && e.EmployeeId == dto.EmployeeId.Trim());
            if (duplicate is not null)
            {
                throw new InvalidOperationException("الكود الوظيفي مستخدم بالفعل.");
            }
        }

        Employee entity;
        var action = dto.Id == 0 ? "Create" : "Update";
        if (dto.Id == 0)
        {
            entity = new Employee { CreatedAt = DateTime.UtcNow };
            _context.Employees.Add(entity);
        }
        else
        {
            entity = await _context.Employees.FirstOrDefaultAsync(e => e.Id == dto.Id)
                ?? throw new InvalidOperationException("الموظف غير موجود.");
        }

        entity.FullName = ServiceHelpers.Clean(dto.FullName);
        entity.EmployeeId = ServiceHelpers.Clean(dto.EmployeeId);
        entity.PhoneNumber = ServiceHelpers.Clean(dto.PhoneNumber);
        entity.Email = ServiceHelpers.Clean(dto.Email);
        entity.Department = ServiceHelpers.Clean(dto.Department);
        entity.Position = ServiceHelpers.Clean(dto.Position);
        entity.HireDate = dto.HireDate == default ? null : dto.HireDate;
        entity.TerminationDate = ServiceHelpers.OrNull(dto.TerminationDate);
        entity.Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status;
        entity.Notes = ServiceHelpers.Clean(dto.Notes);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync(action, "Employee", entity.Id, null, entity.FullName);
        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new InvalidOperationException("الموظف غير موجود.");

        var hasDependents =
            await _context.Custodies.AnyAsync(c => c.EmployeeId == id) ||
            await _context.Trips.AnyAsync(t => t.RequesterEmployeeId == id || t.SupervisorEmployeeId == id);

        if (hasDependents)
        {
            throw new InvalidOperationException("لا يمكن حذف الموظف لارتباطه بسجلات عهدة أو رحلات.");
        }

        _context.Employees.Remove(entity);
        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync("Delete", "Employee", entity.Id, entity.FullName, null);
    }

    public async Task<List<EmployeeLookupDto>> GetLookupAsync() =>
        await _context.Employees
            .OrderBy(e => e.FullName)
            .Select(e => new EmployeeLookupDto
            {
                Id = e.Id,
                FullName = e.FullName,
                Position = e.Position
            })
            .ToListAsync();
}

public sealed class TripService(FleetDbContext context, IAuditService auditService) : ITripService
{
    private readonly FleetDbContext _context = context;
    private readonly IAuditService _auditService = auditService;

    public async Task<List<TripDto>> GetAllAsync(int? vehicleId = null)
    {
        var query = _context.Trips
            .Include(t => t.Vehicle)
            .Include(t => t.Driver)
            .Include(t => t.RequesterEmployee)
            .Include(t => t.SupervisorEmployee)
            .AsQueryable();

        if (vehicleId.HasValue)
        {
            query = query.Where(t => t.VehicleId == vehicleId.Value);
        }

        var items = await query
            .OrderByDescending(t => t.StartDate)
            .ToListAsync();

        return items.Select(t => t.ToDto()).ToList();
    }

    public async Task<TripDto?> GetByIdAsync(int id)
    {
        var entity = await _context.Trips
            .Include(t => t.Vehicle)
            .Include(t => t.Driver)
            .Include(t => t.RequesterEmployee)
            .Include(t => t.SupervisorEmployee)
            .FirstOrDefaultAsync(t => t.Id == id);

        return entity?.ToDto();
    }

    public async Task<TripDto> SaveAsync(TripFormDto dto)
    {
        if (dto.VehicleId <= 0)
        {
            throw new InvalidOperationException("رقم السيارة مطلوب.");
        }

        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == dto.VehicleId)
            ?? throw new InvalidOperationException("المركبة غير موجودة.");

        if (!dto.DriverId.HasValue)
        {
            throw new InvalidOperationException("اسم السائق مطلوب.");
        }

        var requesterNameText = ServiceHelpers.Clean(dto.RequesterNameText);

        if (string.IsNullOrWhiteSpace(requesterNameText) && !dto.RequesterEmployeeId.HasValue)
        {
            throw new InvalidOperationException("طالب التشغيل مطلوب.");
        }

        if (!dto.SupervisorEmployeeId.HasValue)
        {
            throw new InvalidOperationException("المشرف مطلوب.");
        }

        if (string.IsNullOrWhiteSpace(dto.StartLocation))
        {
            throw new InvalidOperationException("منطقة التحرك مطلوبة.");
        }

        if (string.IsNullOrWhiteSpace(dto.EndLocation))
        {
            throw new InvalidOperationException("منطقة الوصول مطلوبة.");
        }

        if (string.IsNullOrWhiteSpace(dto.Purpose))
        {
            throw new InvalidOperationException("الغرض مطلوب.");
        }

        if (!await _context.Drivers.AnyAsync(d => d.Id == dto.DriverId.Value))
        {
            throw new InvalidOperationException("السائق غير موجود.");
        }

        if (dto.RequesterEmployeeId.HasValue &&
            !await _context.Employees.AnyAsync(e => e.Id == dto.RequesterEmployeeId.Value))
        {
            throw new InvalidOperationException("طالب التشغيل غير موجود.");
        }

        if (!await _context.Employees.AnyAsync(e => e.Id == dto.SupervisorEmployeeId.Value))
        {
            throw new InvalidOperationException("المشرف غير موجود.");
        }

        if (dto.EndDate.HasValue && dto.EndDate.Value < dto.StartDate)
        {
            throw new InvalidOperationException("تاريخ نهاية الرحلة لا يمكن أن يكون قبل البداية.");
        }

        var effectiveStartMileage = dto.StartMileage > 0 ? dto.StartMileage : vehicle.Mileage;
        if (dto.EndMileage.HasValue && dto.EndMileage.Value < effectiveStartMileage)
        {
            throw new InvalidOperationException("قراءة العداد النهائية لا يمكن أن تكون أقل من البداية.");
        }

        var hasVehicleOpenTrip = await _context.Trips.AnyAsync(t =>
            t.Id != dto.Id &&
            t.VehicleId == dto.VehicleId &&
            (t.EndDate == null ||
             t.Status == "Open" ||
             t.Status == "Planned" ||
             t.Status == "InProgress" ||
             t.Status == "InTrip" ||
             t.Status == "مفتوحة" ||
             t.Status == "جارية"));

        if (hasVehicleOpenTrip)
        {
            throw new InvalidOperationException($"لا يمكن بدء تشغيل جديد للعربية {vehicle.PlateNumber} لأن لديها تشغيلة مفتوحة. أغلق التشغيلة المفتوحة أو احذفها أولًا.");
        }

        var hasOpenMaintenanceForVehicle = await _context.MaintenanceRequests.AnyAsync(m =>
            m.VehicleId == dto.VehicleId &&
            (m.Status == "Open" ||
             m.Status == "InProgress" ||
             m.Status == "مفتوحة" ||
             m.Status == "جارية"));

        if (hasOpenMaintenanceForVehicle)
        {
            throw new InvalidOperationException($"العربية {vehicle.PlateNumber} عليها طلب صيانة مفتوح، ولا يمكن بدء تشغيلة قبل إغلاق الصيانة.");
        }

        if (ServiceHelpers.IsVehicleInactive(vehicle.Status) && dto.Id == 0)
        {
            throw new InvalidOperationException($"العربية {vehicle.PlateNumber} حالتها '{vehicle.Status}' وليست صالحة للتشغيل. عدل الحالة من شاشة المركبات أولًا.");
        }

        if (vehicle.RegistrationExpiryDate.HasValue && vehicle.RegistrationExpiryDate.Value.Date < DateTime.Today)
        {
            throw new InvalidOperationException($"لا يمكن تشغيل العربية {vehicle.PlateNumber} لأن الترخيص منتهي بتاريخ {vehicle.RegistrationExpiryDate:yyyy-MM-dd}.");
        }

        var hasActiveInsurance = await _context.Insurances.AnyAsync(i =>
            i.VehicleId == dto.VehicleId &&
            i.ExpiryDate.Date >= DateTime.Today);

        if (!hasActiveInsurance &&
            (string.IsNullOrWhiteSpace(vehicle.AccidentInsuranceDetails) ||
             string.IsNullOrWhiteSpace(vehicle.SocialInsuranceDetails)))
        {
            throw new InvalidOperationException($"لا يمكن تشغيل العربية {vehicle.PlateNumber} قبل تسجيل تأمين ساري أو استكمال بيانات التأمين في شاشة المركبات.");
        }

        if (dto.DriverId.HasValue)
        {
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == dto.DriverId.Value)
                ?? throw new InvalidOperationException("السائق غير موجود.");

            if (!driver.IsActive)
            {
                throw new InvalidOperationException("السائق غير مفعل.");
            }

            if (driver.LicenseExpiryDate.HasValue && driver.LicenseExpiryDate.Value.Date < DateTime.Today)
            {
                throw new InvalidOperationException("لا يمكن إسناد رحلة لسائق رخصته منتهية.");
            }
        }

        if (dto.DriverId.HasValue)
        {
            var hasDriverOpenTrip = await _context.Trips.AnyAsync(t =>
                t.Id != dto.Id &&
                t.DriverId == dto.DriverId &&
                (t.EndDate == null ||
                 t.Status == "Open" ||
                 t.Status == "Planned" ||
                 t.Status == "InProgress" ||
                 t.Status == "InTrip" ||
                 t.Status == "مفتوحة" ||
                 t.Status == "جارية"));

            if (hasDriverOpenTrip)
            {
                throw new InvalidOperationException("لا يمكن إسناد السائق لرحلة جديدة لوجود رحلة مفتوحة بالفعل.");
            }
        }

        Trip entity;
        var action = dto.Id == 0 ? "Create" : "Update";
        if (dto.Id == 0)
        {
            entity = new Trip { CreatedAt = DateTime.UtcNow };
            _context.Trips.Add(entity);
        }
        else
        {
            entity = await _context.Trips.FirstOrDefaultAsync(t => t.Id == dto.Id)
                ?? throw new InvalidOperationException("الرحلة غير موجودة.");
        }

        var isClosing = dto.EndDate.HasValue || string.Equals(dto.Status, "Closed", StringComparison.OrdinalIgnoreCase) || string.Equals(dto.Status, "مغلقة", StringComparison.OrdinalIgnoreCase);

        entity.VehicleId = dto.VehicleId;
        entity.DriverId = dto.DriverId;
        entity.RequesterEmployeeId = dto.RequesterEmployeeId;
        entity.RequesterNameText = requesterNameText;
        entity.SupervisorEmployeeId = dto.SupervisorEmployeeId;
        entity.StartDate = ServiceHelpers.OrToday(dto.StartDate);
        entity.EndDate = ServiceHelpers.OrNull(dto.EndDate);
        entity.StartLocation = ServiceHelpers.Clean(dto.StartLocation);
        entity.EndLocation = ServiceHelpers.Clean(dto.EndLocation);
        entity.StartMileage = effectiveStartMileage;
        entity.EndMileage = dto.EndMileage;
        entity.Distance = ServiceHelpers.Distance(effectiveStartMileage, dto.EndMileage, dto.Distance);
        entity.Purpose = ServiceHelpers.Clean(dto.Purpose);
        entity.Status = isClosing ? "Closed" : string.IsNullOrWhiteSpace(dto.Status) ? "Open" : dto.Status;
        entity.FuelConsumed = dto.FuelConsumed;
        entity.TripCost = dto.TripCost;
        entity.Notes = ServiceHelpers.Clean(dto.Notes);
        entity.UpdatedAt = DateTime.UtcNow;

        if (dto.EndMileage.HasValue && dto.EndMileage.Value > vehicle.Mileage)
        {
            vehicle.Mileage = dto.EndMileage.Value;
            vehicle.UpdatedAt = DateTime.UtcNow;
        }

        if (isClosing)
        {
            var hasOpenMaintenance = await _context.MaintenanceRequests.AnyAsync(m =>
                m.VehicleId == dto.VehicleId &&
                (m.Status == "Open" ||
                 m.Status == "InProgress" ||
                 m.Status == "مفتوحة" ||
                 m.Status == "جارية"));

            vehicle.Status = hasOpenMaintenance ? "Maintenance" : "Available";
        }
        else
        {
            vehicle.Status = "InTrip";
        }

        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync(action, "Trip", entity.Id, null, entity.Status);
        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Trips.FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new InvalidOperationException("الرحلة غير موجودة.");

        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == entity.VehicleId);
        var wasOpenTrip =
            entity.EndDate is null ||
            entity.Status == "Open" ||
            entity.Status == "Planned" ||
            entity.Status == "InProgress" ||
            entity.Status == "InTrip" ||
            entity.Status == "مفتوحة" ||
            entity.Status == "جارية";

        _context.Trips.Remove(entity);
        await _context.SaveChangesAsync();

        if (vehicle is not null && wasOpenTrip)
        {
            var hasAnotherOpenTrip = await _context.Trips.AnyAsync(t =>
                t.VehicleId == vehicle.Id &&
                (t.EndDate == null ||
                 t.Status == "Open" ||
                 t.Status == "Planned" ||
                 t.Status == "InProgress" ||
                 t.Status == "InTrip" ||
                 t.Status == "مفتوحة" ||
                 t.Status == "جارية"));

            if (!hasAnotherOpenTrip)
            {
                var hasOpenMaintenance = await _context.MaintenanceRequests.AnyAsync(m =>
                    m.VehicleId == vehicle.Id &&
                    (m.Status == "Open" ||
                     m.Status == "InProgress" ||
                     m.Status == "مفتوحة" ||
                     m.Status == "جارية"));

                vehicle.Status = hasOpenMaintenance ? "Maintenance" : "Available";
                vehicle.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        await _auditService.LogActionAsync("Delete", "Trip", entity.Id, entity.Status, null);
    }
}
