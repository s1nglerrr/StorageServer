using Microsoft.AspNetCore.Mvc;
using StorageServer.Services;

namespace StorageServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileInfoController : ControllerBase
{
    private readonly IFileStorage _storage;
    public FileInfoController(IFileStorage storage) => _storage = storage;

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var meta = await _storage.GetByIdAsync(id);
        return meta is null ? NotFound(new { error = "Не найдено" }) : Ok(meta);
    }
}