namespace Anno.EasyMod.Mods.ModioMods
{
    public interface IModioModCollectionFactory
    {
        Task<IModCollection> GetAsync();
    }
}