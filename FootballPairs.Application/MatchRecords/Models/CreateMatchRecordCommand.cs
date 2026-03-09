namespace FootballPairs.Application.MatchRecords.Models;

public sealed record CreateMatchRecordCommand(
    int MatchId,
    int PlayerId,
    int FromMinute,
    int? ToMinute);
