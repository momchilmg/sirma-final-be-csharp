using System.ComponentModel.DataAnnotations;
using FootballPairs.Api.Contracts.Responses.Analytics;
using FootballPairs.Application.Analytics;
using FootballPairs.Application.Analytics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballPairs.Api.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public sealed class AnalyticsController(IAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet("players/{playerAId:int}/with/{playerBId:int}/played-time")]
    [ProducesResponseType<PlayedTimeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayedTimeResponse>> GetPlayedTimeTogether(
        [FromRoute, Range(1, int.MaxValue)] int playerAId,
        [FromRoute, Range(1, int.MaxValue)] int playerBId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery, Range(1, int.MaxValue)] int? matchId,
        CancellationToken cancellationToken)
    {
        var result = await analyticsService.GetPlayedTimeTogetherAsync(
            playerAId,
            playerBId,
            fromDate,
            toDate,
            matchId,
            cancellationToken);
        return Ok(new PlayedTimeResponse(result.MinutesTogether));
    }

    [HttpGet("players/{playerAId:int}/with/{playerBId:int}/common-matches")]
    [ProducesResponseType<CommonMatchesResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommonMatchesResponse>> GetCommonMatches(
        [FromRoute, Range(1, int.MaxValue)] int playerAId,
        [FromRoute, Range(1, int.MaxValue)] int playerBId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var result = await analyticsService.GetCommonMatchesAsync(
            playerAId,
            playerBId,
            fromDate,
            toDate,
            cancellationToken);
        return Ok(Map(result));
    }

    private static CommonMatchesResponse Map(CommonMatchesResultDto result)
    {
        var matches = result.Matches
            .Select(match => new CommonMatchItemResponse(
                match.MatchId,
                match.MatchDate,
                match.HomeTeamId,
                match.AwayTeamId,
                match.MinutesTogether))
            .ToList();
        return new CommonMatchesResponse(matches, result.TotalMinutesTogether);
    }
}
