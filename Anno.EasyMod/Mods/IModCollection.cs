using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Mods
{
    public interface IModCollection<TMod> :
        IReadOnlyCollection<TMod>,
        INotifyCollectionChanged
        where TMod : IMod
    {
        int ActiveMods { get; }
        int ActiveSizeInMBs { get; }
        int InstalledSizeInMBs { get; }
        IEnumerable<string> ModIDs { get; }
        IReadOnlyList<TMod> Mods { get; }
        string ModsPath { get; }

        Task ChangeActivationAsync(IEnumerable<TMod> mods, bool active, CancellationToken ct = default);
        Task ChangeActivationAsync(TMod mod, bool active, CancellationToken ct = default);

        Task RemoveAsync(IEnumerable<TMod> mods, CancellationToken ct = default);
        Task RemoveAsync(TMod mod, CancellationToken ct);

        Task MakeObsoleteAsync(TMod mod, string path, CancellationToken ct = default);

        Task MoveIntoAsync(IModCollection<TMod> source, bool allowOldToOverwrite = false, CancellationToken ct = default);
        Task AddAsync(IEnumerable<TMod> mods, bool allowOldToOverwrite = false, CancellationToken ct = default);
        Task AddAsync(TMod mod, bool allowOldToOverwrite = false, CancellationToken ct = default);

        //Task LoadProfileAsync(ModActivationProfile profile);
        //IEnumerable<Mod> WithAttribute(IAttributeType attributeType);
    }
}
