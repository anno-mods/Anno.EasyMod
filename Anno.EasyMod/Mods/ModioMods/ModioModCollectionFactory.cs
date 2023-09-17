using Anno.EasyMod.Accountdata;
using Anno.EasyMod.ModioWrapper;
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
        private IModioClientProvider _clientProvider;
        private AccountdataAccess _access;

        public ModioModCollectionFactory(
            IModioModFactory modFactory,
            IModioClientProvider client,
            AccountdataAccess access)
        {
            _modFactory = modFactory;
            _clientProvider = client;
            _access = access;
        }

        public async Task<IModCollection> GetAsync()
        {
            if (!_clientProvider.IsAuthenticated())
                throw new InvalidOperationException("Modio Client is not authenticated!");

            var mods = await _clientProvider.Client!.User.GetSubscriptions().ToList();
            var modCollection = new ModioModCollection(_clientProvider, _access);

            var modsConverted = mods.Select(modDto => _modFactory.Get(modDto)).ToArray();
            await modCollection.AddAsync(modsConverted);
            return modCollection;
        }
    }
}
