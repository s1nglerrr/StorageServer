using Microsoft.AspNetCore.Mvc;
using StorageServer.Services;

namespace StorageServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetadataController : ControllerBase
{
    private readonly IFileStorage _storage;
    public MetadataController(IFileStorage storage) => _storage = storage;

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var meta = await _storage.GetByIdAsync(id);
        if (meta is null) return NotFound();

        // Намеренно не отдаём содержимое файла — только метаданные
        return Ok(new
        {
            meta.Id,
            meta.OriginalName,
            meta.ContentType,
            meta.Size,
            meta.UploadedAtUtc,
            downloadUrl = $"/api/download/{meta.Id}"
        });
    }
}