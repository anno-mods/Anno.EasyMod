using Anno.EasyMod.Mods.LocalMods;
using Modio;
using Modio.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Mods.ModioMods
{
    public class ModioModCollection : IModCollection
    {
        public int ActiveMods { get => Mods.Count; }
        public int ActiveSizeInMBs { get => (int)Mods.Select(x => x.SizeInMB).Aggregate((x, y) => x + y); }
        public int InstalledSizeInMBs { get => ActiveSizeInMBs;}

        public IEnumerable<string> ModIDs => Mods.Select(x => x.ModID);
        public IReadOnlyList<IMod> Mods { get => _mods; }
        private List<IMod> _mods;

        public string ModsPath => throw new NotImplementedException();
        public int Count { get => Mods.Count; }

        private Modio.Client _client; 

        public ModioModCollection(Modio.Client client) 
        {
            _client = client; 
            _mods = new List<IMod>();
        }

        private void ThrowIfNotModioMod(IMod mod)
        {
            if (mod is not ModioMod)
                throw new ArgumentException($"Invalid Type for LocalModCollection. Expected: {typeof(ModioMod).FullName}. Actual: {mod.GetType().FullName}");
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged = delegate { };

        public async Task AddAsync(IEnumerable<IMod> mods, bool allowOldToOverwrite = false, CancellationToken ct = default)
        {
            foreach (IMod mod in mods)
            {
                ThrowIfNotModioMod(mod);
                await AddAsync_NoEvents(mod, allowOldToOverwrite, ct);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, mods));
        }

        public async Task AddAsync(IMod mod, bool allowOldToOverwrite = false, CancellationToken ct = default)
        {
            ThrowIfNotModioMod(mod);
            await AddAsync_NoEvents(mod, allowOldToOverwrite, ct);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, mod));
        }

        private async Task AddAsync_NoEvents(IMod mod, bool allowOldToOverwrite = false, CancellationToken ct = default)
        {
            var modioMod = mod as ModioMod;
            if (Mods.Contains(mod))
                return;
            await _client.Games[4169].Mods.Subscribe(modioMod!.ResourceID);
            _mods.Add(mod);
        }

        //We need access to users accountdata for this :( happy filedb patching
        public Task ChangeActivationAsync(IEnumerable<IMod> mods, bool active, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task ChangeActivationAsync(IMod mod, bool active, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task MakeObsoleteAsync(IMod mod, string path, CancellationToken ct = default)
        {
            ThrowIfNotModioMod(mod);
            await ChangeActivationAsync(mod, false, ct);
            mod.IsObsolete = true;
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, mod));
        }

        public async Task MoveIntoAsync(IModCollection source, bool allowOldToOverwrite = false, CancellationToken ct = default)
        {
            await AddAsync(source.Mods, allowOldToOverwrite, ct);
        }

        public async Task RemoveAsync(IEnumerable<IMod> mods, CancellationToken ct = default)
        {
            foreach (var mod in mods)
            {
                ThrowIfNotModioMod(mod);
                if (!ct.IsCancellationRequested)
                    await RemoveAsync_NoEvents(mod, ct);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, mods.ToList()));
        }

        public async Task RemoveAsync(IMod mod, CancellationToken ct)
        {
            ThrowIfNotModioMod(mod);
            await RemoveAsync_NoEvents(mod, ct);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new[]{ mod }));
        }

        public async Task RemoveAsync_NoEvents(IMod mod, CancellationToken ct)
        {
            ThrowIfNotModioMod(mod);
            var modioMod = mod as ModioMod;
            await _client.Games[4169].Mods.Unsubscribe(modioMod!.ResourceID);
        }

        IEnumerator IEnumerable.GetEnumerator() => Mods.GetEnumerator();
        public IEnumerator<IMod> GetEnumerator() => Mods.GetEnumerator();

        public IEnumerable<TMod> OfModType<TMod>() where TMod : IMod
        {
            return Mods.Cast<TMod>();
        }
    }
}
