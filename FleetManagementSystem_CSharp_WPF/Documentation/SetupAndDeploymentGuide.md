# Fleet Management System - Setup and Deployment Guide

## Overview

This solution is a Windows desktop fleet operations system built with:

- `C#`
- `WPF`
- `.NET 8`
- `EF Core`
- `Pomelo.EntityFrameworkCore.MySql`
- `MySqlConnector`

Production deployment target:

- One central `MySQL` server inside the company LAN
- 4 to 5 Windows clients running the WPF application
- Shared central database over LAN
- No internet dependency after LAN/database are available

The application also keeps a `SQLite` option for local development/testing only.

## Current Functional Scope

Implemented and wired modules:

- Dashboard
- Vehicles
- Contracts
- Maintenance
- Drivers
- Employees
- Trips
- Fuel
- Expenses
- Oil changes
- Treasury
- Driver licenses
- Vehicle insurance
- Custody
- Master data
- Notifications
- Users and roles
- Reporting
- Company/settings
- Audit logging

## Prerequisites

- Windows 10 or later
- .NET 8 SDK/runtime
- MySQL Server 8.x on the LAN server
- Open LAN access from clients to MySQL port, usually `3306`

## MySQL Server Preparation

1. Install MySQL Server on the internal server machine.
2. Create the target database:

```sql
CREATE DATABASE FleetManagementDB
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;
```

3. Create an application user with LAN access:

```sql
CREATE USER 'fleet_user'@'%' IDENTIFIED BY 'StrongPasswordHere!';
GRANT ALL PRIVILEGES ON FleetManagementDB.* TO 'fleet_user'@'%';
FLUSH PRIVILEGES;
```

4. If the server firewall is enabled, open inbound port `3306` for the LAN only.

## First Client Run

On first run, the application opens `إعدادات قاعدة البيانات` automatically if no valid saved connection exists.

Enter:

- Provider: `MySQL على الشبكة`
- Server/IP: example `192.168.1.10`
- Port: usually `3306`
- Database: `FleetManagementDB`
- Username: `fleet_user`
- Password: your LAN DB password

Then use:

- `اختبار الاتصال` to verify connectivity
- `حفظ` to save the settings locally

The connection profile is stored encrypted on the client machine using Windows user-scoped protection.

## Default Login

- Username: `admin`
- Password: `Admin@123`

This account is seeded automatically on first database initialization.

## Client Configuration Notes

- Each client stores its own encrypted connection profile under local app data.
- Admins can reopen database connection settings from:
  - Login window
  - Main window header
  - Settings tab
- After saving a new connection, the application restarts automatically.

## Build and Run

From the solution root:

```powershell
dotnet build FleetManagementSystem.sln
dotnet test FleetManagementSystem.Tests\FleetManagementSystem.Tests.csproj
dotnet run --project FleetManagementSystem.WPF
```

## Operational Notes

- Trips are blocked if:
  - the vehicle is not available
  - the vehicle registration is expired
  - there is no valid insurance
  - another open trip exists for the same vehicle
  - another open trip exists for the same driver
- Opening a trip moves the vehicle to `InTrip`
- Closing a trip returns the vehicle to `Available` unless open maintenance still exists
- Fuel and expenses can optionally create linked treasury out transactions
- Oil changes generate due indicators when the vehicle approaches the next oil-change odometer

## Roles

Supported roles:

- `Admin`
- `OperationsManager`
- `OperationsDataEntry`
- `MaintenanceOfficer`
- `TreasuryOfficer`
- `Viewer`

The current implementation applies module-level visibility by role and read-only behavior for `Viewer`.

## Recommended LAN Rollout

1. Prepare MySQL on the server.
2. Run the application once on one client and verify login.
3. Enter sample master data:
   - vehicle types
   - vehicles
   - drivers
   - employees
   - insurance
4. Test trip open/close flow.
5. Deploy the same desktop build to the remaining client machines.
6. Configure the same server/IP on each client through the connection settings window.

## Troubleshooting

- If startup shows the database settings window repeatedly, the saved connection is unreachable or invalid.
- If trips cannot be saved, check insurance and registration expiry first.
- If treasury balances look wrong, review linked fuel/expense rows with `PaidFromTreasury = true`.
- If the UI opens but data is missing, confirm all clients point to the same MySQL server and database.
