using Microsoft.AspNetCore.Mvc;
using StorageServer.Services;

namespace StorageServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IFileStorage _storage;
    public UploadController(IFileStorage storage) => _storage = storage;

    [HttpPost]
    [RequestSizeLimit(1024L * 1024 * 1024)] // 1 GB
    public async Task<IActionResult> Upload([FromForm] IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "Файл не передан или пустой" });

        var meta = await _storage.SaveAsync(file);
        return Ok(meta);
    }

    [HttpPost("many")]
    public async Task<IActionResult> UploadMany([FromForm] List<IFormFile> files)
    {
        if (files is null || files.Count == 0)
            return BadRequest(new { error = "Файлы не переданы" });

        var result = new List<object>();
        foreach (var f in files)
        {
            var m = await _storage.SaveAsync(f);
            result.Add(m);
        }
        return Ok(result);
    }
}