using StorageServer.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IFileStorage, FileStorage>();
builder.Services.AddEndpointsApiExplorer();

WebApplication app = builder.Build();

app.MapControllers();
app.MapGet("/", () => Results.Ok(new
{
    service = "StorageServer",
    version = "1.0",
    endpoints = new[]
    {
        "POST   /api/upload",
        "GET    /api/download/{id}",
        "GET    /api/files",
        "GET    /api/fileinfo/{id}",
        "DELETE /api/delete/{id}",
        "GET    /api/search?name=...",
        "GET    /api/stats",
        "GET    /api/metadata/{id}",
        "GET    /api/health",
        "POST   /api/admin/cleanup"
    }
}));

app.Run();

public partial class Program { }