namespace FootballPairs.Application.MatchRecords;

public static class MatchRecordIntervals
{
    public static bool Overlaps(int startA, int endA, int startB, int endB)
    {
        return Math.Max(startA, startB) < Math.Min(endA, endB);
    }
}
