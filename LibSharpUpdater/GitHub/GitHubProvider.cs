using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace LibSharpUpdater.GitHub;

public class GitHubProvider : UpdateProvider
{
    public override async Task<UpdateDownloadResult> DownloadAsync(UpdateDownloadEntry entry, Stream stream, ReportUpdateDownloadProgressHandler? progressHandler)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, entry.Uri);
        req.Headers.Accept.Clear();
        req.Headers.Accept.Add(new("application/octet-stream"));

        using var res = await Http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
        if (!res.IsSuccessStatusCode) return new(false, await res.Content.ReadAsStringAsync(), 0);

        using var s = await res.Content.ReadAsStreamAsync();
        byte[] buffer = new byte[2048];
        ulong downloaded = 0;
        while(true)
        {
            int br = await s.ReadAsync(buffer, 0, buffer.Length);
            if (br == 0) break;

            stream.Write(buffer, 0, br);
            downloaded += (ulong)br;
            progressHandler?.Invoke(entry, downloaded);
        }

        return new(true, null, downloaded);
    }

    public override async Task<UpdateFile[]> ProvideUpdateFilesAsync() => await ProvideUpdateFilesAsync(0);
    public virtual async Task<UpdateFile[]> ProvideUpdateFilesAsync(int maximum)
    {
        using var res = await Http.GetAsync(maximum <= 0 ? "releases" : $"releases?per_page={maximum}");

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
            var file = await ProvideUpdateFileAsync(r);
            if (file is null || file.Downloads.Length == 0) continue;

            result.Add(file);
        }

        return result.ToArray();
    }

    public override void Dispose() => Http.Dispose();

    protected virtual Task<UpdateFile?> ProvideUpdateFileAsync(GitHubRelease release) => Task.FromResult(release.ToUpdateFile(AssetFilter));

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
    protected HttpClient Http { get; } = new(new HttpClientHandler()
    {
        AllowAutoRedirect = true,
        MaxAutomaticRedirections = 50
    });
    public GitHubAssetFilter AssetFilter { get; }
    protected static JsonSerializerOptions JsonOptions { get; } = new()
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