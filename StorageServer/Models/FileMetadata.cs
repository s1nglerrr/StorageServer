namespace StorageServer.Models;

public class FileMetadata
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string OriginalName { get; set; } = string.Empty;
    public string StoredName { get; set; } = string.Empty; // id + extension
    public string ContentType { get; set; } = "application/octet-stream";
    public long Size { get; set; }
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}