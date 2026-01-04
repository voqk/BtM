# Project Context

## Purpose
**Build the Mountain (BtM)** is a workout, habit, and goal tracking application. The app helps users build consistent habits and track their fitness progress toward their goals.

## Tech Stack
- **.NET 9** - Target framework
- **Blazor Server** - Interactive server-side rendering
- **ASP.NET Identity** - Authentication and user management
- **Entity Framework Core** - ORM and data access
- **SQL Server** - Database
- **C# 13** - Primary language with nullable reference types enabled

## Project Conventions

### Code Style
- Use file-scoped namespaces (`namespace BtM.Data;`)
- Enable nullable reference types (`<Nullable>enable</Nullable>`)
- Use primary constructors where appropriate
- Follow standard .NET naming conventions:
  - PascalCase for public members, types, and methods
  - camelCase for local variables and parameters
  - _camelCase for private fields
- Prefer expression-bodied members for simple getters/methods

### Architecture Patterns
- **Component-based UI**: Razor components organized in `/Components`
- **Feature folders**: Group related components, pages, and logic by feature
- **Repository pattern**: Abstract data access behind repository interfaces
- **Dependency injection**: Use built-in DI container for all services

### Testing Strategy
- **xUnit** for unit and integration tests
- **bUnit** for Blazor component testing
- Test naming: `MethodName_Scenario_ExpectedBehavior`
- Aim for high coverage on business logic and services
- Integration tests for critical user flows

### Git Workflow
- **Feature branches** merged to `main` via pull requests
- Branch naming: `feature/`, `bugfix/`, `hotfix/` prefixes
- Conventional commits recommended (e.g., `feat:`, `fix:`, `docs:`)
- Require PR reviews before merging to main

## Domain Context
- **Workouts**: Exercise sessions with sets, reps, weights, and duration
- **Habits**: Daily/weekly recurring activities users want to track
- **Goals**: Target outcomes users are working toward (e.g., "Run a 5K", "Lose 10 lbs")
- **Progress tracking**: Historical data showing improvement over time
- **Streaks**: Consecutive days/weeks of habit completion

## Important Constraints
- **User data ownership**: Each user owns their data (private by default) but can selectively share specific data with other users
- **Data privacy**: User fitness and health data requires careful handling

## SQL Server & Entity Framework Constraints

### Multiple Cascade Paths
SQL Server does not allow multiple cascade paths to the same table. This occurs when deleting a parent entity could cascade-delete a child through more than one path.

**Example of problematic design:**
```
User -> WorkoutPlan (Cascade)
User -> WorkoutSession (Cascade)
WorkoutPlan -> WorkoutSession (Cascade or SetNull)
```
Deleting a User creates two paths to WorkoutSession, which SQL Server rejects.

**Solution patterns:**
1. **Use `NoAction` for secondary paths**: Keep cascade on the primary ownership path (User -> Entity), use `NoAction` on relationships between user-owned entities
2. **Handle in application code**: When using `NoAction`, implement the cascade/set-null logic in service methods before deleting the parent
3. **Use `Restrict`**: For relationships where deletion should be blocked if children exist

**Guidelines for new entities:**
- Each user-owned entity should have exactly ONE cascade path from User
- Relationships between user-owned entities (e.g., WorkoutPlan -> WorkoutSession) should use `NoAction` or `Restrict`
- When using `NoAction` where SetNull semantics are desired, nullify the FK in application code before deleting the parent
- Document cascade behavior in DbContext with comments explaining why NoAction was chosen

## External Dependencies
- None currently configured
- Future considerations: Email service, push notifications, external fitness APIs
