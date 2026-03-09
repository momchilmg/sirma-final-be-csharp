using System.ComponentModel.DataAnnotations;
using FootballPairs.Api.Configuration;
using FootballPairs.Api.Contracts.Requests.Import;
using FootballPairs.Api.Contracts.Responses.Import;
using FootballPairs.Application.Import;
using FootballPairs.Application.Import.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FootballPairs.Api.Controllers;

[ApiController]
[Route("api/import")]
[Authorize]
[Authorize(Roles = "admin")]
public sealed class ImportController(
    IImportService importService,
    IOptions<ImportPathOptions> importPathOptions,
    IWebHostEnvironment environment) : ControllerBase
{
    private const long MaxImportFileBytes = 100 * 1024 * 1024;
    private readonly string[] allowedRootPaths = ResolveAllowedRootPaths(importPathOptions.Value, environment.ContentRootPath);

    [HttpPost("teams")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<ImportSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ImportSummaryResponse>> ImportTeams([FromForm] ImportCsvRequest request, CancellationToken cancellationToken)
    {
        await using var sourceStream = OpenSourceStream(request);
        var summary = await importService.ImportTeamsAsync(sourceStream, cancellationToken);
        return Ok(Map(summary));
    }

    [HttpPost("players")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<ImportSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ImportSummaryResponse>> ImportPlayers([FromForm] ImportCsvRequest request, CancellationToken cancellationToken)
    {
        await using var sourceStream = OpenSourceStream(request);
        var summary = await importService.ImportPlayersAsync(sourceStream, cancellationToken);
        return Ok(Map(summary));
    }

    [HttpPost("matches")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<ImportSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ImportSummaryResponse>> ImportMatches([FromForm] ImportCsvRequest request, CancellationToken cancellationToken)
    {
        await using var sourceStream = OpenSourceStream(request);
        var summary = await importService.ImportMatchesAsync(sourceStream, cancellationToken);
        return Ok(Map(summary));
    }

    [HttpPost("match-records")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<ImportSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ImportSummaryResponse>> ImportMatchRecords([FromForm] ImportCsvRequest request, CancellationToken cancellationToken)
    {
        await using var sourceStream = OpenSourceStream(request);
        var summary = await importService.ImportMatchRecordsAsync(sourceStream, cancellationToken);
        return Ok(Map(summary));
    }

    private Stream OpenSourceStream(ImportCsvRequest request)
    {
        var hasFile = request.File is not null;
        var hasPath = !string.IsNullOrWhiteSpace(request.Path);
        if (hasFile == hasPath)
        {
            throw new ValidationException("Exactly one source must be provided: file or path.");
        }

        if (hasFile)
        {
            if (request.File!.Length > MaxImportFileBytes)
            {
                throw new ValidationException($"File size must be at most {MaxImportFileBytes / (1024 * 1024)}MB.");
            }

            return request.File.OpenReadStream();
        }

        var rawPath = request.Path!.Trim();
        var fullPath = ResolveAllowedImportPath(rawPath);
        var allowedRootPath = GetContainingAllowedRoot(fullPath);
        if (allowedRootPath is null)
        {
            throw new ValidationException(
                $"Path '{rawPath}' is not allowed. Allowed roots are configured under '{ImportPathOptions.SectionName}:AllowedRoots'.");
        }

        FileStream fileStream;
        try
        {
            fileStream = System.IO.File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            throw new ValidationException($"Path '{rawPath}' does not exist.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new ValidationException($"Path '{rawPath}' cannot be opened.");
        }

        try
        {
            EnsureNoReparsePoints(fullPath, allowedRootPath, rawPath);
            if (fileStream.Length > MaxImportFileBytes)
            {
                throw new ValidationException($"File size must be at most {MaxImportFileBytes / (1024 * 1024)}MB.");
            }

            return fileStream;
        }
        catch
        {
            fileStream.Dispose();
            throw;
        }
    }

    private string ResolveAllowedImportPath(string rawPath)
    {
        string fullPath;
        try
        {
            var candidatePath = Path.IsPathRooted(rawPath)
                ? rawPath
                : Path.Combine(environment.ContentRootPath, rawPath);
            fullPath = Path.GetFullPath(candidatePath);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new ValidationException($"Path '{rawPath}' is invalid.");
        }

        if (GetContainingAllowedRoot(fullPath) is null)
        {
            throw new ValidationException(
                $"Path '{rawPath}' is not allowed. Allowed roots are configured under '{ImportPathOptions.SectionName}:AllowedRoots'.");
        }

        return fullPath;
    }

    private string? GetContainingAllowedRoot(string fullPath)
    {
        return allowedRootPaths.FirstOrDefault(allowedRootPath => IsPathInsideAllowedRoot(fullPath, allowedRootPath));
    }

    private static bool IsPathInsideAllowedRoot(string fullPath, string allowedRootPath)
    {
        var relativePath = Path.GetRelativePath(allowedRootPath, fullPath);
        return relativePath != ".."
            && !relativePath.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
            && !Path.IsPathRooted(relativePath);
    }

    private static void EnsureNoReparsePoints(string fullPath, string allowedRootPath, string rawPath)
    {
        try
        {
            var allowedRootDirectory = new DirectoryInfo(allowedRootPath);
            if ((allowedRootDirectory.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                throw new ValidationException($"Path '{rawPath}' is not allowed because configured allowed root uses a reparse point.");
            }

            var fileInfo = new FileInfo(fullPath);
            if ((fileInfo.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                throw new ValidationException($"Path '{rawPath}' is not allowed because it uses a reparse point.");
            }

            var currentDirectory = fileInfo.Directory;
            while (currentDirectory is not null && !PathsEqual(currentDirectory.FullName, allowedRootPath))
            {
                if ((currentDirectory.Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    throw new ValidationException($"Path '{rawPath}' is not allowed because it uses a reparse point.");
                }

                currentDirectory = currentDirectory.Parent;
            }
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            throw new ValidationException($"Path '{rawPath}' cannot be validated.");
        }
    }

    private static bool PathsEqual(string left, string right)
    {
        return string.Equals(Path.GetFullPath(left), Path.GetFullPath(right), StringComparison.OrdinalIgnoreCase);
    }

    private static string[] ResolveAllowedRootPaths(ImportPathOptions options, string contentRootPath)
    {
        var configuredRoots = (options.AllowedRoots ?? [])
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .Select(static path => path.Trim())
            .ToArray();
        var rootsToResolve = configuredRoots.Length == 0 ? ["samples"] : configuredRoots;
        var resolvedRoots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var root in rootsToResolve)
        {
            try
            {
                var candidatePath = Path.IsPathRooted(root)
                    ? root
                    : Path.Combine(contentRootPath, root);
                resolvedRoots.Add(Path.GetFullPath(candidatePath));
            }
            catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException or System.Security.SecurityException)
            {
                continue;
            }
        }

        if (resolvedRoots.Count == 0)
        {
            resolvedRoots.Add(Path.GetFullPath(Path.Combine(contentRootPath, "samples")));
        }

        return resolvedRoots.ToArray();
    }

    private static ImportSummaryResponse Map(ImportSummaryDto summary)
    {
        return new ImportSummaryResponse(summary.Entity, summary.CreatedCount);
    }
}
