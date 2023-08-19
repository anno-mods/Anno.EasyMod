using Modio.Models;
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
    public interface IModCollection: 
        IReadOnlyCollection<IMod>,
        INotifyCollectionChanged
    {
        int ActiveMods { get; }
        int ActiveSizeInMBs { get; }
        int InstalledSizeInMBs { get; }
        IEnumerable<string> ModIDs { get; }
        string ModsPath { get; }

        IReadOnlyList<IMod> Mods { get; }

        Task ChangeActivationAsync(IEnumerable<IMod> mods, bool active, CancellationToken ct = default);
        Task ChangeActivationAsync(IMod mod, bool active, CancellationToken ct = default);

        Task RemoveAsync(IEnumerable<IMod> mods, CancellationToken ct = default);
        Task RemoveAsync(IMod mod, CancellationToken ct = default);

        Task MakeObsoleteAsync(IMod mod, string path, CancellationToken ct = default);

        Task AddAsync(IEnumerable<IMod> mods, bool allowOldToOverwrite = false, CancellationToken ct = default);
        Task AddAsync(IMod mod, bool allowOldToOverwrite = false, CancellationToken ct = default);
    }

    public interface IModCollection<TMod> :
        IModCollection
        where TMod : IMod
    {
        Task ChangeActivationAsync(IEnumerable<TMod> mods, bool active, CancellationToken ct = default);
        Task ChangeActivationAsync(TMod mod, bool active, CancellationToken ct = default);

        Task RemoveAsync(IEnumerable<TMod> mods, CancellationToken ct = default);
        Task RemoveAsync(TMod mod, CancellationToken ct);

        Task MakeObsoleteAsync(TMod mod, string path, CancellationToken ct = default);

        [Obsolete("This Method is dangerous, broken and not supported." +
            "Use AddAsync instead and clean the source mod directory up by yourself! Only adding this so iMYA can respect that later.")]
        Task MoveIntoAsync(IModCollection<TMod> source, bool allowOldToOverwrite = false, CancellationToken ct = default);
        Task AddAsync(IEnumerable<TMod> mods, bool allowOldToOverwrite = false, CancellationToken ct = default);
        Task AddAsync(TMod mod, bool allowOldToOverwrite = false, CancellationToken ct = default);

        //Task LoadProfileAsync(ModActivationProfile profile);
        //IEnumerable<Mod> WithAttribute(IAttributeType attributeType);

        new IReadOnlyList<TMod> Mods { get; }
        IReadOnlyList<IMod> IModCollection.Mods { get => Mods.Cast<IMod>().ToList(); }

        Task IModCollection.ChangeActivationAsync(IEnumerable<IMod> mods, bool active, CancellationToken ct)
            => ChangeActivationAsync(mods.Cast<TMod>(), active, ct);
        Task IModCollection.ChangeActivationAsync(IMod mod, bool active, CancellationToken ct)
            => ChangeActivationAsync((TMod)mod, active, ct);

        Task IModCollection.RemoveAsync(IEnumerable<IMod> mods, CancellationToken ct)
            => RemoveAsync(mods.Cast<TMod>(), ct); 
        Task IModCollection.RemoveAsync(IMod mod, CancellationToken ct)
            => RemoveAsync((TMod)mod, ct);

        Task IModCollection.MakeObsoleteAsync(IMod mod, string path, CancellationToken ct)
            => MakeObsoleteAsync((TMod)mod, path, ct);

        Task IModCollection.AddAsync(IEnumerable<IMod> mods, bool allowOldToOverwrite, CancellationToken ct)
            => AddAsync(mods.Cast<TMod>(), allowOldToOverwrite, ct);
        Task IModCollection.AddAsync(IMod mod, bool allowOldToOverwrite, CancellationToken ct)
            => AddAsync((TMod)mod, allowOldToOverwrite, ct);
    }
}
