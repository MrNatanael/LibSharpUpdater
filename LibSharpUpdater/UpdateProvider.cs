using System;
using System.IO;
using System.Threading.Tasks;

namespace LibSharpUpdater;

public abstract class UpdateProvider : IDisposable
{
    public abstract Task<UpdateFile[]> ProvideUpdateFilesAsync();
    public abstract Task<Stream> DownloadAsync(UpdateDownloadEntry entry, ReportUpdateDownloadProgressHandler? progressHandler);

    public virtual UpdateFile ProvideLatestVersion(UpdateFile[] files)
    {
        FileVersion? latestVersion = null;
        int latestVersionIdx = 0;
        for(int i = 0; i < files.Length; i++)
        {
            var fileVersion = files[i].Version;
            if(latestVersion is null || fileVersion > latestVersion)
            {
                latestVersion = fileVersion;
                latestVersionIdx = i;
            }
        }

        return files[latestVersionIdx];
    }
    public virtual async Task<UpdateFile> ProvideLatestVersionAsync()
    {
        var files = await ProvideUpdateFilesAsync();
        return ProvideLatestVersion(files);
    }

    public virtual void Dispose() { }
}

public delegate void ReportUpdateDownloadProgressHandler(UpdateDownloadEntry entry, ulong downloaded);