# Compile and Run Checklist

## Build

Run from the solution root:

```powershell
dotnet build FleetManagementSystem.sln
```

Expected result:

- build succeeds
- no warnings
- no errors

## Tests

Run:

```powershell
dotnet test FleetManagementSystem.Tests\FleetManagementSystem.Tests.csproj
```

Current smoke coverage includes:

- bootstrap seeding
- vehicle save + audit log
- dashboard metrics
- trip open/close logic
- fuel to treasury linkage
- oil change due metrics

## Run the WPF App

```powershell
dotnet run --project FleetManagementSystem.WPF
```

Or launch the compiled executable:

`FleetManagementSystem.WPF\bin\Debug\net8.0-windows\FleetManagementSystem.WPF.exe`

## Before Hand-off

Verify these items:

- login window opens
- database connection settings window can test and save a LAN MySQL connection
- admin login works
- dashboard loads without crash
- vehicles tab loads
- trips tab opens
- oil changes tab opens
- treasury tab opens
- users/permissions tab opens for admin
- reports tab generates data

## Default Credentials

- `admin`
- `Admin@123`

## Production Reminder

Use `MySQL على الشبكة` in production.

Use `SQLite` only for local development/testing.
