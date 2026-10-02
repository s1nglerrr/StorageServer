using System.Collections.Concurrent;
using System.Text.Json;
using StorageServer.Models;

namespace StorageServer.Services;

public class FileStorage : IFileStorage
{
    private readonly ConcurrentDictionary<string, FileMetadata> _index = new();
    private readonly ILogger<FileStorage> _logger;
    private readonly string _indexFile;

    public string StoragePath { get; }

    public FileStorage(IConfiguration config, ILogger<FileStorage> logger)
    {
        _logger = logger;
        StoragePath = config["Storage:Path"]
            ?? Path.Combine(AppContext.BaseDirectory, "storage");

        Directory.CreateDirectory(StoragePath);
        _indexFile = Path.Combine(StoragePath, "_index.json");
        LoadIndex();
    }

    private void LoadIndex()
    {
        if (!File.Exists(_indexFile)) return;
        try
        {
            var json = File.ReadAllText(_indexFile);
            var list = JsonSerializer.Deserialize<List<FileMetadata>>(json) ?? new();
            foreach (var m in list) _index[m.Id] = m;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось прочитать индекс файлов");
        }
    }

    private void PersistIndex()
    {
        var json = JsonSerializer.Serialize(_index.Values, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_indexFile, json);
    }

    public async Task<FileMetadata> SaveAsync(IFormFile file)
    {
        var id = Guid.NewGuid().ToString("N");
        var ext = Path.GetExtension(file.FileName);
        var storedName = id + ext;
        var fullPath = Path.Combine(StoragePath, storedName);

        await using (var fs = File.Create(fullPath))
        {
            await file.CopyToAsync(fs);
        }

        var meta = new FileMetadata
        {
            Id = id,
            OriginalName = file.FileName,
            StoredName = storedName,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType,
            Size = new FileInfo(fullPath).Length
        };

        _index[id] = meta;
        PersistIndex();
        return meta;
    }

    public Task<FileMetadata?> GetByIdAsync(string id)
        => Task.FromResult(_index.TryGetValue(id, out var m) ? m : null);

    public Task<FileMetadata?> GetByNameAsync(string name)
    {
        var m = _index.Values.FirstOrDefault(x =>
            string.Equals(x.OriginalName, name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(m);
    }

    public IReadOnlyCollection<FileMetadata> GetAll() => _index.Values.ToList();

    public Task<bool> DeleteAsync(string id)
    {
        if (!_index.TryRemove(id, out var meta)) return Task.FromResult(false);
        var path = Path.Combine(StoragePath, meta.StoredName);
        if (File.Exists(path)) File.Delete(path);
        PersistIndex();
        return Task.FromResult(true);
    }

    public Stream? OpenRead(string id)
    {
        if (!_index.TryGetValue(id, out var meta)) return null;
        var path = Path.Combine(StoragePath, meta.StoredName);
        return File.Exists(path) ? File.OpenRead(path) : null;
    }
}