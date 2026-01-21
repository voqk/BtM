## Context
The application currently uses SQL Server with PascalCase naming conventions. PostgreSQL is the preferred database due to its open-source nature, cost effectiveness, and widespread adoption. PostgreSQL conventions favor snake_case for table and column names.

## Goals / Non-Goals

**Goals:**
- Replace SQL Server with PostgreSQL
- Apply snake_case naming conventions to all tables and columns
- Configure EF Identity tables to use snake_case naming
- Maintain all existing functionality and relationships

**Non-Goals:**
- Migrate existing data (fresh start is acceptable)
- Change application-level naming (C# remains PascalCase)
- Modify entity relationships or business logic

## Decisions

### Decision 1: Use Npgsql EF Core Provider
**What:** Use `Npgsql.EntityFrameworkCore.PostgreSQL` package for PostgreSQL support.
**Why:** Official, well-maintained provider with full EF Core feature support.

### Decision 2: Use EFCore.NamingConventions for snake_case
**What:** Use `EFCore.NamingConventions` package with `.UseSnakeCaseNamingConvention()`.
**Why:** Automatically converts all table and column names without manual configuration. Cleaner than manually specifying `ToTable()` and `HasColumnName()` for every entity.

**Alternatives considered:**
- Manual `ToTable()` and `HasColumnName()` calls: More verbose, error-prone, requires updating every entity
- Custom `IDbContextOptionsExtension`: Over-engineered for this use case

### Decision 3: Configure Identity Table Names via ModelBuilder
**What:** Override Identity table names in `OnModelCreating` to ensure snake_case.
**Why:** The naming convention package applies to EF Core entities, but Identity tables may need explicit configuration to ensure consistency.

```csharp
// In OnModelCreating
builder.Entity<ApplicationUser>().ToTable("asp_net_users");
builder.Entity<IdentityRole>().ToTable("asp_net_roles");
// ... etc for all Identity entities
```

### Decision 4: Delete All Existing Migrations
**What:** Remove all files in `Data/Migrations/` and generate fresh migrations for PostgreSQL.
**Why:** SQL Server migrations are incompatible with PostgreSQL. Since data preservation is not required, starting fresh is cleaner.

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| EF Core naming convention package may not cover all cases | Verify all tables after migration; add manual overrides if needed |
| PostgreSQL-specific SQL differences (e.g., no multiple cascade paths issue) | Review and potentially simplify cascade delete configurations |
| Development environment requires PostgreSQL | Document Docker setup or use cloud-hosted dev database |

## Migration Plan

1. **Add NuGet packages:**
   - Remove `Microsoft.EntityFrameworkCore.SqlServer`
   - Add `Npgsql.EntityFrameworkCore.PostgreSQL`
   - Add `EFCore.NamingConventions`

2. **Update Program.cs:**
   - Replace `UseSqlServer()` with `UseNpgsql()`
   - Add `.UseSnakeCaseNamingConvention()`

3. **Update ApplicationDbContext:**
   - Configure Identity table names explicitly
   - Remove SQL Server-specific cascade workarounds if PostgreSQL handles them natively

4. **Update connection strings:**
   - `appsettings.json` and `appsettings.Development.json` with PostgreSQL format

5. **Regenerate migrations:**
   - Delete `Data/Migrations/*`
   - Run `dotnet ef migrations add InitialCreate`

6. **Update project documentation:**
   - Modify `openspec/project.md` to reflect PostgreSQL
   - Remove SQL Server-specific constraints section

7. **Verify:**
   - Run `dotnet ef database update`
   - Confirm all tables use snake_case naming

## Open Questions
- Should we add a Docker Compose file for local PostgreSQL development?
- Do we need to update any CI/CD pipelines for PostgreSQL?
