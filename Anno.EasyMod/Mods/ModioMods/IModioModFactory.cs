using Modio.Models;

namespace Anno.EasyMod.Mods.ModioMods
{
    public interface IModioModFactory
    {
        ModioMod Get(Mod modDto);
    }
}