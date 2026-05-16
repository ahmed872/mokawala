// ============================================================================
// FINAL AUTHORITATIVE SERVICES LAYER
// LOCATION: FleetManagementSystem.Services/
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FleetManagementSystem.Core.DTOs;
using FleetManagementSystem.Core.Entities;
using FleetManagementSystem.Data;

namespace FleetManagementSystem.Services
{
    // ========================================================================
    // VEHICLE SERVICE
    // ========================================================================

    /// <summary>
    /// Vehicle service interface.
    /// </summary>
    public interface IVehicleService
    {
        Task<PagedResultDto<VehicleListItemDto>> GetVehiclesAsync(FilterDto filter);
        Task<VehicleDto> GetVehicleByIdAsync(int id);
        Task<VehicleDto> CreateVehicleAsync(VehicleFormDto dto);
        Task<VehicleDto> UpdateVehicleAsync(int id, VehicleFormDto dto);
        Task DeleteVehicleAsync(int id);
        Task<List<VehicleLookupDto>> GetVehicleLookupAsync();
        Task<PagedResultDto<VehicleListItemDto>> SearchVehiclesAsync(string searchText, FilterDto filter);
        Task UpdateVehicleStatusAsync(int id, string status);
        Task UpdateVehicleMileageAsync(int id, decimal mileage);
    }

    /// <summary>
    /// Vehicle service implementation.
    /// </summary>
    public class VehicleService : IVehicleService
    {
        private readonly FleetDbContext _context;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;

        public VehicleService(FleetDbContext context, IAuditService auditService, INotificationService notificationService)
        {
            _context = context;
            _auditService = auditService;
            _notificationService = notificationService;
        }

        public async Task<PagedResultDto<VehicleListItemDto>> GetVehiclesAsync(FilterDto filter)
        {
            var query = _context.Vehicles
                .Include(v => v.VehicleType)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var vehicles = await query
                .OrderByDescending(v => v.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(v => new VehicleListItemDto
                {
                    Id = v.Id,
                    PlateNumber = v.PlateNumber,
                    VehicleType = v.VehicleType.Name,
                    Model = v.Model,
                    Year = v.Year,
                    Status = v.Status,
                    AssignedTo = v.AssignedTo,
                    CurrentMileage = v.CurrentMileage,
                    RegistrationExpiryDate = v.RegistrationExpiryDate
                })
                .ToListAsync();

            return new PagedResultDto<VehicleListItemDto>
            {
                Items = vehicles,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<VehicleDto> GetVehicleByIdAsync(int id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.VehicleType)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
                throw new InvalidOperationException($"Vehicle with ID {id} not found.");

            return new VehicleDto
            {
                Id = vehicle.Id,
                PlateNumber = vehicle.PlateNumber,
                VehicleTypeId = vehicle.VehicleTypeId,
                VehicleType = vehicle.VehicleType.Name,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Manufacturer = vehicle.Manufacturer,
                Color = vehicle.Color,
                ChassisNumber = vehicle.ChassisNumber,
                EngineNumber = vehicle.EngineNumber,
                CurrentMileage = vehicle.CurrentMileage,
                Status = vehicle.Status,
                AssignedTo = vehicle.AssignedTo,
                PurchaseDate = vehicle.PurchaseDate,
                PurchasePrice = vehicle.PurchasePrice,
                RegistrationExpiryDate = vehicle.RegistrationExpiryDate,
                Notes = vehicle.Notes
            };
        }

        public async Task<VehicleDto> CreateVehicleAsync(VehicleFormDto dto)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(dto.PlateNumber))
                throw new InvalidOperationException("Plate number is required.");

            var existingVehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.PlateNumber == dto.PlateNumber);

            if (existingVehicle != null)
                throw new InvalidOperationException("A vehicle with this plate number already exists.");

            var vehicle = new Vehicle
            {
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
                RegistrationExpiryDate = dto.RegistrationExpiryDate,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            // Audit logging
            await _auditService.LogActionAsync("Create", "Vehicle", vehicle.Id, null, vehicle.PlateNumber);

            // Notification
            await _notificationService.CreateNotificationAsync(
                "Vehicle Created",
                $"New vehicle {vehicle.PlateNumber} has been added to the system.",
                "Info",
                "Vehicle",
                vehicle.Id
            );

            return await GetVehicleByIdAsync(vehicle.Id);
        }

        public async Task<VehicleDto> UpdateVehicleAsync(int id, VehicleFormDto dto)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null)
                throw new InvalidOperationException($"Vehicle with ID {id} not found.");

            var oldValues = $"PlateNumber: {vehicle.PlateNumber}, Status: {vehicle.Status}, Mileage: {vehicle.CurrentMileage}";

            vehicle.PlateNumber = dto.PlateNumber;
            vehicle.VehicleTypeId = dto.VehicleTypeId;
            vehicle.Model = dto.Model;
            vehicle.Year = dto.Year;
            vehicle.Manufacturer = dto.Manufacturer;
            vehicle.Color = dto.Color;
            vehicle.ChassisNumber = dto.ChassisNumber;
            vehicle.EngineNumber = dto.EngineNumber;
            vehicle.CurrentMileage = dto.CurrentMileage;
            vehicle.Status = dto.Status;
            vehicle.AssignedTo = dto.AssignedTo;
            vehicle.PurchaseDate = dto.PurchaseDate;
            vehicle.PurchasePrice = dto.PurchasePrice;
            vehicle.RegistrationExpiryDate = dto.RegistrationExpiryDate;
            vehicle.Notes = dto.Notes;
            vehicle.UpdatedAt = DateTime.UtcNow;

            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();

            var newValues = $"PlateNumber: {vehicle.PlateNumber}, Status: {vehicle.Status}, Mileage: {vehicle.CurrentMileage}";
            await _auditService.LogActionAsync("Update", "Vehicle", vehicle.Id, oldValues, newValues);

            return await GetVehicleByIdAsync(vehicle.Id);
        }

        public async Task DeleteVehicleAsync(int id)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null)
                throw new InvalidOperationException($"Vehicle with ID {id} not found.");

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Delete", "Vehicle", id, vehicle.PlateNumber, null);
        }

        public async Task<List<VehicleLookupDto>> GetVehicleLookupAsync()
        {
            return await _context.Vehicles
                .Where(v => v.Status == "Active")
                .Select(v => new VehicleLookupDto
                {
                    Id = v.Id,
                    PlateNumber = v.PlateNumber,
                    Model = v.Model
                })
                .ToListAsync();
        }

        public async Task<PagedResultDto<VehicleListItemDto>> SearchVehiclesAsync(string searchText, FilterDto filter)
        {
            var query = _context.Vehicles
                .Include(v => v.VehicleType)
                .Where(v => v.PlateNumber.Contains(searchText) || 
                           v.Model.Contains(searchText) ||
                           v.Manufacturer.Contains(searchText))
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var vehicles = await query
                .OrderByDescending(v => v.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(v => new VehicleListItemDto
                {
                    Id = v.Id,
                    PlateNumber = v.PlateNumber,
                    VehicleType = v.VehicleType.Name,
                    Model = v.Model,
                    Year = v.Year,
                    Status = v.Status,
                    AssignedTo = v.AssignedTo,
                    CurrentMileage = v.CurrentMileage,
                    RegistrationExpiryDate = v.RegistrationExpiryDate
                })
                .ToListAsync();

            return new PagedResultDto<VehicleListItemDto>
            {
                Items = vehicles,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task UpdateVehicleStatusAsync(int id, string status)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null)
                throw new InvalidOperationException($"Vehicle with ID {id} not found.");

            var oldStatus = vehicle.Status;
            vehicle.Status = status;
            vehicle.UpdatedAt = DateTime.UtcNow;

            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Update", "Vehicle", id, $"Status: {oldStatus}", $"Status: {status}");
        }

        public async Task UpdateVehicleMileageAsync(int id, decimal mileage)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null)
                throw new InvalidOperationException($"Vehicle with ID {id} not found.");

            var oldMileage = vehicle.CurrentMileage;
            vehicle.CurrentMileage = mileage;
            vehicle.UpdatedAt = DateTime.UtcNow;

            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Update", "Vehicle", id, $"Mileage: {oldMileage}", $"Mileage: {mileage}");
        }
    }

    // ========================================================================
    // CONTRACT SERVICE
    // ========================================================================

    /// <summary>
    /// Contract service interface.
    /// </summary>
    public interface IContractService
    {
        Task<PagedResultDto<ContractListItemDto>> GetContractsAsync(FilterDto filter);
        Task<ContractDto> GetContractByIdAsync(int id);
        Task<ContractDto> CreateContractAsync(ContractFormDto dto);
        Task<ContractDto> UpdateContractAsync(int id, ContractFormDto dto);
        Task DeleteContractAsync(int id);
        Task<PagedResultDto<ContractListItemDto>> GetExpiringContractsAsync(FilterDto filter);
        Task UpdateContractStatusAsync(int id, string status);
        Task RecordPaymentAsync(int id, decimal amount);
    }

    /// <summary>
    /// Contract service implementation.
    /// </summary>
    public class ContractService : IContractService
    {
        private readonly FleetDbContext _context;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;

        public ContractService(FleetDbContext context, IAuditService auditService, INotificationService notificationService)
        {
            _context = context;
            _auditService = auditService;
            _notificationService = notificationService;
        }

        public async Task<PagedResultDto<ContractListItemDto>> GetContractsAsync(FilterDto filter)
        {
            var query = _context.Contracts
                .Include(c => c.Vehicle)
                .Include(c => c.ContractStatus)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var contracts = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(c => new ContractListItemDto
                {
                    Id = c.Id,
                    ContractNumber = c.ContractNumber,
                    VehiclePlateNumber = c.Vehicle.PlateNumber,
                    ClientName = c.ClientName,
                    Status = c.ContractStatus.Name,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    ContractValue = c.ContractValue,
                    PaidAmount = c.PaidAmount,
                    IsExpiring = c.EndDate <= DateTime.UtcNow.AddDays(30)
                })
                .ToListAsync();

            return new PagedResultDto<ContractListItemDto>
            {
                Items = contracts,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<ContractDto> GetContractByIdAsync(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Vehicle)
                .Include(c => c.ContractStatus)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                throw new InvalidOperationException($"Contract with ID {id} not found.");

            return new ContractDto
            {
                Id = contract.Id,
                ContractNumber = contract.ContractNumber,
                VehicleId = contract.VehicleId,
                VehiclePlateNumber = contract.Vehicle.PlateNumber,
                ContractStatusId = contract.ContractStatusId,
                Status = contract.ContractStatus.Name,
                ClientName = contract.ClientName,
                ClientEmail = contract.ClientEmail,
                ClientPhoneNumber = contract.ClientPhoneNumber,
                ClientAddress = contract.ClientAddress,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                ContractValue = contract.ContractValue,
                PaidAmount = contract.PaidAmount,
                PaymentTerms = contract.PaymentTerms,
                ContractTerms = contract.ContractTerms,
                DocumentUrl = contract.DocumentUrl,
                Notes = contract.Notes
            };
        }

        public async Task<ContractDto> CreateContractAsync(ContractFormDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ContractNumber))
                throw new InvalidOperationException("Contract number is required.");

            var existingContract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.ContractNumber == dto.ContractNumber);

            if (existingContract != null)
                throw new InvalidOperationException("A contract with this number already exists.");

            var contract = new Contract
            {
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
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Create", "Contract", contract.Id, null, contract.ContractNumber);
            await _notificationService.CreateNotificationAsync(
                "Contract Created",
                $"New contract {contract.ContractNumber} for {contract.ClientName} has been created.",
                "Info",
                "Contract",
                contract.Id
            );

            return await GetContractByIdAsync(contract.Id);
        }

        public async Task<ContractDto> UpdateContractAsync(int id, ContractFormDto dto)
        {
            var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.Id == id);
            if (contract == null)
                throw new InvalidOperationException($"Contract with ID {id} not found.");

            var oldValues = $"Status: {contract.ContractStatusId}, PaidAmount: {contract.PaidAmount}";

            contract.ContractNumber = dto.ContractNumber;
            contract.VehicleId = dto.VehicleId;
            contract.ContractStatusId = dto.ContractStatusId;
            contract.ClientName = dto.ClientName;
            contract.ClientEmail = dto.ClientEmail;
            contract.ClientPhoneNumber = dto.ClientPhoneNumber;
            contract.ClientAddress = dto.ClientAddress;
            contract.StartDate = dto.StartDate;
            contract.EndDate = dto.EndDate;
            contract.ContractValue = dto.ContractValue;
            contract.PaidAmount = dto.PaidAmount;
            contract.PaymentTerms = dto.PaymentTerms;
            contract.ContractTerms = dto.ContractTerms;
            contract.DocumentUrl = dto.DocumentUrl;
            contract.Notes = dto.Notes;
            contract.UpdatedAt = DateTime.UtcNow;

            _context.Contracts.Update(contract);
            await _context.SaveChangesAsync();

            var newValues = $"Status: {contract.ContractStatusId}, PaidAmount: {contract.PaidAmount}";
            await _auditService.LogActionAsync("Update", "Contract", id, oldValues, newValues);

            return await GetContractByIdAsync(id);
        }

        public async Task DeleteContractAsync(int id)
        {
            var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.Id == id);
            if (contract == null)
                throw new InvalidOperationException($"Contract with ID {id} not found.");

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Delete", "Contract", id, contract.ContractNumber, null);
        }

        public async Task<PagedResultDto<ContractListItemDto>> GetExpiringContractsAsync(FilterDto filter)
        {
            var expiryThreshold = DateTime.UtcNow.AddDays(30);

            var query = _context.Contracts
                .Include(c => c.Vehicle)
                .Include(c => c.ContractStatus)
                .Where(c => c.EndDate <= expiryThreshold && c.EndDate > DateTime.UtcNow)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var contracts = await query
                .OrderBy(c => c.EndDate)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(c => new ContractListItemDto
                {
                    Id = c.Id,
                    ContractNumber = c.ContractNumber,
                    VehiclePlateNumber = c.Vehicle.PlateNumber,
                    ClientName = c.ClientName,
                    Status = c.ContractStatus.Name,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    ContractValue = c.ContractValue,
                    PaidAmount = c.PaidAmount,
                    IsExpiring = true
                })
                .ToListAsync();

            return new PagedResultDto<ContractListItemDto>
            {
                Items = contracts,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task UpdateContractStatusAsync(int id, string status)
        {
            var contract = await _context.Contracts
                .Include(c => c.ContractStatus)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                throw new InvalidOperationException($"Contract with ID {id} not found.");

            var statusRecord = await _context.ContractStatuses
                .FirstOrDefaultAsync(cs => cs.Name == status);

            if (statusRecord == null)
                throw new InvalidOperationException($"Contract status '{status}' not found.");

            var oldStatus = contract.ContractStatus.Name;
            contract.ContractStatusId = statusRecord.Id;
            contract.UpdatedAt = DateTime.UtcNow;

            _context.Contracts.Update(contract);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Update", "Contract", id, $"Status: {oldStatus}", $"Status: {status}");
            await _notificationService.CreateNotificationAsync(
                "Contract Status Updated",
                $"Contract {contract.ContractNumber} status changed to {status}.",
                "Info",
                "Contract",
                id
            );
        }

        public async Task RecordPaymentAsync(int id, decimal amount)
        {
            var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.Id == id);
            if (contract == null)
                throw new InvalidOperationException($"Contract with ID {id} not found.");

            var oldPaidAmount = contract.PaidAmount;
            contract.PaidAmount += amount;
            contract.UpdatedAt = DateTime.UtcNow;

            _context.Contracts.Update(contract);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Update", "Contract", id, 
                $"PaidAmount: {oldPaidAmount}", $"PaidAmount: {contract.PaidAmount}");
            await _notificationService.CreateNotificationAsync(
                "Payment Recorded",
                $"Payment of {amount} recorded for contract {contract.ContractNumber}.",
                "Info",
                "Contract",
                id
            );
        }
    }

    // ========================================================================
    // MAINTENANCE SERVICE
    // ========================================================================

    /// <summary>
    /// Maintenance service interface.
    /// </summary>
    public interface IMaintenanceService
    {
        Task<PagedResultDto<MaintenanceRequestListItemDto>> GetMaintenanceRequestsAsync(FilterDto filter);
        Task<MaintenanceRequestDto> GetMaintenanceRequestByIdAsync(int id);
        Task<MaintenanceRequestDto> CreateMaintenanceRequestAsync(MaintenanceRequestFormDto dto);
        Task<MaintenanceRequestDto> UpdateMaintenanceRequestAsync(int id, MaintenanceRequestFormDto dto);
        Task DeleteMaintenanceRequestAsync(int id);
        Task<PagedResultDto<MaintenanceRequestListItemDto>> GetOpenMaintenanceRequestsAsync(FilterDto filter);
        Task CompleteMaintenanceRequestAsync(int id, decimal actualCost, string workPerformed);
        Task UpdateMaintenanceStatusAsync(int id, string status);
    }

    /// <summary>
    /// Maintenance service implementation.
    /// </summary>
    public class MaintenanceService : IMaintenanceService
    {
        private readonly FleetDbContext _context;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;

        public MaintenanceService(FleetDbContext context, IAuditService auditService, INotificationService notificationService)
        {
            _context = context;
            _auditService = auditService;
            _notificationService = notificationService;
        }

        public async Task<PagedResultDto<MaintenanceRequestListItemDto>> GetMaintenanceRequestsAsync(FilterDto filter)
        {
            var query = _context.MaintenanceRequests
                .Include(m => m.Vehicle)
                .Include(m => m.MaintenanceType)
                .Include(m => m.ServiceProvider)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var requests = await query
                .OrderByDescending(m => m.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(m => new MaintenanceRequestListItemDto
                {
                    Id = m.Id,
                    VehiclePlateNumber = m.Vehicle.PlateNumber,
                    MaintenanceType = m.MaintenanceType.Name,
                    RequestDate = m.RequestDate,
                    CompletionDate = m.CompletionDate,
                    Status = m.Status,
                    ActualCost = m.ActualCost,
                    ServiceProvider = m.ServiceProvider != null ? m.ServiceProvider.Name : "Not Assigned"
                })
                .ToListAsync();

            return new PagedResultDto<MaintenanceRequestListItemDto>
            {
                Items = requests,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<MaintenanceRequestDto> GetMaintenanceRequestByIdAsync(int id)
        {
            var request = await _context.MaintenanceRequests
                .Include(m => m.Vehicle)
                .Include(m => m.MaintenanceType)
                .Include(m => m.ServiceProvider)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (request == null)
                throw new InvalidOperationException($"Maintenance request with ID {id} not found.");

            return new MaintenanceRequestDto
            {
                Id = request.Id,
                VehicleId = request.VehicleId,
                VehiclePlateNumber = request.Vehicle.PlateNumber,
                MaintenanceTypeId = request.MaintenanceTypeId,
                MaintenanceType = request.MaintenanceType.Name,
                ServiceProviderId = request.ServiceProviderId,
                ServiceProvider = request.ServiceProvider?.Name,
                RequestDate = request.RequestDate,
                CompletionDate = request.CompletionDate,
                Status = request.Status,
                Description = request.Description,
                EstimatedCost = request.EstimatedCost,
                ActualCost = request.ActualCost,
                WorkPerformed = request.WorkPerformed,
                Notes = request.Notes,
                DocumentUrl = request.DocumentUrl
            };
        }

        public async Task<MaintenanceRequestDto> CreateMaintenanceRequestAsync(MaintenanceRequestFormDto dto)
        {
            var request = new MaintenanceRequest
            {
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
                DocumentUrl = dto.DocumentUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.MaintenanceRequests.Add(request);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Create", "MaintenanceRequest", request.Id, null, $"Status: {request.Status}");
            await _notificationService.CreateNotificationAsync(
                "Maintenance Request Created",
                $"New maintenance request has been created.",
                "Info",
                "MaintenanceRequest",
                request.Id
            );

            return await GetMaintenanceRequestByIdAsync(request.Id);
        }

        public async Task<MaintenanceRequestDto> UpdateMaintenanceRequestAsync(int id, MaintenanceRequestFormDto dto)
        {
            var request = await _context.MaintenanceRequests.FirstOrDefaultAsync(m => m.Id == id);
            if (request == null)
                throw new InvalidOperationException($"Maintenance request with ID {id} not found.");

            var oldValues = $"Status: {request.Status}, ActualCost: {request.ActualCost}";

            request.VehicleId = dto.VehicleId;
            request.MaintenanceTypeId = dto.MaintenanceTypeId;
            request.ServiceProviderId = dto.ServiceProviderId;
            request.RequestDate = dto.RequestDate;
            request.CompletionDate = dto.CompletionDate;
            request.Status = dto.Status;
            request.Description = dto.Description;
            request.EstimatedCost = dto.EstimatedCost;
            request.ActualCost = dto.ActualCost;
            request.WorkPerformed = dto.WorkPerformed;
            request.Notes = dto.Notes;
            request.DocumentUrl = dto.DocumentUrl;
            request.UpdatedAt = DateTime.UtcNow;

            _context.MaintenanceRequests.Update(request);
            await _context.SaveChangesAsync();

            var newValues = $"Status: {request.Status}, ActualCost: {request.ActualCost}";
            await _auditService.LogActionAsync("Update", "MaintenanceRequest", id, oldValues, newValues);

            return await GetMaintenanceRequestByIdAsync(id);
        }

        public async Task DeleteMaintenanceRequestAsync(int id)
        {
            var request = await _context.MaintenanceRequests.FirstOrDefaultAsync(m => m.Id == id);
            if (request == null)
                throw new InvalidOperationException($"Maintenance request with ID {id} not found.");

            _context.MaintenanceRequests.Remove(request);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Delete", "MaintenanceRequest", id, request.Status, null);
        }

        public async Task<PagedResultDto<MaintenanceRequestListItemDto>> GetOpenMaintenanceRequestsAsync(FilterDto filter)
        {
            var query = _context.MaintenanceRequests
                .Include(m => m.Vehicle)
                .Include(m => m.MaintenanceType)
                .Include(m => m.ServiceProvider)
                .Where(m => m.Status == "Open")
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var requests = await query
                .OrderBy(m => m.RequestDate)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(m => new MaintenanceRequestListItemDto
                {
                    Id = m.Id,
                    VehiclePlateNumber = m.Vehicle.PlateNumber,
                    MaintenanceType = m.MaintenanceType.Name,
                    RequestDate = m.RequestDate,
                    CompletionDate = m.CompletionDate,
                    Status = m.Status,
                    ActualCost = m.ActualCost,
                    ServiceProvider = m.ServiceProvider != null ? m.ServiceProvider.Name : "Not Assigned"
                })
                .ToListAsync();

            return new PagedResultDto<MaintenanceRequestListItemDto>
            {
                Items = requests,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task CompleteMaintenanceRequestAsync(int id, decimal actualCost, string workPerformed)
        {
            var request = await _context.MaintenanceRequests.FirstOrDefaultAsync(m => m.Id == id);
            if (request == null)
                throw new InvalidOperationException($"Maintenance request with ID {id} not found.");

            var oldStatus = request.Status;
            request.Status = "Completed";
            request.ActualCost = actualCost;
            request.WorkPerformed = workPerformed;
            request.CompletionDate = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;

            _context.MaintenanceRequests.Update(request);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Update", "MaintenanceRequest", id, 
                $"Status: {oldStatus}", $"Status: Completed");
            await _notificationService.CreateNotificationAsync(
                "Maintenance Completed",
                $"Maintenance request has been completed with cost {actualCost}.",
                "Info",
                "MaintenanceRequest",
                id
            );
        }

        public async Task UpdateMaintenanceStatusAsync(int id, string status)
        {
            var request = await _context.MaintenanceRequests.FirstOrDefaultAsync(m => m.Id == id);
            if (request == null)
                throw new InvalidOperationException($"Maintenance request with ID {id} not found.");

            var oldStatus = request.Status;
            request.Status = status;
            request.UpdatedAt = DateTime.UtcNow;

            _context.MaintenanceRequests.Update(request);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Update", "MaintenanceRequest", id, 
                $"Status: {oldStatus}", $"Status: {status}");
        }
    }

    // ========================================================================
    // DRIVER SERVICE
    // ========================================================================

    /// <summary>
    /// Driver service interface.
    /// </summary>
    public interface IDriverService
    {
        Task<PagedResultDto<DriverListItemDto>> GetDriversAsync(FilterDto filter);
        Task<DriverDto> GetDriverByIdAsync(int id);
        Task<DriverDto> CreateDriverAsync(DriverFormDto dto);
        Task<DriverDto> UpdateDriverAsync(int id, DriverFormDto dto);
        Task DeleteDriverAsync(int id);
        Task<List<DriverLookupDto>> GetDriverLookupAsync();
        Task<PagedResultDto<DriverListItemDto>> GetDriversWithExpiringLicensesAsync(FilterDto filter);
    }

    /// <summary>
    /// Driver service implementation.
    /// </summary>
    public class DriverService : IDriverService
    {
        private readonly FleetDbContext _context;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;

        public DriverService(FleetDbContext context, IAuditService auditService, INotificationService notificationService)
        {
            _context = context;
            _auditService = auditService;
            _notificationService = notificationService;
        }

        public async Task<PagedResultDto<DriverListItemDto>> GetDriversAsync(FilterDto filter)
        {
            var query = _context.Drivers.AsQueryable();

            var totalCount = await query.CountAsync();

            var drivers = await query
                .OrderByDescending(d => d.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(d => new DriverListItemDto
                {
                    Id = d.Id,
                    FullName = d.FullName,
                    NationalId = d.NationalId,
                    PhoneNumber = d.PhoneNumber,
                    LicenseNumber = d.LicenseNumber,
                    LicenseExpiryDate = d.LicenseExpiryDate,
                    IsLicenseExpiring = d.LicenseExpiryDate <= DateTime.UtcNow.AddDays(30),
                    IsActive = d.IsActive
                })
                .ToListAsync();

            return new PagedResultDto<DriverListItemDto>
            {
                Items = drivers,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<DriverDto> GetDriverByIdAsync(int id)
        {
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == id);
            if (driver == null)
                throw new InvalidOperationException($"Driver with ID {id} not found.");

            return new DriverDto
            {
                Id = driver.Id,
                FullName = driver.FullName,
                NationalId = driver.NationalId,
                PhoneNumber = driver.PhoneNumber,
                Email = driver.Email,
                Address = driver.Address,
                DateOfBirth = driver.DateOfBirth,
                LicenseNumber = driver.LicenseNumber,
                LicenseExpiryDate = driver.LicenseExpiryDate,
                LicenseType = driver.LicenseType,
                IsActive = driver.IsActive,
                Notes = driver.Notes
            };
        }

        public async Task<DriverDto> CreateDriverAsync(DriverFormDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName))
                throw new InvalidOperationException("Driver name is required.");

            var existingDriver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.NationalId == dto.NationalId);

            if (existingDriver != null)
                throw new InvalidOperationException("A driver with this national ID already exists.");

            var driver = new Driver
            {
                FullName = dto.FullName,
                NationalId = dto.NationalId,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                Address = dto.Address,
                DateOfBirth = dto.DateOfBirth,
                LicenseNumber = dto.LicenseNumber,
                LicenseExpiryDate = dto.LicenseExpiryDate,
                LicenseType = dto.LicenseType,
                IsActive = dto.IsActive,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Drivers.Add(driver);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Create", "Driver", driver.Id, null, driver.FullName);

            if (driver.LicenseExpiryDate <= DateTime.UtcNow.AddDays(30))
            {
                await _notificationService.CreateNotificationAsync(
                    "License Expiring Soon",
                    $"Driver {driver.FullName}'s license will expire on {driver.LicenseExpiryDate:yyyy-MM-dd}.",
                    "Warning",
                    "Driver",
                    driver.Id
                );
            }

            return await GetDriverByIdAsync(driver.Id);
        }

        public async Task<DriverDto> UpdateDriverAsync(int id, DriverFormDto dto)
        {
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == id);
            if (driver == null)
                throw new InvalidOperationException($"Driver with ID {id} not found.");

            var oldValues = $"LicenseExpiry: {driver.LicenseExpiryDate}";

            driver.FullName = dto.FullName;
            driver.NationalId = dto.NationalId;
            driver.PhoneNumber = dto.PhoneNumber;
            driver.Email = dto.Email;
            driver.Address = dto.Address;
            driver.DateOfBirth = dto.DateOfBirth;
            driver.LicenseNumber = dto.LicenseNumber;
            driver.LicenseExpiryDate = dto.LicenseExpiryDate;
            driver.LicenseType = dto.LicenseType;
            driver.IsActive = dto.IsActive;
            driver.Notes = dto.Notes;
            driver.UpdatedAt = DateTime.UtcNow;

            _context.Drivers.Update(driver);
            await _context.SaveChangesAsync();

            var newValues = $"LicenseExpiry: {driver.LicenseExpiryDate}";
            await _auditService.LogActionAsync("Update", "Driver", id, oldValues, newValues);

            return await GetDriverByIdAsync(id);
        }

        public async Task DeleteDriverAsync(int id)
        {
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == id);
            if (driver == null)
                throw new InvalidOperationException($"Driver with ID {id} not found.");

            _context.Drivers.Remove(driver);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Delete", "Driver", id, driver.FullName, null);
        }

        public async Task<List<DriverLookupDto>> GetDriverLookupAsync()
        {
            return await _context.Drivers
                .Where(d => d.IsActive)
                .Select(d => new DriverLookupDto
                {
                    Id = d.Id,
                    FullName = d.FullName,
                    LicenseNumber = d.LicenseNumber
                })
                .ToListAsync();
        }

        public async Task<PagedResultDto<DriverListItemDto>> GetDriversWithExpiringLicensesAsync(FilterDto filter)
        {
            var expiryThreshold = DateTime.UtcNow.AddDays(30);

            var query = _context.Drivers
                .Where(d => d.LicenseExpiryDate <= expiryThreshold && d.LicenseExpiryDate > DateTime.UtcNow)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var drivers = await query
                .OrderBy(d => d.LicenseExpiryDate)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(d => new DriverListItemDto
                {
                    Id = d.Id,
                    FullName = d.FullName,
                    NationalId = d.NationalId,
                    PhoneNumber = d.PhoneNumber,
                    LicenseNumber = d.LicenseNumber,
                    LicenseExpiryDate = d.LicenseExpiryDate,
                    IsLicenseExpiring = true,
                    IsActive = d.IsActive
                })
                .ToListAsync();

            return new PagedResultDto<DriverListItemDto>
            {
                Items = drivers,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }
    }

    // ========================================================================
    // EMPLOYEE SERVICE
    // ========================================================================

    /// <summary>
    /// Employee service interface.
    /// </summary>
    public interface IEmployeeService
    {
        Task<PagedResultDto<EmployeeListItemDto>> GetEmployeesAsync(FilterDto filter);
        Task<EmployeeDto> GetEmployeeByIdAsync(int id);
        Task<EmployeeDto> CreateEmployeeAsync(EmployeeFormDto dto);
        Task<EmployeeDto> UpdateEmployeeAsync(int id, EmployeeFormDto dto);
        Task DeleteEmployeeAsync(int id);
        Task<List<EmployeeLookupDto>> GetEmployeeLookupAsync();
    }

    /// <summary>
    /// Employee service implementation.
    /// </summary>
    public class EmployeeService : IEmployeeService
    {
        private readonly FleetDbContext _context;
        private readonly IAuditService _auditService;

        public EmployeeService(FleetDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<PagedResultDto<EmployeeListItemDto>> GetEmployeesAsync(FilterDto filter)
        {
            var query = _context.Employees.AsQueryable();

            var totalCount = await query.CountAsync();

            var employees = await query
                .OrderByDescending(e => e.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(e => new EmployeeListItemDto
                {
                    Id = e.Id,
                    FullName = e.FullName,
                    EmployeeId = e.EmployeeId,
                    Department = e.Department,
                    Position = e.Position,
                    Status = e.Status,
                    HireDate = e.HireDate
                })
                .ToListAsync();

            return new PagedResultDto<EmployeeListItemDto>
            {
                Items = employees,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<EmployeeDto> GetEmployeeByIdAsync(int id)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null)
                throw new InvalidOperationException($"Employee with ID {id} not found.");

            return new EmployeeDto
            {
                Id = employee.Id,
                FullName = employee.FullName,
                EmployeeId = employee.EmployeeId,
                PhoneNumber = employee.PhoneNumber,
                Email = employee.Email,
                Department = employee.Department,
                Position = employee.Position,
                HireDate = employee.HireDate,
                TerminationDate = employee.TerminationDate,
                Status = employee.Status,
                Notes = employee.Notes
            };
        }

        public async Task<EmployeeDto> CreateEmployeeAsync(EmployeeFormDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName))
                throw new InvalidOperationException("Employee name is required.");

            var employee = new Employee
            {
                FullName = dto.FullName,
                EmployeeId = dto.EmployeeId,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                Department = dto.Department,
                Position = dto.Position,
                HireDate = dto.HireDate,
                TerminationDate = dto.TerminationDate,
                Status = dto.Status,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Create", "Employee", employee.Id, null, employee.FullName);

            return await GetEmployeeByIdAsync(employee.Id);
        }

        public async Task<EmployeeDto> UpdateEmployeeAsync(int id, EmployeeFormDto dto)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null)
                throw new InvalidOperationException($"Employee with ID {id} not found.");

            var oldValues = $"Department: {employee.Department}, Position: {employee.Position}";

            employee.FullName = dto.FullName;
            employee.EmployeeId = dto.EmployeeId;
            employee.PhoneNumber = dto.PhoneNumber;
            employee.Email = dto.Email;
            employee.Department = dto.Department;
            employee.Position = dto.Position;
            employee.HireDate = dto.HireDate;
            employee.TerminationDate = dto.TerminationDate;
            employee.Status = dto.Status;
            employee.Notes = dto.Notes;
            employee.UpdatedAt = DateTime.UtcNow;

            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();

            var newValues = $"Department: {employee.Department}, Position: {employee.Position}";
            await _auditService.LogActionAsync("Update", "Employee", id, oldValues, newValues);

            return await GetEmployeeByIdAsync(id);
        }

        public async Task DeleteEmployeeAsync(int id)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null)
                throw new InvalidOperationException($"Employee with ID {id} not found.");

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Delete", "Employee", id, employee.FullName, null);
        }

        public async Task<List<EmployeeLookupDto>> GetEmployeeLookupAsync()
        {
            return await _context.Employees
                .Where(e => e.Status == "Active")
                .Select(e => new EmployeeLookupDto
                {
                    Id = e.Id,
                    FullName = e.FullName,
                    Position = e.Position
                })
                .ToListAsync();
        }
    }

    // ========================================================================
    // TRIP SERVICE
    // ========================================================================

    /// <summary>
    /// Trip service interface.
    /// </summary>
    public interface ITripService
    {
        Task<PagedResultDto<TripListItemDto>> GetTripsAsync(FilterDto filter);
        Task<TripDto> GetTripByIdAsync(int id);
        Task<TripDto> CreateTripAsync(TripFormDto dto);
        Task<TripDto> UpdateTripAsync(int id, TripFormDto dto);
        Task DeleteTripAsync(int id);
        Task CompleteTripAsync(int id, decimal endMileage);
    }

    /// <summary>
    /// Trip service implementation.
    /// </summary>
    public class TripService : ITripService
    {
        private readonly FleetDbContext _context;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;

        public TripService(FleetDbContext context, IAuditService auditService, INotificationService notificationService)
        {
            _context = context;
            _auditService = auditService;
            _notificationService = notificationService;
        }

        public async Task<PagedResultDto<TripListItemDto>> GetTripsAsync(FilterDto filter)
        {
            var query = _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Include(t => t.Employee)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var trips = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(t => new TripListItemDto
                {
                    Id = t.Id,
                    VehiclePlateNumber = t.Vehicle.PlateNumber,
                    DriverName = t.Driver != null ? t.Driver.FullName : "Not Assigned",
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    StartLocation = t.StartLocation,
                    EndLocation = t.EndLocation,
                    Distance = t.Distance,
                    Status = t.Status
                })
                .ToListAsync();

            return new PagedResultDto<TripListItemDto>
            {
                Items = trips,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<TripDto> GetTripByIdAsync(int id)
        {
            var trip = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Include(t => t.Employee)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trip == null)
                throw new InvalidOperationException($"Trip with ID {id} not found.");

            return new TripDto
            {
                Id = trip.Id,
                VehicleId = trip.VehicleId,
                VehiclePlateNumber = trip.Vehicle.PlateNumber,
                DriverId = trip.DriverId,
                DriverName = trip.Driver?.FullName,
                EmployeeId = trip.EmployeeId,
                EmployeeName = trip.Employee?.FullName,
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
        }

        public async Task<TripDto> CreateTripAsync(TripFormDto dto)
        {
            var trip = new Trip
            {
                VehicleId = dto.VehicleId,
                DriverId = dto.DriverId,
                EmployeeId = dto.EmployeeId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                StartLocation = dto.StartLocation,
                EndLocation = dto.EndLocation,
                StartMileage = dto.StartMileage,
                EndMileage = dto.EndMileage,
                Distance = dto.Distance,
                Purpose = dto.Purpose,
                Status = dto.Status,
                FuelConsumed = dto.FuelConsumed,
                TripCost = dto.TripCost,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Create", "Trip", trip.Id, null, $"Status: {trip.Status}");

            return await GetTripByIdAsync(trip.Id);
        }

        public async Task<TripDto> UpdateTripAsync(int id, TripFormDto dto)
        {
            var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == id);
            if (trip == null)
                throw new InvalidOperationException($"Trip with ID {id} not found.");

            var oldValues = $"Status: {trip.Status}, Distance: {trip.Distance}";

            trip.VehicleId = dto.VehicleId;
            trip.DriverId = dto.DriverId;
            trip.EmployeeId = dto.EmployeeId;
            trip.StartDate = dto.StartDate;
            trip.EndDate = dto.EndDate;
            trip.StartLocation = dto.StartLocation;
            trip.EndLocation = dto.EndLocation;
            trip.StartMileage = dto.StartMileage;
            trip.EndMileage = dto.EndMileage;
            trip.Distance = dto.Distance;
            trip.Purpose = dto.Purpose;
            trip.Status = dto.Status;
            trip.FuelConsumed = dto.FuelConsumed;
            trip.TripCost = dto.TripCost;
            trip.Notes = dto.Notes;
            trip.UpdatedAt = DateTime.UtcNow;

            _context.Trips.Update(trip);
            await _context.SaveChangesAsync();

            var newValues = $"Status: {trip.Status}, Distance: {trip.Distance}";
            await _auditService.LogActionAsync("Update", "Trip", id, oldValues, newValues);

            return await GetTripByIdAsync(id);
        }

        public async Task DeleteTripAsync(int id)
        {
            var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == id);
            if (trip == null)
                throw new InvalidOperationException($"Trip with ID {id} not found.");

            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Delete", "Trip", id, trip.Status, null);
        }

        public async Task CompleteTripAsync(int id, decimal endMileage)
        {
            var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == id);
            if (trip == null)
                throw new InvalidOperationException($"Trip with ID {id} not found.");

            var oldStatus = trip.Status;
            trip.Status = "Completed";
            trip.EndMileage = endMileage;
            trip.Distance = endMileage - trip.StartMileage;
            trip.EndDate = DateTime.UtcNow;
            trip.UpdatedAt = DateTime.UtcNow;

            _context.Trips.Update(trip);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Update", "Trip", id, 
                $"Status: {oldStatus}", $"Status: Completed");
        }
    }

    // ========================================================================
    // FUEL SERVICE
    // ========================================================================

    /// <summary>
    /// Fuel service interface.
    /// </summary>
    public interface IFuelService
    {
        Task<PagedResultDto<FuelTransactionListItemDto>> GetFuelTransactionsAsync(FilterDto filter);
        Task<FuelTransactionDto> GetFuelTransactionByIdAsync(int id);
        Task<FuelTransactionDto> CreateFuelTransactionAsync(FuelTransactionFormDto dto);
        Task<FuelTransactionDto> UpdateFuelTransactionAsync(int id, FuelTransactionFormDto dto);
        Task DeleteFuelTransactionAsync(int id);
        Task<decimal> GetAverageFuelConsumptionAsync(int vehicleId, int days = 30);
    }

    /// <summary>
    /// Fuel service implementation.
    /// </summary>
    public class FuelService : IFuelService
    {
        private readonly FleetDbContext _context;
        private readonly IAuditService _auditService;

        public FuelService(FleetDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<PagedResultDto<FuelTransactionListItemDto>> GetFuelTransactionsAsync(FilterDto filter)
        {
            var query = _context.FuelTransactions
                .Include(f => f.Vehicle)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var transactions = await query
                .OrderByDescending(f => f.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(f => new FuelTransactionListItemDto
                {
                    Id = f.Id,
                    VehiclePlateNumber = f.Vehicle.PlateNumber,
                    TransactionDate = f.TransactionDate,
                    FuelType = f.FuelType,
                    Quantity = f.Quantity,
                    TotalCost = f.TotalCost,
                    FuelStation = f.FuelStation
                })
                .ToListAsync();

            return new PagedResultDto<FuelTransactionListItemDto>
            {
                Items = transactions,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<FuelTransactionDto> GetFuelTransactionByIdAsync(int id)
        {
            var transaction = await _context.FuelTransactions
                .Include(f => f.Vehicle)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (transaction == null)
                throw new InvalidOperationException($"Fuel transaction with ID {id} not found.");

            return new FuelTransactionDto
            {
                Id = transaction.Id,
                VehicleId = transaction.VehicleId,
                VehiclePlateNumber = transaction.Vehicle.PlateNumber,
                TransactionDate = transaction.TransactionDate,
                FuelType = transaction.FuelType,
                Quantity = transaction.Quantity,
                UnitPrice = transaction.UnitPrice,
                TotalCost = transaction.TotalCost,
                FuelStation = transaction.FuelStation,
                Odometer = transaction.Odometer,
                Notes = transaction.Notes
            };
        }

        public async Task<FuelTransactionDto> CreateFuelTransactionAsync(FuelTransactionFormDto dto)
        {
            var transaction = new FuelTransaction
            {
                VehicleId = dto.VehicleId,
                TransactionDate = dto.TransactionDate,
                FuelType = dto.FuelType,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                TotalCost = dto.TotalCost,
                FuelStation = dto.FuelStation,
                Odometer = dto.Odometer,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.FuelTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Create", "FuelTransaction", transaction.Id, null, $"Cost: {transaction.TotalCost}");

            return await GetFuelTransactionByIdAsync(transaction.Id);
        }

        public async Task<FuelTransactionDto> UpdateFuelTransactionAsync(int id, FuelTransactionFormDto dto)
        {
            var transaction = await _context.FuelTransactions.FirstOrDefaultAsync(f => f.Id == id);
            if (transaction == null)
                throw new InvalidOperationException($"Fuel transaction with ID {id} not found.");

            var oldCost = transaction.TotalCost;

            transaction.VehicleId = dto.VehicleId;
            transaction.TransactionDate = dto.TransactionDate;
            transaction.FuelType = dto.FuelType;
            transaction.Quantity = dto.Quantity;
            transaction.UnitPrice = dto.UnitPrice;
            transaction.TotalCost = dto.TotalCost;
            transaction.FuelStation = dto.FuelStation;
            transaction.Odometer = dto.Odometer;
            transaction.Notes = dto.Notes;
            transaction.UpdatedAt = DateTime.UtcNow;

            _context.FuelTransactions.Update(transaction);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Update", "FuelTransaction", id, $"Cost: {oldCost}", $"Cost: {transaction.TotalCost}");

            return await GetFuelTransactionByIdAsync(id);
        }

        public async Task DeleteFuelTransactionAsync(int id)
        {
            var transaction = await _context.FuelTransactions.FirstOrDefaultAsync(f => f.Id == id);
            if (transaction == null)
                throw new InvalidOperationException($"Fuel transaction with ID {id} not found.");

            _context.FuelTransactions.Remove(transaction);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Delete", "FuelTransaction", id, $"Cost: {transaction.TotalCost}", null);
        }

        public async Task<decimal> GetAverageFuelConsumptionAsync(int vehicleId, int days = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);

            var totalQuantity = await _context.FuelTransactions
                .Where(f => f.VehicleId == vehicleId && f.TransactionDate >= startDate)
                .SumAsync(f => f.Quantity);

            var transactionCount = await _context.FuelTransactions
                .Where(f => f.VehicleId == vehicleId && f.TransactionDate >= startDate)
                .CountAsync();

            if (transactionCount == 0)
                return 0;

            return totalQuantity / transactionCount;
        }
    }

    // ========================================================================
    // EXPENSE SERVICE
    // ========================================================================

    /// <summary>
    /// Expense service interface.
    /// </summary>
    public interface IExpenseService
    {
        Task<PagedResultDto<ExpenseListItemDto>> GetExpensesAsync(FilterDto filter);
        Task<ExpenseDto> GetExpenseByIdAsync(int id);
        Task<ExpenseDto> CreateExpenseAsync(ExpenseFormDto dto);
        Task<ExpenseDto> UpdateExpenseAsync(int id, ExpenseFormDto dto);
        Task DeleteExpenseAsync(int id);
        Task<decimal> GetTotalExpensesAsync(int vehicleId, DateTime? startDate = null, DateTime? endDate = null);
    }

    /// <summary>
    /// Expense service implementation.
    /// </summary>
    public class ExpenseService : IExpenseService
    {
        private readonly FleetDbContext _context;
        private readonly IAuditService _auditService;

        public ExpenseService(FleetDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<PagedResultDto<ExpenseListItemDto>> GetExpensesAsync(FilterDto filter)
        {
            var query = _context.Expenses
                .Include(e => e.Vehicle)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var expenses = await query
                .OrderByDescending(e => e.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(e => new ExpenseListItemDto
                {
                    Id = e.Id,
                    VehiclePlateNumber = e.Vehicle.PlateNumber,
                    ExpenseDate = e.ExpenseDate,
                    Category = e.Category,
                    Amount = e.Amount,
                    Vendor = e.Vendor,
                    Status = e.Status
                })
                .ToListAsync();

            return new PagedResultDto<ExpenseListItemDto>
            {
                Items = expenses,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<ExpenseDto> GetExpenseByIdAsync(int id)
        {
            var expense = await _context.Expenses
                .Include(e => e.Vehicle)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expense == null)
                throw new InvalidOperationException($"Expense with ID {id} not found.");

            return new ExpenseDto
            {
                Id = expense.Id,
                VehicleId = expense.VehicleId,
                VehiclePlateNumber = expense.Vehicle.PlateNumber,
                ExpenseDate = expense.ExpenseDate,
                Category = expense.Category,
                Amount = expense.Amount,
                Description = expense.Description,
                Vendor = expense.Vendor,
                ReceiptUrl = expense.ReceiptUrl,
                Status = expense.Status,
                Notes = expense.Notes
            };
        }

        public async Task<ExpenseDto> CreateExpenseAsync(ExpenseFormDto dto)
        {
            var expense = new Expense
            {
                VehicleId = dto.VehicleId,
                ExpenseDate = dto.ExpenseDate,
                Category = dto.Category,
                Amount = dto.Amount,
                Description = dto.Description,
                Vendor = dto.Vendor,
                ReceiptUrl = dto.ReceiptUrl,
                Status = dto.Status,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Create", "Expense", expense.Id, null, $"Amount: {expense.Amount}");

            return await GetExpenseByIdAsync(expense.Id);
        }

        public async Task<ExpenseDto> UpdateExpenseAsync(int id, ExpenseFormDto dto)
        {
            var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id);
            if (expense == null)
                throw new InvalidOperationException($"Expense with ID {id} not found.");

            var oldAmount = expense.Amount;

            expense.VehicleId = dto.VehicleId;
            expense.ExpenseDate = dto.ExpenseDate;
            expense.Category = dto.Category;
            expense.Amount = dto.Amount;
            expense.Description = dto.Description;
            expense.Vendor = dto.Vendor;
            expense.ReceiptUrl = dto.ReceiptUrl;
            expense.Status = dto.Status;
            expense.Notes = dto.Notes;
            expense.UpdatedAt = DateTime.UtcNow;

            _context.Expenses.Update(expense);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Update", "Expense", id, $"Amount: {oldAmount}", $"Amount: {expense.Amount}");

            return await GetExpenseByIdAsync(id);
        }

        public async Task DeleteExpenseAsync(int id)
        {
            var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id);
            if (expense == null)
                throw new InvalidOperationException($"Expense with ID {id} not found.");

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Delete", "Expense", id, $"Amount: {expense.Amount}", null);
        }

        public async Task<decimal> GetTotalExpensesAsync(int vehicleId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Expenses
                .Where(e => e.VehicleId == vehicleId)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(e => e.ExpenseDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.ExpenseDate <= endDate.Value);

            return await query.SumAsync(e => e.Amount);
        }
    }

    // ========================================================================
    // LICENSE SERVICE
    // ========================================================================

    /// <summary>
    /// License service interface.
    /// </summary>
    public interface ILicenseService
    {
        Task<PagedResultDto<LicenseListItemDto>> GetLicensesAsync(FilterDto filter);
        Task<LicenseDto> GetLicenseByIdAsync(int id);
        Task<LicenseDto> CreateLicenseAsync(LicenseFormDto dto);
        Task<LicenseDto> UpdateLicenseAsync(int id, LicenseFormDto dto);
        Task DeleteLicenseAsync(int id);
        Task<PagedResultDto<LicenseListItemDto>> GetExpiringLicensesAsync(FilterDto filter);
    }

    /// <summary>
    /// License service implementation.
    /// </summary>
    public class LicenseService : ILicenseService
    {
        private readonly FleetDbContext _context;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;

        public LicenseService(FleetDbContext context, IAuditService auditService, INotificationService notificationService)
        {
            _context = context;
            _auditService = auditService;
            _notificationService = notificationService;
        }

        public async Task<PagedResultDto<LicenseListItemDto>> GetLicensesAsync(FilterDto filter)
        {
            var query = _context.Licenses
                .Include(l => l.Driver)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var licenses = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(l => new LicenseListItemDto
                {
                    Id = l.Id,
                    DriverName = l.Driver.FullName,
                    LicenseNumber = l.LicenseNumber,
                    LicenseType = l.LicenseType,
                    ExpiryDate = l.ExpiryDate,
                    Status = l.Status,
                    IsExpiring = l.ExpiryDate <= DateTime.UtcNow.AddDays(30)
                })
                .ToListAsync();

            return new PagedResultDto<LicenseListItemDto>
            {
                Items = licenses,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<LicenseDto> GetLicenseByIdAsync(int id)
        {
            var license = await _context.Licenses
                .Include(l => l.Driver)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (license == null)
                throw new InvalidOperationException($"License with ID {id} not found.");

            return new LicenseDto
            {
                Id = license.Id,
                DriverId = license.DriverId,
                DriverName = license.Driver.FullName,
                LicenseNumber = license.LicenseNumber,
                LicenseType = license.LicenseType,
                IssueDate = license.IssueDate,
                ExpiryDate = license.ExpiryDate,
                IssuingAuthority = license.IssuingAuthority,
                Status = license.Status,
                Notes = license.Notes
            };
        }

        public async Task<LicenseDto> CreateLicenseAsync(LicenseFormDto dto)
        {
            var license = new License
            {
                DriverId = dto.DriverId,
                LicenseNumber = dto.LicenseNumber,
                LicenseType = dto.LicenseType,
                IssueDate = dto.IssueDate,
                ExpiryDate = dto.ExpiryDate,
                IssuingAuthority = dto.IssuingAuthority,
                Status = dto.Status,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Licenses.Add(license);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Create", "License", license.Id, null, license.LicenseNumber);

            if (license.ExpiryDate <= DateTime.UtcNow.AddDays(30))
            {
                await _notificationService.CreateNotificationAsync(
                    "License Expiring Soon",
                    $"License {license.LicenseNumber} will expire on {license.ExpiryDate:yyyy-MM-dd}.",
                    "Warning",
                    "License",
                    license.Id
                );
            }

            return await GetLicenseByIdAsync(license.Id);
        }

        public async Task<LicenseDto> UpdateLicenseAsync(int id, LicenseFormDto dto)
        {
            var license = await _context.Licenses.FirstOrDefaultAsync(l => l.Id == id);
            if (license == null)
                throw new InvalidOperationException($"License with ID {id} not found.");

            var oldValues = $"ExpiryDate: {license.ExpiryDate}";

            license.DriverId = dto.DriverId;
            license.LicenseNumber = dto.LicenseNumber;
            license.LicenseType = dto.LicenseType;
            license.IssueDate = dto.IssueDate;
            license.ExpiryDate = dto.ExpiryDate;
            license.IssuingAuthority = dto.IssuingAuthority;
            license.Status = dto.Status;
            license.Notes = dto.Notes;
            license.UpdatedAt = DateTime.UtcNow;

            _context.Licenses.Update(license);
            await _context.SaveChangesAsync();

            var newValues = $"ExpiryDate: {license.ExpiryDate}";
            await _auditService.LogActionAsync("Update", "License", id, oldValues, newValues);

            return await GetLicenseByIdAsync(id);
        }

        public async Task DeleteLicenseAsync(int id)
        {
            var license = await _context.Licenses.FirstOrDefaultAsync(l => l.Id == id);
            if (license == null)
                throw new InvalidOperationException($"License with ID {id} not found.");

            _context.Licenses.Remove(license);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Delete", "License", id, license.LicenseNumber, null);
        }

        public async Task<PagedResultDto<LicenseListItemDto>> GetExpiringLicensesAsync(FilterDto filter)
        {
            var expiryThreshold = DateTime.UtcNow.AddDays(30);

            var query = _context.Licenses
                .Include(l => l.Driver)
                .Where(l => l.ExpiryDate <= expiryThreshold && l.ExpiryDate > DateTime.UtcNow)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var licenses = await query
                .OrderBy(l => l.ExpiryDate)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(l => new LicenseListItemDto
                {
                    Id = l.Id,
                    DriverName = l.Driver.FullName,
                    LicenseNumber = l.LicenseNumber,
                    LicenseType = l.LicenseType,
                    ExpiryDate = l.ExpiryDate,
                    Status = l.Status,
                    IsExpiring = true
                })
                .ToListAsync();

            return new PagedResultDto<LicenseListItemDto>
            {
                Items = licenses,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }
    }

    // ========================================================================
    // INSURANCE SERVICE
    // ========================================================================

    /// <summary>
    /// Insurance service interface.
    /// </summary>
    public interface IInsuranceService
    {
        Task<PagedResultDto<InsuranceListItemDto>> GetInsurancesAsync(FilterDto filter);
        Task<InsuranceDto> GetInsuranceByIdAsync(int id);
        Task<InsuranceDto> CreateInsuranceAsync(InsuranceFormDto dto);
        Task<InsuranceDto> UpdateInsuranceAsync(int id, InsuranceFormDto dto);
        Task DeleteInsuranceAsync(int id);
        Task<PagedResultDto<InsuranceListItemDto>> GetExpiringInsurancesAsync(FilterDto filter);
    }

    /// <summary>
    /// Insurance service implementation.
    /// </summary>
    public class InsuranceService : IInsuranceService
    {
        private readonly FleetDbContext _context;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;

        public InsuranceService(FleetDbContext context, IAuditService auditService, INotificationService notificationService)
        {
            _context = context;
            _auditService = auditService;
            _notificationService = notificationService;
        }

        public async Task<PagedResultDto<InsuranceListItemDto>> GetInsurancesAsync(FilterDto filter)
        {
            var query = _context.Insurances
                .Include(i => i.Vehicle)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var insurances = await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(i => new InsuranceListItemDto
                {
                    Id = i.Id,
                    VehiclePlateNumber = i.Vehicle.PlateNumber,
                    PolicyNumber = i.PolicyNumber,
                    InsuranceCompany = i.InsuranceCompany,
                    ExpiryDate = i.ExpiryDate,
                    PremiumAmount = i.PremiumAmount,
                    Status = i.Status,
                    IsExpiring = i.ExpiryDate <= DateTime.UtcNow.AddDays(30)
                })
                .ToListAsync();

            return new PagedResultDto<InsuranceListItemDto>
            {
                Items = insurances,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<InsuranceDto> GetInsuranceByIdAsync(int id)
        {
            var insurance = await _context.Insurances
                .Include(i => i.Vehicle)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (insurance == null)
                throw new InvalidOperationException($"Insurance with ID {id} not found.");

            return new InsuranceDto
            {
                Id = insurance.Id,
                VehicleId = insurance.VehicleId,
                VehiclePlateNumber = insurance.Vehicle.PlateNumber,
                PolicyNumber = insurance.PolicyNumber,
                InsuranceCompany = insurance.InsuranceCompany,
                PolicyType = insurance.PolicyType,
                StartDate = insurance.StartDate,
                ExpiryDate = insurance.ExpiryDate,
                PremiumAmount = insurance.PremiumAmount,
                CoverageAmount = insurance.CoverageAmount,
                CoverageDetails = insurance.CoverageDetails,
                AgentName = insurance.AgentName,
                AgentPhoneNumber = insurance.AgentPhoneNumber,
                Status = insurance.Status,
                DocumentUrl = insurance.DocumentUrl,
                Notes = insurance.Notes
            };
        }

        public async Task<InsuranceDto> CreateInsuranceAsync(InsuranceFormDto dto)
        {
            var insurance = new Insurance
            {
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
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Insurances.Add(insurance);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Create", "Insurance", insurance.Id, null, insurance.PolicyNumber);

            if (insurance.ExpiryDate <= DateTime.UtcNow.AddDays(30))
            {
                await _notificationService.CreateNotificationAsync(
                    "Insurance Expiring Soon",
                    $"Insurance policy {insurance.PolicyNumber} will expire on {insurance.ExpiryDate:yyyy-MM-dd}.",
                    "Warning",
                    "Insurance",
                    insurance.Id
                );
            }

            return await GetInsuranceByIdAsync(insurance.Id);
        }

        public async Task<InsuranceDto> UpdateInsuranceAsync(int id, InsuranceFormDto dto)
        {
            var insurance = await _context.Insurances.FirstOrDefaultAsync(i => i.Id == id);
            if (insurance == null)
                throw new InvalidOperationException($"Insurance with ID {id} not found.");

            var oldValues = $"ExpiryDate: {insurance.ExpiryDate}";

            insurance.VehicleId = dto.VehicleId;
            insurance.PolicyNumber = dto.PolicyNumber;
            insurance.InsuranceCompany = dto.InsuranceCompany;
            insurance.PolicyType = dto.PolicyType;
            insurance.StartDate = dto.StartDate;
            insurance.ExpiryDate = dto.ExpiryDate;
            insurance.PremiumAmount = dto.PremiumAmount;
            insurance.CoverageAmount = dto.CoverageAmount;
            insurance.CoverageDetails = dto.CoverageDetails;
            insurance.AgentName = dto.AgentName;
            insurance.AgentPhoneNumber = dto.AgentPhoneNumber;
            insurance.Status = dto.Status;
            insurance.DocumentUrl = dto.DocumentUrl;
            insurance.Notes = dto.Notes;
            insurance.UpdatedAt = DateTime.UtcNow;

            _context.Insurances.Update(insurance);
            await _context.SaveChangesAsync();

            var newValues = $"ExpiryDate: {insurance.ExpiryDate}";
            await _auditService.LogActionAsync("Update", "Insurance", id, oldValues, newValues);

            return await GetInsuranceByIdAsync(id);
        }

        public async Task DeleteInsuranceAsync(int id)
        {
            var insurance = await _context.Insurances.FirstOrDefaultAsync(i => i.Id == id);
            if (insurance == null)
                throw new InvalidOperationException($"Insurance with ID {id} not found.");

            _context.Insurances.Remove(insurance);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Delete", "Insurance", id, insurance.PolicyNumber, null);
        }

        public async Task<PagedResultDto<InsuranceListItemDto>> GetExpiringInsurancesAsync(FilterDto filter)
        {
            var expiryThreshold = DateTime.UtcNow.AddDays(30);

            var query = _context.Insurances
                .Include(i => i.Vehicle)
                .Where(i => i.ExpiryDate <= expiryThreshold && i.ExpiryDate > DateTime.UtcNow)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var insurances = await query
                .OrderBy(i => i.ExpiryDate)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(i => new InsuranceListItemDto
                {
                    Id = i.Id,
                    VehiclePlateNumber = i.Vehicle.PlateNumber,
                    PolicyNumber = i.PolicyNumber,
                    InsuranceCompany = i.InsuranceCompany,
                    ExpiryDate = i.ExpiryDate,
                    PremiumAmount = i.PremiumAmount,
                    Status = i.Status,
                    IsExpiring = true
                })
                .ToListAsync();

            return new PagedResultDto<InsuranceListItemDto>
            {
                Items = insurances,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }
    }

    // ========================================================================
    // CUSTODY SERVICE
    // ========================================================================

    /// <summary>
    /// Custody service interface.
    /// </summary>
    public interface ICustodyService
    {
        Task<PagedResultDto<CustodyListItemDto>> GetCustodiesAsync(FilterDto filter);
        Task<CustodyDto> GetCustodyByIdAsync(int id);
        Task<CustodyDto> CreateCustodyAsync(CustodyFormDto dto);
        Task<CustodyDto> UpdateCustodyAsync(int id, CustodyFormDto dto);
        Task DeleteCustodyAsync(int id);
        Task ReturnCustodyAsync(int id);
    }

    /// <summary>
    /// Custody service implementation.
    /// </summary>
    public class CustodyService : ICustodyService
    {
        private readonly FleetDbContext _context;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;

        public CustodyService(FleetDbContext context, IAuditService auditService, INotificationService notificationService)
        {
            _context = context;
            _auditService = auditService;
            _notificationService = notificationService;
        }

        public async Task<PagedResultDto<CustodyListItemDto>> GetCustodiesAsync(FilterDto filter)
        {
            var query = _context.Custodies
                .Include(c => c.Vehicle)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var custodies = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(c => new CustodyListItemDto
                {
                    Id = c.Id,
                    CustodyNumber = c.CustodyNumber,
                    VehiclePlateNumber = c.Vehicle.PlateNumber,
                    CustodianName = c.CustodianName,
                    HandoverDate = c.HandoverDate,
                    ReturnDate = c.ReturnDate,
                    Status = c.Status
                })
                .ToListAsync();

            return new PagedResultDto<CustodyListItemDto>
            {
                Items = custodies,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<CustodyDto> GetCustodyByIdAsync(int id)
        {
            var custody = await _context.Custodies
                .Include(c => c.Vehicle)
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (custody == null)
                throw new InvalidOperationException($"Custody with ID {id} not found.");

            return new CustodyDto
            {
                Id = custody.Id,
                VehicleId = custody.VehicleId,
                VehiclePlateNumber = custody.Vehicle.PlateNumber,
                CustodyNumber = custody.CustodyNumber,
                CustodianName = custody.CustodianName,
                CustodianPosition = custody.CustodianPosition,
                HandoverDate = custody.HandoverDate,
                ReturnDate = custody.ReturnDate,
                Status = custody.Status,
                VehicleConditionRating = custody.VehicleConditionRating,
                Notes = custody.Notes,
                DocumentUrl = custody.DocumentUrl,
                Items = custody.Items.Select(i => new CustodyItemDto
                {
                    Id = i.Id,
                    CustodyId = i.CustodyId,
                    ItemName = i.ItemName,
                    ItemDescription = i.ItemDescription,
                    Quantity = i.Quantity,
                    SerialNumber = i.SerialNumber,
                    IsReturned = i.IsReturned,
                    ReturnDate = i.ReturnDate,
                    Condition = i.Condition
                }).ToList()
            };
        }

        public async Task<CustodyDto> CreateCustodyAsync(CustodyFormDto dto)
        {
            var custody = new Custody
            {
                VehicleId = dto.VehicleId,
                CustodyNumber = dto.CustodyNumber,
                CustodianName = dto.CustodianName,
                CustodianPosition = dto.CustodianPosition,
                HandoverDate = dto.HandoverDate,
                ReturnDate = dto.ReturnDate,
                Status = dto.Status,
                VehicleConditionRating = dto.VehicleConditionRating,
                Notes = dto.Notes,
                DocumentUrl = dto.DocumentUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Custodies.Add(custody);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Create", "Custody", custody.Id, null, custody.CustodyNumber);
            await _notificationService.CreateNotificationAsync(
                "Custody Created",
                $"New custody record {custody.CustodyNumber} for {custody.CustodianName} has been created.",
                "Info",
                "Custody",
                custody.Id
            );

            return await GetCustodyByIdAsync(custody.Id);
        }

        public async Task<CustodyDto> UpdateCustodyAsync(int id, CustodyFormDto dto)
        {
            var custody = await _context.Custodies.FirstOrDefaultAsync(c => c.Id == id);
            if (custody == null)
                throw new InvalidOperationException($"Custody with ID {id} not found.");

            var oldValues = $"Status: {custody.Status}";

            custody.VehicleId = dto.VehicleId;
            custody.CustodyNumber = dto.CustodyNumber;
            custody.CustodianName = dto.CustodianName;
            custody.CustodianPosition = dto.CustodianPosition;
            custody.HandoverDate = dto.HandoverDate;
            custody.ReturnDate = dto.ReturnDate;
            custody.Status = dto.Status;
            custody.VehicleConditionRating = dto.VehicleConditionRating;
            custody.Notes = dto.Notes;
            custody.DocumentUrl = dto.DocumentUrl;
            custody.UpdatedAt = DateTime.UtcNow;

            _context.Custodies.Update(custody);
            await _context.SaveChangesAsync();

            var newValues = $"Status: {custody.Status}";
            await _auditService.LogActionAsync("Update", "Custody", id, oldValues, newValues);

            return await GetCustodyByIdAsync(id);
        }

        public async Task DeleteCustodyAsync(int id)
        {
            var custody = await _context.Custodies.FirstOrDefaultAsync(c => c.Id == id);
            if (custody == null)
                throw new InvalidOperationException($"Custody with ID {id} not found.");

            _context.Custodies.Remove(custody);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Delete", "Custody", id, custody.CustodyNumber, null);
        }

        public async Task ReturnCustodyAsync(int id)
        {
            var custody = await _context.Custodies.FirstOrDefaultAsync(c => c.Id == id);
            if (custody == null)
                throw new InvalidOperationException($"Custody with ID {id} not found.");

            var oldStatus = custody.Status;
            custody.Status = "Returned";
            custody.ReturnDate = DateTime.UtcNow;
            custody.UpdatedAt = DateTime.UtcNow;

            _context.Custodies.Update(custody);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Update", "Custody", id, $"Status: {oldStatus}", $"Status: Returned");
            await _notificationService.CreateNotificationAsync(
                "Custody Returned",
                $"Custody record {custody.CustodyNumber} has been returned.",
                "Info",
                "Custody",
                id
            );
        }
    }

    // ========================================================================
    // MASTER DATA SERVICE
    // ========================================================================

    /// <summary>
    /// Master data service interface.
    /// </summary>
    public interface IMasterDataService
    {
        // Vehicle Types
        Task<List<VehicleTypeDto>> GetVehicleTypesAsync();
        Task<VehicleTypeDto> CreateVehicleTypeAsync(VehicleTypeDto dto);
        Task<VehicleTypeDto> UpdateVehicleTypeAsync(int id, VehicleTypeDto dto);
        Task DeleteVehicleTypeAsync(int id);

        // Contract Statuses
        Task<List<ContractStatusDto>> GetContractStatusesAsync();
        Task<ContractStatusDto> CreateContractStatusAsync(ContractStatusDto dto);
        Task<ContractStatusDto> UpdateContractStatusAsync(int id, ContractStatusDto dto);
        Task DeleteContractStatusAsync(int id);

        // Maintenance Types
        Task<List<MaintenanceTypeDto>> GetMaintenanceTypesAsync();
        Task<MaintenanceTypeDto> CreateMaintenanceTypeAsync(MaintenanceTypeDto dto);
        Task<MaintenanceTypeDto> UpdateMaintenanceTypeAsync(int id, MaintenanceTypeDto dto);
        Task DeleteMaintenanceTypeAsync(int id);

        // Service Providers
        Task<PagedResultDto<ServiceProviderDto>> GetServiceProvidersAsync(FilterDto filter);
        Task<ServiceProviderDto> GetServiceProviderByIdAsync(int id);
        Task<ServiceProviderDto> CreateServiceProviderAsync(ServiceProviderDto dto);
        Task<ServiceProviderDto> UpdateServiceProviderAsync(int id, ServiceProviderDto dto);
        Task DeleteServiceProviderAsync(int id);
    }

    /// <summary>
    /// Master data service implementation.
    /// </summary>
    public class MasterDataService : IMasterDataService
    {
        private readonly FleetDbContext _context;
        private readonly IAuditService _auditService;

        public MasterDataService(FleetDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // Vehicle Types
        public async Task<List<VehicleTypeDto>> GetVehicleTypesAsync()
        {
            return await _context.VehicleTypes
                .Select(vt => new VehicleTypeDto
                {
                    Id = vt.Id,
                    Name = vt.Name,
                    NameEn = vt.NameEn,
                    Description = vt.Description,
                    IsActive = vt.IsActive
                })
                .ToListAsync();
        }

        public async Task<VehicleTypeDto> CreateVehicleTypeAsync(VehicleTypeDto dto)
        {
            var vehicleType = new VehicleType
            {
                Name = dto.Name,
                NameEn = dto.NameEn,
                Description = dto.Description,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.VehicleTypes.Add(vehicleType);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Create", "VehicleType", vehicleType.Id, null, vehicleType.Name);

            return new VehicleTypeDto
            {
                Id = vehicleType.Id,
                Name = vehicleType.Name,
                NameEn = vehicleType.NameEn,
                Description = vehicleType.Description,
                IsActive = vehicleType.IsActive
            };
        }

        public async Task<VehicleTypeDto> UpdateVehicleTypeAsync(int id, VehicleTypeDto dto)
        {
            var vehicleType = await _context.VehicleTypes.FirstOrDefaultAsync(vt => vt.Id == id);
            if (vehicleType == null)
                throw new InvalidOperationException($"Vehicle type with ID {id} not found.");

            vehicleType.Name = dto.Name;
            vehicleType.NameEn = dto.NameEn;
            vehicleType.Description = dto.Description;
            vehicleType.IsActive = dto.IsActive;
            vehicleType.UpdatedAt = DateTime.UtcNow;

            _context.VehicleTypes.Update(vehicleType);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Update", "VehicleType", id, null, vehicleType.Name);

            return new VehicleTypeDto
            {
                Id = vehicleType.Id,
                Name = vehicleType.Name,
                NameEn = vehicleType.NameEn,
                Description = vehicleType.Description,
                IsActive = vehicleType.IsActive
            };
        }

        public async Task DeleteVehicleTypeAsync(int id)
        {
            var vehicleType = await _context.VehicleTypes.FirstOrDefaultAsync(vt => vt.Id == id);
            if (vehicleType == null)
                throw new InvalidOperationException($"Vehicle type with ID {id} not found.");

            _context.VehicleTypes.Remove(vehicleType);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Delete", "VehicleType", id, vehicleType.Name, null);
        }

        // Contract Statuses
        public async Task<List<ContractStatusDto>> GetContractStatusesAsync()
        {
            return await _context.ContractStatuses
                .Select(cs => new ContractStatusDto
                {
                    Id = cs.Id,
                    Name = cs.Name,
                    NameEn = cs.NameEn,
                    Description = cs.Description,
                    Color = cs.Color,
                    IsActive = cs.IsActive
                })
                .ToListAsync();
        }

        public async Task<ContractStatusDto> CreateContractStatusAsync(ContractStatusDto dto)
        {
            var contractStatus = new ContractStatus
            {
                Name = dto.Name,
                NameEn = dto.NameEn,
                Description = dto.Description,
                Color = dto.Color,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ContractStatuses.Add(contractStatus);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Create", "ContractStatus", contractStatus.Id, null, contractStatus.Name);

            return new ContractStatusDto
            {
                Id = contractStatus.Id,
                Name = contractStatus.Name,
                NameEn = contractStatus.NameEn,
                Description = contractStatus.Description,
                Color = contractStatus.Color,
                IsActive = contractStatus.IsActive
            };
        }

        public async Task<ContractStatusDto> UpdateContractStatusAsync(int id, ContractStatusDto dto)
        {
            var contractStatus = await _context.ContractStatuses.FirstOrDefaultAsync(cs => cs.Id == id);
            if (contractStatus == null)
                throw new InvalidOperationException($"Contract status with ID {id} not found.");

            contractStatus.Name = dto.Name;
            contractStatus.NameEn = dto.NameEn;
            contractStatus.Description = dto.Description;
            contractStatus.Color = dto.Color;
            contractStatus.IsActive = dto.IsActive;
            contractStatus.UpdatedAt = DateTime.UtcNow;

            _context.ContractStatuses.Update(contractStatus);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Update", "ContractStatus", id, null, contractStatus.Name);

            return new ContractStatusDto
            {
                Id = contractStatus.Id,
                Name = contractStatus.Name,
                NameEn = contractStatus.NameEn,
                Description = contractStatus.Description,
                Color = contractStatus.Color,
                IsActive = contractStatus.IsActive
            };
        }

        public async Task DeleteContractStatusAsync(int id)
        {
            var contractStatus = await _context.ContractStatuses.FirstOrDefaultAsync(cs => cs.Id == id);
            if (contractStatus == null)
                throw new InvalidOperationException($"Contract status with ID {id} not found.");

            _context.ContractStatuses.Remove(contractStatus);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Delete", "ContractStatus", id, contractStatus.Name, null);
        }

        // Maintenance Types
        public async Task<List<MaintenanceTypeDto>> GetMaintenanceTypesAsync()
        {
            return await _context.MaintenanceTypes
                .Select(mt => new MaintenanceTypeDto
                {
                    Id = mt.Id,
                    Name = mt.Name,
                    NameEn = mt.NameEn,
                    Description = mt.Description,
                    EstimatedCost = mt.EstimatedCost,
                    EstimatedDurationDays = mt.EstimatedDurationDays,
                    IsActive = mt.IsActive
                })
                .ToListAsync();
        }

        public async Task<MaintenanceTypeDto> CreateMaintenanceTypeAsync(MaintenanceTypeDto dto)
        {
            var maintenanceType = new MaintenanceType
            {
                Name = dto.Name,
                NameEn = dto.NameEn,
                Description = dto.Description,
                EstimatedCost = dto.EstimatedCost,
                EstimatedDurationDays = dto.EstimatedDurationDays,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.MaintenanceTypes.Add(maintenanceType);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Create", "MaintenanceType", maintenanceType.Id, null, maintenanceType.Name);

            return new MaintenanceTypeDto
            {
                Id = maintenanceType.Id,
                Name = maintenanceType.Name,
                NameEn = maintenanceType.NameEn,
                Description = maintenanceType.Description,
                EstimatedCost = maintenanceType.EstimatedCost,
                EstimatedDurationDays = maintenanceType.EstimatedDurationDays,
                IsActive = maintenanceType.IsActive
            };
        }

        public async Task<MaintenanceTypeDto> UpdateMaintenanceTypeAsync(int id, MaintenanceTypeDto dto)
        {
            var maintenanceType = await _context.MaintenanceTypes.FirstOrDefaultAsync(mt => mt.Id == id);
            if (maintenanceType == null)
                throw new InvalidOperationException($"Maintenance type with ID {id} not found.");

            maintenanceType.Name = dto.Name;
            maintenanceType.NameEn = dto.NameEn;
            maintenanceType.Description = dto.Description;
            maintenanceType.EstimatedCost = dto.EstimatedCost;
            maintenanceType.EstimatedDurationDays = dto.EstimatedDurationDays;
            maintenanceType.IsActive = dto.IsActive;
            maintenanceType.UpdatedAt = DateTime.UtcNow;

            _context.MaintenanceTypes.Update(maintenanceType);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Update", "MaintenanceType", id, null, maintenanceType.Name);

            return new MaintenanceTypeDto
            {
                Id = maintenanceType.Id,
                Name = maintenanceType.Name,
                NameEn = maintenanceType.NameEn,
                Description = maintenanceType.Description,
                EstimatedCost = maintenanceType.EstimatedCost,
                EstimatedDurationDays = maintenanceType.EstimatedDurationDays,
                IsActive = maintenanceType.IsActive
            };
        }

        public async Task DeleteMaintenanceTypeAsync(int id)
        {
            var maintenanceType = await _context.MaintenanceTypes.FirstOrDefaultAsync(mt => mt.Id == id);
            if (maintenanceType == null)
                throw new InvalidOperationException($"Maintenance type with ID {id} not found.");

            _context.MaintenanceTypes.Remove(maintenanceType);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Delete", "MaintenanceType", id, maintenanceType.Name, null);
        }

        // Service Providers
        public async Task<PagedResultDto<ServiceProviderDto>> GetServiceProvidersAsync(FilterDto filter)
        {
            var query = _context.ServiceProviders.AsQueryable();

            var totalCount = await query.CountAsync();

            var providers = await query
                .OrderByDescending(sp => sp.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(sp => new ServiceProviderDto
                {
                    Id = sp.Id,
                    Name = sp.Name,
                    NameEn = sp.NameEn,
                    ContactPerson = sp.ContactPerson,
                    PhoneNumber = sp.PhoneNumber,
                    Email = sp.Email,
                    Address = sp.Address,
                    Specialization = sp.Specialization,
                    AverageRating = sp.AverageRating,
                    IsActive = sp.IsActive
                })
                .ToListAsync();

            return new PagedResultDto<ServiceProviderDto>
            {
                Items = providers,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<ServiceProviderDto> GetServiceProviderByIdAsync(int id)
        {
            var provider = await _context.ServiceProviders.FirstOrDefaultAsync(sp => sp.Id == id);
            if (provider == null)
                throw new InvalidOperationException($"Service provider with ID {id} not found.");

            return new ServiceProviderDto
            {
                Id = provider.Id,
                Name = provider.Name,
                NameEn = provider.NameEn,
                ContactPerson = provider.ContactPerson,
                PhoneNumber = provider.PhoneNumber,
                Email = provider.Email,
                Address = provider.Address,
                Specialization = provider.Specialization,
                AverageRating = provider.AverageRating,
                IsActive = provider.IsActive
            };
        }

        public async Task<ServiceProviderDto> CreateServiceProviderAsync(ServiceProviderDto dto)
        {
            var provider = new ServiceProvider
            {
                Name = dto.Name,
                NameEn = dto.NameEn,
                ContactPerson = dto.ContactPerson,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                Address = dto.Address,
                Specialization = dto.Specialization,
                AverageRating = dto.AverageRating,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ServiceProviders.Add(provider);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Create", "ServiceProvider", provider.Id, null, provider.Name);

            return new ServiceProviderDto
            {
                Id = provider.Id,
                Name = provider.Name,
                NameEn = provider.NameEn,
                ContactPerson = provider.ContactPerson,
                PhoneNumber = provider.PhoneNumber,
                Email = provider.Email,
                Address = provider.Address,
                Specialization = provider.Specialization,
                AverageRating = provider.AverageRating,
                IsActive = provider.IsActive
            };
        }

        public async Task<ServiceProviderDto> UpdateServiceProviderAsync(int id, ServiceProviderDto dto)
        {
            var provider = await _context.ServiceProviders.FirstOrDefaultAsync(sp => sp.Id == id);
            if (provider == null)
                throw new InvalidOperationException($"Service provider with ID {id} not found.");

            provider.Name = dto.Name;
            provider.NameEn = dto.NameEn;
            provider.ContactPerson = dto.ContactPerson;
            provider.PhoneNumber = dto.PhoneNumber;
            provider.Email = dto.Email;
            provider.Address = dto.Address;
            provider.Specialization = dto.Specialization;
            provider.AverageRating = dto.AverageRating;
            provider.IsActive = dto.IsActive;
            provider.UpdatedAt = DateTime.UtcNow;

            _context.ServiceProviders.Update(provider);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Update", "ServiceProvider", id, null, provider.Name);

            return new ServiceProviderDto
            {
                Id = provider.Id,
                Name = provider.Name,
                NameEn = provider.NameEn,
                ContactPerson = provider.ContactPerson,
                PhoneNumber = provider.PhoneNumber,
                Email = provider.Email,
                Address = provider.Address,
                Specialization = provider.Specialization,
                AverageRating = provider.AverageRating,
                IsActive = provider.IsActive
            };
        }

        public async Task DeleteServiceProviderAsync(int id)
        {
            var provider = await _context.ServiceProviders.FirstOrDefaultAsync(sp => sp.Id == id);
            if (provider == null)
                throw new InvalidOperationException($"Service provider with ID {id} not found.");

            _context.ServiceProviders.Remove(provider);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Delete", "ServiceProvider", id, provider.Name, null);
        }
    }

    // ========================================================================
    // AUDIT SERVICE
    // ========================================================================

    /// <summary>
    /// Audit service interface.
    /// </summary>
    public interface IAuditService
    {
        Task LogActionAsync(string action, string entityName, int entityId, string oldValues, string newValues);
        Task<PagedResultDto<AuditLogDto>> GetAuditLogsAsync(FilterDto filter);
    }

    /// <summary>
    /// Audit service implementation.
    /// </summary>
    public class AuditService : IAuditService
    {
        private readonly FleetDbContext _context;

        public AuditService(FleetDbContext context)
        {
            _context = context;
        }

        public async Task LogActionAsync(string action, string entityName, int entityId, string oldValues, string newValues)
        {
            var auditLog = new AuditLog
            {
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                OldValues = oldValues ?? string.Empty,
                NewValues = newValues ?? string.Empty,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResultDto<AuditLogDto>> GetAuditLogsAsync(FilterDto filter)
        {
            var query = _context.AuditLogs.AsQueryable();

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(al => al.Timestamp)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(al => new AuditLogDto
                {
                    Id = al.Id,
                    UserId = al.UserId,
                    UserName = al.UserName,
                    Action = al.Action,
                    EntityName = al.EntityName,
                    EntityId = al.EntityId,
                    OldValues = al.OldValues,
                    NewValues = al.NewValues,
                    IpAddress = al.IpAddress,
                    Timestamp = al.Timestamp
                })
                .ToListAsync();

            return new PagedResultDto<AuditLogDto>
            {
                Items = logs,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }
    }

    // ========================================================================
    // NOTIFICATION SERVICE
    // ========================================================================

    /// <summary>
    /// Notification service interface.
    /// </summary>
    public interface INotificationService
    {
        Task CreateNotificationAsync(string title, string message, string type, string relatedEntityType, int? relatedEntityId);
        Task<List<NotificationDto>> GetUnreadNotificationsAsync(int userId);
        Task MarkAsReadAsync(int notificationId);
        Task DeleteNotificationAsync(int notificationId);
    }

    /// <summary>
    /// Notification service implementation.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly FleetDbContext _context;

        public NotificationService(FleetDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(string title, string message, string type, string relatedEntityType, int? relatedEntityId)
        {
            var notification = new Notification
            {
                UserId = 1, // TODO: Get from current user context
                Title = title,
                Message = message,
                Type = type,
                RelatedEntityType = relatedEntityType,
                RelatedEntityId = relatedEntityId,
                IsRead = false,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<NotificationDto>> GetUnreadNotificationsAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead && n.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    RelatedEntityType = n.RelatedEntityType,
                    RelatedEntityId = n.RelatedEntityId,
                    IsRead = n.IsRead,
                    ReadAt = n.ReadAt,
                    ExpiresAt = n.ExpiresAt,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteNotificationAsync(int notificationId)
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }
    }

    // ========================================================================
    // REPORTING SERVICE
    // ========================================================================

    /// <summary>
    /// Reporting service interface.
    /// </summary>
    public interface IReportingService
    {
        Task<DashboardMetricsDto> GetDashboardMetricsAsync();
        Task<ReportDataDto> GenerateVehicleReportAsync(ReportFilterDto filter);
        Task<ReportDataDto> GenerateContractReportAsync(ReportFilterDto filter);
        Task<ReportDataDto> GenerateMaintenanceReportAsync(ReportFilterDto filter);
    }

    /// <summary>
    /// Reporting service implementation.
    /// </summary>
    public class ReportingService : IReportingService
    {
        private readonly FleetDbContext _context;

        public ReportingService(FleetDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardMetricsDto> GetDashboardMetricsAsync()
        {
            var totalVehicles = await _context.Vehicles.CountAsync();
            var activeVehicles = await _context.Vehicles.CountAsync(v => v.Status == "Active");
            var vehiclesInMaintenance = await _context.MaintenanceRequests.CountAsync(m => m.Status == "Open");
            var totalContracts = await _context.Contracts.CountAsync();
            var activeContracts = await _context.Contracts.CountAsync(c => c.ContractStatusId == 2); // Active status ID
            var expiringContracts = await _context.Contracts.CountAsync(c => c.EndDate <= DateTime.UtcNow.AddDays(30) && c.EndDate > DateTime.UtcNow);
            var openMaintenanceRequests = await _context.MaintenanceRequests.CountAsync(m => m.Status == "Open");
            var completedMaintenanceRequests = await _context.MaintenanceRequests.CountAsync(m => m.Status == "Completed");
            var driversWithExpiringLicenses = await _context.Drivers.CountAsync(d => d.LicenseExpiryDate <= DateTime.UtcNow.AddDays(30) && d.LicenseExpiryDate > DateTime.UtcNow);
            var insurancePoliciesExpiring = await _context.Insurances.CountAsync(i => i.ExpiryDate <= DateTime.UtcNow.AddDays(30) && i.ExpiryDate > DateTime.UtcNow);
            var totalExpenses = await _context.Expenses.SumAsync(e => (decimal?)e.Amount) ?? 0;
            var totalFuelCost = await _context.FuelTransactions.SumAsync(f => (decimal?)f.TotalCost) ?? 0;

            return new DashboardMetricsDto
            {
                TotalVehicles = totalVehicles,
                ActiveVehicles = activeVehicles,
                VehiclesInMaintenance = vehiclesInMaintenance,
                TotalContracts = totalContracts,
                ActiveContracts = activeContracts,
                ExpiringContracts = expiringContracts,
                OpenMaintenanceRequests = openMaintenanceRequests,
                CompletedMaintenanceRequests = completedMaintenanceRequests,
                DriversWithExpiringLicenses = driversWithExpiringLicenses,
                InsurancePoliciesExpiring = insurancePoliciesExpiring,
                TotalExpenses = totalExpenses,
                TotalFuelCost = totalFuelCost
            };
        }

        public async Task<ReportDataDto> GenerateVehicleReportAsync(ReportFilterDto filter)
        {
            var vehicles = await _context.Vehicles
                .Include(v => v.VehicleType)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            var data = vehicles.Select(v => new Dictionary<string, object>
            {
                { "Plate Number", v.PlateNumber },
                { "Type", v.VehicleType.Name },
                { "Model", v.Model },
                { "Year", v.Year },
                { "Status", v.Status },
                { "Mileage", v.CurrentMileage },
                { "Purchase Price", v.PurchasePrice }
            }).ToList();

            return new ReportDataDto
            {
                ReportTitle = "Vehicle Report",
                GeneratedDate = DateTime.UtcNow,
                GeneratedBy = "System",
                Data = data,
                Columns = new List<string> { "Plate Number", "Type", "Model", "Year", "Status", "Mileage", "Purchase Price" }
            };
        }

        public async Task<ReportDataDto> GenerateContractReportAsync(ReportFilterDto filter)
        {
            var contracts = await _context.Contracts
                .Include(c => c.Vehicle)
                .Include(c => c.ContractStatus)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var data = contracts.Select(c => new Dictionary<string, object>
            {
                { "Contract Number", c.ContractNumber },
                { "Vehicle", c.Vehicle.PlateNumber },
                { "Client", c.ClientName },
                { "Status", c.ContractStatus.Name },
                { "Start Date", c.StartDate },
                { "End Date", c.EndDate },
                { "Value", c.ContractValue },
                { "Paid", c.PaidAmount }
            }).ToList();

            return new ReportDataDto
            {
                ReportTitle = "Contract Report",
                GeneratedDate = DateTime.UtcNow,
                GeneratedBy = "System",
                Data = data,
                Columns = new List<string> { "Contract Number", "Vehicle", "Client", "Status", "Start Date", "End Date", "Value", "Paid" }
            };
        }

        public async Task<ReportDataDto> GenerateMaintenanceReportAsync(ReportFilterDto filter)
        {
            var requests = await _context.MaintenanceRequests
                .Include(m => m.Vehicle)
                .Include(m => m.MaintenanceType)
                .Include(m => m.ServiceProvider)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            var data = requests.Select(m => new Dictionary<string, object>
            {
                { "Vehicle", m.Vehicle.PlateNumber },
                { "Type", m.MaintenanceType.Name },
                { "Request Date", m.RequestDate },
                { "Completion Date", m.CompletionDate ?? DateTime.MinValue },
                { "Status", m.Status },
                { "Estimated Cost", m.EstimatedCost },
                { "Actual Cost", m.ActualCost },
                { "Provider", m.ServiceProvider?.Name ?? "Not Assigned" }
            }).ToList();

            return new ReportDataDto
            {
                ReportTitle = "Maintenance Report",
                GeneratedDate = DateTime.UtcNow,
                GeneratedBy = "System",
                Data = data,
                Columns = new List<string> { "Vehicle", "Type", "Request Date", "Completion Date", "Status", "Estimated Cost", "Actual Cost", "Provider" }
            };
        }
    }

    // ========================================================================
    // SETTINGS SERVICE
    // ========================================================================

    /// <summary>
    /// Settings service interface.
    /// </summary>
    public interface ISettingsService
    {
        Task<CompanySettingsDto> GetCompanySettingsAsync();
        Task<CompanySettingsDto> UpdateCompanySettingsAsync(CompanySettingsDto dto);
    }

    // INCOMPLETE SettingsService implementation removed to fix syntax error. Complete and move to its own file if needed.
}
