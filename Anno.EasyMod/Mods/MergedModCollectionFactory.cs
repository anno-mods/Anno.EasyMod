using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Mods
{
    public class MergedModCollectionFactory
    {
        public MergedModCollectionFactory() { }

        public MergedModCollection Get(IEnumerable<IModCollection> collections)
        {
            Dictionary<IMod, IModCollection> collectionLookup = new();
            foreach (IModCollection collection in collections)
            {
                foreach (IMod mod in collection)
                    collectionLookup.Add(mod, collection);
            }
            return new MergedModCollection(collectionLookup, collections.ToList());
        }

        public MergedModCollection Get(params IModCollection[] collections)
            => Get((IEnumerable<IModCollection>)collections);
    }
}
