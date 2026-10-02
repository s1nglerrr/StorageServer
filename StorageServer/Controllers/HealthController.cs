using Microsoft.AspNetCore.Mvc;
using StorageServer.Services;

namespace StorageServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IFileStorage _storage;
    public HealthController(IFileStorage storage) => _storage = storage;

    [HttpGet]
    public IActionResult Get()
    {
        var exists = Directory.Exists(_storage.StoragePath);
        var status = exists ? "healthy" : "unhealthy";
        return exists
            ? Ok(new { status, storage = _storage.StoragePath, timeUtc = DateTime.UtcNow })
            : StatusCode(503, new { status, storage = _storage.StoragePath });
    }
}