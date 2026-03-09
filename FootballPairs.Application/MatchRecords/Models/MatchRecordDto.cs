namespace FootballPairs.Application.MatchRecords.Models;

public sealed record MatchRecordDto(
    int Id,
    int MatchId,
    int PlayerId,
    int FromMinute,
    int? ToMinute);
