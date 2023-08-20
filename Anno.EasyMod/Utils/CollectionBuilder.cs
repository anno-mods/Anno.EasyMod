using Anno.EasyMod.Mods;
using Anno.EasyMod.Mods.LocalMods;
using Anno.EasyMod.Mods.ModioMods;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Utils
{
    public class CollectionBuilder
    {
        private ILocalModCollectionFactory _localCollectionFactory; 
        private IModioModCollectionFactory? _modioCollectionFactory;

        private List<Task<IModCollection>> _buildTasks; 

        public CollectionBuilder(
            ILocalModCollectionFactory localModFactory, 
            IModioModCollectionFactory? modCollectionFactory) 
        {
            _localCollectionFactory = localModFactory;
            _modioCollectionFactory = modCollectionFactory;
            _buildTasks = new();
        }

        public CollectionBuilder AddDocumentsFolder()
        {
            var documentsFolderLocation = Path.Combine(SpecialDirectories.MyDocuments, "Anno 1800", "mods");
            _buildTasks.Add(_localCollectionFactory.GetAsync(documentsFolderLocation));
            return this;
        }

        public CollectionBuilder AddFromLocalSource(String modsLocation) 
        {
            _buildTasks.Add(_localCollectionFactory.GetAsync(modsLocation));
            return this; 
        }

        public CollectionBuilder AddModio()
        {
            if (_modioCollectionFactory is null)
                throw new NotSupportedException("Modio was not configured. To add modio to your collections, please use ConfigureModio on your servicecollection");
            _buildTasks.Add(_modioCollectionFactory.GetAsync());
            return this;
        }

        public async Task<IModCollection> BuildAsync() 
        {
            var completed = await Task.WhenAll(_buildTasks);
            if (completed.Count() == 1)
                return completed.First();
            return new MergedModCollection(completed);
        }
    }
}
