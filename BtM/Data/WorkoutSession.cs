namespace BtM.Data;

public class WorkoutSession
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public Guid? WorkoutPlanId { get; set; }
    public string WorkoutPlanName { get; set; } = null!;
    public decimal? BodyWeightLbs { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal TonnageLbs { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public WorkoutPlan? WorkoutPlan { get; set; }
    public ICollection<ExerciseSet> ExerciseSets { get; set; } = [];
}
