﻿using Anno.EasyMod.Metadata;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Mods.ModioMods
{
    [AddINotifyPropertyChangedInterface]
    [DebuggerDisplay("modio | Name: {Name}")]
    public class ModioMod : IMod
    {
        public string ModID { get => ResourceID.ToString(); }
        public uint ResourceID { get; init; }

        public string FullModPath { get => Path.Combine(BasePath, FolderName); }
        public string BasePath { get; init; }
        public string FolderName { get => ResourceID.ToString(); }
        public string FullFolderName { get => FolderName; }

        public string Name { get; init; }
        public string[] Tags { get; init; }
        public float SizeInMB { get; init; }

        public IList<IMod> SubMods { get; private set; }
        public IEnumerable<IMod> DistinctSubMods { get => SubMods.DistinctBy(x => (x.ModID, x.Version)); }
        public bool HasSubmods { get => SubMods.Count() > 0; }
        public IEnumerable<IMod> DistinctSubModsIncludingSelf { get => DistinctSubMods.Prepend(this); }

        public Version? Version { get; init; }
        public Modinfo Modinfo { get; init; }

        public bool IsActive { get; set; }
        public bool IsRemoved { get ; set; }
        public bool IsObsolete { get; set ; }

        internal ModioMod()
        { 
            SubMods = new List<IMod>();
        }

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
