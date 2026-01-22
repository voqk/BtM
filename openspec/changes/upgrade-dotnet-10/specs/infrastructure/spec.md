## ADDED Requirements

### Requirement: Framework Version

The application SHALL target .NET 10 (LTS) with C# 14 language features enabled.

#### Scenario: Project targets .NET 10
- **WHEN** the project is built
- **THEN** it compiles against the `net10.0` target framework

#### Scenario: C# 14 features available
- **WHEN** developers write application code
- **THEN** C# 14 language features are available for use

### Requirement: Package Compatibility

All NuGet package dependencies SHALL be updated to versions compatible with .NET 10.

#### Scenario: EF Core packages updated
- **WHEN** the project references Entity Framework Core packages
- **THEN** they are version 10.x compatible with .NET 10

#### Scenario: ASP.NET packages updated
- **WHEN** the project references ASP.NET Core packages
- **THEN** they are version 10.x compatible with .NET 10

#### Scenario: PostgreSQL provider updated
- **WHEN** the project references Npgsql.EntityFrameworkCore.PostgreSQL
- **THEN** it is a version compatible with EF Core 10 and .NET 10
