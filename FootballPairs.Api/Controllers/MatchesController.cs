using System.ComponentModel.DataAnnotations;
using FootballPairs.Api.Contracts.Requests.Matches;
using FootballPairs.Api.Contracts.Responses.Matches;
using FootballPairs.Application.Matches;
using FootballPairs.Application.Matches.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballPairs.Api.Controllers;

[ApiController]
[Route("api/matches")]
[Authorize]
public sealed class MatchesController(IMatchService matchService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "admin")]
    [ProducesResponseType<MatchResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MatchResponse>> Create([FromBody] CreateMatchRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateMatchCommand(request.MatchDate, request.HomeTeamId, request.AwayTeamId, request.Score);
        var match = await matchService.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = match.Id }, Map(match));
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<MatchResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MatchResponse>>> List(CancellationToken cancellationToken)
    {
        var matches = await matchService.ListAsync(cancellationToken);
        return Ok(matches.Select(Map).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<MatchResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MatchResponse>> GetById([FromRoute, Range(1, int.MaxValue)] int id, CancellationToken cancellationToken)
    {
        var match = await matchService.GetByIdAsync(id, cancellationToken);
        return Ok(Map(match));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType<MatchResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MatchResponse>> Update(
        [FromRoute, Range(1, int.MaxValue)] int id,
        [FromBody] UpdateMatchRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateMatchCommand(request.MatchDate, request.HomeTeamId, request.AwayTeamId, request.Score);
        var match = await matchService.UpdateAsync(id, command, cancellationToken);
        return Ok(Map(match));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute, Range(1, int.MaxValue)] int id, CancellationToken cancellationToken)
    {
        await matchService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private static MatchResponse Map(MatchDto match)
    {
        return new MatchResponse(match.Id, match.MatchDate, match.HomeTeamId, match.AwayTeamId, match.Score, match.EndMinute);
    }
}
