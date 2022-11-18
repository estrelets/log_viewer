using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public record GetGroupResultDto(int Total, GroupDto[] Groups);
public record GroupDto(int Line, DateTime Start, DateTime End, string Message, LogDto[] Logs, int Duration, MessageType Type);
public record LogDto(int Line, DateTime Time, MessageType Type, string? RequestId, string MessagePreview, int Duration);


[ApiController, Route("[controller]")]
[LoadedCheckFilter]
public class LocatorController : ControllerBase
{
    private readonly LogStore _logStore;

    public LocatorController(LogStore logStore)
    {
        _logStore = logStore;
    }

    [HttpGet("period")]
    [ProducesResponseType(200, Type = typeof(LogEntry[]))]
    public LogEntry[] GetInPeriod([FromQuery] DateTime from,
        [FromQuery] DateTime? to = null,
        [FromQuery] int? take = 100,
        [FromQuery] int? skip = null)
    {
        var q = _logStore.Logs!
            .Where(x => x.Time >= from)
            .Where(x => to == null || x.Time <= to);

        if (skip != null)
        {
            q = q.Skip(skip.Value);
        }

        if (take != null)
        {
            q = q.Take(take.Value);
        }

        return q.ToArray();
    }

    [HttpGet("groups")]
    [ProducesResponseType(200, Type = typeof(LogEntry[]))]
    public GetGroupResultDto GetGroupsInPeriod([FromQuery] DateTime from,
        [FromQuery] DateTime? to = null,
        [FromQuery] int? take = 100,
        [FromQuery] int? skip = null,
        [FromQuery] string? text = null)
    {
        var q = _logStore.Groups!
            .Where(x => x.Start >= from)
            .Where(x => to == null || x.Start <= to);

        if (!String.IsNullOrEmpty(text))
        {
            q = _logStore.Groups!.Where(x =>
                x.Entries.Any(e => e.MessagePreview.Contains(text))
                );
        }

        var total = q.Count();

        if (skip != null)
        {
            q = q.Skip(skip.Value);
        }

        if (take != null)
        {
            q = q.Take(take.Value);
        }

        var groups =  q
            .Select(Map)
            .ToArray();
        return new GetGroupResultDto(total, groups);
    }

    [HttpGet("groups/count")]
    public int GetGroupsCount()
    {
        return _logStore.Groups!.Count;
    }

    private GroupDto Map(LogGroup x)
    {
        var duration = (int)(x.End - x.Start).TotalMilliseconds;
        var first = x.Entries.First();
        var error = x.Entries.Any(x => x.Type == MessageType.Err);
        var type = error ? MessageType.Err : MessageType.Inf;


        var entries = x.Entries
            .Select((current, i) =>
            {
                var next = x.Entries.ElementAtOrDefault(i + 1);
                var duration = TimeSpan.Zero;
                if (next != null)
                {
                    duration = next.Time - current.Time;
                }

                var durationMs = (int)duration.TotalMilliseconds;

                return new LogDto(current.Line, current.Time, current.Type,
                    current.RequestId, current.MessagePreview, durationMs);
            })
            .ToArray();



        return new GroupDto(first.Line, x.Start, x.End, first.MessagePreview, entries, duration, type);
    }

    [HttpGet("group")]
    [ProducesResponseType(404)]
    [ProducesResponseType(200, Type = typeof(LogEntry[]))]
    public IActionResult GetGroup([FromQuery] string groupId)
    {
        if (_logStore.GroupsDictionary!.TryGetValue(groupId, out var entries))
        {
            return Ok(entries);
        }
        else
        {
            return NotFound();
        }
    }
}
