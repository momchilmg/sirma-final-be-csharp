using System.ComponentModel.DataAnnotations;
using FootballPairs.Application.Common.Errors;
using FootballPairs.Application.MatchRecords.Models;
using FootballPairs.Application.Matches;
using FootballPairs.Application.Players;
using FootballPairs.Domain;
using FootballPairs.Domain.Entities;

namespace FootballPairs.Application.MatchRecords;

public sealed class MatchRecordService(
    IMatchRecordRepository matchRecordRepository,
    IMatchRepository matchRepository,
    IPlayerRepository playerRepository) : IMatchRecordService
{
    public async Task<MatchRecordDto> CreateAsync(CreateMatchRecordCommand command, CancellationToken cancellationToken)
    {
        var matchRecord = await BuildMatchRecordAsync(command, cancellationToken);
        await matchRecordRepository.AddAsync(matchRecord, cancellationToken);
        await matchRecordRepository.SaveChangesAsync(cancellationToken);
        return Map(matchRecord);
    }

    public async Task QueueCreateForImportAsync(CreateMatchRecordCommand command, CancellationToken cancellationToken)
    {
        var matchRecord = await BuildMatchRecordAsync(command, cancellationToken);
        await matchRecordRepository.AddAsync(matchRecord, cancellationToken);
    }

    public Task FlushImportAsync(CancellationToken cancellationToken)
    {
        return matchRecordRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MatchRecordDto>> ListAsync(int? matchId, CancellationToken cancellationToken)
    {
        if (matchId.HasValue && matchId.Value <= 0)
        {
            throw new ValidationException("Match id must be positive.");
        }

        var records = await matchRecordRepository.ListAsync(matchId, cancellationToken);
        return records.Select(Map).ToList();
    }

    public async Task<MatchRecordDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var matchRecord = await matchRecordRepository.GetByIdReadOnlyAsync(id, cancellationToken);
        if (matchRecord is null)
        {
            throw new NotFoundException("Match record was not found.");
        }

        return Map(matchRecord);
    }

    public async Task<MatchRecordDto> UpdateAsync(int id, UpdateMatchRecordCommand command, CancellationToken cancellationToken)
    {
        var matchRecord = await matchRecordRepository.GetByIdAsync(id, cancellationToken);
        if (matchRecord is null)
        {
            throw new NotFoundException("Match record was not found.");
        }

        var match = await EnsureMatchExistsAsync(command.MatchId, cancellationToken);
        var player = await EnsurePlayerExistsAsync(command.PlayerId, cancellationToken);
        EnsurePlayerParticipatesInMatch(player, match);
        ValidateMinutes(command.FromMinute, command.ToMinute, match.EndMinute);
        await EnsureSingleRecordPerMatchPlayerAsync(command.MatchId, command.PlayerId, id, cancellationToken);

        matchRecord.MatchId = command.MatchId;
        matchRecord.PlayerId = command.PlayerId;
        matchRecord.FromMinute = command.FromMinute;
        matchRecord.ToMinute = command.ToMinute;
        await matchRecordRepository.SaveChangesAsync(cancellationToken);
        return Map(matchRecord);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var matchRecord = await matchRecordRepository.GetByIdAsync(id, cancellationToken);
        if (matchRecord is null)
        {
            throw new NotFoundException("Match record was not found.");
        }

        await matchRecordRepository.DeleteAsync(matchRecord, cancellationToken);
        await matchRecordRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Match> EnsureMatchExistsAsync(int matchId, CancellationToken cancellationToken)
    {
        if (matchId <= 0)
        {
            throw new ValidationException("Match id must be positive.");
        }

        var match = await matchRepository.GetByIdAsync(matchId, cancellationToken);
        if (match is null)
        {
            throw new ValidationException("Referenced match does not exist.");
        }

        return match;
    }

    private async Task<Player> EnsurePlayerExistsAsync(int playerId, CancellationToken cancellationToken)
    {
        if (playerId <= 0)
        {
            throw new ValidationException("Player id must be positive.");
        }

        var player = await playerRepository.GetByIdReadOnlyAsync(playerId, cancellationToken);
        if (player is null)
        {
            throw new ValidationException("Referenced player does not exist.");
        }

        return player;
    }

    private static void EnsurePlayerParticipatesInMatch(Player player, Match match)
    {
        if (player.TeamId != match.HomeTeamId && player.TeamId != match.AwayTeamId)
        {
            throw new ValidationException(
                $"Player with id {player.Id} belongs to team {player.TeamId}, which does not participate in match {match.Id}.");
        }
    }

    private static int ValidateMinutes(int fromMinute, int? toMinute, int endMinute)
    {
        var normalizedEndMinute = endMinute > 0 ? endMinute : DomainLimits.MatchDefaultEndMinute;
        if (fromMinute < 0)
        {
            throw new ValidationException("From minute must be greater than or equal to 0.");
        }

        var effectiveToMinute = toMinute ?? normalizedEndMinute;
        if (toMinute.HasValue && fromMinute >= toMinute.Value)
        {
            throw new ValidationException("From minute must be less than to minute.");
        }

        if (effectiveToMinute > normalizedEndMinute)
        {
            throw new ValidationException($"To minute must be less than or equal to {normalizedEndMinute}.");
        }

        if (fromMinute >= effectiveToMinute)
        {
            throw new ValidationException("From minute must be less than the effective end minute.");
        }

        return effectiveToMinute;
    }

    private async Task EnsureSingleRecordPerMatchPlayerAsync(
        int matchId,
        int playerId,
        int? excludedId,
        CancellationToken cancellationToken)
    {
        var existingRecords = await matchRecordRepository.ListByMatchAndPlayerAsync(matchId, playerId, excludedId, cancellationToken);
        if (existingRecords.Count > 0)
        {
            throw new ConflictException("A player can have only one match record per match.");
        }
    }

    private static MatchRecordDto Map(MatchRecord matchRecord)
    {
        return new MatchRecordDto(matchRecord.Id, matchRecord.MatchId, matchRecord.PlayerId, matchRecord.FromMinute, matchRecord.ToMinute);
    }

    private async Task<MatchRecord> BuildMatchRecordAsync(CreateMatchRecordCommand command, CancellationToken cancellationToken)
    {
        var match = await EnsureMatchExistsAsync(command.MatchId, cancellationToken);
        var player = await EnsurePlayerExistsAsync(command.PlayerId, cancellationToken);
        EnsurePlayerParticipatesInMatch(player, match);
        ValidateMinutes(command.FromMinute, command.ToMinute, match.EndMinute);
        await EnsureSingleRecordPerMatchPlayerAsync(command.MatchId, command.PlayerId, null, cancellationToken);
        return new MatchRecord
        {
            MatchId = command.MatchId,
            PlayerId = command.PlayerId,
            FromMinute = command.FromMinute,
            ToMinute = command.ToMinute
        };
    }
}
