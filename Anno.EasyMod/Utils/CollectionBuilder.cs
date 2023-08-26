using Anno.EasyMod.Mods;
using Anno.EasyMod.Mods.LocalMods;
using Anno.EasyMod.Mods.ModioMods;
using Microsoft.VisualBasic.FileIO;

namespace Anno.EasyMod.Utils
{
    public class CollectionBuilder
    {
        private ILocalModCollectionFactory _localCollectionFactory; 
        private IModioModCollectionFactory? _modioCollectionFactory;

        private List<Task<IModCollection>> _buildTasks;
        private Dictionary<Type, Task<IModCollection>> _mainCollections;

        public CollectionBuilder(
            ILocalModCollectionFactory localModFactory, 
            IModioModCollectionFactory? modCollectionFactory) 
        {
            _localCollectionFactory = localModFactory;
            _modioCollectionFactory = modCollectionFactory;
            _buildTasks = new();
            _mainCollections = new(); 
        }

        public CollectionBuilder AddDocumentsFolder(bool asMainLocal = false)
        {
            var documentsFolderLocation = Path.Combine(SpecialDirectories.MyDocuments, "Anno 1800", "mods");
            var task = _localCollectionFactory.GetAsync(documentsFolderLocation);
            _buildTasks.Add(task);
            if (asMainLocal)
                ConfigureMainLocal(task);
            return this;
        }

        public CollectionBuilder AddFromLocalSource(String modsLocation, bool asMainLocal = true) 
        {
            var task = _localCollectionFactory.GetAsync(modsLocation);
            _buildTasks.Add(task);
            if (asMainLocal)
                ConfigureMainLocal(task);
            return this; 
        }

        private void ConfigureMainLocal(Task<IModCollection> local)
        {
            if (_mainCollections.ContainsKey(typeof(LocalMod)))
                throw new InvalidOperationException("A main Local collection has already been added");
            _mainCollections.Add(typeof(LocalMod), local);
        }

        public CollectionBuilder AddModio(bool asMainModio = true)
        {
            if (_modioCollectionFactory is null)
                throw new NotSupportedException("Modio was not configured. To add modio to your collections, please use ConfigureModio on your servicecollection");
            if (_mainCollections.ContainsKey(typeof(ModioMod)) && asMainModio)
                throw new InvalidOperationException("A main Modio collection has already been added");

            var task = _modioCollectionFactory.GetAsync();
            _buildTasks.Add(task);
            if(asMainModio)
                _mainCollections.Add(typeof(ModioMod), task);
            return this;
        }

        public async Task<IModCollection> BuildAsync() 
        {
            var completed = await Task.WhenAll(_buildTasks);
            if (completed.Count() == 1)
                return completed.First();

            var pairs = await Task.WhenAll(
                _mainCollections
                    .Select(async x => new { Key = x.Key, Value = await x.Value })
                );
            var dict = pairs.ToDictionary(x => x.Key, x => x.Value);
            return new MergedModCollection(completed, dict);
        }
    }
}
