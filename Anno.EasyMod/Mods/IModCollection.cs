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

        IEnumerable<TMod> OfModType<TMod>() where TMod : IMod;
    }
}
