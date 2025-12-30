## Context
The BtM application currently uses SQLite for local development simplicity, but the documented tech stack specifies SQL Server. Moving to SQL Server enables production-ready features including better concurrency, larger scale support, and enterprise database tooling.

## Goals / Non-Goals
- Goals:
  - Configure EF Core to use SQL Server provider
  - Support both local SQL Server (or LocalDB) for development and remote SQL Server for production
  - Maintain existing schema and Identity tables
- Non-Goals:
  - Data migration from existing SQLite database (manual process if needed)
  - Setting up SQL Server infrastructure (assumed available)
  - Implementing connection resilience or retry policies (future enhancement)

## Decisions
- **Decision**: Use local SQL Server instance for development
  - Developer has SQL Server installed locally, providing full feature parity with production
  - Connection string will target the local instance (e.g., `Server=localhost;Database=BtM;...`)
  - Alternatives considered:
    - LocalDB - Lightweight but limited features compared to full SQL Server
    - Docker SQL Server container - More portable but unnecessary given local installation
    - Keep SQLite for development - Inconsistent with production, may hide SQL Server-specific issues

- **Decision**: Use environment-specific connection strings
  - `appsettings.Development.json` for local SQL Server connection
  - `appsettings.json` / User Secrets / Environment variables for production
  - This follows ASP.NET Core configuration best practices

- **Decision**: Regenerate migrations from scratch for SQL Server
  - SQLite migrations may contain SQLite-specific SQL
  - Cleaner to start fresh with SQL Server-compatible migrations
  - Alternatives considered:
    - Modify existing migrations - Error-prone and may miss SQLite-specific syntax

## Risks / Trade-offs
- **Data Loss Risk**: Existing SQLite data will not be automatically migrated
  - Mitigation: Document manual data export/import process if needed
  - For development, losing test data is acceptable
- **Development Environment**: Developers need SQL Server installed locally
  - Mitigation: Clear setup instructions in README
- **CI/CD Pipeline**: May need SQL Server container for integration tests
  - Mitigation: Can use SQLite for unit tests, SQL Server for integration tests (future consideration)

## Migration Plan
1. Update NuGet packages (remove SQLite, add SqlServer)
2. Update `Program.cs` to use `UseSqlServer()`
3. Add development connection string for LocalDB
4. Delete existing SQLite migrations
5. Create new initial migration for SQL Server
6. Update `.csproj` to remove app.db reference
7. Test Identity and data operations
8. Rollback: Revert package and code changes if issues arise

## Open Questions
- Should we maintain SQLite support for unit testing? (Deferred - not in scope)
- What SQL Server version should be minimum supported? (Suggest SQL Server 2019+)
