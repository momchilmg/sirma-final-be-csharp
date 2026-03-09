using System.ComponentModel.DataAnnotations;

namespace FootballPairs.Api.Contracts.Requests.MatchRecords;

public sealed class UpdateMatchRecordRequest
{
    [Range(1, int.MaxValue)]
    public int MatchId { get; set; }

    [Range(1, int.MaxValue)]
    public int PlayerId { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int? FromMinute { get; set; }

    [Range(0, int.MaxValue)]
    public int? ToMinute { get; set; }
}
