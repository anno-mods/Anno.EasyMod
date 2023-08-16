using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Metadata
{
    public interface IModinfoLoader
    {
        public bool TryLoadFromFile(string filePath, out Modinfo? modinfo, bool autofix = false);
        public Modinfo GetDummy(String name);
    }
}
