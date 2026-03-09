namespace FootballPairs.Domain.Entities;

public sealed class MatchRecord
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public Match? Match { get; set; }
    public int PlayerId { get; set; }
    public Player? Player { get; set; }
    public int FromMinute { get; set; }
    public int? ToMinute { get; set; }
}
