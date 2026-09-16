using GameChanger.Api.Domain.Cycles;
using GameChanger.Api.Features.Cycles;

namespace GameChanger.Api.IntegrationTests;

public sealed class CycleDomainTests
{
    [Fact]
    public void NewCycleStartsAsDraft()
    {
        var cycle = Cycle.Create(
            "owner-1",
            "Focused autumn",
            new DateOnly(2026, 9, 21),
            "Atlantic/Reykjavik",
            10,
            new DateTimeOffset(2026, 9, 16, 20, 0, 0, TimeSpan.Zero));

        Assert.Equal(CycleStatus.Draft, cycle.Status);
        Assert.Equal("Focused autumn", cycle.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(53)]
    public void CycleLengthOutsideAllowedRangeIsRejected(int lengthInWeeks)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Cycle.Create(
            "owner-1",
            "Focused autumn",
            new DateOnly(2026, 9, 21),
            "Atlantic/Reykjavik",
            lengthInWeeks,
            new DateTimeOffset(2026, 9, 16, 20, 0, 0, TimeSpan.Zero)));

        Assert.Equal("lengthInWeeks", exception.ParamName);
    }

    [Fact]
    public void UnknownTimeZoneIsRejectedByRequestValidation()
    {
        var request = new CreateCycleRequest(
            "Focused autumn",
            new DateOnly(2026, 9, 21),
            "Not/A_Time_Zone",
            10);

        var errors = CreateCycleRequestValidator.Validate(request);

        Assert.Contains(nameof(request.TimeZoneId), errors.Keys);
    }
}
