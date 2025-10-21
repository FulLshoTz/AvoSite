using System.Net.Http.Json;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);

// Env vars
var apiKey = Environment.GetEnvironmentVariable("YOUTUBE_API_KEY") ?? "";
var channelIdEnv = Environment.GetEnvironmentVariable("CHANNEL_ID") ?? "";
var channelHandle = Environment.GetEnvironmentVariable("CHANNEL_HANDLE") ?? "";

// HttpClient para YouTube
builder.Services.AddHttpClient("yt", c =>
{
    c.BaseAddress = new Uri("https://www.googleapis.com/youtube/v3/");
    c.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

// servir index.html e estáticos
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/live", async (IHttpClientFactory httpFactory) =>
{
    var http = httpFactory.CreateClient("yt");
    string channelId = channelIdEnv;

    // 1) Resolver handle -> channelId (se não houver CHANNEL_ID)
    if (string.IsNullOrWhiteSpace(channelId) && !string.IsNullOrWhiteSpace(channelHandle))
    {
        var handleUrl = $"channels?part=id&forHandle={Uri.EscapeDataString(channelHandle)}&key={Uri.EscapeDataString(apiKey)}";
        try
        {
            JsonNode? handleResp = await http.GetFromJsonAsync<JsonNode>(handleUrl);
            channelId = handleResp?["items"]?[0]?["id"]?.GetValue<string>() ?? "";
        }
        catch (Exception ex)
        {
            return Results.Json(new { live = false, error = "Erro a resolver handle: " + ex.Message });
        }
    }

    if (string.IsNullOrWhiteSpace(channelId))
        return Results.Json(new { live = false, error = "CHANNEL_ID não definido e não foi possível resolver via CHANNEL_HANDLE." });

    // 2) Procurar live atual
    var searchUrl =
        $"search?part=snippet&channelId={Uri.EscapeDataString(channelId)}&eventType=live&type=video&maxResults=1&key={Uri.EscapeDataString(apiKey)}";

    try
    {
        JsonNode? resp = await http.GetFromJsonAsync<JsonNode>(searchUrl);
        var items = resp?["items"] as JsonArray;

        if (items == null || items.Count == 0)
            return Results.Json(new { live = false });

        string? vid = items[0]?["id"]?["videoId"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(vid))
            return Results.Json(new { live = false });

        var url = $"https://www.youtube.com/watch?v={vid}";
        return Results.Json(new { live = true, videoId = vid, url });
    }
    catch (Exception ex)
    {
        return Results.Json(new { live = false, error = "Erro a consultar YouTube: " + ex.Message });
    }
});

app.Run();
