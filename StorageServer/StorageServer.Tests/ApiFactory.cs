using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dir =
        Path.Combine(Path.GetTempPath(), "storage-tests-" + Guid.NewGuid());

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, cfg) =>
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:Path"] = _dir
            }));
        builder.ConfigureLogging(l => l.ClearProviders());
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try { Directory.Delete(_dir, true); } catch { }
    }
}