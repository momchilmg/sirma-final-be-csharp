using System.ComponentModel.DataAnnotations;

namespace FootballPairs.Api.Contracts.Requests.Auth;

public sealed class LoginRequest
{
    [Required]
    [StringLength(64, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}
