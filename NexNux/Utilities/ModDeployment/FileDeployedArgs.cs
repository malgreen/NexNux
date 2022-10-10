using System;

namespace NexNux.Utilities.ModDeployment;

public class FileDeployedArgs : EventArgs
{
    public double Progress { get; set; }
}