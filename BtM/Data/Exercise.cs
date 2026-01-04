namespace BtM.Data;

public class Exercise
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public ExerciseType ExerciseType { get; set; }
    public DateTime CreatedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public ICollection<PlanExercise> PlanExercises { get; set; } = [];
    public ICollection<ExerciseSet> ExerciseSets { get; set; } = [];
}
