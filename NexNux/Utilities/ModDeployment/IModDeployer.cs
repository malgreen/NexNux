using System.Collections.Generic;
using NexNux.Models;

namespace NexNux.Utilities.ModDeployment;

public interface IModDeployer
{
    public Game CurrentGame { get; }
    public void Deploy(List<Mod> mods);
    public void Clear();
}