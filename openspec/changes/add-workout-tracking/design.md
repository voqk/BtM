## Context

This change introduces workout tracking as a core feature of Build the Mountain. Users need to define their own exercises, create reusable workout plans, and log actual workout sessions with detailed set data. The data model must support future features like progress graphs and exercise sharing between users.

## Goals / Non-Goals

**Goals:**
- Allow users to create and manage custom exercises
- Support three exercise types with different weight calculation logic
- Enable workout plan creation as reusable templates
- Log workout sessions with timestamped set data
- Calculate tonnage as a workout intensity metric
- Preserve historical data for future reporting

**Non-Goals:**
- Progress graphs and reports (future scope)
- Exercise sharing between users (future scope)
- Preset/default exercise library
- Rest timer or workout guidance features
- Integration with external fitness devices/APIs

## Decisions

### Data Model

**Exercise entity:**
```
Exercise
├── Id (Guid)
├── UserId (FK to AspNetUsers)
├── Name (string, max 100)
├── ExerciseType (enum: Standard=0, Bodyweight=1, Assisted=2)
├── CreatedAt (DateTime)
└── Unique constraint: (UserId, Name)
```

**WorkoutPlan entity:**
```
WorkoutPlan
├── Id (Guid)
├── UserId (FK to AspNetUsers)
├── Name (string, max 100)
├── CreatedAt (DateTime)
├── UpdatedAt (DateTime)
└── Unique constraint: (UserId, Name)
```

**PlanExercise entity (join table with payload):**
```
PlanExercise
├── Id (Guid)
├── WorkoutPlanId (FK)
├── ExerciseId (FK)
├── OrderIndex (int) -- for maintaining exercise order
├── Sets (int) -- number of sets
└── TargetReps (int) -- target reps per set
```

**WorkoutSession entity:**
```
WorkoutSession
├── Id (Guid)
├── UserId (FK to AspNetUsers)
├── WorkoutPlanId (FK, nullable after plan deletion)
├── WorkoutPlanName (string) -- denormalized for history
├── BodyWeightLbs (decimal?, nullable)
├── StartedAt (DateTime)
├── CompletedAt (DateTime?, nullable)
└── TonnageLbs (decimal) -- calculated, updated on each set
```

**ExerciseSet entity:**
```
ExerciseSet
├── Id (Guid)
├── WorkoutSessionId (FK)
├── ExerciseId (FK)
├── ExerciseName (string) -- denormalized for history
├── ExerciseType (enum) -- denormalized for calculation
├── SetNumber (int)
├── Reps (int)
├── WeightLbs (decimal) -- positive for standard/bodyweight, negative for assisted
├── RecordedAt (DateTime)
└── Index: (WorkoutSessionId, ExerciseId, SetNumber)
```

### Weight Storage Convention

- **Standard exercises:** Store actual weight lifted (e.g., 135 for 135 lbs)
- **Bodyweight exercises:** Store additional weight only (e.g., 25 for +25 lbs, 0 for bodyweight only)
- **Assisted exercises:** Store assistance as negative (e.g., -50 for 50 lbs assistance)

This convention keeps the `WeightLbs` column simple while the `ExerciseType` determines calculation logic.

### Tonnage Calculation Logic

```
For each set in session:
  if ExerciseType == Standard:
    effective_weight = WeightLbs
  else if ExerciseType == Bodyweight:
    effective_weight = BodyWeightLbs + WeightLbs
  else if ExerciseType == Assisted:
    effective_weight = BodyWeightLbs + WeightLbs  // WeightLbs is negative

  set_tonnage = Reps * effective_weight

Total tonnage = sum of all set_tonnage values
```

Tonnage is recalculated and stored on `WorkoutSession.TonnageLbs` after each set is logged or modified.

### Denormalization Rationale

- `WorkoutPlanName` on `WorkoutSession`: Preserves plan name even if plan is deleted
- `ExerciseName` and `ExerciseType` on `ExerciseSet`: Enables accurate historical tonnage display without joining to Exercise table (which could be deleted)

### Alternatives Considered

1. **Store effective weight instead of raw weight on sets**
   - Rejected: Loses information about what was actually logged; makes editing confusing

2. **Prevent exercise/plan deletion entirely**
   - Rejected: Users should be able to clean up; denormalization handles history

3. **Use weight in kilograms**
   - Considered: Could add unit preference later; starting with lbs for simplicity

## Risks / Trade-offs

- **Denormalized data could drift**: Mitigated by only denormalizing at write time, never updating historical records
- **Tonnage for bodyweight exercises requires weigh-in**: If user skips weigh-in, bodyweight exercises use 0 for body weight (under-counts tonnage). Consider prompting user to enter body weight when session includes bodyweight exercises.

## Migration Plan

1. Create new tables via EF Core migration
2. No existing data to migrate (new feature)
3. Rollback: Drop tables (no data loss for existing features)

## Open Questions

- Should there be a default body weight that persists across sessions (user profile setting)?
- Should we warn users when starting a session with bodyweight exercises but no weigh-in entered?
