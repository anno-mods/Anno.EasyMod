using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Metadata
{
    public class ModinfoWriter
    {
        public static bool TrySaveToFile(String Filename, Modinfo m)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(File.Create(Filename)))
                {
                    writer.Write(JsonConvert.SerializeObject(m, Formatting.Indented));
                }
                Console.WriteLine($"Saved Modinfo file to {Filename}");
                return true;
            }
            catch (JsonSerializationException)
            {
                Console.WriteLine("Json Serialization failed: {0}", Filename);
            }
            catch (IOException)
            {
                Console.WriteLine("File not found: {0}", Filename);
            }
            return false;
        }
    }
}
