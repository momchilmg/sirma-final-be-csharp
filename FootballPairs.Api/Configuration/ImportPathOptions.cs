namespace FootballPairs.Api.Configuration;

public sealed class ImportPathOptions
{
    public const string SectionName = "ImportPaths";

    public List<string> AllowedRoots { get; set; } = [];
}
