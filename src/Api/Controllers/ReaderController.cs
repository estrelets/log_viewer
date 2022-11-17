using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController, Route("[controller]")]
public class ReaderController : ControllerBase
{
    private readonly Reader _reader;
    private readonly LogStore _logStore;
    private readonly ILogger<ReaderController> _logger;

    public ReaderController(Reader reader, LogStore logStore, ILogger<ReaderController> logger)
    {
        _reader = reader;
        _logStore = logStore;
        _logger = logger;
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [HttpPost("upload")]
    [LoggerMessage]
    [RequestSizeLimit(Int32.MaxValue)]
    public async Task<IActionResult> Read(IFormFile file, CancellationToken ct)
    {
        try
        {
            await using var stream = file.OpenReadStream();
            var logs = await _reader.Read(stream, ct);
            _logStore.Fill(logs);
            return Ok();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error while reading");
            return BadRequest(ex.Message);
        }
    }
}