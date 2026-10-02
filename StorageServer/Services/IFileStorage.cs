using StorageServer.Models;

namespace StorageServer.Services;

public interface IFileStorage
{
    Task<FileMetadata> SaveAsync(IFormFile file);
    Task<FileMetadata?> GetByIdAsync(string id);
    Task<FileMetadata?> GetByNameAsync(string name);
    IReadOnlyCollection<FileMetadata> GetAll();
    Task<bool> DeleteAsync(string id);
    Stream? OpenRead(string id);
    string StoragePath { get; }
}