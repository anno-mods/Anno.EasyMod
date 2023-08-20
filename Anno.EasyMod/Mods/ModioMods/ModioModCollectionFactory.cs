using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Mods.ModioMods
{
    public class ModioModCollectionFactory : IModioModCollectionFactory
    {
        private IModioModFactory _modFactory;
        private Modio.Client _client;

        public ModioModCollectionFactory(
            IModioModFactory modFactory,
            Modio.Client client)
        {
            _modFactory = modFactory;
            _client = client;
        }

        public async Task<IModCollection> GetAsync()
        {
            var mods = await _client.User.GetSubscriptions().ToList();
            var modCollection = new ModioModCollection(_client);

            var modsConverted = mods.Select(modDto => _modFactory.Get(modDto)).ToArray();
            await modCollection.AddAsync(modsConverted);
            return modCollection;
        }
    }
}
