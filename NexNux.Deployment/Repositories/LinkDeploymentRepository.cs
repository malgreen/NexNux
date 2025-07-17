using System.Runtime.InteropServices;
using NexNux.Core.Models;
using NexNux.Core.Utilities.Serialization;
using NexNux.Deployment.Models;
using NexNux.Deployment.Utilities.Serialization;

namespace NexNux.Deployment.Repositories;

public class LinkDeploymentRepository : ILinkDeploymentRepository
{
    private readonly string _cacheDirectory;
    private readonly string _gameDirectory;

    private readonly string _jsonPath;
    private readonly string _modsDirectory;

    public LinkDeploymentRepository(Game game)
    {
        _gameDirectory = game.GameDirectory;
        _modsDirectory = Path.Combine(game.NexNuxDirectory, "mods");
        _cacheDirectory = Path.Combine(game.NexNuxDirectory, "cache");
        _jsonPath = Path.Combine(game.NexNuxDirectory, "deployed_files.json");

        Directory.CreateDirectory(_cacheDirectory);
    }

    public event EventHandler<DeployingModEventArgs>? DeployingMod;

    public async Task LinkModsBottomUp(List<Mod> mods)
    {
        mods.Reverse();
        var totalMods = mods.Count;
        var modNumber = 0;
        foreach (var mod in mods)
        {
            DeployingMod?.Invoke(this, new DeployingModEventArgs(mod.Name, modNumber, totalMods));
            var linkedFiles = await GetLinkedFiles();
            foreach (var file in Directory.GetFiles(mod.Path))
            {
                var relativeFilePath = Path.GetRelativePath(FullPath(mod.Path), FullPath(file));
                var finalFilePath = Path.Combine(FullPath(_gameDirectory), relativeFilePath);
                var cachedFilePath = Path.Combine(FullPath(_cacheDirectory), relativeFilePath);

                if (linkedFiles.Exists(f => FullPath(f) == FullPath(finalFilePath)))
                    continue;

                if (File.Exists(finalFilePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(cachedFilePath) ??
                                              throw new InvalidOperationException());
                    File.Move(finalFilePath, cachedFilePath);
                }

                LinkFile(FullPath(file), FullPath(finalFilePath));
                linkedFiles.Add(FullPath(finalFilePath));
            }

            await JsonListHelper.SerializeListToJsonAsync(linkedFiles, _jsonPath, StringsSerializerContext.Default.ListString);
            modNumber++;
        }
    }

    public async Task RestoreCache()
    {
        var linkedFiles = await GetLinkedFiles();
        foreach (var file in linkedFiles) File.Delete(FullPath(file));

        await JsonListHelper.SerializeListToJsonAsync(new List<string>(), _jsonPath, StringsSerializerContext.Default.ListString);

        var cachedFiles = GetCachedFiles();
        foreach (var file in cachedFiles)
        {
            var relativePath = Path.GetRelativePath(FullPath(_cacheDirectory), FullPath(file));
            var finalPath = Path.Combine(_gameDirectory, relativePath);
            File.Move(FullPath(file), finalPath, true);
        }
    }

    private async Task<List<string>> GetLinkedFiles()
    {
        return await JsonListHelper.DeserializeJsonToListAsync(_jsonPath, StringsSerializerContext.Default.ListString);
    }

    private static bool LinkFile(string fromPath, string toPath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return CreateHardLink(toPath, fromPath, IntPtr.Zero);
        var link = File.CreateSymbolicLink(FullPath(toPath), FullPath(fromPath));
        return link.Exists;
    }

    private List<string> GetCachedFiles()
    {
        return Directory.GetFiles(_cacheDirectory, "*.*", SearchOption.AllDirectories).ToList();
    }

    /// <summary>
    ///     Shorthand for the Path.GetFullPath(string) method.
    /// </summary>
    /// <param name="path">Input path to a file/directory</param>
    /// <returns>A full path of a string.</returns>
    private static string FullPath(string path) => Path.GetFullPath(path);

    /// <summary>
    ///     Creates a hard link using Windows DLL import, therefore this method only works on Windows.
    ///     Taken from https://stackoverflow.com/a/3387777
    ///     FIXME: Should use LibraryImport attribute
    /// </summary>
    /// <param name="lpFileName">Target path</param>
    /// <param name="lpExistingFileName">Source path</param>
    /// <param name="lpSecurityAttributes">Should be IntPtr.Zero</param>
    /// <returns></returns>
    [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern bool CreateHardLink(
        string lpFileName,
        string lpExistingFileName,
        IntPtr lpSecurityAttributes
    );
}