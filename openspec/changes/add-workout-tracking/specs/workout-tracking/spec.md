## ADDED Requirements

### Requirement: Exercise Management

The system SHALL allow users to create, view, edit, and delete exercises. Each exercise SHALL have a name and an exercise type. Exercise types SHALL be: **Standard** (uses external weight only), **Bodyweight** (uses body weight plus additional weight), or **Assisted** (uses body weight minus assistance weight). Each exercise SHALL be owned by a single user. Exercise names SHALL be unique per user.

#### Scenario: Create a standard exercise
- **GIVEN** a logged-in user
- **WHEN** the user creates an exercise named "Bench Press" with type "Standard"
- **THEN** the exercise is saved and associated with the user
- **AND** the exercise appears in the user's exercise list

#### Scenario: Create a bodyweight exercise
- **GIVEN** a logged-in user
- **WHEN** the user creates an exercise named "Pull-ups" with type "Bodyweight"
- **THEN** the exercise is saved with the bodyweight type
- **AND** the exercise can be used in workout plans

#### Scenario: Create an assisted exercise
- **GIVEN** a logged-in user
- **WHEN** the user creates an exercise named "Assisted Pull-ups" with type "Assisted"
- **THEN** the exercise is saved with the assisted type
- **AND** the exercise can track assistance weight as a negative value

#### Scenario: Duplicate exercise name rejected
- **GIVEN** a user who already has an exercise named "Bench Press"
- **WHEN** the user attempts to create another exercise named "Bench Press"
- **THEN** the system rejects the creation with a duplicate name error

#### Scenario: Delete exercise
- **GIVEN** a user with an exercise not used in any workout plan or session
- **WHEN** the user deletes the exercise
- **THEN** the exercise is removed from the system

---

### Requirement: Workout Plan Management

The system SHALL allow users to create, view, edit, and delete workout plans. Each workout plan SHALL have a name and contain an ordered list of plan exercises. Each plan exercise SHALL reference an exercise and specify the number of sets and target reps. Workout plans SHALL be owned by a single user. Plan names SHALL be unique per user.

#### Scenario: Create a workout plan
- **GIVEN** a logged-in user with exercises "Bench Press" and "Incline Press"
- **WHEN** the user creates a workout plan named "Chest Day" with:
  - Bench Press: 3 sets, 10 target reps
  - Incline Press: 3 sets, 8 target reps
- **THEN** the workout plan is saved with the exercises in order
- **AND** the plan appears in the user's workout plans list

#### Scenario: Edit workout plan exercises
- **GIVEN** a user with a workout plan "Chest Day"
- **WHEN** the user adds "Dumbbell Fly" with 3 sets, 12 target reps
- **THEN** the exercise is added to the end of the plan
- **AND** the plan maintains the existing exercises

#### Scenario: Reorder exercises in plan
- **GIVEN** a workout plan with exercises in order A, B, C
- **WHEN** the user reorders to B, A, C
- **THEN** the plan saves with the new order

#### Scenario: Delete workout plan
- **GIVEN** a user with a workout plan
- **WHEN** the user deletes the workout plan
- **THEN** the plan is removed
- **AND** associated workout sessions are NOT deleted (historical data preserved)

---

### Requirement: Workout Session Logging

The system SHALL allow users to start a workout session from a workout plan. Each session SHALL record a start timestamp and optionally a completion timestamp. Users SHALL be able to enter an optional body weight (weigh-in) at the start of the session. The session SHALL track which workout plan was used.

#### Scenario: Start a workout session
- **GIVEN** a user with a workout plan "Chest Day"
- **WHEN** the user starts a workout session for "Chest Day"
- **THEN** a new session is created with the current timestamp
- **AND** the session is associated with the "Chest Day" plan

#### Scenario: Start session with weigh-in
- **GIVEN** a user starting a workout session
- **WHEN** the user enters body weight of 180 lbs
- **THEN** the session records the body weight
- **AND** the body weight is used for bodyweight exercise calculations

#### Scenario: Complete a workout session
- **GIVEN** an active workout session
- **WHEN** the user marks the session as complete
- **THEN** the completion timestamp is recorded

---

### Requirement: Exercise Set Logging

The system SHALL allow users to log individual sets during a workout session. Each set SHALL record the exercise, set number, reps performed, weight used, and timestamp. For bodyweight exercises, the weight represents additional weight (positive) or zero. For assisted exercises, the weight represents assistance (stored as negative). Users SHALL be able to log more or fewer sets than specified in the plan.

#### Scenario: Log a standard exercise set
- **GIVEN** an active workout session with "Bench Press" (Standard type)
- **WHEN** the user logs set 1 with 10 reps at 135 lbs
- **THEN** the set is saved with exercise, set number, reps, weight, and timestamp

#### Scenario: Log a bodyweight exercise set with additional weight
- **GIVEN** an active workout session with body weight 180 lbs
- **AND** "Weighted Pull-ups" (Bodyweight type)
- **WHEN** the user logs set 1 with 7 reps at +25 lbs additional weight
- **THEN** the set is saved with 25 lbs as the weight value
- **AND** the effective weight for tonnage is calculated as 180 + 25 = 205 lbs

#### Scenario: Log a bodyweight exercise set without additional weight
- **GIVEN** an active workout session with body weight 180 lbs
- **AND** "Pull-ups" (Bodyweight type)
- **WHEN** the user logs set 1 with 10 reps at 0 lbs additional weight
- **THEN** the effective weight for tonnage is the body weight (180 lbs)

#### Scenario: Log an assisted exercise set
- **GIVEN** an active workout session with body weight 180 lbs
- **AND** "Assisted Pull-ups" (Assisted type)
- **WHEN** the user logs set 1 with 8 reps at -50 lbs assistance
- **THEN** the set is saved with -50 lbs as the weight value
- **AND** the effective weight for tonnage is calculated as 180 - 50 = 130 lbs

#### Scenario: Log extra sets beyond plan
- **GIVEN** a workout plan specifying 3 sets of Bench Press
- **WHEN** the user logs a 4th set
- **THEN** the system accepts and saves the additional set

---

### Requirement: Tonnage Calculation

The system SHALL calculate tonnage (total weight moved) for each workout session. Tonnage SHALL be the sum of (reps × effective weight) for all logged sets. For Standard exercises, effective weight equals the logged weight. For Bodyweight exercises, effective weight equals session body weight plus the logged weight. For Assisted exercises, effective weight equals session body weight minus the absolute value of the logged assistance. The tonnage SHALL be displayed to the user during and after the session.

#### Scenario: Calculate tonnage for standard exercises
- **GIVEN** a completed session with:
  - Bench Press: 10 reps × 135 lbs, 8 reps × 135 lbs, 6 reps × 155 lbs
- **WHEN** tonnage is calculated
- **THEN** tonnage = (10×135) + (8×135) + (6×155) = 1350 + 1080 + 930 = 3,360 lbs

#### Scenario: Calculate tonnage for bodyweight exercises
- **GIVEN** a session with body weight 180 lbs
- **AND** Pull-ups (Bodyweight): 10 reps × 0 additional, 8 reps × 0 additional
- **AND** Weighted Pull-ups (Bodyweight): 7 reps × +25 lbs
- **WHEN** tonnage is calculated
- **THEN** tonnage = (10×180) + (8×180) + (7×205) = 1800 + 1440 + 1435 = 4,675 lbs

#### Scenario: Calculate tonnage for assisted exercises
- **GIVEN** a session with body weight 180 lbs
- **AND** Assisted Pull-ups: 10 reps × -50 lbs assistance
- **WHEN** tonnage is calculated
- **THEN** tonnage = 10 × (180 - 50) = 10 × 130 = 1,300 lbs

#### Scenario: Display running tonnage during session
- **GIVEN** an active workout session
- **WHEN** the user logs each set
- **THEN** the running tonnage total is updated and displayed

---

### Requirement: Workout History Preservation

The system SHALL preserve all workout session data including sets, reps, weights, and timestamps for historical reporting. Deleting an exercise that has been used in logged sets SHALL be prevented. Deleting a workout plan SHALL NOT delete associated historical sessions.

#### Scenario: Prevent deletion of used exercise
- **GIVEN** an exercise "Bench Press" that has been logged in a session
- **WHEN** the user attempts to delete the exercise
- **THEN** the system prevents deletion with an error message explaining the exercise is in use

#### Scenario: Historical sessions preserved after plan deletion
- **GIVEN** a workout plan "Chest Day" with 5 completed sessions
- **WHEN** the user deletes the "Chest Day" plan
- **THEN** the 5 historical sessions and their logged sets remain in the system
- **AND** the sessions indicate the original plan name for reference
