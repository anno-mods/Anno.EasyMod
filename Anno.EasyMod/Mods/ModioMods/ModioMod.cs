using Anno.EasyMod.Metadata;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Mods.ModioMods
{
    public class ModioMod : IMod
    {
        public string ModID { get => ResourceID.ToString(); }
        public uint ResourceID { get; init; }

        public string FullModPath { get; init; }
        public string BasePath { get; init; }
        public string FolderName { get; init; }
        public string FullFolderName { get; init; }

        public string Name { get; init; }
        public string[] Tags { get; init; }
        public float SizeInMB { get; init; }

        public IList<IMod> SubMods { get; init; }
        public IEnumerable<IMod> DistinctSubMods { get; init; }
        public IEnumerable<IMod> DistinctSubModsIncludingSelf { get; init; }

        public bool HasSubmods { get; init; }

        public Version? Version { get; init; }
        public Modinfo Modinfo { get; init; }

        public bool IsActive { get; set; }
        public bool IsRemoved { get ; set; }
        public bool IsObsolete { get; set ; }

        public void AdaptToCulture(CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public bool HasSameContentAs(IMod? Target)
        {
            throw new NotImplementedException();
        }

        public bool IsUpdateOf(IMod? Target)
        {
            throw new NotImplementedException();
        }
    }
}
