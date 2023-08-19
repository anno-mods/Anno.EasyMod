using Anno.EasyMod.Metadata;
using Anno.EasyMod.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Mods.LocalMods
{
    public class LocalModFactory : IModFactory<LocalMod>
    {
        public IModinfoLoader _modinfoLoader;
        private ILogger<LocalModFactory> _logger;

        public LocalModFactory(
            IModinfoLoader modinfoLoader,
            ILogger<LocalModFactory> logger
            )
        {
            _modinfoLoader = modinfoLoader;
            _logger = logger;
        }

        public LocalMod? GetFromFolder(string modFolderPath, bool normalize = false)
        {
            var basePath = Path.GetDirectoryName(modFolderPath);
            if (basePath is null || !Directory.Exists(modFolderPath))
                return null;

            var folder = Path.GetFileName(modFolderPath);
            var isActive = !folder.StartsWith("-");
            var folderName = isActive ? folder : folder[1..];

            if (!_modinfoLoader.TryLoadFromFile(Path.Combine(modFolderPath, "modinfo.json"), out Modinfo? modinfo)) 
            {
                modinfo = _modinfoLoader.GetDummy(folderName);
            }

            var mod = new LocalMod(
                isActive,
                folderName,
                modinfo!,
                basePath);

            string[] modinfos = Directory.GetFiles(Path.Combine(basePath, folder), "modinfo.json", SearchOption.AllDirectories);

            if (modinfos.Length > 1)
            {
                foreach (var submodinfo in modinfos)
                {
                    if (submodinfo.ToLower() == Path.Combine(basePath, folder, "modinfo.json").ToLower())
                    {
                        continue;
                    }

                    LocalMod? submod = GetFromFolder(Path.GetDirectoryName(submodinfo) ?? "");
                    if (submod is not null)
                    {
                        mod.SubMods.Add(submod);
                    }
                }
            }
            if(normalize)
                Normalize(mod);
            return mod;
        }

        public void Normalize(LocalMod mod)
        {
            if (!mod.FolderName.StartsWith("-")) return;

            var trimAllDash = mod.FolderName;
            while (trimAllDash.StartsWith("-"))
                trimAllDash = trimAllDash[1..];

            string sourcePath = Path.Combine(mod.BasePath, mod.FullFolderName);
            string targetPath = Path.Combine(mod.BasePath, mod.IsActive ? "" : "-" + trimAllDash);
            try
            {
                DirectoryEx.CleanMove(sourcePath, targetPath);
                _logger.LogInformation($"Removed duplicate '-' from {mod.FullFolderName}");
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to remove duplicate '-' from {mod.FullFolderName}. Cause: {e.Message}");
            }

            mod.FolderName = trimAllDash;
        }

    }
}
