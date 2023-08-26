using Anno.EasyMod.Metadata;
using Anno.EasyMod.Mods.LocalMods;
using Anno.EasyMod.Utils;
using Modio.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Mods.ModioMods
{
    public class ModioModFactory : IModioModFactory
    {
        private String ModioPath = "C:\\Users\\Public\\mod.io\\4169\\mods";

        private IModinfoLoader _modinfoLoader;
        private IModFactory<LocalMod> _localModFactory;

        public ModioModFactory(
            IModFactory<LocalMod> localModFactory,
            IModinfoLoader modinfoLoader)
        {
            _localModFactory = localModFactory;
            _modinfoLoader = modinfoLoader;
        }

        public ModioMod Get(Modio.Models.Mod modDto)
        {
            if (!VersionEx.TryParse(modDto.Modfile?.Version, out var version))
            {
                version = Version.Parse("1.0.0");
            }
            var modinfo = _modinfoLoader.GetModio(modDto);

            var mod = new ModioMod()
            {
                ResourceID = modDto.Id,
                Name = modDto.Name ?? String.Empty,
                Tags = modDto.Tags.Where(x => x.Name is not null).Select(x => x.Name!).ToArray(),
                Size = modDto.Modfile?.FileSize ?? 0,
                Version = version!,
                Modinfo = modinfo,

                IsActive = true,    //load that value from the accountdata...
                IsRemoved = false,
                IsObsolete = false,
                BasePath = ModioPath,
                Image = modDto.Logo?.Thumb1280x720
            };

            if (!mod.HasLocalAccess)
                return mod;

            string[] modinfos = Directory.GetFiles(mod.FullModPath, "modinfo.json", SearchOption.AllDirectories);

            //load submods with the local mod thingy;
            if (modinfos.Length > 0)
            {
                foreach (var submodinfo in modinfos)
                {
                    LocalMod? submod = _localModFactory.GetFromFolder(Path.GetDirectoryName(submodinfo) ?? "");
                    if (submod is not null)
                    {
                        mod.SubMods.Add(submod);
                    }
                }
            }
            return mod;
        }
    }
}
