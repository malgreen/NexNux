using NexNux.Core.Models;
using NexNux.Deployment.Models;

namespace NexNux.Deployment.Repositories;

public interface ILinkDeploymentRepository
{
    public Task LinkModsBottomUp(List<Mod> mods);
    public Task RestoreCache();
    public event EventHandler<DeployingModEventArgs> DeployingMod;
}