using Microsoft.AspNetCore.Mvc;
using StorageServer.Services;

namespace StorageServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IFileStorage _storage;
    public AdminController(IFileStorage storage) => _storage = storage;

    /// <summary>Удаляет записи из индекса, для которых нет файлов на диске.</summary>
    [HttpPost("cleanup")]
    public async Task<IActionResult> Cleanup()
    {
        var removed = new List<string>();
        foreach (var meta in _storage.GetAll().ToList())
        {
            var path = Path.Combine(_storage.StoragePath, meta.StoredName);
            if (!System.IO.File.Exists(path))   // <-- было File.Exists(path)
            {
                await _storage.DeleteAsync(meta.Id);
                removed.Add(meta.Id);
            }
        }
        return Ok(new { removedCount = removed.Count, removedIds = removed });
    }

    /// <summary>Полный список файлов с сортировкой по размеру.</summary>
    [HttpGet("largest")]
    public IActionResult Largest([FromQuery] int take = 10)
    {
        var items = _storage.GetAll()
            .OrderByDescending(f => f.Size)
            .Take(take)
            .ToList();
        return Ok(items);
    }
}