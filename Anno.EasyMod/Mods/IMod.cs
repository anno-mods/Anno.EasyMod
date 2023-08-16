using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Anno.EasyMod.Metadata;

namespace Anno.EasyMod.Mods
{
    public interface IMod
    {
        public string ModID { get; }

        public bool IsActive { get; internal set; }
        public bool IsActiveAndValid { get => IsActive && !IsRemoved; }
        public bool IsRemoved { get; internal set; }
        public bool IsObsolete { get; internal set; }

        public string FullModPath { get; }
        public string BasePath { get; }
        public string FolderName { get; }
        public string FullFolderName { get; }

        public string Name { get; }
        public string[] Tags { get; }
        public float SizeInMB { get; }

        public IList<IMod> SubMods { get; }
        public IEnumerable<IMod> DistinctSubMods { get; }
        public IEnumerable<IMod> DistinctSubModsIncludingSelf { get; }
        public bool HasSubmods { get; }

        public Version? Version { get; }
        public Modinfo Modinfo { get; }

        bool HasSameContentAs(IMod? Target);
        bool IsUpdateOf(IMod? Target);

        public void AdaptToCulture(CultureInfo culture);
    }
}
