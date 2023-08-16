using Anno.EasyMod.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Mods.LocalMods
{
    public class LocalModCollectionFactory
    {
        bool AutofixSubfolder = false; 

        private IModFactory<LocalMod> _modFactory; 
        IServiceProvider _serviceProvider;

        public LocalModCollectionFactory(
            IModFactory<LocalMod> modFactory,
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _modFactory = modFactory;
        }

        public LocalModCollection Get(string Filepath)
        {
            return GetAsync(Filepath).Result;    
        }

        public async Task<LocalModCollection> GetAsync(string Filepath)
        {
            List<LocalMod> loadedMods = new(); 
            await Task.Run(() => loadedMods = LoadMods(Filepath));
            var collection = new LocalModCollection(
                _serviceProvider.GetRequiredService<IModFactory<LocalMod>>(),
                _serviceProvider.GetRequiredService<ILogger<LocalModCollection>>(),
                loadedMods)
            {
                ModsPath = Filepath
            };
            return collection; 
        }

        public List<LocalMod> LoadMods(String modsPath)
        {
            List<LocalMod> mods = new(); 
            if (Directory.Exists(modsPath))
            {
                if (AutofixSubfolder)
                    AutofixSubfolders(modsPath);

                var folders = Directory.EnumerateDirectories(modsPath)
                                        .Where(x => !Path.GetFileName(x).StartsWith("."));
                mods = folders.SelectNoNull(x => _modFactory.GetFromFolder(x, loadImages: true)).ToList();

                int i = 0;
            }
            else
            {
                mods = new();
            }
            return mods; 
        }

        private static void AutofixSubfolders(string modsPath)
        {
            foreach (var folder in Directory.EnumerateDirectories(modsPath))
            {
                if (Directory.Exists(Path.Combine(folder, "data")) || File.Exists(Path.Combine(folder, "modinfo.json")))
                    continue;

                var potentialMods = DirectoryEx.FindFolder(folder, "data").Select(x => Path.GetDirectoryName(x)!);
                foreach (var potentialMod in potentialMods)
                {
                    try
                    {
                        DirectoryEx.CleanMove(potentialMod, Path.Combine(modsPath, Path.GetFileName(potentialMod)));
                    }
                    catch
                    {
                        // TODO should we say something?
                    }
                }

                // only remove the parent folder if all content has been moved
                if (!Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories).Any())
                {
                    try
                    {
                        Directory.Delete(folder, true);
                    }
                    catch
                    {
                        // tough luck, but not harmful
                    }
                }
            }
        }
    }
}
