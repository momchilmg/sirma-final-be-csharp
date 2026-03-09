namespace FootballPairs.Api.Contracts.Responses.MatchRecords;

public sealed record MatchRecordResponse(
    int Id,
    int MatchId,
    int PlayerId,
    int FromMinute,
    int? ToMinute);
