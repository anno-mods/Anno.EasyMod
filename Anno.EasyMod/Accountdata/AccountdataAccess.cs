namespace Anno.EasyMod.Accountdata
{
    public class AccountdataAccess
    {
        public IEnumerable<String> GetDisabledLocalMods()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> GetActiveMods()
        {
            throw new NotImplementedException();
        }

        public void ChangeActivationStatus(string modId, bool active, bool asLocal = true)
        {
            throw new NotImplementedException();
        }

        private void RemoveLocallyDisabledMod(string modId) 
        {
            throw new NotImplementedException();
        }

        private void AddLocallyDisabledMod(string modId)
        {
            throw new NotImplementedException();
        }

        private void AddModioActiveMod(string modID)
        {
            throw new NotImplementedException();
        }

        private void RemoveModioActiveMod(string modId)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }


    }
}
