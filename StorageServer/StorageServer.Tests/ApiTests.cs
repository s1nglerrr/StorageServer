using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

public class ApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public ApiTests(ApiFactory factory) => _client = factory.CreateClient();

    private record Meta(string Id, string OriginalName, string ContentType, long Size);

    private static string UniqueName() => "f" + Guid.NewGuid().ToString("N") + ".txt";

    private static ByteArrayContent TextPart(string content)
    {
        var part = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
        part.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        return part;
    }

    private async Task<Meta> UploadAsync(string name, string content)
    {
        using var form = new MultipartFormDataContent();
        form.Add(TextPart(content), "file", name);
        var r = await _client.PostAsync("/api/upload", form);
        r.EnsureSuccessStatusCode();
        return (await r.Content.ReadFromJsonAsync<Meta>())!;
    }

    // ---------- Служебные ----------

    [Fact]
    public async Task Root_ReturnsServiceInfo()
    {
        var r = await _client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Contains("StorageServer", await r.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var r = await _client.GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }

    // ---------- Загрузка и скачивание ----------

    [Fact]
    public async Task Upload_ThenDownload_ReturnsSameContent()
    {
        var name = UniqueName();
        const string content = "hello storage";
        var meta = await UploadAsync(name, content);

        Assert.Equal(name, meta.OriginalName);
        Assert.Equal((long)content.Length, meta.Size);

        var r = await _client.GetAsync($"/api/download/{meta.Id}");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal(content, await r.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task DownloadByName_ReturnsFile()
    {
        var name = UniqueName();
        await UploadAsync(name, "by name");

        var r = await _client.GetAsync($"/api/download/byname/{name}");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal("by name", await r.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Download_UnknownId_ReturnsNotFound()
    {
        var r = await _client.GetAsync("/api/download/no-such-id");
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
    }

    [Fact]
    public async Task Upload_WithoutFile_ReturnsBadRequest()
    {
        using var form = new MultipartFormDataContent();
        var r = await _client.PostAsync("/api/upload", form);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task UploadMany_SavesAllFiles()
    {
        using var form = new MultipartFormDataContent();
        form.Add(TextPart("one"), "files", UniqueName());
        form.Add(TextPart("two"), "files", UniqueName());

        var r = await _client.PostAsync("/api/upload/many", form);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);

        var list = await r.Content.ReadFromJsonAsync<List<Meta>>();
        Assert.Equal(2, list!.Count);
    }

    // ---------- Информация о файлах ----------

    [Fact]
    public async Task FileInfo_ReturnsMetadata()
    {
        var name = UniqueName();
        var meta = await UploadAsync(name, "info");

        var r = await _client.GetAsync($"/api/fileinfo/{meta.Id}");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);

        var got = await r.Content.ReadFromJsonAsync<Meta>();
        Assert.Equal(name, got!.OriginalName);
    }

    [Fact]
    public async Task FileInfo_UnknownId_ReturnsNotFound()
    {
        var r = await _client.GetAsync("/api/fileinfo/no-such-id");
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
    }

    [Fact]
    public async Task Metadata_ContainsDownloadUrl()
    {
        var meta = await UploadAsync(UniqueName(), "meta");

        var r = await _client.GetAsync($"/api/metadata/{meta.Id}");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);

        var json = await r.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal($"/api/download/{meta.Id}", json.GetProperty("downloadUrl").GetString());
    }

    [Fact]
    public async Task Files_ListContainsUploaded()
    {
        var meta = await UploadAsync(UniqueName(), "list");

        var r = await _client.GetAsync("/api/files");
        var json = await r.Content.ReadFromJsonAsync<JsonElement>();
        var ids = json.GetProperty("items").EnumerateArray()
                      .Select(i => i.GetProperty("id").GetString());

        Assert.Contains(meta.Id, ids);
    }

    // ---------- Поиск ----------

    [Fact]
    public async Task Search_FindsByName()
    {
        var unique = Guid.NewGuid().ToString("N");
        await UploadAsync($"report-{unique}.txt", "search");

        var r = await _client.GetAsync($"/api/search?name={unique}");
        var json = await r.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(1, json.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task Search_EmptyName_ReturnsBadRequest()
    {
        var r = await _client.GetAsync("/api/search?name=");
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    // ---------- Удаление ----------

    [Fact]
    public async Task Delete_RemovesFile()
    {
        var meta = await UploadAsync(UniqueName(), "to delete");

        var del = await _client.DeleteAsync($"/api/delete/{meta.Id}");
        Assert.Equal(HttpStatusCode.OK, del.StatusCode);

        var get = await _client.GetAsync($"/api/download/{meta.Id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task Delete_UnknownId_ReturnsNotFound()
    {
        var r = await _client.DeleteAsync("/api/delete/no-such-id");
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
    }

    // ---------- Статистика и админка ----------

    [Fact]
    public async Task Stats_CountsUploadedFiles()
    {
        await UploadAsync(UniqueName(), "stats");

        var r = await _client.GetAsync("/api/stats");
        var json = await r.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(json.GetProperty("count").GetInt32() >= 1);
        Assert.True(json.GetProperty("totalSize").GetInt64() >= 5);
    }

    [Fact]
    public async Task AdminCleanup_KeepsExistingFiles()
    {
        var meta = await UploadAsync(UniqueName(), "keep me");

        var r = await _client.PostAsync("/api/admin/cleanup", null);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);

        var get = await _client.GetAsync($"/api/fileinfo/{meta.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }
}