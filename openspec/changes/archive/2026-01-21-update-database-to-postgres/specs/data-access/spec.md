## ADDED Requirements

### Requirement: PostgreSQL Database Provider
The application SHALL use PostgreSQL as the primary database provider via the `Npgsql.EntityFrameworkCore.PostgreSQL` package.

#### Scenario: Database connection established
- **WHEN** the application starts
- **THEN** it connects to PostgreSQL using the configured connection string

#### Scenario: EF Core operations function correctly
- **WHEN** CRUD operations are performed via Entity Framework Core
- **THEN** they execute successfully against the PostgreSQL database

### Requirement: Snake Case Naming Convention
All database tables and columns SHALL use snake_case naming convention (lowercase with underscores).

#### Scenario: Application entity tables use snake_case
- **WHEN** EF Core generates the database schema
- **THEN** tables are named: `exercises`, `workout_plans`, `plan_exercises`, `workout_sessions`, `exercise_sets`

#### Scenario: Identity tables use snake_case
- **WHEN** EF Core generates ASP.NET Identity tables
- **THEN** tables are named: `asp_net_users`, `asp_net_roles`, `asp_net_user_roles`, `asp_net_user_claims`, `asp_net_user_logins`, `asp_net_user_tokens`, `asp_net_role_claims`

#### Scenario: Column names use snake_case
- **WHEN** EF Core generates column names
- **THEN** columns use snake_case format (e.g., `user_id`, `workout_plan_id`, `created_at`, `exercise_name`)

#### Scenario: Primary key columns use snake_case
- **WHEN** EF Core generates primary key columns
- **THEN** they are named `id` for simple keys or `{table}_id` pattern for composite keys

#### Scenario: Foreign key columns use snake_case
- **WHEN** EF Core generates foreign key columns
- **THEN** they follow the pattern `{referenced_table}_id` in snake_case

### Requirement: PostgreSQL Connection Configuration
The application SHALL read PostgreSQL connection settings from configuration files.

#### Scenario: Production connection string configured
- **WHEN** the application runs in production
- **THEN** it uses the connection string from `appsettings.json`

#### Scenario: Development connection string configured
- **WHEN** the application runs in development
- **THEN** it uses the connection string from `appsettings.Development.json`

#### Scenario: Connection string format
- **WHEN** a PostgreSQL connection string is configured
- **THEN** it follows the format: `Host=<host>;Database=<db>;Username=<user>;Password=<pass>`

## REMOVED Requirements

### Requirement: SQL Server Database Provider
**Reason**: Replacing SQL Server with PostgreSQL for cost-effectiveness and open-source benefits.
**Migration**: All SQL Server-specific code and configurations will be removed and replaced with PostgreSQL equivalents.

### Requirement: SQL Server Multiple Cascade Path Workarounds
**Reason**: PostgreSQL does not have the same multiple cascade path limitations as SQL Server. The NoAction workarounds documented in `project.md` may be simplified or removed.
**Migration**: Review cascade delete configurations and simplify where PostgreSQL allows.
