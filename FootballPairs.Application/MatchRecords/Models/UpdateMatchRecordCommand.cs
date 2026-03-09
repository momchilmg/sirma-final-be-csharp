namespace FootballPairs.Application.MatchRecords.Models;

public sealed record UpdateMatchRecordCommand(
    int MatchId,
    int PlayerId,
    int FromMinute,
    int? ToMinute);
