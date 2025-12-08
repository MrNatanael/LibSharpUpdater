using System;
using System.IO;
using System.Threading.Tasks;

namespace LibSharpUpdater;

public abstract class UpdateDeployer : IDisposable
{
    public abstract Task<UpdateResult> DeployAsync(UpdateDownloadEntry entry, Stream stream);
    public virtual void Dispose() {  }
}

public class UpdateResult(bool success, string? errorMessage, UpdateDownloadEntry target)
{
    public bool Success { get; } = success;
    public string? ErrorMessage { get; } = errorMessage;
    public UpdateDownloadEntry Target { get; } = target;
}