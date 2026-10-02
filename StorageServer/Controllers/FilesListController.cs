using Microsoft.AspNetCore.Mvc;
using StorageServer.Services;

namespace StorageServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFileStorage _storage;
    public FilesController(IFileStorage storage) => _storage = storage;

    [HttpGet]
    public IActionResult Get([FromQuery] int skip = 0, [FromQuery] int take = 100)
    {
        var all = _storage.GetAll()
            .OrderByDescending(f => f.UploadedAtUtc)
            .Skip(skip).Take(take)
            .ToList();

        return Ok(new { total = _storage.GetAll().Count, skip, take, items = all });
    }
}