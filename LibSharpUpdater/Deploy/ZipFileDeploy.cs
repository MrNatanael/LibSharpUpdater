using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LibSharpUpdater.Deploy;

public class ZipFileDeploy : UpdateDeployer
{
    public override Task<UpdateResult> DeployAsync(UpdateDownloadEntry entry, Stream stream)
    {
        throw new NotImplementedException();
    }
}
