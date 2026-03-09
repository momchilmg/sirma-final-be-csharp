namespace FootballPairs.Domain.Entities;

public sealed class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ManagerFullName { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public ICollection<Player> Players { get; set; } = [];
}
