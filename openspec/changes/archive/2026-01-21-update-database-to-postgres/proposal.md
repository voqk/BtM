# Change: Update Database from SQL Server to PostgreSQL

## Why
PostgreSQL is a robust, open-source database with excellent EF Core support. Switching from SQL Server eliminates licensing costs and provides consistent behavior across development and production environments. Additionally, PostgreSQL naming conventions (snake_case) will be applied to all entities for idiomatic database design.

## What Changes
- **BREAKING**: Replace SQL Server with PostgreSQL as the database provider
- **BREAKING**: All table and column names will use snake_case naming convention instead of PascalCase
- **BREAKING**: ASP.NET Identity tables will be renamed to use snake_case (e.g., `AspNetUsers` → `asp_net_users`)
- Replace `Microsoft.EntityFrameworkCore.SqlServer` package with `Npgsql.EntityFrameworkCore.PostgreSQL`
- Update `Program.cs` to use `UseNpgsql()` instead of `UseSqlServer()`
- Update connection string format for PostgreSQL
- Configure EF Core to use snake_case naming via `NpgsqlSnakeCaseNamingConvention` or custom naming conventions
- Remove existing SQL Server migrations and generate new PostgreSQL migrations
- Update `openspec/project.md` to reflect PostgreSQL as the database technology

## Impact
- Affected specs: data-access (new capability spec)
- Affected code:
  - `BtM/BtM.csproj` - Package references
  - `BtM/Program.cs` - DbContext configuration with naming conventions
  - `BtM/Data/ApplicationDbContext.cs` - Configure snake_case naming for all entities
  - `BtM/appsettings.json` - PostgreSQL connection string
  - `BtM/appsettings.Development.json` - Development PostgreSQL connection string
  - `BtM/Data/Migrations/*` - Delete and regenerate all migrations
  - `openspec/project.md` - Update tech stack documentation

## Table Naming Changes

### Application Entities
| Current (PascalCase) | New (snake_case) |
|---------------------|------------------|
| Exercises | exercises |
| WorkoutPlans | workout_plans |
| PlanExercises | plan_exercises |
| WorkoutSessions | workout_sessions |
| ExerciseSets | exercise_sets |

### Identity Entities
| Current | New (snake_case) |
|---------|------------------|
| AspNetUsers | asp_net_users |
| AspNetRoles | asp_net_roles |
| AspNetUserRoles | asp_net_user_roles |
| AspNetUserClaims | asp_net_user_claims |
| AspNetUserLogins | asp_net_user_logins |
| AspNetUserTokens | asp_net_user_tokens |
| AspNetRoleClaims | asp_net_role_claims |

### Column Naming
All columns will be converted to snake_case:
- `UserId` → `user_id`
- `WorkoutPlanId` → `workout_plan_id`
- `CreatedAt` → `created_at`
- `ExerciseName` → `exercise_name`
- etc.

## Data Migration
Data preservation is **not required** for this change. The database will be recreated from scratch with new migrations.
