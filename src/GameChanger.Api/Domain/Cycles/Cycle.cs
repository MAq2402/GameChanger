namespace GameChanger.Api.Domain.Cycles;

public sealed class Cycle
{
    private Cycle()
    {
    }

    private Cycle(
        Guid id,
        string ownerId,
        string name,
        DateOnly startDate,
        string timeZoneId,
        int lengthInWeeks,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        OwnerId = ownerId;
        Name = name;
        StartDate = startDate;
        TimeZoneId = timeZoneId;
        LengthInWeeks = lengthInWeeks;
        Status = CycleStatus.Draft;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public string OwnerId { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public DateOnly StartDate { get; private set; }

    public string TimeZoneId { get; private set; } = string.Empty;

    public int LengthInWeeks { get; private set; }

    public CycleStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static Cycle Create(
        string ownerId,
        string name,
        DateOnly startDate,
        string timeZoneId,
        int lengthInWeeks,
        DateTimeOffset createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);

        if (lengthInWeeks is < 1 or > 52)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lengthInWeeks),
                lengthInWeeks,
                "Cycle length must be between 1 and 52 weeks.");
        }

        return new Cycle(
            Guid.NewGuid(),
            ownerId,
            name.Trim(),
            startDate,
            timeZoneId.Trim(),
            lengthInWeeks,
            createdAtUtc);
    }
}
