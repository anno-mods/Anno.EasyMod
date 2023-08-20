namespace Anno.EasyMod.Mods.LocalMods
{
    public interface ILocalModCollectionFactory
    {
        IModCollection Get(string Filepath);
        Task<IModCollection> GetAsync(string Filepath);
    }
}