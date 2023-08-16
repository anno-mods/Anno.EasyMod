using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Mods
{
    public class MergedModCollection : IModCollection<IMod>
    {
        private Dictionary<IMod, IModCollection<IMod>> _collectionLookup;
        private List<IModCollection<IMod>> _collections;

        public int ActiveMods => _collections.Select(x => x.ActiveMods).Aggregate((x, y) => x + y);
        public int ActiveSizeInMBs => _collections.Select(x => x.ActiveSizeInMBs).Aggregate((x, y) => x + y);
        public int InstalledSizeInMBs => _collections.Select(x => x.ActiveMods).Aggregate((x, y) => x + y);

        public IEnumerable<string> ModIDs =>  _collections
            .Select(x => x.ModIDs)
            .Aggregate((x, y) => new List<string>(x)
                .Concat(new List<string>(y)));

        public IReadOnlyList<IMod> Mods => _collections
            .Select(x => x.Mods)
            .Aggregate((x, y) => (IReadOnlyList<IMod>)new List<IMod>(x)
                .Concat(new List<IMod>(y)));

        public string ModsPath => throw new NotImplementedException();

        public int Count => _collections.Select(x => x.Count).Aggregate((x, y) => x + y);

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public void StartListening()
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
            throw new NotImplementedException();
        }

        public async Task ChangeActivationAsync(IMod mod, bool active, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IMod> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Task MakeObsoleteAsync(IMod mod, string path, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task MoveIntoAsync(IModCollection<IMod> source, bool allowOldToOverwrite = false, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(IEnumerable<IMod> mods, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(IMod mod, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
