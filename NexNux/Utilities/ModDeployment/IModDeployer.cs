using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NexNux.Models;

namespace NexNux.Utilities.ModDeployment;

public interface IModDeployer
{
    public event EventHandler<FileDeployedArgs> FileDeployed; 
    public Game CurrentGame { get; }
    public Task Deploy(List<Mod?> mods);
    public Task Clear();
}