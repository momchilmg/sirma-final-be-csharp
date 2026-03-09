using System.ComponentModel.DataAnnotations;
using FootballPairs.Application.Common.Errors;
using FootballPairs.Application.Matches.Models;
using FootballPairs.Application.Teams;
using FootballPairs.Domain.Entities;

namespace FootballPairs.Application.Matches;

public sealed class MatchService(IMatchRepository matchRepository, ITeamRepository teamRepository) : IMatchService
{
    public async Task<MatchDto> CreateAsync(CreateMatchCommand command, CancellationToken cancellationToken)
    {
        var match = await BuildMatchAsync(command, cancellationToken);
        await matchRepository.AddAsync(match, cancellationToken);
        try
        {
            await matchRepository.SaveChangesAsync(cancellationToken);
        }
        catch (ConflictException)
        {
            throw new ValidationException("Referenced team does not exist.");
        }

        return Map(match);
    }

    public async Task QueueCreateForImportAsync(CreateMatchCommand command, CancellationToken cancellationToken)
    {
        var match = await BuildMatchAsync(command, cancellationToken);
        await matchRepository.AddAsync(match, cancellationToken);
    }

    public async Task FlushImportAsync(CancellationToken cancellationToken)
    {
        try
        {
            await matchRepository.SaveChangesAsync(cancellationToken);
        }
        catch (ConflictException)
        {
            throw new ValidationException("Referenced team does not exist.");
        }
    }

    public async Task<IReadOnlyList<MatchDto>> ListAsync(CancellationToken cancellationToken)
    {
        var matches = await matchRepository.ListAsync(cancellationToken);
        return matches.Select(Map).ToList();
    }

    public async Task<MatchDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var match = await matchRepository.GetByIdReadOnlyAsync(id, cancellationToken);
        if (match is null)
        {
            throw new NotFoundException("Match was not found.");
        }

        return Map(match);
    }

    public async Task<MatchDto> UpdateAsync(int id, UpdateMatchCommand command, CancellationToken cancellationToken)
    {
        var match = await matchRepository.GetByIdAsync(id, cancellationToken);
        if (match is null)
        {
            throw new NotFoundException("Match was not found.");
        }

        var normalizedScore = Validate(command.MatchDate, command.HomeTeamId, command.AwayTeamId, command.Score);
        await EnsureTeamsExistAsync(command.HomeTeamId, command.AwayTeamId, cancellationToken);
        await EnsureNoTeamDailyConflictAsync(command.MatchDate, command.HomeTeamId, command.AwayTeamId, id, cancellationToken);
        match.MatchDate = command.MatchDate;
        match.HomeTeamId = command.HomeTeamId;
        match.AwayTeamId = command.AwayTeamId;
        match.Score = normalizedScore;
        try
        {
            await matchRepository.SaveChangesAsync(cancellationToken);
        }
        catch (ConflictException)
        {
            throw new ValidationException("Referenced team does not exist.");
        }

        return Map(match);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var match = await matchRepository.GetByIdAsync(id, cancellationToken);
        if (match is null)
        {
            throw new NotFoundException("Match was not found.");
        }

        await matchRepository.DeleteAsync(match, cancellationToken);
        try
        {
            await matchRepository.SaveChangesAsync(cancellationToken);
        }
        catch (ConflictException)
        {
            throw new ConflictException("Match cannot be deleted while match records are assigned.");
        }
    }

    private async Task EnsureTeamsExistAsync(int homeTeamId, int awayTeamId, CancellationToken cancellationToken)
    {
        if (!await teamRepository.TeamExistsAsync(homeTeamId, cancellationToken))
        {
            throw new ValidationException("Home team does not exist.");
        }

        if (!await teamRepository.TeamExistsAsync(awayTeamId, cancellationToken))
        {
            throw new ValidationException("Away team does not exist.");
        }
    }

    private async Task<Match> BuildMatchAsync(CreateMatchCommand command, CancellationToken cancellationToken)
    {
        var normalizedScore = Validate(command.MatchDate, command.HomeTeamId, command.AwayTeamId, command.Score);
        await EnsureTeamsExistAsync(command.HomeTeamId, command.AwayTeamId, cancellationToken);
        await EnsureNoTeamDailyConflictAsync(command.MatchDate, command.HomeTeamId, command.AwayTeamId, null, cancellationToken);
        return new Match
        {
            MatchDate = command.MatchDate,
            HomeTeamId = command.HomeTeamId,
            AwayTeamId = command.AwayTeamId,
            Score = normalizedScore,
            EndMinute = 90
        };
    }

    private static string Validate(DateTime matchDate, int homeTeamId, int awayTeamId, string score)
    {
        if (homeTeamId <= 0 || awayTeamId <= 0)
        {
            throw new ValidationException("Home and away team ids must be positive.");
        }

        if (homeTeamId == awayTeamId)
        {
            throw new ValidationException("HomeTeamId and AwayTeamId must be different.");
        }

        if (matchDate == default)
        {
            throw new ValidationException("Match date is required.");
        }

        if (string.IsNullOrWhiteSpace(score))
        {
            throw new ValidationException("Score is required.");
        }

        var normalizedScore = score.Trim();
        if (normalizedScore.Length > 20)
        {
            throw new ValidationException("Score must be at most 20 characters.");
        }

        if (!MatchScoreValidator.IsValid(normalizedScore))
        {
            throw new ValidationException("Score format is invalid.");
        }

        return normalizedScore;
    }

    private async Task EnsureNoTeamDailyConflictAsync(
        DateTime matchDate,
        int homeTeamId,
        int awayTeamId,
        int? excludedMatchId,
        CancellationToken cancellationToken)
    {
        if (await matchRepository.ExistsTeamDailyConflictAsync(
                matchDate,
                homeTeamId,
                awayTeamId,
                excludedMatchId,
                cancellationToken))
        {
            throw new ConflictException("A team can participate in only one match per date.");
        }
    }

    private static MatchDto Map(Match match)
    {
        return new MatchDto(match.Id, match.MatchDate, match.HomeTeamId, match.AwayTeamId, match.Score, match.EndMinute);
    }
}
