# Change: Update Database from SQLite to SQL Server

## Why
The project documentation (`openspec/project.md`) specifies SQL Server as the target database, but the current implementation uses SQLite. This change aligns the codebase with the documented tech stack and prepares the application for production deployment with a robust, scalable database.

## What Changes
- Replace `Microsoft.EntityFrameworkCore.Sqlite` NuGet package with `Microsoft.EntityFrameworkCore.SqlServer`
- Update `Program.cs` to use `UseSqlServer()` instead of `UseSqlite()`
- Update connection string configuration in `appsettings.json` and `appsettings.Development.json`
- Remove SQLite-specific configuration (app.db file reference)
- Generate new EF Core migrations for SQL Server
- **BREAKING**: Existing SQLite data will need to be migrated manually or recreated

## Impact
- Affected specs: data-access (new capability spec)
- Affected code:
  - `BtM/BtM.csproj` - Package references
  - `BtM/Program.cs` - DbContext configuration
  - `BtM/appsettings.json` - Connection string
  - `BtM/appsettings.Development.json` - Development connection string (new file)
  - `BtM/Data/Migrations/*` - New migrations for SQL Server
