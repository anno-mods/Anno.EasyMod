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
    internal class ModioModCollection : IModCollection<ModioMod>
    {
        public int ActiveMods => throw new NotImplementedException();
        public int ActiveSizeInMBs => throw new NotImplementedException();
        public int InstalledSizeInMBs => throw new NotImplementedException();

        public IEnumerable<string> ModIDs => throw new NotImplementedException();
        public IReadOnlyList<ModioMod> Mods { get => _mods; }
        private List<ModioMod> _mods;

        public string ModsPath => throw new NotImplementedException();
        public int Count => throw new NotImplementedException();

        private Modio.Client _client; 

        public ModioModCollection(Modio.Client client) 
        {
            _client = client; 
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public async Task AddAsync(IEnumerable<ModioMod> mods, bool allowOldToOverwrite = false, CancellationToken ct = default)
        {
            foreach (ModioMod mod in mods)
            {
                await AddAsync_NoEvents(mod, allowOldToOverwrite, ct);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, mods));
        }

        public async Task AddAsync(ModioMod mod, bool allowOldToOverwrite = false, CancellationToken ct = default)
        {
            await AddAsync_NoEvents(mod, allowOldToOverwrite, ct);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, mod));
        }

        private async Task AddAsync_NoEvents(ModioMod mod, bool allowOldToOverwrite = false, CancellationToken ct = default)
        {
            if (Mods.Contains(mod))
                return;
            await _client.Games[4169].Mods.Subscribe(mod.ResourceID);
            _mods.Add(mod);
        }

        //We need access to users accountdata for this :( happy filedb patching
        public Task ChangeActivationAsync(IEnumerable<ModioMod> mods, bool active, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task ChangeActivationAsync(ModioMod mod, bool active, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task MakeObsoleteAsync(ModioMod mod, string path, CancellationToken ct = default)
        {
            await ChangeActivationAsync(mod, false, ct);
            mod.IsObsolete = true;
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, mod));
        }

        public async Task MoveIntoAsync(IModCollection<ModioMod> source, bool allowOldToOverwrite = false, CancellationToken ct = default)
        {
            await AddAsync(source, allowOldToOverwrite, ct);
        }

        public async Task RemoveAsync(IEnumerable<ModioMod> mods, CancellationToken ct = default)
        {
            foreach (var mod in mods)
            {
                if (!ct.IsCancellationRequested)
                    await RemoveAsync_NoEvents(mod, ct);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, mods.ToList()));
        }

        public async Task RemoveAsync(ModioMod mod, CancellationToken ct)
        {
            await RemoveAsync_NoEvents(mod, ct);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new[]{ mod }));
        }

        public async Task RemoveAsync_NoEvents(ModioMod mod, CancellationToken ct)
        {
            await _client.Games[4169].Mods.Unsubscribe(mod.ResourceID);
        }

        IEnumerator IEnumerable.GetEnumerator() => Mods.GetEnumerator();
        public IEnumerator<ModioMod> GetEnumerator() => Mods.GetEnumerator();
    }
}
