using Microsoft.AspNetCore.Mvc;
using StorageServer.Services;

namespace StorageServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly IFileStorage _storage;
    public StatsController(IFileStorage storage) => _storage = storage;

    [HttpGet]
    public IActionResult Get()
    {
        var all = _storage.GetAll();
        var byType = all
            .GroupBy(f => f.ContentType)
            .ToDictionary(g => g.Key, g => g.Count());

        return Ok(new
        {
            count = all.Count,
            totalSize = all.Sum(f => f.Size),
            byContentType = byType,
            storagePath = _storage.StoragePath
        });
    }
}