using BtM.Data;
using Microsoft.EntityFrameworkCore;

namespace BtM.Services;

public sealed class WorkoutPlanService(ApplicationDbContext db)
{
    public async Task<List<WorkoutPlan>> GetUserPlansAsync(string userId)
    {
        return await db.WorkoutPlans
            .Include(p => p.PlanExercises)
                .ThenInclude(pe => pe.Exercise)
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<WorkoutPlan?> GetPlanAsync(Guid id, string userId)
    {
        return await db.WorkoutPlans
            .Include(p => p.PlanExercises.OrderBy(pe => pe.OrderIndex))
                .ThenInclude(pe => pe.Exercise)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
    }

    public async Task<(bool Success, string? Error, Guid? PlanId)> CreatePlanAsync(string userId, string name)
    {
        var exists = await db.WorkoutPlans.AnyAsync(p => p.UserId == userId && p.Name == name);
        if (exists)
        {
            return (false, "A workout plan with this name already exists.", null);
        }

        var plan = new WorkoutPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.WorkoutPlans.Add(plan);
        await db.SaveChangesAsync();
        return (true, null, plan.Id);
    }

    public async Task<(bool Success, string? Error)> UpdatePlanNameAsync(Guid id, string userId, string name)
    {
        var plan = await db.WorkoutPlans.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (plan is null)
        {
            return (false, "Workout plan not found.");
        }

        var nameConflict = await db.WorkoutPlans.AnyAsync(p => p.UserId == userId && p.Name == name && p.Id != id);
        if (nameConflict)
        {
            return (false, "A workout plan with this name already exists.");
        }

        plan.Name = name.Trim();
        plan.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> AddExerciseToPlanAsync(Guid planId, string userId, Guid exerciseId, int sets, int targetReps)
    {
        var plan = await db.WorkoutPlans
            .Include(p => p.PlanExercises)
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId);

        if (plan is null)
        {
            return (false, "Workout plan not found.");
        }

        var exercise = await db.Exercises.FirstOrDefaultAsync(e => e.Id == exerciseId && e.UserId == userId);
        if (exercise is null)
        {
            return (false, "Exercise not found.");
        }

        var maxOrder = plan.PlanExercises.Any() ? plan.PlanExercises.Max(pe => pe.OrderIndex) : -1;

        var planExercise = new PlanExercise
        {
            Id = Guid.NewGuid(),
            WorkoutPlanId = planId,
            ExerciseId = exerciseId,
            OrderIndex = maxOrder + 1,
            Sets = sets,
            TargetReps = targetReps
        };

        db.PlanExercises.Add(planExercise);
        plan.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdatePlanExerciseAsync(Guid planExerciseId, string userId, int sets, int targetReps)
    {
        var planExercise = await db.PlanExercises
            .Include(pe => pe.WorkoutPlan)
            .FirstOrDefaultAsync(pe => pe.Id == planExerciseId && pe.WorkoutPlan.UserId == userId);

        if (planExercise is null)
        {
            return (false, "Plan exercise not found.");
        }

        planExercise.Sets = sets;
        planExercise.TargetReps = targetReps;
        planExercise.WorkoutPlan.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RemoveExerciseFromPlanAsync(Guid planExerciseId, string userId)
    {
        var planExercise = await db.PlanExercises
            .Include(pe => pe.WorkoutPlan)
            .FirstOrDefaultAsync(pe => pe.Id == planExerciseId && pe.WorkoutPlan.UserId == userId);

        if (planExercise is null)
        {
            return (false, "Plan exercise not found.");
        }

        db.PlanExercises.Remove(planExercise);
        planExercise.WorkoutPlan.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ReorderExercisesAsync(Guid planId, string userId, List<Guid> orderedPlanExerciseIds)
    {
        var plan = await db.WorkoutPlans
            .Include(p => p.PlanExercises)
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId);

        if (plan is null)
        {
            return (false, "Workout plan not found.");
        }

        for (int i = 0; i < orderedPlanExerciseIds.Count; i++)
        {
            var pe = plan.PlanExercises.FirstOrDefault(x => x.Id == orderedPlanExerciseIds[i]);
            if (pe is not null)
            {
                pe.OrderIndex = i;
            }
        }

        plan.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeletePlanAsync(Guid id, string userId)
    {
        var plan = await db.WorkoutPlans.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (plan is null)
        {
            return (false, "Workout plan not found.");
        }

        // Nullify WorkoutPlanId on related sessions (handled in app code due to SQL Server NoAction constraint)
        await db.WorkoutSessions
            .Where(s => s.WorkoutPlanId == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.WorkoutPlanId, (Guid?)null));

        db.WorkoutPlans.Remove(plan);
        await db.SaveChangesAsync();
        return (true, null);
    }
}
