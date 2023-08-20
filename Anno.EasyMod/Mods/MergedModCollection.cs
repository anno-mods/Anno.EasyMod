using Anno.EasyMod.Mods.LocalMods;
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
        public int ActiveSizeInMBs => _collections.Select(x => x.ActiveSizeInMBs).Aggregate((x, y) => x + y);
        public int InstalledSizeInMBs => _collections.Select(x => x.InstalledSizeInMBs).Aggregate((x, y) => x + y);

        public IEnumerable<string> ModIDs =>  _collections
            .Select(x => x.ModIDs)
            .Aggregate((x, y) => new List<string>(x)
                .Concat(new List<string>(y)));

        public IReadOnlyList<IMod> Mods => _collections
            .Select(x => x.Mods)
            .Aggregate((x, y) 
                    => (new List<IMod>(x).Concat(new List<IMod>(y))).ToList());

        public string ModsPath => throw new NotImplementedException();

        public int Count => _collections.Select(x => x.Count).Aggregate((x, y) => x + y);

        public event NotifyCollectionChangedEventHandler? CollectionChanged = delegate { };

        internal MergedModCollection(IEnumerable<IModCollection> collections)
        {
            _collections = collections;
            _collectionLookup = new();
            foreach (IModCollection collection in _collections)
            {
                foreach (IMod mod in collection)
                    _collectionLookup.Add(mod, collection);
            }

            StartRedirectingEvents();
        }

        public void StartRedirectingEvents()
        {
            foreach (var collection in _collections)
            {
                collection.CollectionChanged += (sender, e) =>
                {
                    if (CollectionChanged is not null)
                        CollectionChanged.Invoke(sender, e);
                };
            }
        }

        public async Task AddAsync(IEnumerable<IMod> mods, bool allowOldToOverwrite = false, CancellationToken ct = default)
        {
            var groups = mods.GroupBy(x => _collectionLookup.GetValueOrDefault(x));

            foreach (var group in groups)
            {
                if (group.Key is null)
                    throw new InvalidOperationException($"MergedCollection does not contain a Collection for type {group.Key}");
                var collection = group.Key;
                await collection!.AddAsync(group.ToArray(), allowOldToOverwrite, ct);
            }
        }

        public async Task AddAsync(IMod mod, bool allowOldToOverwrite = false, CancellationToken ct = default)
        {
            var collection = _collectionLookup.GetValueOrDefault(mod)!;
            await collection.AddAsync(mod, allowOldToOverwrite, ct);
        }

        public async Task ChangeActivationAsync(IEnumerable<IMod> mods, bool active, CancellationToken ct = default)
        {
            var groups = mods.GroupBy(x => _collectionLookup.GetValueOrDefault(x));

            foreach (var group in groups)
            {
                if (group.Key is null)
                    throw new InvalidOperationException($"MergedCollection does not contain a Collection for type {group.Key}");
                var collection = group.Key;
                await collection!.ChangeActivationAsync(group.ToArray(), active, ct);
            }
        }

        public async Task ChangeActivationAsync(IMod mod, bool active, CancellationToken ct = default)
        {
            var type = mod.GetType();
            var collection = _collectionLookup.GetValueOrDefault(mod)!;
            await collection.ChangeActivationAsync(mod, active, ct);
        }

        public async Task MakeObsoleteAsync(IMod mod, string path, CancellationToken ct = default)
        {
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
            var groups = mods.GroupBy(x => _collectionLookup.GetValueOrDefault(x));

            foreach (var group in groups)
            {
                if (group.Key is null)
                    throw new InvalidOperationException($"MergedCollection does not contain a Collection for type {group.Key}");
                var collection = group.Key;
                await collection!.RemoveAsync(group.ToArray(), ct);
            }
        }

        public async Task RemoveAsync(IMod mod, CancellationToken ct)
        {
            var collection = _collectionLookup.GetValueOrDefault(mod)!;
            await collection.RemoveAsync(mod, ct);
        }

        IEnumerator IEnumerable.GetEnumerator() => new MergedModCollectionEnumerator(this);
        public IEnumerator<IMod> GetEnumerator() => new MergedModCollectionEnumerator(this);

        public IEnumerable<TMod> OfModType<TMod>() where TMod : IMod
        {
            return Mods.Where(x => x.GetType() == typeof(TMod)).Cast<TMod>();
        }

        private class MergedModCollectionEnumerator : IEnumerator<IMod>
        {
            private IEnumerator<IModCollection> _collectionIterator;
            private IEnumerator<IMod> _currentCollectionModsIterator; 
            private MergedModCollection _collection;

#pragma warning disable CS8618
            public MergedModCollectionEnumerator(MergedModCollection collection)
#pragma warning restore CS8618 
            {
                _collection = collection;
                Reset(); 
            }

            public IMod Current { get; private set; }
            object IEnumerator.Current { get => Current; }

            public void Dispose() {
                _collectionIterator.Dispose();
                _currentCollectionModsIterator.Dispose(); 
            }

            public bool MoveNext()
            {
                if (_currentCollectionModsIterator.MoveNext())
                { 
                    Current = _currentCollectionModsIterator.Current;
                    return true; 
                }

                //we are done with the current collection. try to advance. If we are done with the collection iterator, we're done entirely. 
                if (!_collectionIterator.MoveNext())
                    return false; 
                //if there is still another collection at the end, we are starting to enumerate from there.
                _currentCollectionModsIterator = _collectionIterator.Current.GetEnumerator();
                return MoveNext(); 
            }

            public void Reset()
            {
                _collectionIterator = _collection._collections.GetEnumerator();
                //go to the first collection. Imagine StartPosition as Collection 0, Position -1;
                _collectionIterator.MoveNext();
                _currentCollectionModsIterator = _collectionIterator.Current.GetEnumerator(); 
            }
        }
    }



}
