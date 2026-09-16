namespace GameChanger.Api.Features.Cycles;

public sealed record CreateCycleRequest(
    string? Name,
    DateOnly? StartDate,
    string? TimeZoneId,
    int? LengthInWeeks);
