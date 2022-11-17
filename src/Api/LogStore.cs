namespace Api;

public class LogStore
{
    public IReadOnlyCollection<LogEntry>? Logs { get; private set; }
    public IReadOnlyCollection<LogGroup>? Groups { get; private set; }
    public IReadOnlyDictionary<string, LogGroup>? GroupsDictionary { get; private set; }
    
    public bool Loaded => Logs != null;
    
    public void Fill(List<LogEntry> src)
    {
        var groups = src
            .Where(x => !String.IsNullOrEmpty(x.RequestId))
            .GroupBy(x => x.RequestId)
            .Where(g => !g.Any(x => x.MessagePreview.Contains("HTTP GET /api/healthcheck")))
            .Select(v => new LogGroup(
                v.Key!,
                v.Min(x => x.Time),
                v.Max(x => x.Time),
                v.Count(),
                v.ToArray()))
            .ToArray();

        var groupsDictionary = groups
            .ToDictionary(k => k.RequestId, v => v);

        Logs = src;
        Groups = groups;
        GroupsDictionary = groupsDictionary;
    }
}