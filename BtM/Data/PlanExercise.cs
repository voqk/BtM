namespace BtM.Data;

public class PlanExercise
{
    public Guid Id { get; set; }
    public Guid WorkoutPlanId { get; set; }
    public Guid ExerciseId { get; set; }
    public int OrderIndex { get; set; }
    public int Sets { get; set; }
    public int TargetReps { get; set; }

    public WorkoutPlan WorkoutPlan { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
}
