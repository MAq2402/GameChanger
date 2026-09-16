using GameChanger.Api.Domain.Cycles;

namespace GameChanger.Api.Features.Cycles;

public sealed record CycleResponse(
    Guid Id,
    string Name,
    DateOnly StartDate,
    string TimeZoneId,
    int LengthInWeeks,
    string Status,
    DateTimeOffset CreatedAtUtc)
{
    public static CycleResponse FromCycle(Cycle cycle) =>
        new(
            cycle.Id,
            cycle.Name,
            cycle.StartDate,
            cycle.TimeZoneId,
            cycle.LengthInWeeks,
            cycle.Status.ToString(),
            cycle.CreatedAtUtc);
}
