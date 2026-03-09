using System.ComponentModel.DataAnnotations;

namespace FootballPairs.Api.Contracts.Requests.Teams;

public sealed class UpdateTeamRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(150, MinimumLength = 1)]
    public string ManagerFullName { get; set; } = string.Empty;

    [Required]
    [StringLength(20, MinimumLength = 1)]
    public string Group { get; set; } = string.Empty;
}
