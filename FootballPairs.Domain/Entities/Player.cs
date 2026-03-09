namespace FootballPairs.Domain.Entities;

public sealed class Player
{
    public int Id { get; set; }
    public int TeamNumber { get; set; }
    public string Position { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int TeamId { get; set; }
    public Team? Team { get; set; }
}
