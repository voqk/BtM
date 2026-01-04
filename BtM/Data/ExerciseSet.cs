namespace BtM.Data;

public class ExerciseSet
{
    public Guid Id { get; set; }
    public Guid WorkoutSessionId { get; set; }
    public Guid ExerciseId { get; set; }
    public string ExerciseName { get; set; } = null!;
    public ExerciseType ExerciseType { get; set; }
    public int SetNumber { get; set; }
    public int Reps { get; set; }
    public decimal WeightLbs { get; set; }
    public DateTime RecordedAt { get; set; }

    public WorkoutSession WorkoutSession { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
}
