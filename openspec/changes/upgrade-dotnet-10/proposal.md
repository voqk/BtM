# Change: Upgrade to .NET 10

## Why
.NET 10 brings performance improvements, new language features (C# 14), and long-term support (LTS). Staying current with the framework ensures access to security patches, improved tooling, and ecosystem compatibility.

## What Changes
- Update `TargetFramework` from `net9.0` to `net10.0`
- Update all NuGet package references to .NET 10 compatible versions
- Update project documentation to reflect .NET 10 and C# 14
- Regenerate EF Core migrations if schema changes are needed

## Impact
- Affected specs: infrastructure (new capability to document framework version)
- Affected code:
  - `BtM/BtM.csproj` - Framework and package updates
  - `openspec/project.md` - Tech stack documentation
  - Existing EF Core migrations may need regeneration

## Migration Notes
- .NET 10 is an LTS release (supported until November 2028)
- No breaking changes expected for Blazor Server apps
- All current NuGet packages have .NET 10 compatible versions available
- PostgreSQL provider (Npgsql) supports .NET 10
