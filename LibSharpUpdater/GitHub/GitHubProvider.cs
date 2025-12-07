using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace LibSharpUpdater.GitHub;

public class GitHubProvider : UpdateProvider
{
    public override async Task<Stream> DownloadAsync(UpdateDownloadEntry entry, ReportUpdateDownloadProgressHandler? progressHandler)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, entry.Uri);
        using var res = await Http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);

        MemoryStream ms = new();

        using var s = await res.Content.ReadAsStreamAsync();
        byte[] buffer = new byte[2048];
        while(true)
        {
            int br = await s.ReadAsync(buffer, 0, buffer.Length);
            if (br == 0) break;

            ms.Write(buffer, 0, br);
            progressHandler?.Invoke(entry, (ulong)ms.Length);
        }

        ms.Position = 0;
        return ms;
    }

    public override async Task<UpdateFile[]> ProvideUpdateFilesAsync()
    {
        using var res = await Http.GetAsync("releases");
        GitHubRelease[]? releases;
        try
        {
            releases = await res.Content.ReadFromJsonAsync<GitHubRelease[]>(JsonOptions);
            if (releases == null) return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return [];
        }

        List<UpdateFile> result = new();
        foreach (var r in releases)
        {
            var file = r.ToUpdateFile(AssetFilter);
            if (file is null || file.Downloads.Length == 0) continue;

            result.Add(file);
        }

        return result.ToArray();
    }

    public override void Dispose() => Http.Dispose();

    public GitHubProvider(GitHubRepository repository, string token, GitHubAssetFilter filter)
    {
        Repository = repository;
        AssetFilter = filter;

        Http.BaseAddress = new Uri($"https://api.github.com/repos/{repository}/");
        Http.DefaultRequestHeaders.Authorization = new("Bearer", token);
        Http.DefaultRequestHeaders.Accept.Add(new("application/vnd.github+json"));
        Http.DefaultRequestHeaders.Accept.Add(new("application/octet-stream"));
        Http.DefaultRequestHeaders.UserAgent.Add(new("LibSharpUpdater", "1.0"));
    }

    public GitHubRepository Repository { get; }
    private HttpClient Http { get; } = new(new HttpClientHandler()
    {
        AllowAutoRedirect = true,
        MaxAutomaticRedirections = 50
    });
    public GitHubAssetFilter AssetFilter { get; }
    private static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };
}

public struct GitHubRepository
{
    public string Owner { get; }
    public string Name { get; }
    public GitHubRepository(string owner, string name)
    {
        Owner = owner;
        Name = name;
    }
    public override string ToString() => $"{Owner}/{Name}";
}

public delegate IEnumerable<GitHubAsset> GitHubAssetFilter(GitHubRelease release);