﻿using Anno.EasyMod.Accountdata;
using Anno.EasyMod.ModioWrapper;
using Modio.Models;
using System.Collections;
using System.Collections.Specialized;

namespace Anno.EasyMod.Mods.ModioMods
{
    public class ModioModCollection : IModCollection
    {
        public int ActiveMods { get => Mods.Count; }
        public long ActiveSize { get => (int)Mods.Select(x => x.Size).Aggregate((x, y) => x + y); }
        public long InstalledSize { get => ActiveSize;}

        public IEnumerable<string> ModIDs => Mods.Select(x => x.ModID);
        public IReadOnlyList<IMod> Mods { get => _mods; }
        private List<IMod> _mods;

        public string ModsPath => throw new NotImplementedException();
        public int Count { get => Mods.Count; }

        private IModioClientProvider _clientProvider;
        private AccountdataAccess _access;

        public ModioModCollection(IModioClientProvider clientProvider, AccountdataAccess access) 
        {
            _clientProvider = clientProvider;
            _access = access;
            _mods = new List<IMod>();
        }

        private void ThrowIfNotModioMod(IMod mod)
        {
            if (mod is not ModioMod)
                throw new ArgumentException($"Invalid Type for LocalModCollection. Expected: {typeof(ModioMod).FullName}. Actual: {mod.GetType().FullName}");
        }

        private void ThrowIfUnauthenticated()
        {
            if (!_clientProvider.IsAuthenticated())
                throw new InvalidOperationException("Modio Client is not authenticated!");
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
            ThrowIfUnauthenticated(); 

            var modioMod = mod as ModioMod;
            if (Mods.Contains(mod))
                return;
            await _clientProvider.Client!.Games[4169].Mods.Subscribe(modioMod!.ResourceID);
            _mods.Add(mod);
        }

        //We need access to users accountdata for this :( happy filedb patching
        public async Task ChangeActivationAsync(IEnumerable<IMod> mods, bool active, CancellationToken ct = default)
        {
            var ids = mods.Select(x => int.Parse(x.ModID));
            if (active)
                _access.AddModioActiveMod(ids);
            else
                _access.RemoveModioActiveMod(ids);
        }

        public async Task ChangeActivationAsync(IMod mod, bool active, CancellationToken ct = default)
        {
            var mods = new IMod[] { mod };
            await ChangeActivationAsync(mods, active, ct);
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

        private async Task RemoveAsync_NoEvents(IMod mod, CancellationToken ct)
        {
            ThrowIfUnauthenticated();

            var modioMod = mod as ModioMod;
            await _clientProvider.Client!.Games[4169].Mods.Unsubscribe(modioMod!.ResourceID);
            _mods.Remove(modioMod);
        }

        IEnumerator IEnumerable.GetEnumerator() => Mods.GetEnumerator();
        public IEnumerator<IMod> GetEnumerator() => Mods.GetEnumerator();

        public IEnumerable<TMod> OfModType<TMod>() where TMod : IMod
        {
            return Mods.Cast<TMod>();
        }
    }
}
