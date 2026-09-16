namespace GameChanger.Api.Features.Cycles;

public static class CreateCycleRequestValidator
{
    public const int MaximumNameLength = 200;
    public const int MaximumTimeZoneIdLength = 100;

    public static IReadOnlyDictionary<string, string[]> Validate(CreateCycleRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors[nameof(request.Name)] = ["A cycle name is required."];
        }
        else if (request.Name.Trim().Length > MaximumNameLength)
        {
            errors[nameof(request.Name)] = [$"The cycle name cannot exceed {MaximumNameLength} characters."];
        }

        if (request.StartDate is null)
        {
            errors[nameof(request.StartDate)] = ["A start date is required."];
        }

        if (string.IsNullOrWhiteSpace(request.TimeZoneId))
        {
            errors[nameof(request.TimeZoneId)] = ["A time zone is required."];
        }
        else if (request.TimeZoneId.Trim().Length > MaximumTimeZoneIdLength)
        {
            errors[nameof(request.TimeZoneId)] = [$"The time zone cannot exceed {MaximumTimeZoneIdLength} characters."];
        }
        else if (!IsKnownTimeZone(request.TimeZoneId.Trim()))
        {
            errors[nameof(request.TimeZoneId)] = ["Use a recognized IANA or Windows time zone identifier."];
        }

        if (request.LengthInWeeks is null)
        {
            errors[nameof(request.LengthInWeeks)] = ["A cycle length is required."];
        }
        else if (request.LengthInWeeks is < 1 or > 52)
        {
            errors[nameof(request.LengthInWeeks)] = ["Cycle length must be between 1 and 52 weeks."];
        }

        return errors;
    }

    private static bool IsKnownTimeZone(string timeZoneId)
    {
        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            return false;
        }
    }
}
