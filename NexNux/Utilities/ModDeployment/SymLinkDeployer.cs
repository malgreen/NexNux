using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using NexNux.Models;

namespace NexNux.Utilities.ModDeployment;

public class SymLinkDeployer : IModDeployer
{
    public SymLinkDeployer(Game game)
    {
        CurrentGame = game;
        _deployPath = CurrentGame.DeployDirectory;
        _cachePath = Path.Combine(CurrentGame.ModSettingsDirectory, "__deploycache");
        _jsonPath = Path.Combine(CurrentGame.ModSettingsDirectory, "DeployedFiles.json");
        _deployedFiles = new List<string>();
        _cachedFiles = new List<string>();

        Directory.CreateDirectory(_cachePath);
    }

    public event EventHandler<FileDeployedArgs>? FileDeployed;
    public Game CurrentGame { get; }
    private readonly string _deployPath;
    private readonly string _cachePath;
    private readonly string _jsonPath;
    private List<string> _deployedFiles;
    private List<string> _cachedFiles;

    /// <summary>
    /// Deploys given list of files to the deployer's game's 'deploy' folder
    /// </summary>
    /// <param name="mods"></param>
    public Task Deploy(List<Mod?> mods)
    {
        CheckAppPrivilege();
        LoadLinkedMods();
        RestoreCache();
        LinkMods(mods);
        SaveLinkedMods();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Purges all deployed files from the deployer's game's 'deploy' folder
    /// </summary>
    public void Clear()
    {
        RestoreCache();
    }

    /// <summary>
    /// Removes all links in the DeployedFiles.json, and then moves all files in __deploycache to deploy directory
    /// </summary>
    private void RestoreCache()
    {
        // First remove the already deployed files
        foreach (string filePath in _deployedFiles)
        {
            File.Delete(filePath);
        }
        // Moves all files in cache to deploy dir
        DirectoryInfo cacheDir = new DirectoryInfo(_cachePath);
        foreach (string filePath in _cachedFiles)
        {
            string subPath = Path.GetRelativePath(_cachePath, filePath);
            string finalPath = Path.Combine(_deployPath, subPath);
            File.Move(filePath, finalPath);
        }

        _deployedFiles = new List<string>();
    }

    private void LinkMods(List<Mod?> mods)
    {
        double fileNumber = 0;
        foreach (Mod? mod in mods)
        {
            if (mod == null) continue;
            DirectoryInfo modDir = new DirectoryInfo(mod.ModPath);
            foreach (FileInfo file in modDir.GetFiles("*", SearchOption.AllDirectories))
            {
                LinkFile(file, modDir);
                FileDeployedArgs args = new FileDeployedArgs();
                args.Progress = fileNumber;
                OnFileLinked(args);
                fileNumber++;
            }
        }
    }

    private void LinkFile(FileInfo file, DirectoryInfo modDir)
    {
        string subPath = Path.GetRelativePath(modDir.FullName, file.FullName); // to remove the mods directory + mod name
        string finalPath = Path.Combine(_deployPath, subPath); // final path to deploy to, including file name

        //if the file already exists, and was not deployed by NexNux, we move it to the deploy cache
        if (File.Exists(finalPath) && !_deployedFiles.Exists(p => p == finalPath))
        {
            string cacheFile = Path.Combine(_cachePath, Path.GetFileName(finalPath));
            Directory.CreateDirectory(Path.GetDirectoryName(cacheFile));
            File.Move(finalPath, cacheFile);
            _cachedFiles.Add(cacheFile);
        }

        // if the file already exists and was deployed already by nexnux
        else if (File.Exists(finalPath) && _deployedFiles.Exists(p => p == finalPath))
        {
            File.Delete(finalPath);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(finalPath));
        File.CreateSymbolicLink(finalPath, file.FullName);
        _deployedFiles.Add(finalPath);
    }

    private void SaveLinkedMods()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_jsonPath));
        using FileStream createStream = File.Create(_jsonPath);
        JsonSerializer.Serialize(createStream, _deployedFiles, new JsonSerializerOptions() { WriteIndented = true });
        createStream.Dispose();
    }
    private void LoadLinkedMods()
    {
        if (!File.Exists(_jsonPath))
            SaveLinkedMods();
        string fileContent = File.ReadAllText(_jsonPath);
        _deployedFiles = JsonSerializer.Deserialize<List<string>>(fileContent);
    }

    private void CheckAppPrivilege()
    {
        // Creating symbolic links in Windows requires admin rights
        // Taken from https://stackoverflow.com/a/52745016
        string name = System.AppDomain.CurrentDomain.FriendlyName;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    throw new InvalidOperationException($"Application must be run as administrator. Right click the {name} file and select 'run as administrator'.");
                }
            }
        }
        // Linux doesn't require admin rights for symlinks, so this check is not needed AFAIK
        //else
        //{
        //    throw new InvalidOperationException($"Application must be run as root/sudo. From terminal, run the executable as 'sudo {name}'");
        //}
    }

    protected virtual void OnFileLinked(FileDeployedArgs e)
    {
        EventHandler<FileDeployedArgs> handler = FileDeployed;
        if (handler == null) return;
        handler(this, e);
    }
}