using FootballPairs.Domain;

namespace FootballPairs.Domain.Entities;

public sealed class Match
{
    public int Id { get; set; }
    public DateTime MatchDate { get; set; }
    public int HomeTeamId { get; set; }
    public Team? HomeTeam { get; set; }
    public int AwayTeamId { get; set; }
    public Team? AwayTeam { get; set; }
    public string Score { get; set; } = string.Empty;
    public int EndMinute { get; set; } = DomainLimits.MatchDefaultEndMinute;
}
