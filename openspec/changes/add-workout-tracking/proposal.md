# Change: Add Workout Tracking

## Why

Users need a way to define exercises, create reusable workout plans, and log their actual workout sessions with sets, reps, and weights. This enables progress tracking over time and provides metrics like "tonnage" (total weight moved) to help users gauge workout intensity.

## What Changes

- Add **Exercise** management: users can create exercises categorized as Standard (external weight), Bodyweight (body weight + additional), or Assisted (body weight - assistance)
- Add **Workout Plan** creation: templates containing a list of exercises with target sets and reps
- Add **Workout Session** logging: perform a plan and record actual sets with weight, reps, and timestamp
- Add **Weigh-In** capability: optional body weight entry per session for accurate bodyweight exercise calculations
- Add **Tonnage** calculation: sum of (reps × effective weight) across all sets in a session
- Exercises are user-owned but designed for future sharing capability

## Impact

- Affected specs: `workout-tracking` (new capability)
- Affected code:
  - New EF Core entities: `Exercise`, `WorkoutPlan`, `PlanExercise`, `WorkoutSession`, `ExerciseSet`
  - New repository interfaces and implementations
  - New Blazor components for exercise/plan/session management
  - Database migrations for new tables
