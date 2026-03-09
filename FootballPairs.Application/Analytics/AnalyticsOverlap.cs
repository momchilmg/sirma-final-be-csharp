using FootballPairs.Domain;
using FootballPairs.Domain.Entities;

namespace FootballPairs.Application.Analytics;

public static class AnalyticsOverlap
{
    public static int ComputeTotalOverlap(IReadOnlyList<MatchRecord> playerARecords, IReadOnlyList<MatchRecord> playerBRecords, int endMinute)
    {
        var normalizedEndMinute = endMinute > 0 ? endMinute : DomainLimits.MatchDefaultEndMinute;
        var total = 0;
        foreach (var playerARecord in playerARecords)
        {
            var startA = playerARecord.FromMinute;
            var endA = playerARecord.ToMinute ?? normalizedEndMinute;
            foreach (var playerBRecord in playerBRecords)
            {
                var startB = playerBRecord.FromMinute;
                var endB = playerBRecord.ToMinute ?? normalizedEndMinute;
                total += Math.Max(0, Math.Min(endA, endB) - Math.Max(startA, startB));
            }
        }

        return total;
    }
}
