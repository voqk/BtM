## ADDED Requirements

### Requirement: SQL Server Database Provider
The application SHALL use Microsoft SQL Server as its database provider via Entity Framework Core.

#### Scenario: Application startup with SQL Server
- **WHEN** the application starts
- **THEN** it SHALL connect to SQL Server using the configured connection string
- **AND** EF Core SHALL use the SQL Server provider for all database operations

#### Scenario: Missing connection string
- **WHEN** the application starts without a configured connection string
- **THEN** it SHALL throw an InvalidOperationException with a descriptive message

### Requirement: Environment-Specific Connection Strings
The application SHALL support different connection strings for development and production environments.

#### Scenario: Development environment uses local SQL Server
- **WHEN** running in Development environment
- **THEN** the application SHALL use the connection string from appsettings.Development.json
- **AND** the development connection SHALL target the local SQL Server instance

#### Scenario: Production environment uses configured SQL Server
- **WHEN** running in Production environment
- **THEN** the application SHALL use the connection string from appsettings.json, User Secrets, or environment variables
- **AND** sensitive connection details SHALL NOT be stored in source control

### Requirement: Entity Framework Migrations for SQL Server
Database migrations SHALL be compatible with SQL Server and generate SQL Server-specific DDL.

#### Scenario: Initial migration creates Identity schema
- **WHEN** migrations are applied to an empty database
- **THEN** all ASP.NET Identity tables SHALL be created
- **AND** the generated SQL SHALL be valid SQL Server syntax

#### Scenario: Migrations can be applied via CLI
- **WHEN** running `dotnet ef database update`
- **THEN** pending migrations SHALL be applied to the target SQL Server database
