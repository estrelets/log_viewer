using System.Text;
using System.Text.RegularExpressions;

namespace Api;

public record LogEntry(int Line, DateTime Time, MessageType Type, string? RequestId, string MessagePreview);
public record LogGroup(string RequestId, DateTime Start, DateTime End, int Count, LogEntry[] Entries);
public enum MessageType { Err, Inf, Wrn }

public class Reader
{
    private static readonly Regex MessageRegex = new("\\[(?<DATE>.+) (?<TYPE>\\w+) (?<REQUEST_ID>.+)?\\]\\s+(?<MESSAGE>.+)");

    private static readonly string DateGroupName = "DATE";
    private static readonly string TypeGroupName = "TYPE";
    private static readonly string RequestIdGroupName = "REQUEST_ID";
    private static readonly string MessageGroupName = "MESSAGE";
    private static readonly int PreviewMessageSize = 20;

    public async Task<List<LogEntry>> Read(Stream stream, CancellationToken ct)
    {
        using var sr = new StreamReader(stream, leaveOpen: true);

        var result = new List<LogEntry>();
        
        int lineNumber = 0;

        LogEntry? last = null;
        var message = new StringBuilder();
        
        while (!sr.EndOfStream)
        {
            var line = await sr.ReadLineAsync()!;

            var entry = Parse(line, lineNumber++);

            if (entry == null)
            {
                message.AppendLine(line);
            }
            else
            {
                if (last == null)
                {
                    last = entry;
                    message = new StringBuilder(last.MessagePreview);
                    continue;
                }
                
                last = last with { MessagePreview = message.ToString() };
                result.Add(last);
                last = entry;
                message = new StringBuilder(last.MessagePreview + Environment.NewLine);
            }
        }

        return result;
    }

    private LogEntry? Parse(string line, int lineNumber)
    {
        var match = MessageRegex.Match(line);

        if (!match.Success)
        {
            return null;
        }

        if (!DateTime.TryParse(match.Groups[DateGroupName].ValueSpan, out var time))
            return null;

        if (!Enum.TryParse<MessageType>(match.Groups[TypeGroupName].ValueSpan, true, out var type))
            return null;

        var messagePreview = match.Groups[MessageGroupName].Value;

        var requestId = match.Groups[RequestIdGroupName].Value;
        if (String.IsNullOrEmpty(requestId))
        {
            requestId = Guid.NewGuid().ToString();
        }
        
        return new LogEntry(lineNumber, time, type, requestId, messagePreview);
    }
}