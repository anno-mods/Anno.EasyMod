using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Mods
{
    public interface IModFactory<TMod> where TMod : IMod
    {
        TMod? GetFromFolder(string modFolderPath, bool loadImages = false);
    }
}
