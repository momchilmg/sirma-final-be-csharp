using System.ComponentModel.DataAnnotations;

namespace FootballPairs.Api.Contracts.Requests.Matches;

public sealed class UpdateMatchRequest
{
    [Required]
    public DateTime MatchDate { get; set; }

    [Range(1, int.MaxValue)]
    public int HomeTeamId { get; set; }

    [Range(1, int.MaxValue)]
    public int AwayTeamId { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 3)]
    public string Score { get; set; } = string.Empty;
}
