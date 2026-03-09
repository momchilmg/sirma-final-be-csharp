using Microsoft.AspNetCore.Http;

namespace FootballPairs.Api.Contracts.Requests.Import;

public sealed class ImportCsvRequest
{
    public IFormFile? File { get; set; }
    public string? Path { get; set; }
}
