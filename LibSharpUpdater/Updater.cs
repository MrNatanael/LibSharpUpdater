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
    public virtual Task<Stream> DownloadUpdateAsync(UpdateDownloadEntry entry, ReportUpdateDownloadProgressHandler? progressHandler) => Provider.DownloadAsync(entry, progressHandler);
    public virtual async Task<UpdateResult> DownloadAndDeployAsync(UpdateDownloadEntry entry, ReportUpdateDownloadProgressHandler? progressHandler)
    {
        using var s = await DownloadUpdateAsync(entry, progressHandler);
        return await Deployer.DeployAsync(entry, s);
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
    public UpdateFile LastestVersion { get; } = lastestVersion;
    public UpdateFile[] AvailableVersions { get; } = availableVersions;
    public FileVersion CurrentVersion { get; } = currentVersion;
    public bool IsUpdateAvailable { get; } = currentVersion < lastestVersion.Version;
}