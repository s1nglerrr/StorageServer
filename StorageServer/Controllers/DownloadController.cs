using Microsoft.AspNetCore.Mvc;
using StorageServer.Services;

namespace StorageServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DownloadController : ControllerBase
{
    private readonly IFileStorage _storage;
    public DownloadController(IFileStorage storage) => _storage = storage;

    [HttpGet("{id}")]
    public async Task<IActionResult> ById(string id)
    {
        var meta = await _storage.GetByIdAsync(id);
        if (meta is null) return NotFound(new { error = "Не найдено" });

        var stream = _storage.OpenRead(id);
        if (stream is null) return NotFound(new { error = "Файл отсутствует на диске" });

        return File(stream, meta.ContentType, meta.OriginalName);
    }

    [HttpGet("byname/{name}")]
    public async Task<IActionResult> ByName(string name)
    {
        var meta = await _storage.GetByNameAsync(name);
        if (meta is null) return NotFound(new { error = "Не найдено" });

        var stream = _storage.OpenRead(meta.Id);
        if (stream is null) return NotFound(new { error = "Файл отсутствует на диске" });

        return File(stream, meta.ContentType, meta.OriginalName);
    }
}