using System.ComponentModel.DataAnnotations;

namespace FootballPairs.Api.Contracts.Requests.Players;

public sealed class UpdatePlayerRequest
{
    [Range(1, int.MaxValue)]
    public int TeamNumber { get; set; }

    [Required]
    [StringLength(10, MinimumLength = 1)]
    public string Position { get; set; } = string.Empty;

    [Required]
    [StringLength(150, MinimumLength = 1)]
    public string FullName { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int TeamId { get; set; }
}
