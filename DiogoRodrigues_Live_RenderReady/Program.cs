using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

var apiKey = Environment.GetEnvironmentVariable("YOUTUBE_API_KEY") ?? "";
var channelIdEnv = Environment.GetEnvironmentVariable("CHANNEL_ID") ?? "";
var channelHandle = Environment.GetEnvironmentVariable("CHANNEL_HANDLE") ?? "";

builder.Services.AddHttpClient("yt", c =>
{
    c.BaseAddress = new Uri("https://www.googleapis.com/youtube/v3/");
    c.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/live", async (IHttpClientFactory httpFactory) =>
{
    var http = httpFactory.CreateClient("yt");
    string channelId = channelIdEnv;

    if (string.IsNullOrWhiteSpace(channelId) && !string.IsNullOrWhiteSpace(channelHandle))
    {
        var handleUrl = $"channels?part=id&forHandle={Uri.EscapeDataString(channelHandle)}&key={Uri.EscapeDataString(apiKey)}";
        try
        {
            var handleResp = await http.GetFromJsonAsync<dynamic>(handleUrl);
            channelId = (string?)handleResp?.items?[0]?.id ?? "";
        }
        catch {}
    }

    if (string.IsNullOrWhiteSpace(channelId))
        return Results.Json(new { live = false, error = "CHANNEL_ID não definido e não foi possível resolver via handle." });

    var searchUrl = $"search?part=snippet&channelId={Uri.EscapeDataString(channelId)}&eventType=live&type=video&maxResults=1&key={Uri.EscapeDataString(apiKey)}";
    try
    {
        var resp = await http.GetFromJsonAsync<dynamic>(searchUrl);
        var items = resp?.items;
        if (items is null || items.Count == 0) return Results.Json(new { live = false });
        var vid = (string?)items[0]?.id?.videoId ?? "";
        if (string.IsNullOrWhiteSpace(vid)) return Results.Json(new { live = false });
        var url = $"https://www.youtube.com/watch?v={vid}";
        return Results.Json(new { live = true, videoId = vid, url });
    }
    catch (Exception ex)
    {
        return Results.Json(new { live = false, error = ex.Message });
    }
});

app.Run();