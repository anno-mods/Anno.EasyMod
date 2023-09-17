namespace Anno.EasyMod.Accountdata
{
    public interface IAccountdataAccess
    {
        void AddModioActiveMod(IEnumerable<int> modIDs);
        void RemoveModioActiveMod(IEnumerable<int> modIDs);

        IEnumerable<int> GetActiveMods();
        IEnumerable<string> GetDisabledLocalMods();

        void Load();
        void Save();

        void AddLocallyDisabledMod(string modId);
        void RemoveLocallyDisabledMod(string modId);
    }
}