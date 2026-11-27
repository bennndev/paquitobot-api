using PaquitoBot.Application.Common.Text;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Sync.Mapping;

internal static class AcademicPeriodResolver
{
    public static AcademicPeriod Resolve(string? externalId, DateOnly? sectionStartDate,
        IReadOnlyDictionary<string, AcademicPeriod> periodsByExternalId,
        IEnumerable<AcademicPeriod> availablePeriods)
    {
        var id = TextNormalizer.Optional(externalId);
        if (id is not null)
        {
            return periodsByExternalId.TryGetValue(id, out var period)
                ? period
                : throw new InvalidOperationException($"Academic period '{id}' was not found.");
        }

        if (sectionStartDate is not { } start)
        {
            throw new InvalidOperationException(
                "The batch has no start date, so its academic period cannot be deduced.");
        }

        var matches = availablePeriods
            .Where(period => period.StartDate <= start && start <= period.EndDate)
            .ToList();

        return matches.Count switch
        {
            1 => matches[0],
            0 => throw new InvalidOperationException(
                $"No academic period contains the batch start date {start:yyyy-MM-dd}."),
            _ => throw new InvalidOperationException(
                $"More than one academic period contains the batch start date {start:yyyy-MM-dd}.")
        };
    }
}
