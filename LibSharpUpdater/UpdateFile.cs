using System;

namespace LibSharpUpdater;

public class UpdateFile(string name, FileVersion version, DateTime? date, UpdateDownloadEntry[] downloads, string? changelogs)
{
    public string Name { get; } = name;
    public FileVersion Version { get; } = version;
    public DateTime? Date { get; } = date;
    public UpdateDownloadEntry[] Downloads { get; } = downloads;
    public string? Changelogs { get; } = changelogs;
}

public record UpdateDownloadEntry(Uri Uri, ulong? Size)
{
    public Uri Uri { get; } = Uri;
    public ulong? Size { get; } = Size;
}