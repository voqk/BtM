using BtM.Data;
using Microsoft.EntityFrameworkCore;

namespace BtM.Services;

public sealed class WorkoutSessionService(ApplicationDbContext db)
{
    public async Task<List<WorkoutSession>> GetUserSessionsAsync(string userId, int limit = 50)
    {
        return await db.WorkoutSessions
            .Include(s => s.ExerciseSets)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<WorkoutSession?> GetSessionAsync(Guid id, string userId)
    {
        return await db.WorkoutSessions
            .Include(s => s.ExerciseSets.OrderBy(es => es.RecordedAt))
            .Include(s => s.WorkoutPlan)
                .ThenInclude(p => p!.PlanExercises.OrderBy(pe => pe.OrderIndex))
                    .ThenInclude(pe => pe.Exercise)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    public async Task<WorkoutSession?> GetActiveSessionAsync(string userId)
    {
        return await db.WorkoutSessions
            .Include(s => s.ExerciseSets.OrderBy(es => es.RecordedAt))
            .Include(s => s.WorkoutPlan)
                .ThenInclude(p => p!.PlanExercises.OrderBy(pe => pe.OrderIndex))
                    .ThenInclude(pe => pe.Exercise)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.CompletedAt == null);
    }

    public async Task<(bool Success, string? Error, Guid? SessionId)> StartSessionAsync(
        string userId,
        Guid planId,
        decimal? bodyWeightLbs)
    {
        var existingActive = await db.WorkoutSessions.AnyAsync(s => s.UserId == userId && s.CompletedAt == null);
        if (existingActive)
        {
            return (false, "You already have an active workout session. Complete or cancel it first.", null);
        }

        var plan = await db.WorkoutPlans.FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId);
        if (plan is null)
        {
            return (false, "Workout plan not found.", null);
        }

        var session = new WorkoutSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            WorkoutPlanId = planId,
            WorkoutPlanName = plan.Name,
            BodyWeightLbs = bodyWeightLbs,
            StartedAt = DateTime.UtcNow,
            TonnageLbs = 0
        };

        db.WorkoutSessions.Add(session);
        await db.SaveChangesAsync();
        return (true, null, session.Id);
    }

    public async Task<(bool Success, string? Error)> UpdateBodyWeightAsync(Guid sessionId, string userId, decimal? bodyWeightLbs)
    {
        var session = await db.WorkoutSessions
            .Include(s => s.ExerciseSets)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        if (session is null)
        {
            return (false, "Session not found.");
        }

        if (session.CompletedAt is not null)
        {
            return (false, "Cannot modify a completed session.");
        }

        session.BodyWeightLbs = bodyWeightLbs;
        RecalculateTonnage(session);
        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> LogSetAsync(
        Guid sessionId,
        string userId,
        Guid exerciseId,
        int reps,
        decimal weightLbs)
    {
        var session = await db.WorkoutSessions
            .Include(s => s.ExerciseSets)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        if (session is null)
        {
            return (false, "Session not found.");
        }

        if (session.CompletedAt is not null)
        {
            return (false, "Cannot add sets to a completed session.");
        }

        var exercise = await db.Exercises.FirstOrDefaultAsync(e => e.Id == exerciseId && e.UserId == userId);
        if (exercise is null)
        {
            return (false, "Exercise not found.");
        }

        var existingSetsForExercise = session.ExerciseSets.Count(es => es.ExerciseId == exerciseId);

        var set = new ExerciseSet
        {
            Id = Guid.NewGuid(),
            WorkoutSessionId = sessionId,
            ExerciseId = exerciseId,
            ExerciseName = exercise.Name,
            ExerciseType = exercise.ExerciseType,
            SetNumber = existingSetsForExercise + 1,
            Reps = reps,
            WeightLbs = weightLbs,
            RecordedAt = DateTime.UtcNow
        };

        db.ExerciseSets.Add(set);
        session.ExerciseSets.Add(set);
        RecalculateTonnage(session);
        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateSetAsync(
        Guid setId,
        string userId,
        int reps,
        decimal weightLbs)
    {
        var set = await db.ExerciseSets
            .Include(es => es.WorkoutSession)
                .ThenInclude(s => s.ExerciseSets)
            .FirstOrDefaultAsync(es => es.Id == setId && es.WorkoutSession.UserId == userId);

        if (set is null)
        {
            return (false, "Set not found.");
        }

        if (set.WorkoutSession.CompletedAt is not null)
        {
            return (false, "Cannot modify sets in a completed session.");
        }

        set.Reps = reps;
        set.WeightLbs = weightLbs;
        RecalculateTonnage(set.WorkoutSession);
        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteSetAsync(Guid setId, string userId)
    {
        var set = await db.ExerciseSets
            .Include(es => es.WorkoutSession)
                .ThenInclude(s => s.ExerciseSets)
            .FirstOrDefaultAsync(es => es.Id == setId && es.WorkoutSession.UserId == userId);

        if (set is null)
        {
            return (false, "Set not found.");
        }

        if (set.WorkoutSession.CompletedAt is not null)
        {
            return (false, "Cannot delete sets from a completed session.");
        }

        var session = set.WorkoutSession;
        var exerciseId = set.ExerciseId;
        var deletedSetNumber = set.SetNumber;

        db.ExerciseSets.Remove(set);
        session.ExerciseSets.Remove(set);

        foreach (var remainingSet in session.ExerciseSets
            .Where(es => es.ExerciseId == exerciseId && es.SetNumber > deletedSetNumber))
        {
            remainingSet.SetNumber--;
        }

        RecalculateTonnage(session);
        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CompleteSessionAsync(Guid sessionId, string userId)
    {
        var session = await db.WorkoutSessions.FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
        if (session is null)
        {
            return (false, "Session not found.");
        }

        if (session.CompletedAt is not null)
        {
            return (false, "Session is already completed.");
        }

        session.CompletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CancelSessionAsync(Guid sessionId, string userId)
    {
        var session = await db.WorkoutSessions
            .Include(s => s.ExerciseSets)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        if (session is null)
        {
            return (false, "Session not found.");
        }

        if (session.CompletedAt is not null)
        {
            return (false, "Cannot cancel a completed session.");
        }

        db.ExerciseSets.RemoveRange(session.ExerciseSets);
        db.WorkoutSessions.Remove(session);
        await db.SaveChangesAsync();
        return (true, null);
    }

    private void RecalculateTonnage(WorkoutSession session)
    {
        decimal tonnage = 0;

        foreach (var set in session.ExerciseSets)
        {
            decimal effectiveWeight = set.ExerciseType switch
            {
                ExerciseType.Standard => set.WeightLbs,
                ExerciseType.Bodyweight => (session.BodyWeightLbs ?? 0) + set.WeightLbs,
                ExerciseType.Assisted => (session.BodyWeightLbs ?? 0) + set.WeightLbs,
                _ => set.WeightLbs
            };

            tonnage += set.Reps * effectiveWeight;
        }

        session.TonnageLbs = tonnage;
    }

    public static decimal CalculateEffectiveWeight(ExerciseType exerciseType, decimal weightLbs, decimal? bodyWeightLbs)
    {
        return exerciseType switch
        {
            ExerciseType.Standard => weightLbs,
            ExerciseType.Bodyweight => (bodyWeightLbs ?? 0) + weightLbs,
            ExerciseType.Assisted => (bodyWeightLbs ?? 0) + weightLbs,
            _ => weightLbs
        };
    }
}
