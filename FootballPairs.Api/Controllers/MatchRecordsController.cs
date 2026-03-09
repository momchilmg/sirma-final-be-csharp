using System.ComponentModel.DataAnnotations;
using FootballPairs.Api.Contracts.Requests.MatchRecords;
using FootballPairs.Api.Contracts.Responses.MatchRecords;
using FootballPairs.Application.MatchRecords;
using FootballPairs.Application.MatchRecords.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballPairs.Api.Controllers;

[ApiController]
[Route("api/match-records")]
[Authorize]
public sealed class MatchRecordsController(IMatchRecordService matchRecordService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "admin")]
    [ProducesResponseType<MatchRecordResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MatchRecordResponse>> Create([FromBody] CreateMatchRecordRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateMatchRecordCommand(request.MatchId, request.PlayerId, request.FromMinute!.Value, request.ToMinute);
        var matchRecord = await matchRecordService.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = matchRecord.Id }, Map(matchRecord));
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<MatchRecordResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<MatchRecordResponse>>> List(
        [FromQuery, Range(1, int.MaxValue)] int? matchId,
        CancellationToken cancellationToken)
    {
        var matchRecords = await matchRecordService.ListAsync(matchId, cancellationToken);
        return Ok(matchRecords.Select(Map).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<MatchRecordResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MatchRecordResponse>> GetById([FromRoute, Range(1, int.MaxValue)] int id, CancellationToken cancellationToken)
    {
        var matchRecord = await matchRecordService.GetByIdAsync(id, cancellationToken);
        return Ok(Map(matchRecord));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType<MatchRecordResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MatchRecordResponse>> Update(
        [FromRoute, Range(1, int.MaxValue)] int id,
        [FromBody] UpdateMatchRecordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateMatchRecordCommand(request.MatchId, request.PlayerId, request.FromMinute!.Value, request.ToMinute);
        var matchRecord = await matchRecordService.UpdateAsync(id, command, cancellationToken);
        return Ok(Map(matchRecord));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute, Range(1, int.MaxValue)] int id, CancellationToken cancellationToken)
    {
        await matchRecordService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private static MatchRecordResponse Map(MatchRecordDto matchRecord)
    {
        return new MatchRecordResponse(
            matchRecord.Id,
            matchRecord.MatchId,
            matchRecord.PlayerId,
            matchRecord.FromMinute,
            matchRecord.ToMinute);
    }
}
