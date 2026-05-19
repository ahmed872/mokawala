// ============================================================================
// FILE: VehicleServiceTests.cs
// LOCATION: FleetManagementSystem.Tests/Services/VehicleServiceTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetManagementSystem.Tests.Infrastructure;
using Xunit;

namespace FleetManagementSystem.Tests.Services
{
    public class VehicleServiceTests : IntegrationTestBase
    {
        public VehicleServiceTests(IntegrationTestFixture fixture) : base(fixture) { }

        [Fact]
        public async Task CreateVehicle_WithValidData_ShouldSucceed()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle(
                plateNumber: "XYZ-789",
                model: "Honda Civic",
                year: 2023
            );

            // Act
            var result = await VehicleService.CreateVehicleAsync(vehicle);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("XYZ-789", result.PlateNumber);
            Assert.Equal("Honda Civic", result.Model);
        }

        [Fact]
        public async Task CreateVehicle_WithDuplicatePlateNumber_ShouldFail()
        {
            // Arrange
            var vehicle1 = TestDataBuilder.CreateTestVehicle(plateNumber: "DUP-001");
            var vehicle2 = TestDataBuilder.CreateTestVehicle(plateNumber: "DUP-001");

            await VehicleService.CreateVehicleAsync(vehicle1);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => VehicleService.CreateVehicleAsync(vehicle2)
            );
            Assert.Contains("already exists", ex.Message);
        }

        [Fact]
        public async Task GetVehicleById_WithValidId_ShouldReturnVehicle()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var created = await VehicleService.CreateVehicleAsync(vehicle);

            // Act
            var result = await VehicleService.GetVehicleByIdAsync(created.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(created.Id, result.Id);
            Assert.Equal(created.PlateNumber, result.PlateNumber);
        }

        [Fact]
        public async Task GetAllVehicles_ShouldReturnAllVehicles()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                TestDataBuilder.CreateTestVehicle(plateNumber: "V-001"),
                TestDataBuilder.CreateTestVehicle(plateNumber: "V-002"),
                TestDataBuilder.CreateTestVehicle(plateNumber: "V-003")
            };

            foreach (var vehicle in vehicles)
            {
                await VehicleService.CreateVehicleAsync(vehicle);
            }

            // Act
            var result = await VehicleService.GetAllVehiclesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 3);
        }

        [Fact]
        public async Task UpdateVehicle_WithValidData_ShouldSucceed()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var created = await VehicleService.CreateVehicleAsync(vehicle);
            created.Status = "تحت الصيانة";
            created.Mileage = 15000;

            // Act
            var result = await VehicleService.UpdateVehicleAsync(created);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("تحت الصيانة", result.Status);
            Assert.Equal(15000, result.Mileage);
        }

        [Fact]
        public async Task DeleteVehicle_WithValidId_ShouldSucceed()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var created = await VehicleService.CreateVehicleAsync(vehicle);

            // Act
            await VehicleService.DeleteVehicleAsync(created.Id);

            // Assert
            var result = await VehicleService.GetVehicleByIdAsync(created.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task SearchVehicles_ByPlateNumber_ShouldReturnMatches()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                TestDataBuilder.CreateTestVehicle(plateNumber: "ABC-111"),
                TestDataBuilder.CreateTestVehicle(plateNumber: "ABC-222"),
                TestDataBuilder.CreateTestVehicle(plateNumber: "XYZ-333")
            };

            foreach (var vehicle in vehicles)
            {
                await VehicleService.CreateVehicleAsync(vehicle);
            }

            // Act
            var result = await VehicleService.SearchVehiclesAsync("ABC");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
            Assert.All(result, v => Assert.Contains("ABC", v.PlateNumber));
        }

        [Fact]
        public async Task FilterVehiclesByStatus_ShouldReturnOnlyMatchingStatus()
        {
            // Arrange
            var vehicle1 = TestDataBuilder.CreateTestVehicle(plateNumber: "S-001", status: "متاح");
            var vehicle2 = TestDataBuilder.CreateTestVehicle(plateNumber: "S-002", status: "متاح");
            var vehicle3 = TestDataBuilder.CreateTestVehicle(plateNumber: "S-003", status: "تحت الصيانة");

            await VehicleService.CreateVehicleAsync(vehicle1);
            await VehicleService.CreateVehicleAsync(vehicle2);
            await VehicleService.CreateVehicleAsync(vehicle3);

            // Act
            var result = await VehicleService.FilterVehiclesByStatusAsync("متاح");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
            Assert.All(result, v => Assert.Equal("متاح", v.Status));
        }

        [Fact]
        public async Task CreateVehicle_ShouldCreateAuditLog()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();

            // Act
            var created = await VehicleService.CreateVehicleAsync(vehicle);

            // Assert - Check audit log
            var auditLogs = await DbContext.AuditLogs
                .Where(al => al.EntityType == "Vehicle" && al.EntityId == created.Id)
                .ToListAsync();

            Assert.NotEmpty(auditLogs);
            Assert.Contains(auditLogs, al => al.Action == "Create");
        }
    }
}

// ============================================================================
// FILE: ContractServiceTests.cs
// LOCATION: FleetManagementSystem.Tests/Services/ContractServiceTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetManagementSystem.Tests.Infrastructure;
using Xunit;

namespace FleetManagementSystem.Tests.Services
{
    public class ContractServiceTests : IntegrationTestBase
    {
        public ContractServiceTests(IntegrationTestFixture fixture) : base(fixture) { }

        [Fact]
        public async Task CreateContract_WithValidData_ShouldSucceed()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var contract = TestDataBuilder.CreateTestContract(
                contractNumber: "CNT-2024-001",
                vehicleId: createdVehicle.Id,
                clientName: "عميل اختبار"
            );

            // Act
            var result = await ContractService.CreateContractAsync(contract);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("CNT-2024-001", result.ContractNumber);
        }

        [Fact]
        public async Task UpdateContractStatus_FromDraftToActive_ShouldSucceed()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var contract = TestDataBuilder.CreateTestContract(
                vehicleId: createdVehicle.Id,
                statusId: 1 // Draft
            );

            var created = await ContractService.CreateContractAsync(contract);

            // Act
            var result = await ContractService.UpdateContractStatusAsync(created.Id, 2); // Active

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.ContractStatusId);
        }

        [Fact]
        public async Task GetContractsByStatus_ShouldReturnOnlyMatchingStatus()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var contract1 = TestDataBuilder.CreateTestContract(vehicleId: createdVehicle.Id, statusId: 2); // Active
            var contract2 = TestDataBuilder.CreateTestContract(vehicleId: createdVehicle.Id, statusId: 2); // Active
            var contract3 = TestDataBuilder.CreateTestContract(vehicleId: createdVehicle.Id, statusId: 3); // Expired

            await ContractService.CreateContractAsync(contract1);
            await ContractService.CreateContractAsync(contract2);
            await ContractService.CreateContractAsync(contract3);

            // Act
            var result = await ContractService.GetContractsByStatusAsync(2);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
            Assert.All(result, c => Assert.Equal(2, c.ContractStatusId));
        }

        [Fact]
        public async Task GetExpiringContracts_ShouldReturnContractsExpiringWithinDays()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var expiringContract = TestDataBuilder.CreateTestContract(
                vehicleId: createdVehicle.Id,
                endDate: DateTime.UtcNow.AddDays(5)
            );

            var futureContract = TestDataBuilder.CreateTestContract(
                vehicleId: createdVehicle.Id,
                endDate: DateTime.UtcNow.AddDays(100)
            );

            await ContractService.CreateContractAsync(expiringContract);
            await ContractService.CreateContractAsync(futureContract);

            // Act
            var result = await ContractService.GetExpiringContractsAsync(30);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.All(result, c => Assert.True((c.EndDate - DateTime.UtcNow).TotalDays <= 30));
        }

        [Fact]
        public async Task DeleteContract_ShouldSucceed()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var contract = TestDataBuilder.CreateTestContract(vehicleId: createdVehicle.Id);
            var created = await ContractService.CreateContractAsync(contract);

            // Act
            await ContractService.DeleteContractAsync(created.Id);

            // Assert
            var result = await ContractService.GetContractByIdAsync(created.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateContract_ShouldCreateAuditLog()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var contract = TestDataBuilder.CreateTestContract(vehicleId: createdVehicle.Id);

            // Act
            var created = await ContractService.CreateContractAsync(contract);

            // Assert - Check audit log
            var auditLogs = await DbContext.AuditLogs
                .Where(al => al.EntityType == "Contract" && al.EntityId == created.Id)
                .ToListAsync();

            Assert.NotEmpty(auditLogs);
            Assert.Contains(auditLogs, al => al.Action == "Create");
        }
    }
}

// ============================================================================
// FILE: MaintenanceServiceTests.cs
// LOCATION: FleetManagementSystem.Tests/Services/MaintenanceServiceTests.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetManagementSystem.Tests.Infrastructure;
using Xunit;

namespace FleetManagementSystem.Tests.Services
{
    public class MaintenanceServiceTests : IntegrationTestBase
    {
        public MaintenanceServiceTests(IntegrationTestFixture fixture) : base(fixture) { }

        [Fact]
        public async Task CreateMaintenanceRequest_WithValidData_ShouldSucceed()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var maintenance = TestDataBuilder.CreateTestMaintenanceRequest(vehicleId: createdVehicle.Id);

            // Act
            var result = await MaintenanceService.CreateMaintenanceRequestAsync(maintenance);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("مفتوح", result.Status);
        }

        [Fact]
        public async Task MarkMaintenanceAsComplete_ShouldUpdateStatus()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var maintenance = TestDataBuilder.CreateTestMaintenanceRequest(vehicleId: createdVehicle.Id);
            var created = await MaintenanceService.CreateMaintenanceRequestAsync(maintenance);

            // Act
            var result = await MaintenanceService.MarkMaintenanceAsCompleteAsync(created.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("مكتمل", result.Status);
        }

        [Fact]
        public async Task GetOpenMaintenanceRequests_ShouldReturnOnlyOpenRequests()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var open1 = TestDataBuilder.CreateTestMaintenanceRequest(vehicleId: createdVehicle.Id, status: "مفتوح");
            var open2 = TestDataBuilder.CreateTestMaintenanceRequest(vehicleId: createdVehicle.Id, status: "مفتوح");
            var completed = TestDataBuilder.CreateTestMaintenanceRequest(vehicleId: createdVehicle.Id, status: "مكتمل");

            await MaintenanceService.CreateMaintenanceRequestAsync(open1);
            await MaintenanceService.CreateMaintenanceRequestAsync(open2);
            await MaintenanceService.CreateMaintenanceRequestAsync(completed);

            // Act
            var result = await MaintenanceService.GetOpenMaintenanceRequestsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
            Assert.All(result, m => Assert.Equal("مفتوح", m.Status));
        }

        [Fact]
        public async Task GetMaintenanceByVehicle_ShouldReturnVehicleMaintenanceHistory()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var maintenance1 = TestDataBuilder.CreateTestMaintenanceRequest(vehicleId: createdVehicle.Id);
            var maintenance2 = TestDataBuilder.CreateTestMaintenanceRequest(vehicleId: createdVehicle.Id);

            await MaintenanceService.CreateMaintenanceRequestAsync(maintenance1);
            await MaintenanceService.CreateMaintenanceRequestAsync(maintenance2);

            // Act
            var result = await MaintenanceService.GetMaintenanceByVehicleAsync(createdVehicle.Id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
            Assert.All(result, m => Assert.Equal(createdVehicle.Id, m.VehicleId));
        }

        [Fact]
        public async Task DeleteMaintenanceRequest_ShouldSucceed()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var maintenance = TestDataBuilder.CreateTestMaintenanceRequest(vehicleId: createdVehicle.Id);
            var created = await MaintenanceService.CreateMaintenanceRequestAsync(maintenance);

            // Act
            await MaintenanceService.DeleteMaintenanceRequestAsync(created.Id);

            // Assert
            var result = await MaintenanceService.GetMaintenanceByIdAsync(created.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateMaintenanceRequest_ShouldCreateAuditLog()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var maintenance = TestDataBuilder.CreateTestMaintenanceRequest(vehicleId: createdVehicle.Id);

            // Act
            var created = await MaintenanceService.CreateMaintenanceRequestAsync(maintenance);

            // Assert - Check audit log
            var auditLogs = await DbContext.AuditLogs
                .Where(al => al.EntityType == "MaintenanceRequest" && al.EntityId == created.Id)
                .ToListAsync();

            Assert.NotEmpty(auditLogs);
            Assert.Contains(auditLogs, al => al.Action == "Create");
        }

        [Fact]
        public async Task MarkMaintenanceAsComplete_ShouldCreateAuditLog()
        {
            // Arrange
            var vehicle = TestDataBuilder.CreateTestVehicle();
            var createdVehicle = await VehicleService.CreateVehicleAsync(vehicle);

            var maintenance = TestDataBuilder.CreateTestMaintenanceRequest(vehicleId: createdVehicle.Id);
            var created = await MaintenanceService.CreateMaintenanceRequestAsync(maintenance);

            // Act
            await MaintenanceService.MarkMaintenanceAsCompleteAsync(created.Id);

            // Assert - Check audit log
            var auditLogs = await DbContext.AuditLogs
                .Where(al => al.EntityType == "MaintenanceRequest" && al.EntityId == created.Id)
                .ToListAsync();

            Assert.NotEmpty(auditLogs);
            Assert.Contains(auditLogs, al => al.Action == "Update" && al.Changes.Contains("Status"));
        }
    }
}
