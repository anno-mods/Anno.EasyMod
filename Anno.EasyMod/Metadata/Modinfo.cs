using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Metadata
{
    public class Modinfo
    {
        public Modinfo() { }

        /// <summary>
        /// A string representation of a version. <br/>This is only the value of the Modinfo, which can be null (aka "the modinfo file provides no version") <br/>
        /// You find the definitive version of a mod at <see cref="Mods.IMod.Version"/>
        /// </summary>
        public string? Version { get; set; }
        /// <summary>
        /// The ModID provided by the Modinfo <br/>This is only the value of the Modinfo, which can be null (aka "the modinfo file provides no ModID") <br/>
        /// You find the definitive ModID of a mod at <see cref="Mods.IMod.ModID"/>
        /// </summary>
        public string? ModID { get; set; }

        public string[]? IncompatibleIds { get; set; }
        public string[]? DeprecateIds { get; set; }
        public string[]? ModDependencies { get; set; }
        public Localized? Category { get; set; }
        public Localized? ModName { get; set; }
        public Localized? Description { get; set; }
        public Localized[]? KnownIssues { get; set; }
        public Dlc[]? DLCDependencies { get; set; }
        public string? CreatorName { get; set; }
        public string? CreatorContact { get; set; }
        public string[]? LoadAfterIds { get; set; }
        public int? ModioResourceId { get; set; }
    }
}
