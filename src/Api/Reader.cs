using System.Text;
using System.Text.RegularExpressions;

namespace Api;

public record LogEntry(int Line, DateTime Time, MessageType Type, string? RequestId, string MessagePreview);
public record LogGroup(string RequestId, DateTime Start, DateTime End, int Count, LogEntry[] Entries);
public enum MessageType { Err, Inf, Wrn }

public class Reader
{
    private static readonly Regex MessageRegex = new("\\[(?<DATE>.+?) (?<TYPE>\\w+?) (?<REQUEST_ID>.+?)?\\]\\s+(?<MESSAGE>.+)");

    private static readonly string DateGroupName = "DATE";
    private static readonly string TypeGroupName = "TYPE";
    private static readonly string RequestIdGroupName = "REQUEST_ID";
    private static readonly string MessageGroupName = "MESSAGE";
    private static readonly int PreviewMessageSize = 20;

    public async Task<List<LogEntry>> Read(Stream stream, CancellationToken ct)
    {
        using var sr = new StreamReader(stream, leaveOpen: true);
        var result = new List<LogEntry>();

        LogEntry? last = null;
        var message = new StringBuilder();
        var lineNumber = 0;

        while (!sr.EndOfStream)
        {
            var line = await sr.ReadLineAsync();
            lineNumber++;

            var entry = Parse(line, lineNumber);

            var isText = entry == null;
            var isFirst = last == null;
            var isFinished = sr.EndOfStream;

            switch (isFirst, isText)
            {
                case (true, true):
                    continue;
                case (true, false):
                    StartNew(entry!);
                    AppendMessage(entry!.MessagePreview);
                    break;
                case (false, true):
                    AppendMessage(line);
                    break;
                case (false, false):
                    FinishLast();
                    StartNew(entry!);
                    AppendMessage(entry!.MessagePreview);
                    break;
            }

            if (isFinished)
            {
                FinishLast();
            }
        }


        return result;

        void StartNew(LogEntry entry)
        {
            last = entry;
            message.Clear();
        }

        void FinishLast()
        {
            last = last with { MessagePreview = message.ToString() };
            result.Add(last);
        }

        void AppendMessage(string line)
        {
            message.AppendLine(line);
        }
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
