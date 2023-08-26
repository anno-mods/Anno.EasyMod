using Anno.EasyMod.Mods.LocalMods;
using Anno.EasyMod.Mods.ModioMods;
using Modio.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Mods
{
    public class MergedModCollection : IModCollection
    {
        private Dictionary<IMod, IModCollection> _collectionLookup;
        private IEnumerable<IModCollection> _collections;

        public int ActiveMods => _collections.Select(x => x.ActiveMods).Aggregate((x, y) => x + y);
        public long ActiveSize => _collections.Select(x => x.ActiveSize).Aggregate((x, y) => x + y);
        public long InstalledSize => _collections.Select(x => x.InstalledSize).Aggregate((x, y) => x + y);

        public IEnumerable<string> ModIDs =>  _collections.SelectMany(x => x.ModIDs);

        public IReadOnlyList<IMod> Mods => _collections.SelectMany(x => x.Mods).ToList();

        public string ModsPath => throw new NotImplementedException();

        public int Count => _collections.Select(x => x.Count).Aggregate((x, y) => x + y);

        public event NotifyCollectionChangedEventHandler? CollectionChanged = delegate { };

        private Dictionary<Type, IModCollection> _mainCollections;

        internal MergedModCollection(
            IEnumerable<IModCollection> collections,
            Dictionary<Type, IModCollection> mainCollections)
        {
            foreach (var collection in mainCollections.Values)
            {
                if (!collections.Contains(collection))
                    throw new ArgumentException("A Main Collection needs to be contained in Collections. ");
            }
            _collections = collections;
            _mainCollections = mainCollections;
            _collectionLookup = new();
            foreach (IModCollection collection in _collections)
            {
                foreach (IMod mod in collection)
                    _collectionLookup.Add(mod, collection);
            }

            StartRedirectingEvents();
        }

        private void ThrowIfNoCollectionFor(IMod mod)
        {
            if (!_collectionLookup.ContainsKey(mod))
                throw new ArgumentException($"MergedCollection does not contain a Collection for mod: {mod.Name}");
        }

        private void ThrowIfGroupingNull(IGrouping<IModCollection?, IMod>? context)
        { 
            if(context.Key is null)
                throw new ArgumentException($"MergedCollection does not contain a Collection for mod:" +
                    $" {String.Join('\n', context.Select(x => x.Name).ToArray())}");
        }

        public void StartRedirectingEvents()
        {
            foreach (var collection in _collections)
            {
                collection.CollectionChanged += (sender, e) =>
                {
                    CollectionChanged?.Invoke(this, e);
                };
            }
        }

        public async Task AddAsync(IEnumerable<IMod> mods, bool allowOldToOverwrite = false, CancellationToken ct = default)
        {
            var groups = mods.GroupBy(x => x.GetType());

            foreach (var group in groups)
            {
                if(!_mainCollections.TryGetValue(group.Key!, out var collection))
                    throw new InvalidOperationException($"No MainCollection for Mod Type type {group.Key}");
                var newMods = group.ToArray();
                await collection!.AddAsync(newMods, allowOldToOverwrite, ct);

                foreach (IMod mod in newMods)
                    _collectionLookup.Add(mod, collection);
            }
        }

        public async Task AddAsync(IMod mod, bool allowOldToOverwrite = false, CancellationToken ct = default)
        {
            if (!_mainCollections.TryGetValue(mod.GetType()!, out var collection))
                throw new InvalidOperationException($"No MainCollection for Mod Type type {mod.GetType()}");
            await collection.AddAsync(mod, allowOldToOverwrite, ct); 
            _collectionLookup.Add(mod, collection);
        }

        public async Task ChangeActivationAsync(IEnumerable<IMod> mods, bool active, CancellationToken ct = default)
        {
            var groups = mods.GroupBy(x => _collectionLookup.GetValueOrDefault(x));

            foreach (var group in groups)
            {
                ThrowIfGroupingNull(group);

                var collection = group.Key;
                await collection!.ChangeActivationAsync(group.ToArray(), active, ct);
            }
        }

        public async Task ChangeActivationAsync(IMod mod, bool active, CancellationToken ct = default)
        {
            ThrowIfNoCollectionFor(mod);

            var collection = _collectionLookup.GetValueOrDefault(mod)!;
            await collection.ChangeActivationAsync(mod, active, ct);
        }

        public async Task MakeObsoleteAsync(IMod mod, string path, CancellationToken ct = default)
        {
            ThrowIfNoCollectionFor(mod);

            var collection = _collectionLookup.GetValueOrDefault(mod)!;
            await collection.MakeObsoleteAsync(mod, path, ct);
        }

        [Obsolete]
        public Task MoveIntoAsync(IModCollection source, bool allowOldToOverwrite = false, CancellationToken ct = default)
        {
            throw new NotSupportedException("As I said, it's dangerous to use this shit.");
        }

        public async Task RemoveAsync(IEnumerable<IMod> mods, CancellationToken ct = default)
        {
            var groups = mods.GroupBy(x => _collectionLookup.GetValueOrDefault(x)).ToArray();

            foreach (var group in groups)
            {
                ThrowIfGroupingNull(group);

                var collection = group.Key;
                await collection!.RemoveAsync(group.ToArray(), ct);
            }
            foreach (var mod in mods)
                _collectionLookup.Remove(mod);
        }

        public async Task RemoveAsync(IMod mod, CancellationToken ct)
        {
            ThrowIfNoCollectionFor(mod);

            var collection = _collectionLookup.GetValueOrDefault(mod)!;
            await collection.RemoveAsync(mod, ct);
            _collectionLookup.Remove(mod);
        }

        IEnumerator IEnumerable.GetEnumerator() => Mods.GetEnumerator();
        public IEnumerator<IMod> GetEnumerator() => Mods.GetEnumerator();

        public IEnumerable<TMod> OfModType<TMod>() where TMod : IMod
        {
            return Mods.Where(x => x.GetType() == typeof(TMod)).Cast<TMod>();
        }
    }
}
