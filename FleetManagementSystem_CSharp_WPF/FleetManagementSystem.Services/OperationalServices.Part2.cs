using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Data;
using FleetManagementSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using ServiceProviderEntity = FleetManagementSystem.Data.Entities.ServiceProvider;

namespace FleetManagementSystem.Services;

public sealed class FuelService(FleetDbContext context, IAuditService auditService) : IFuelService
{
    private readonly FleetDbContext _context = context;
    private readonly IAuditService _auditService = auditService;

    public async Task<List<FuelTransactionDto>> GetAllAsync()
    {
        var items = await _context.FuelTransactions
            .Include(f => f.Vehicle)
            .Include(f => f.Trip)
            .OrderByDescending(f => f.TransactionDate)
            .ThenByDescending(f => f.Id)
            .ToListAsync();

        return items.Select(f => f.ToDto()).ToList();
    }

    public async Task<FuelTransactionDto?> GetByIdAsync(int id)
    {
        var entity = await _context.FuelTransactions
            .Include(f => f.Vehicle)
            .Include(f => f.Trip)
            .FirstOrDefaultAsync(f => f.Id == id);

        return entity?.ToDto();
    }

    public async Task<FuelTransactionDto> SaveAsync(FuelTransactionFormDto dto)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == dto.VehicleId)
            ?? throw new InvalidOperationException("المركبة غير موجودة.");

        Trip? trip = null;
        if (dto.TripId.HasValue)
        {
            trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == dto.TripId.Value)
                ?? throw new InvalidOperationException("التشغيلة المرتبطة بسجل البنزين غير موجودة.");

            if (trip.VehicleId != dto.VehicleId)
            {
                throw new InvalidOperationException("لا يمكن ربط سجل وقود بتشغيلة تخص عربية مختلفة.");
            }
        }

        if (dto.Quantity <= 0)
        {
            throw new InvalidOperationException("عدد لترات البنزين يجب أن يكون أكبر من صفر.");
        }

        if (dto.UnitPrice < 0)
        {
            throw new InvalidOperationException("سعر اللتر لا يمكن أن يكون سالبًا.");
        }

        if (dto.TotalCost < 0)
        {
            throw new InvalidOperationException("إجمالي تكلفة البنزين لا يمكن أن يكون سالبًا.");
        }

        if (dto.Odometer > 0)
        {
            var previousOdometers = await _context.FuelTransactions
                .Where(f => f.VehicleId == dto.VehicleId && f.Id != dto.Id)
                .Select(f => f.Odometer)
                .ToListAsync();

            var lastOdometer = previousOdometers.Max();

            if (lastOdometer.HasValue && dto.Odometer < lastOdometer.Value)
            {
                throw new InvalidOperationException($"عداد التموين ({dto.Odometer:0.##}) أقل من آخر عداد بنزين مسجل لنفس العربية ({lastOdometer.Value:0.##}). راجع قراءة العداد قبل الحفظ.");
            }
        }

        FuelTransaction entity;
        var action = dto.Id == 0 ? "Create" : "Update";
        if (dto.Id == 0)
        {
            entity = new FuelTransaction { CreatedAt = DateTime.UtcNow };
            _context.FuelTransactions.Add(entity);
        }
        else
        {
            entity = await _context.FuelTransactions.FirstOrDefaultAsync(f => f.Id == dto.Id)
                ?? throw new InvalidOperationException("سجل البنزين غير موجود.");
        }

        entity.VehicleId = dto.VehicleId;
        entity.TripId = dto.TripId;
        entity.TransactionDate = ServiceHelpers.OrToday(dto.TransactionDate);
        entity.FuelType = string.IsNullOrWhiteSpace(dto.FuelType) ? "Gasoline" : dto.FuelType;
        entity.Quantity = dto.Quantity;
        entity.UnitPrice = dto.UnitPrice;
        entity.TotalCost = dto.TotalCost > 0 ? dto.TotalCost : dto.Quantity * dto.UnitPrice;
        entity.FuelStation = ServiceHelpers.Clean(dto.FuelStation);
        entity.Odometer = dto.Odometer <= 0 ? null : dto.Odometer;
        entity.PaidFromTreasury = dto.PaidFromTreasury;
        entity.Notes = ServiceHelpers.Clean(dto.Notes);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (entity.PaidFromTreasury)
        {
            var treasury = entity.TreasuryTransactionId.HasValue
                ? await _context.TreasuryTransactions.FirstOrDefaultAsync(t => t.Id == entity.TreasuryTransactionId.Value)
                : new TreasuryTransaction { CreatedAt = DateTime.UtcNow };

            if (treasury is null)
            {
                treasury = new TreasuryTransaction { CreatedAt = DateTime.UtcNow };
            }

            if (treasury.Id == 0)
            {
                _context.TreasuryTransactions.Add(treasury);
            }

            treasury.TransactionDate = entity.TransactionDate;
            treasury.TransactionType = "صرف";
            treasury.Amount = entity.TotalCost;
            treasury.Description = $"صرف وقود للمركبة {vehicle.PlateNumber}";
            treasury.RelatedEntityType = "Fuel";
            treasury.RelatedEntityId = entity.Id;
            treasury.PaymentMethod = "نقدي";
            treasury.Notes = entity.Notes;
            treasury.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            entity.TreasuryTransactionId = treasury.Id;
            await _context.SaveChangesAsync();
        }
        else if (entity.TreasuryTransactionId.HasValue)
        {
            var treasury = await _context.TreasuryTransactions.FirstOrDefaultAsync(t => t.Id == entity.TreasuryTransactionId.Value);
            if (treasury is not null)
            {
                _context.TreasuryTransactions.Remove(treasury);
            }

            entity.TreasuryTransactionId = null;
            await _context.SaveChangesAsync();
        }

        await _auditService.LogActionAsync(action, "Fuel", entity.Id, null, entity.TotalCost.ToString("0.##"));
        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.FuelTransactions.FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new InvalidOperationException("سجل البنزين غير موجود.");

        if (entity.TreasuryTransactionId.HasValue)
        {
            var treasury = await _context.TreasuryTransactions.FirstOrDefaultAsync(t => t.Id == entity.TreasuryTransactionId.Value);
            if (treasury is not null)
            {
                _context.TreasuryTransactions.Remove(treasury);
            }
        }

        _context.FuelTransactions.Remove(entity);
        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync("Delete", "Fuel", entity.Id, entity.TotalCost.ToString("0.##"), null);
    }
}

public sealed class ExpenseService(FleetDbContext context, IAuditService auditService) : IExpenseService
{
    private readonly FleetDbContext _context = context;
    private readonly IAuditService _auditService = auditService;

    public async Task<List<ExpenseDto>> GetAllAsync()
    {
        var items = await _context.Expenses
            .Include(e => e.Vehicle)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();

        return items.Select(e => e.ToDto()).ToList();
    }

    public async Task<ExpenseDto?> GetByIdAsync(int id)
    {
        var entity = await _context.Expenses
            .Include(e => e.Vehicle)
            .FirstOrDefaultAsync(e => e.Id == id);

        return entity?.ToDto();
    }

    public async Task<ExpenseDto> SaveAsync(ExpenseFormDto dto)
    {
        if (!await _context.Vehicles.AnyAsync(v => v.Id == dto.VehicleId))
        {
            throw new InvalidOperationException("المركبة غير موجودة.");
        }

        if (dto.Amount < 0)
        {
            throw new InvalidOperationException("قيمة المصروف لا يمكن أن تكون سالبة.");
        }

        Expense entity;
        var action = dto.Id == 0 ? "Create" : "Update";
        if (dto.Id == 0)
        {
            entity = new Expense { CreatedAt = DateTime.UtcNow };
            _context.Expenses.Add(entity);
        }
        else
        {
            entity = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == dto.Id)
                ?? throw new InvalidOperationException("المصروف غير موجود.");
        }

        entity.VehicleId = dto.VehicleId;
        entity.ExpenseDate = ServiceHelpers.OrToday(dto.ExpenseDate);
        entity.Category = ServiceHelpers.Clean(dto.Category);
        entity.Amount = dto.Amount;
        entity.Description = ServiceHelpers.Clean(dto.Description);
        entity.Vendor = ServiceHelpers.Clean(dto.Vendor);
        entity.ReceiptUrl = ServiceHelpers.Clean(dto.ReceiptUrl);
        entity.PaidFromTreasury = dto.PaidFromTreasury;
        entity.Status = string.IsNullOrWhiteSpace(dto.Status) ? "Pending" : dto.Status;
        entity.Notes = ServiceHelpers.Clean(dto.Notes);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (entity.PaidFromTreasury)
        {
            var treasury = entity.TreasuryTransactionId.HasValue
                ? await _context.TreasuryTransactions.FirstOrDefaultAsync(t => t.Id == entity.TreasuryTransactionId.Value)
                : new TreasuryTransaction { CreatedAt = DateTime.UtcNow };

            if (treasury is null)
            {
                treasury = new TreasuryTransaction { CreatedAt = DateTime.UtcNow };
            }

            if (treasury.Id == 0)
            {
                _context.TreasuryTransactions.Add(treasury);
            }

            treasury.TransactionDate = entity.ExpenseDate;
            treasury.TransactionType = "صرف";
            treasury.Amount = entity.Amount;
            treasury.Description = $"مصروف {entity.Category} للمركبة #{entity.VehicleId}";
            treasury.RelatedEntityType = "Expense";
            treasury.RelatedEntityId = entity.Id;
            treasury.PaymentMethod = "نقدي";
            treasury.Notes = entity.Notes;
            treasury.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            entity.TreasuryTransactionId = treasury.Id;
            await _context.SaveChangesAsync();
        }
        else if (entity.TreasuryTransactionId.HasValue)
        {
            var treasury = await _context.TreasuryTransactions.FirstOrDefaultAsync(t => t.Id == entity.TreasuryTransactionId.Value);
            if (treasury is not null)
            {
                _context.TreasuryTransactions.Remove(treasury);
            }

            entity.TreasuryTransactionId = null;
            await _context.SaveChangesAsync();
        }

        await _auditService.LogActionAsync(action, "Expense", entity.Id, null, entity.Amount.ToString("0.##"));
        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new InvalidOperationException("المصروف غير موجود.");

        if (entity.TreasuryTransactionId.HasValue)
        {
            var treasury = await _context.TreasuryTransactions.FirstOrDefaultAsync(t => t.Id == entity.TreasuryTransactionId.Value);
            if (treasury is not null)
            {
                _context.TreasuryTransactions.Remove(treasury);
            }
        }

        _context.Expenses.Remove(entity);
        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync("Delete", "Expense", entity.Id, entity.Amount.ToString("0.##"), null);
    }
}

public sealed class OilChangeService(FleetDbContext context, IAuditService auditService) : IOilChangeService
{
    private readonly FleetDbContext _context = context;
    private readonly IAuditService _auditService = auditService;

    public async Task<List<OilChangeDto>> GetAllAsync()
    {
        var items = await _context.OilChanges
            .Include(x => x.Vehicle)
            .OrderByDescending(x => x.ChangeDate)
            .ThenByDescending(x => x.Id)
            .ToListAsync();

        return items.Select(x => x.ToDto()).ToList();
    }

    public async Task<OilChangeDto?> GetByIdAsync(int id)
    {
        var entity = await _context.OilChanges
            .Include(x => x.Vehicle)
            .FirstOrDefaultAsync(x => x.Id == id);

        return entity?.ToDto();
    }

    public async Task<OilChangeDto> SaveAsync(OilChangeFormDto dto)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == dto.VehicleId)
            ?? throw new InvalidOperationException("المركبة غير موجودة.");

        if (vehicle.OilChangeIntervalKm <= 0)
        {
            throw new InvalidOperationException("قيمة تغيير الزيت كل كام كم غير مسجلة للعربية. سجل عدد الكيلومترات بين كل تغيير زيت من بيانات العربية أولًا.");
        }

        if (dto.OdometerAtChange < 0)
        {
            throw new InvalidOperationException("قراءة العداد لا يمكن أن تكون سالبة.");
        }

        var currentOdometer = dto.CurrentOdometer > 0
            ? dto.CurrentOdometer
            : Math.Max(vehicle.Mileage, dto.OdometerAtChange);
        var currentOdometerDate = dto.CurrentOdometerDate?.Date ?? DateTime.Today;

        if (currentOdometer < 0)
        {
            throw new InvalidOperationException("قراءة العداد اليوم لا يمكن أن تكون سالبة.");
        }

        if (currentOdometer < vehicle.Mileage)
        {
            throw new InvalidOperationException(
                $"قراءة العداد اليوم ({currentOdometer:0.##}) أقل من آخر عداد محفوظ للعربية ({vehicle.Mileage:0.##}). راجع قراءة العداد قبل الحفظ.");
        }

        var previousOilChanges = await _context.OilChanges
            .Where(x => x.VehicleId == dto.VehicleId && x.Id != dto.Id && x.IsOilChanged)
            .Select(x => new { x.Id, x.ChangeDate, x.OdometerAtChange, x.NextOilChangeOdometer })
            .ToListAsync();

        var latestOilChange = previousOilChanges
            .OrderByDescending(x => x.OdometerAtChange)
            .ThenByDescending(x => x.ChangeDate)
            .FirstOrDefault();

        if (!dto.IsOilChanged && latestOilChange is null)
        {
            throw new InvalidOperationException("لا يمكن تسجيل متابعة زيت يومية قبل تسجيل أول تغيير زيت فعلي للعربية.");
        }

        if (dto.IsOilChanged)
        {
            if (currentOdometer < dto.OdometerAtChange)
            {
                throw new InvalidOperationException(
                    $"قراءة العداد اليوم ({currentOdometer:0.##}) لا يمكن أن تكون أقل من عداد تغيير الزيت ({dto.OdometerAtChange:0.##}).");
            }

            if (string.IsNullOrWhiteSpace(dto.OilType))
            {
                throw new InvalidOperationException("نوع الزيت مطلوب عند تسجيل تغيير زيت فعلي.");
            }

            if (dto.Quantity <= 0)
            {
                throw new InvalidOperationException("كمية الزيت يجب أن تكون أكبر من صفر لتر عند تسجيل تغيير زيت فعلي.");
            }

            if (dto.Cost < 0)
            {
                throw new InvalidOperationException("تكلفة الزيت لا يمكن أن تكون سالبة.");
            }
        }
        else if (dto.Cost < 0)
        {
            throw new InvalidOperationException("تكلفة الزيت لا يمكن أن تكون سالبة.");
        }

        var latestOilOdometer = previousOilChanges.Count == 0
            ? null
            : previousOilChanges.Max(x => (decimal?)x.OdometerAtChange);

        if (dto.IsOilChanged && latestOilOdometer.HasValue && dto.OdometerAtChange < latestOilOdometer.Value)
        {
            throw new InvalidOperationException($"قراءة عداد تغيير الزيت لا يمكن أن تكون أقل من آخر تغيير زيت مسجل لنفس العربية عند {latestOilOdometer.Value:0} كم.");
        }

        var odometerAtChange = dto.IsOilChanged
            ? dto.OdometerAtChange
            : latestOilChange!.OdometerAtChange;
        var nextOilChangeOdometer = odometerAtChange + vehicle.OilChangeIntervalKm;

        OilChange entity;
        var action = dto.Id == 0 ? "Create" : "Update";
        if (dto.Id == 0)
        {
            entity = new OilChange { CreatedAt = DateTime.UtcNow };
            _context.OilChanges.Add(entity);
        }
        else
        {
            entity = await _context.OilChanges.FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new InvalidOperationException("سجل تغيير الزيت غير موجود.");
        }

        entity.VehicleId = dto.VehicleId;
        entity.ChangeDate = ServiceHelpers.OrToday(dto.ChangeDate);
        entity.OdometerAtChange = odometerAtChange;
        entity.IsOilChanged = dto.IsOilChanged;
        entity.ServiceItems = dto.IsOilChanged ? NormalizeOilServiceItems(dto.ServiceItems) : string.Empty;
        entity.OilType = dto.IsOilChanged ? ServiceHelpers.Clean(dto.OilType) : string.Empty;
        entity.Quantity = dto.IsOilChanged ? dto.Quantity : 0;
        entity.Cost = dto.IsOilChanged ? dto.Cost : 0;
        entity.NextOilChangeOdometer = nextOilChangeOdometer;
        entity.CurrentOdometer = currentOdometer;
        entity.CurrentOdometerDate = currentOdometerDate;
        var effectiveVehicleMileage = Math.Max(vehicle.Mileage, currentOdometer);
        var remainingKm = nextOilChangeOdometer - effectiveVehicleMileage;
        entity.Status = remainingKm < 0
            ? "Overdue"
            : remainingKm <= ServiceHelpers.DefaultOilAlertThresholdKm
                ? "DueSoon"
                : dto.IsOilChanged ? "Completed" : "DailyCheck";
        entity.Notes = ServiceHelpers.Clean(dto.Notes);
        entity.UpdatedAt = DateTime.UtcNow;

        if (effectiveVehicleMileage > vehicle.Mileage)
        {
            vehicle.Mileage = effectiveVehicleMileage;
            vehicle.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync(action, "OilChange", entity.Id, null, entity.Status);
        return (await GetByIdAsync(entity.Id))!;
    }

    private static string NormalizeOilServiceItems(string? serviceItems)
    {
        var value = ServiceHelpers.Clean(serviceItems);
        if (string.IsNullOrWhiteSpace(value))
        {
            return "زيت فقط";
        }

        return value.Contains("فلتر", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("filter", StringComparison.OrdinalIgnoreCase)
            ? "زيت وفلتر"
            : "زيت فقط";
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.OilChanges.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException("سجل تغيير الزيت غير موجود.");

        _context.OilChanges.Remove(entity);
        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync("Delete", "OilChange", entity.Id, entity.OilType, null);
    }
}

public sealed class TreasuryService(FleetDbContext context, IAuditService auditService) : ITreasuryService
{
    private readonly FleetDbContext _context = context;
    private readonly IAuditService _auditService = auditService;

    public async Task<List<TreasuryTransactionDto>> GetAllAsync()
    {
        var items = await _context.TreasuryTransactions
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.Id)
            .ToListAsync();

        return items.Select(x => x.ToDto()).ToList();
    }

    public async Task<TreasuryTransactionDto?> GetByIdAsync(int id)
    {
        var entity = await _context.TreasuryTransactions.FirstOrDefaultAsync(x => x.Id == id);
        return entity?.ToDto();
    }

    public async Task<TreasuryTransactionDto> SaveAsync(TreasuryTransactionFormDto dto)
    {
        if (dto.Amount < 0)
        {
            throw new InvalidOperationException("المبلغ لا يمكن أن يكون سالبًا.");
        }

        TreasuryTransaction entity;
        var action = dto.Id == 0 ? "Create" : "Update";
        if (dto.Id == 0)
        {
            entity = new TreasuryTransaction { CreatedAt = DateTime.UtcNow };
            _context.TreasuryTransactions.Add(entity);
        }
        else
        {
            entity = await _context.TreasuryTransactions.FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new InvalidOperationException("حركة الخزينة غير موجودة.");
        }

        entity.TransactionDate = ServiceHelpers.OrToday(dto.TransactionDate);
        entity.TransactionType = string.IsNullOrWhiteSpace(dto.TransactionType)
            ? "صرف"
            : ServiceHelpers.TreasuryTypeDisplay(dto.TransactionType);
        entity.Amount = dto.Amount;
        entity.Description = ServiceHelpers.Clean(dto.Description);
        entity.RelatedEntityType = ServiceHelpers.Clean(dto.RelatedEntityType);
        entity.RelatedEntityId = dto.RelatedEntityId;
        entity.PaymentMethod = string.IsNullOrWhiteSpace(dto.PaymentMethod)
            ? "نقدي"
            : ServiceHelpers.PaymentMethodDisplay(dto.PaymentMethod);
        entity.Notes = ServiceHelpers.Clean(dto.Notes);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync(action, "Treasury", entity.Id, null, entity.TransactionType);
        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.TreasuryTransactions.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException("حركة الخزينة غير موجودة.");

        var linkedFuel = await _context.FuelTransactions.AnyAsync(f => f.TreasuryTransactionId == id);
        var linkedExpense = await _context.Expenses.AnyAsync(e => e.TreasuryTransactionId == id);
        if (linkedFuel || linkedExpense)
        {
            throw new InvalidOperationException("لا يمكن حذف حركة خزينة مرتبطة بوقود أو مصروف.");
        }

        _context.TreasuryTransactions.Remove(entity);
        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync("Delete", "Treasury", entity.Id, entity.Description, null);
    }

    public async Task<decimal> GetCurrentBalanceAsync()
    {
        var entries = await _context.TreasuryTransactions
            .Select(x => new { x.TransactionType, x.Amount })
            .ToListAsync();

        return entries.Sum(x => ServiceHelpers.IsTreasuryIncome(x.TransactionType) ? x.Amount : -x.Amount);
    }
}

public sealed class LicenseService(FleetDbContext context, IAuditService auditService) : ILicenseService
{
    private readonly FleetDbContext _context = context;
    private readonly IAuditService _auditService = auditService;

    public async Task<List<LicenseDto>> GetAllAsync()
    {
        var items = await _context.Licenses
            .Include(l => l.Driver)
            .OrderByDescending(l => l.ExpiryDate)
            .ToListAsync();

        return items.Select(l => l.ToDto()).ToList();
    }

    public async Task<LicenseDto?> GetByIdAsync(int id)
    {
        var entity = await _context.Licenses
            .Include(l => l.Driver)
            .FirstOrDefaultAsync(l => l.Id == id);

        return entity?.ToDto();
    }

    public async Task<LicenseDto> SaveAsync(LicenseFormDto dto)
    {
        if (!await _context.Drivers.AnyAsync(d => d.Id == dto.DriverId))
        {
            throw new InvalidOperationException("السائق غير موجود.");
        }

        if (dto.ExpiryDate.Date < dto.IssueDate.Date)
        {
            throw new InvalidOperationException("تاريخ انتهاء الرخصة يجب أن يكون بعد تاريخ البداية.");
        }

        var duplicate = await _context.Licenses.FirstOrDefaultAsync(l => l.Id != dto.Id && l.LicenseNumber == dto.LicenseNumber.Trim());
        if (duplicate is not null)
        {
            throw new InvalidOperationException("رقم الرخصة مستخدم بالفعل.");
        }

        License entity;
        var action = dto.Id == 0 ? "Create" : "Update";
        if (dto.Id == 0)
        {
            entity = new License { CreatedAt = DateTime.UtcNow };
            _context.Licenses.Add(entity);
        }
        else
        {
            entity = await _context.Licenses.FirstOrDefaultAsync(l => l.Id == dto.Id)
                ?? throw new InvalidOperationException("الرخصة غير موجودة.");
        }

        entity.DriverId = dto.DriverId;
        entity.LicenseNumber = ServiceHelpers.Clean(dto.LicenseNumber);
        entity.LicenseType = ServiceHelpers.Clean(dto.LicenseType);
        entity.IssueDate = ServiceHelpers.OrToday(dto.IssueDate);
        entity.ExpiryDate = ServiceHelpers.OrToday(dto.ExpiryDate);
        entity.IssuingAuthority = ServiceHelpers.Clean(dto.IssuingAuthority);
        entity.Status = ServiceHelpers.LicenseStatus(entity.ExpiryDate);
        entity.Notes = ServiceHelpers.Clean(dto.Notes);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync(action, "License", entity.Id, null, entity.LicenseNumber);
        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Licenses.FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new InvalidOperationException("الرخصة غير موجودة.");

        _context.Licenses.Remove(entity);
        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync("Delete", "License", entity.Id, entity.LicenseNumber, null);
    }
}

public sealed class InsuranceService(FleetDbContext context, IAuditService auditService) : IInsuranceService
{
    private readonly FleetDbContext _context = context;
    private readonly IAuditService _auditService = auditService;

    public async Task<List<InsuranceDto>> GetAllAsync()
    {
        var items = await _context.Insurances
            .Include(i => i.Vehicle)
            .OrderByDescending(i => i.ExpiryDate)
            .ToListAsync();

        return items.Select(i => i.ToDto()).ToList();
    }

    public async Task<InsuranceDto?> GetByIdAsync(int id)
    {
        var entity = await _context.Insurances
            .Include(i => i.Vehicle)
            .FirstOrDefaultAsync(i => i.Id == id);

        return entity?.ToDto();
    }

    public async Task<InsuranceDto> SaveAsync(InsuranceFormDto dto)
    {
        if (!await _context.Vehicles.AnyAsync(v => v.Id == dto.VehicleId))
        {
            throw new InvalidOperationException("المركبة غير موجودة.");
        }

        if (string.IsNullOrWhiteSpace(dto.PolicyNumber))
        {
            throw new InvalidOperationException("رقم وثيقة التأمين مطلوب.");
        }

        if (string.IsNullOrWhiteSpace(dto.InsuranceCompany))
        {
            throw new InvalidOperationException("شركة التأمين مطلوبة.");
        }

        if (string.IsNullOrWhiteSpace(dto.PolicyType))
        {
            throw new InvalidOperationException("نوع وثيقة التأمين مطلوب.");
        }

        if (dto.ExpiryDate.Date < dto.StartDate.Date)
        {
            throw new InvalidOperationException("تاريخ انتهاء التأمين يجب أن يكون بعد تاريخ البداية.");
        }

        if (dto.PremiumAmount < 0 || dto.CoverageAmount < 0)
        {
            throw new InvalidOperationException("قيم التأمين لا يمكن أن تكون سالبة.");
        }

        Insurance entity;
        var action = dto.Id == 0 ? "Create" : "Update";
        if (dto.Id == 0)
        {
            entity = new Insurance { CreatedAt = DateTime.UtcNow };
            _context.Insurances.Add(entity);
        }
        else
        {
            entity = await _context.Insurances.FirstOrDefaultAsync(i => i.Id == dto.Id)
                ?? throw new InvalidOperationException("وثيقة التأمين غير موجودة.");
        }

        entity.VehicleId = dto.VehicleId;
        entity.PolicyNumber = ServiceHelpers.Clean(dto.PolicyNumber);
        entity.InsuranceCompany = ServiceHelpers.Clean(dto.InsuranceCompany);
        entity.PolicyType = ServiceHelpers.Clean(dto.PolicyType);
        entity.StartDate = ServiceHelpers.OrToday(dto.StartDate);
        entity.ExpiryDate = ServiceHelpers.OrToday(dto.ExpiryDate);
        entity.PremiumAmount = dto.PremiumAmount;
        entity.CoverageAmount = dto.CoverageAmount;
        entity.CoverageDetails = ServiceHelpers.Clean(dto.CoverageDetails);
        entity.AgentName = ServiceHelpers.Clean(dto.AgentName);
        entity.AgentPhoneNumber = ServiceHelpers.Clean(dto.AgentPhoneNumber);
        entity.Status = ServiceHelpers.InsuranceStatus(entity.ExpiryDate);
        entity.DocumentUrl = ServiceHelpers.Clean(dto.DocumentUrl);
        entity.Notes = ServiceHelpers.Clean(dto.Notes);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync(action, "Insurance", entity.Id, null, entity.PolicyNumber);
        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Insurances.FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new InvalidOperationException("وثيقة التأمين غير موجودة.");

        _context.Insurances.Remove(entity);
        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync("Delete", "Insurance", entity.Id, entity.PolicyNumber, null);
    }
}

public sealed class CustodyService(FleetDbContext context, IAuditService auditService) : ICustodyService
{
    private readonly FleetDbContext _context = context;
    private readonly IAuditService _auditService = auditService;

    public async Task<List<CustodyDto>> GetAllAsync()
    {
        var items = await _context.Custodies
            .Include(c => c.Vehicle)
            .Include(c => c.Employee)
            .OrderByDescending(c => c.HandoverDate)
            .ToListAsync();

        return items.Select(c => c.ToDto()).ToList();
    }

    public async Task<CustodyDto?> GetByIdAsync(int id)
    {
        var entity = await _context.Custodies
            .Include(c => c.Vehicle)
            .Include(c => c.Employee)
            .FirstOrDefaultAsync(c => c.Id == id);

        return entity?.ToDto();
    }

    public async Task<CustodyDto> SaveAsync(CustodyFormDto dto)
    {
        if (!await _context.Vehicles.AnyAsync(v => v.Id == dto.VehicleId))
        {
            throw new InvalidOperationException("المركبة غير موجودة.");
        }

        Custody entity;
        var action = dto.Id == 0 ? "Create" : "Update";
        if (dto.Id == 0)
        {
            entity = new Custody { CreatedAt = DateTime.UtcNow };
            _context.Custodies.Add(entity);
        }
        else
        {
            entity = await _context.Custodies.FirstOrDefaultAsync(c => c.Id == dto.Id)
                ?? throw new InvalidOperationException("العهدة غير موجودة.");
        }

        entity.VehicleId = dto.VehicleId;
        entity.CustodyNumber = ServiceHelpers.Clean(dto.CustodyNumber);
        entity.CustodianName = ServiceHelpers.Clean(dto.CustodianName);
        entity.CustodianPosition = ServiceHelpers.Clean(dto.CustodianPosition);
        entity.HandoverDate = ServiceHelpers.OrToday(dto.HandoverDate);
        entity.ReturnDate = ServiceHelpers.OrNull(dto.ReturnDate);
        entity.Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status;
        entity.VehicleConditionRating = dto.VehicleConditionRating <= 0 ? 5 : dto.VehicleConditionRating;
        entity.Notes = ServiceHelpers.Clean(dto.Notes);
        entity.DocumentUrl = ServiceHelpers.Clean(dto.DocumentUrl);
        entity.UpdatedAt = DateTime.UtcNow;

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.FullName == entity.CustodianName || e.EmployeeId == entity.CustodianName);
        entity.EmployeeId = employee?.Id;

        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync(action, "Custody", entity.Id, null, entity.CustodyNumber);
        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Custodies.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new InvalidOperationException("العهدة غير موجودة.");

        _context.Custodies.Remove(entity);
        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync("Delete", "Custody", entity.Id, entity.CustodyNumber, null);
    }
}

public sealed class MasterDataService(FleetDbContext context, IAuditService auditService) : IMasterDataService
{
    private readonly FleetDbContext _context = context;
    private readonly IAuditService _auditService = auditService;

    public async Task<List<VehicleTypeDto>> GetVehicleTypesAsync()
    {
        var items = await _context.VehicleTypes.OrderBy(x => x.Name).ToListAsync();
        return items.Select(x => x.ToDto()).ToList();
    }

    public async Task<VehicleTypeDto> SaveVehicleTypeAsync(VehicleTypeDto dto)
    {
        VehicleType entity;
        var action = dto.Id == 0 ? "Create" : "Update";
        if (dto.Id == 0)
        {
            entity = new VehicleType { CreatedAt = DateTime.UtcNow };
            _context.VehicleTypes.Add(entity);
        }
        else
        {
            entity = await _context.VehicleTypes.FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new InvalidOperationException("نوع المركبة غير موجود.");
        }

        entity.Name = ServiceHelpers.Clean(dto.Name);
        entity.NameEn = ServiceHelpers.Clean(dto.NameEn);
        entity.Description = ServiceHelpers.Clean(dto.Description);
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync(action, "VehicleType", entity.Id, null, entity.Name);
        return entity.ToDto();
    }

    public async Task DeleteVehicleTypeAsync(int id)
    {
        if (await _context.Vehicles.AnyAsync(v => v.VehicleTypeId == id))
        {
            throw new InvalidOperationException("لا يمكن حذف نوع مركبة مستخدم بالفعل.");
        }

        var entity = await _context.VehicleTypes.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException("نوع المركبة غير موجود.");

        _context.VehicleTypes.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ContractStatusDto>> GetContractStatusesAsync()
    {
        var items = await _context.ContractStatuses.OrderBy(x => x.Name).ToListAsync();
        return items.Select(x => x.ToDto()).ToList();
    }

    public async Task<ContractStatusDto> SaveContractStatusAsync(ContractStatusDto dto)
    {
        ContractStatus entity;
        var action = dto.Id == 0 ? "Create" : "Update";
        if (dto.Id == 0)
        {
            entity = new ContractStatus { CreatedAt = DateTime.UtcNow };
            _context.ContractStatuses.Add(entity);
        }
        else
        {
            entity = await _context.ContractStatuses.FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new InvalidOperationException("حالة العقد غير موجودة.");
        }

        entity.Name = ServiceHelpers.Clean(dto.Name);
        entity.NameEn = ServiceHelpers.Clean(dto.NameEn);
        entity.Description = ServiceHelpers.Clean(dto.Description);
        entity.Color = string.IsNullOrWhiteSpace(dto.Color) ? "#1F2937" : dto.Color;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync(action, "ContractStatus", entity.Id, null, entity.Name);
        return entity.ToDto();
    }

    public async Task DeleteContractStatusAsync(int id)
    {
        if (await _context.Contracts.AnyAsync(c => c.ContractStatusId == id))
        {
            throw new InvalidOperationException("لا يمكن حذف حالة عقد مستخدمة بالفعل.");
        }

        var entity = await _context.ContractStatuses.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException("حالة العقد غير موجودة.");

        _context.ContractStatuses.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<MaintenanceTypeDto>> GetMaintenanceTypesAsync()
    {
        var items = await _context.MaintenanceTypes.OrderBy(x => x.Name).ToListAsync();
        return items.Select(x => x.ToDto()).ToList();
    }

    public async Task<MaintenanceTypeDto> SaveMaintenanceTypeAsync(MaintenanceTypeDto dto)
    {
        MaintenanceType entity;
        var action = dto.Id == 0 ? "Create" : "Update";
        if (dto.Id == 0)
        {
            entity = new MaintenanceType { CreatedAt = DateTime.UtcNow };
            _context.MaintenanceTypes.Add(entity);
        }
        else
        {
            entity = await _context.MaintenanceTypes.FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new InvalidOperationException("نوع الصيانة غير موجود.");
        }

        entity.Name = ServiceHelpers.Clean(dto.Name);
        entity.NameEn = ServiceHelpers.Clean(dto.NameEn);
        entity.Description = ServiceHelpers.Clean(dto.Description);
        entity.EstimatedCost = dto.EstimatedCost;
        entity.EstimatedDurationDays = dto.EstimatedDurationDays;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync(action, "MaintenanceType", entity.Id, null, entity.Name);
        return entity.ToDto();
    }

    public async Task DeleteMaintenanceTypeAsync(int id)
    {
        if (await _context.MaintenanceRequests.AnyAsync(m => m.MaintenanceTypeId == id))
        {
            throw new InvalidOperationException("لا يمكن حذف نوع صيانة مستخدم بالفعل.");
        }

        var entity = await _context.MaintenanceTypes.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException("نوع الصيانة غير موجود.");

        _context.MaintenanceTypes.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ServiceProviderDto>> GetServiceProvidersAsync()
    {
        var items = await _context.ServiceProviders.OrderBy(x => x.Name).ToListAsync();
        return items.Select(x => x.ToDto()).ToList();
    }

    public async Task<ServiceProviderDto> SaveServiceProviderAsync(ServiceProviderDto dto)
    {
        ServiceProviderEntity entity;
        var action = dto.Id == 0 ? "Create" : "Update";
        if (dto.Id == 0)
        {
            entity = new ServiceProviderEntity { CreatedAt = DateTime.UtcNow };
            _context.ServiceProviders.Add(entity);
        }
        else
        {
            entity = await _context.ServiceProviders.FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new InvalidOperationException("مزود الخدمة غير موجود.");
        }

        entity.Name = ServiceHelpers.Clean(dto.Name);
        entity.NameEn = ServiceHelpers.Clean(dto.NameEn);
        entity.ContactPerson = ServiceHelpers.Clean(dto.ContactPerson);
        entity.PhoneNumber = ServiceHelpers.Clean(dto.PhoneNumber);
        entity.Email = ServiceHelpers.Clean(dto.Email);
        entity.Address = ServiceHelpers.Clean(dto.Address);
        entity.Specialization = ServiceHelpers.Clean(dto.Specialization);
        entity.AverageRating = dto.AverageRating;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync(action, "ServiceProvider", entity.Id, null, entity.Name);
        return entity.ToDto();
    }

    public async Task DeleteServiceProviderAsync(int id)
    {
        if (await _context.MaintenanceRequests.AnyAsync(m => m.ServiceProviderId == id))
        {
            throw new InvalidOperationException("لا يمكن حذف مزود خدمة مستخدم في طلبات صيانة.");
        }

        var entity = await _context.ServiceProviders.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException("مزود الخدمة غير موجود.");

        _context.ServiceProviders.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
