using Microsoft.AspNetCore.Mvc;
using StorageServer.Services;

namespace StorageServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IFileStorage _storage;
    public SearchController(IFileStorage storage) => _storage = storage;

    [HttpGet]
    public IActionResult Search([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { error = "Параметр name обязателен" });

        var result = _storage.GetAll()
            .Where(f => f.OriginalName.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Ok(new { query = name, count = result.Count, items = result });
    }
}