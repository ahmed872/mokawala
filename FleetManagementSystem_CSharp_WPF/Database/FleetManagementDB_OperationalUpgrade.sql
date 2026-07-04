-- Fleet Management System
-- Operational upgrade script for LAN/MySQL deployment
-- Adds oil changes, treasury transactions, and operational linkage columns

USE `FleetManagementDB`;

ALTER TABLE `Vehicles`
    ADD COLUMN IF NOT EXISTS `OilChangeIntervalKm` DECIMAL(10,2) NOT NULL DEFAULT 10000,
    ADD COLUMN IF NOT EXISTS `MaintenanceIntervalKm` DECIMAL(10,2) NOT NULL DEFAULT 15000;

ALTER TABLE `FuelTransactions`
    ADD COLUMN IF NOT EXISTS `PaidFromTreasury` BOOLEAN NOT NULL DEFAULT FALSE,
    ADD COLUMN IF NOT EXISTS `TreasuryTransactionId` INT NULL;

ALTER TABLE `Expenses`
    ADD COLUMN IF NOT EXISTS `PaidFromTreasury` BOOLEAN NOT NULL DEFAULT FALSE,
    ADD COLUMN IF NOT EXISTS `TreasuryTransactionId` INT NULL;

CREATE TABLE IF NOT EXISTS `TreasuryTransactions` (
    `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `TransactionDate` DATETIME NOT NULL,
    `TransactionType` VARCHAR(50) NOT NULL,
    `Amount` DECIMAL(12,2) NOT NULL,
    `Description` VARCHAR(500) NULL,
    `RelatedEntityType` VARCHAR(100) NULL,
    `RelatedEntityId` INT NULL,
    `PaymentMethod` VARCHAR(50) NOT NULL DEFAULT 'Cash',
    `Notes` TEXT NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX `idx_treasury_date` (`TransactionDate`),
    INDEX `idx_treasury_type` (`TransactionType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `OilChanges` (
    `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `VehicleId` INT NOT NULL,
    `ChangeDate` DATETIME NOT NULL,
    `OdometerAtChange` DECIMAL(10,2) NOT NULL,
    `OilType` VARCHAR(100) NULL,
    `Quantity` DECIMAL(10,2) NOT NULL DEFAULT 0,
    `Cost` DECIMAL(12,2) NOT NULL DEFAULT 0,
    `NextOilChangeOdometer` DECIMAL(10,2) NOT NULL,
    `CurrentOdometer` DECIMAL(10,2) NULL,
    `CurrentOdometerDate` DATETIME NULL,
    `IsOilChanged` TINYINT(1) NOT NULL DEFAULT 1,
    `ServiceItems` VARCHAR(50) NOT NULL DEFAULT '',
    `Status` VARCHAR(50) NOT NULL DEFAULT 'Scheduled',
    `Notes` TEXT NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT `fk_oilchange_vehicle`
        FOREIGN KEY (`VehicleId`) REFERENCES `Vehicles` (`Id`) ON DELETE RESTRICT,
    INDEX `idx_oilchange_vehicle` (`VehicleId`),
    INDEX `idx_oilchange_date` (`ChangeDate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

ALTER TABLE `FuelTransactions`
    ADD CONSTRAINT `fk_fuel_treasury`
        FOREIGN KEY (`TreasuryTransactionId`) REFERENCES `TreasuryTransactions` (`Id`) ON DELETE SET NULL;

ALTER TABLE `Expenses`
    ADD CONSTRAINT `fk_expense_treasury`
        FOREIGN KEY (`TreasuryTransactionId`) REFERENCES `TreasuryTransactions` (`Id`) ON DELETE SET NULL;

-- Custody is now held by a User (not a Vehicle): who took the money, how much,
-- how much they've spent from it, and how much has been returned to the treasury.
ALTER TABLE `Custody`
    ADD COLUMN IF NOT EXISTS `UserId` INT NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS `Amount` DECIMAL(18,2) NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS `ReturnedAmount` DECIMAL(18,2) NOT NULL DEFAULT 0;

-- Expenses can now be drawn from a custody instead of (or in addition to being
-- unrelated to) a specific vehicle.
ALTER TABLE `Expenses`
    MODIFY COLUMN `VehicleId` INT NULL,
    ADD COLUMN IF NOT EXISTS `CustodyId` INT NULL;
