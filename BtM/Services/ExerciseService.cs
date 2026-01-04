using BtM.Data;
using Microsoft.EntityFrameworkCore;

namespace BtM.Services;

public sealed class ExerciseService(ApplicationDbContext db)
{
    public async Task<List<Exercise>> GetUserExercisesAsync(string userId)
    {
        return await db.Exercises
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<Exercise?> GetExerciseAsync(Guid id, string userId)
    {
        return await db.Exercises
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
    }

    public async Task<(bool Success, string? Error)> CreateExerciseAsync(string userId, string name, ExerciseType exerciseType)
    {
        var exists = await db.Exercises.AnyAsync(e => e.UserId == userId && e.Name == name);
        if (exists)
        {
            return (false, "An exercise with this name already exists.");
        }

        var exercise = new Exercise
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name.Trim(),
            ExerciseType = exerciseType,
            CreatedAt = DateTime.UtcNow
        };

        db.Exercises.Add(exercise);
        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateExerciseAsync(Guid id, string userId, string name, ExerciseType exerciseType)
    {
        var exercise = await db.Exercises.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (exercise is null)
        {
            return (false, "Exercise not found.");
        }

        var nameConflict = await db.Exercises.AnyAsync(e => e.UserId == userId && e.Name == name && e.Id != id);
        if (nameConflict)
        {
            return (false, "An exercise with this name already exists.");
        }

        exercise.Name = name.Trim();
        exercise.ExerciseType = exerciseType;
        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteExerciseAsync(Guid id, string userId)
    {
        var exercise = await db.Exercises.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (exercise is null)
        {
            return (false, "Exercise not found.");
        }

        var inUseInPlan = await db.PlanExercises.AnyAsync(pe => pe.ExerciseId == id);
        if (inUseInPlan)
        {
            return (false, "Cannot delete exercise: it is used in one or more workout plans.");
        }

        var inUseInSession = await db.ExerciseSets.AnyAsync(es => es.ExerciseId == id);
        if (inUseInSession)
        {
            return (false, "Cannot delete exercise: it has been logged in workout sessions.");
        }

        db.Exercises.Remove(exercise);
        await db.SaveChangesAsync();
        return (true, null);
    }
}
