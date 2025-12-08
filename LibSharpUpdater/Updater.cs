using System;
using System.IO;
using System.Threading.Tasks;

namespace LibSharpUpdater;

public class Updater(UpdateProvider provider, UpdateDeployer deployer, FileVersion currentVersion) : IDisposable
{
    public virtual async Task<UpdateInfo> GetUpdateInfoAsync()
    {
        var all = await Provider.ProvideUpdateFilesAsync();
        var latest = Provider.ProvideLatestVersion(all);

        return new(latest, all, CurrentVersion);
    }
    public virtual Task<UpdateDownloadResult> DownloadUpdateAsync(UpdateDownloadEntry entry, Stream stream, ReportUpdateDownloadProgressHandler? progressHandler) => Provider.DownloadAsync(entry, stream, progressHandler);
    public virtual async Task<UpdateResult> DownloadAndDeployAsync(UpdateDownloadEntry entry, Stream stream, ReportUpdateDownloadProgressHandler? progressHandler)
    {
        var result = await DownloadUpdateAsync(entry, stream, progressHandler);
        if (!result.Success) return new(false, result.ErrorMessage, entry);
        return await Deployer.DeployAsync(entry, stream);
    }
    public virtual void Dispose()
    {
        Provider.Dispose();
        Deployer.Dispose();
    }

    public UpdateProvider Provider { get; } = provider;
    public UpdateDeployer Deployer { get; } = deployer;
    public FileVersion CurrentVersion { get; } = currentVersion;
}

public class UpdateInfo(UpdateFile lastestVersion, UpdateFile[] availableVersions, FileVersion currentVersion)
{
    public UpdateFile LatestVersion { get; } = lastestVersion;
    public UpdateFile[] AvailableVersions { get; } = availableVersions;
    public FileVersion CurrentVersion { get; } = currentVersion;
    public bool IsUpdateAvailable { get; } = currentVersion < lastestVersion.Version;
}