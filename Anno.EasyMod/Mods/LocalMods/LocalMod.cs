using Anno.EasyMod.Attributes;
using Anno.EasyMod.Metadata;
using Anno.EasyMod.Utils;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Mods.LocalMods
{
    enum LocalModStatus { Default, New, Updated, Obsolete }

    [AddINotifyPropertyChangedInterface]
    [DebuggerDisplay("Local | Name: {Name}")]
    public class LocalMod : IMod
    {
        public String ModID => Modinfo.ModID ?? FolderName;

        [DoNotNotify]
        public Modinfo Modinfo { get; private init; }

        [DoNotNotify]
        public Version? Version { get; private init; }
        public bool HasModinfo => Modinfo is not null;

        [DoNotNotify]
        public float SizeInMB { get; private set; }

        public string Name { get => Modinfo.ModName?.English!; }

        [DoNotNotify]
        public string[] Tags { get; set; }

        #region ModLoader Info
        /// <summary>
        /// Folder name including activation "-".
        /// </summary>
        public string FullFolderName => (IsActive ? "" : "-") + FolderName;

        /// <summary>
        /// Folder name excluding activation "-".
        /// </summary>
        public string FolderName { get; internal set; }

        /// <summary>
        /// "-" activation.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// "-" activation and valid.
        /// </summary>
        public bool IsActiveAndValid => IsActive && !IsRemoved;

        /// <summary>
        /// Mod disappeared.
        /// </summary>
        public bool IsRemoved { get; set; }

        /// <summary>
        /// Mod has been marked as Obsolete
        /// </summary>
        public bool IsObsolete { get; set; }

        /// <summary>
        /// Full path to mod folder.
        /// </summary>
        public string FullModPath => Path.Combine(BasePath, FullFolderName);

        [DoNotNotify]
        public string BasePath { get; private set; } // TODO use ModDirectory as parent and retrieve it from there as soon as it's not a global manager anymore
        #endregion

        #region SubMods
        [DoNotNotify]
        public IList<IMod> SubMods { get; private set; }

        [DoNotNotify]
        public IEnumerable<IMod> DistinctSubMods { get => SubMods.DistinctBy(x => (x.ModID, x.Version)); }

        [DoNotNotify]
        public bool HasSubmods { get => SubMods.Count() > 0; }

        [DoNotNotify]
        public IEnumerable<IMod> DistinctSubModsIncludingSelf { get => DistinctSubMods.Prepend(this); }

        public IModAttributeCollection Attributes { get; }

        public Uri? Image { get; init; }

        #endregion

        #region Constructor 

        public LocalMod(bool isActive, string folderName, Modinfo modinfo, string basePath)
        { 
            IsActive = isActive;
            FolderName = folderName;
            BasePath = basePath;
            Modinfo = modinfo;

            SubMods = new List<IMod>();
            Attributes = new ModAttributeCollection(); 

            if (VersionEx.TryParse(Modinfo.Version, out var version))
                Version = version;

            //TODO move to file access
            var info = new DirectoryInfo(FullModPath);
            SizeInMB = (float)Math.Round((decimal)info.EnumerateFiles("*", SearchOption.AllDirectories).Sum(x => x.Length) / 1024 / 1024, 1);
        }


        public bool IsUpdateOf(IMod? target)
        {
            if (target is null || target.IsRemoved)
                return true;
            if (target.Modinfo.ModID != Modinfo.ModID)
                return false;

            // compare content when unversioned
            if (target.Version is null && Version is null)
                return !HasSameContentAs(target); // consider same as outdated

            // prefer versioned mods
            if (target.Version is null)
                return true;
            if (Version is null)
                return false;

            if (Version == target.Version)
                return !HasSameContentAs(target); // consider same as outdated

            return Version > target.Version;
        }

        public bool HasSameContentAs(IMod? target)
        {
            if (target is null) return false;

            if (Modinfo.Version != target.Modinfo.Version)
                return false;

            var dirA = new DirectoryInfo(FullModPath);
            var dirB = new DirectoryInfo(target.FullModPath);
            var listA = dirA.GetFiles("*", SearchOption.AllDirectories);
            var listB = dirB.GetFiles("*", SearchOption.AllDirectories);

            // TODO filter tweaking files!

            // first compare path only to avoid costly md5 checks
            var pathComparer = new FilePathComparer(prefixPathA: dirA.FullName, prefixPathB: dirB.FullName);
            bool areIdentical = listA.SequenceEqual(listB, pathComparer);
            if (!areIdentical)
                return false;

            // no path (or file length) difference, now go for file content
            return listA.SequenceEqual(listB, new FileMd5Comparer());
        }

        public void AdaptToCulture(CultureInfo culture)
        { 
            //Todo adapt name    
        }

        #endregion
    }
}
