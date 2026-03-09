namespace FootballPairs.Domain.Entities;

public sealed class RequestLog
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int ErrorCode { get; set; }
    public string? Username { get; set; }
    public string Data { get; set; } = string.Empty;
}
