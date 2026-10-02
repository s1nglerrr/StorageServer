using Microsoft.AspNetCore.Mvc;
using StorageServer.Services;

namespace StorageServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeleteController : ControllerBase
{
    private readonly IFileStorage _storage;
    public DeleteController(IFileStorage storage) => _storage = storage;

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var ok = await _storage.DeleteAsync(id);
        return ok ? Ok(new { deleted = id }) : NotFound(new { error = "Не найдено" });
    }
}