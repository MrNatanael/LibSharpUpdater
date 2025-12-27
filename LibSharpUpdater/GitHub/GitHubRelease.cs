using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LibSharpUpdater.GitHub;

public class GitHubRelease
{
    [JsonInclude] public GitHubAsset[] Assets { get; private set; } = [];
    [JsonInclude] public string Name { get; private set; } = "";
    [JsonInclude] public string TagName { get; private set; } = "";
    [JsonInclude] public string Body { get; private set; } = "";
    [JsonInclude] public DateTime UpdatedAt { get; private set; }
    [JsonInclude] public bool Prerelease { get; private set; }

    public virtual UpdateFile? ToUpdateFile(GitHubAssetFilter filter)
    {
        if (!FileVersion.TryParse(TagName, out var version)) return null;

        List<UpdateDownloadEntry> downloads = new();
        foreach(var asset in filter(this))
        {
            downloads.Add(new(new($"releases/assets/{asset.Id}", UriKind.Relative), asset.Size));
        }

        return new(Name, version!, UpdatedAt, downloads.ToArray(), Body, Prerelease);
    }
}
public class GitHubAsset
{
    [JsonInclude] public ulong Id { get; private set; }
    [JsonInclude] public string Name { get; private set; } = "";
    [JsonInclude] public ulong Size { get; private set; }
    [JsonInclude] public DateTime UpdatedAt { get; private set; }
}