namespace Anno.EasyMod.Mods.LocalMods
{
    public interface ILocalModCollectionFactory
    {
        LocalModCollection Get(string Filepath);
        Task<LocalModCollection> GetAsync(string Filepath);
    }
}