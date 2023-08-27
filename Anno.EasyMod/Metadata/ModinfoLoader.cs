using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Anno.EasyMod.Metadata
{
    public class ModinfoLoader : IModinfoLoader
    {
        public bool TryLoadFromFile(string filePath, out Modinfo? modinfo, bool autofix = false)
        {
            var shortPath = Path.Combine(Path.GetFileName(Path.GetDirectoryName(filePath) ?? ""), Path.GetFileName(filePath));

            if (autofix && AutofixModinfoArrays(filePath))
            {
                Console.WriteLine($"Fixed modloader relevant arrays in modinfo from {shortPath}");
            }

            try
            {
                var settings = new JsonSerializerSettings();
                // Every field is optional, thus be kind and forgive errors.
                settings.Error += (obj, args) =>
                {
                    args.ErrorContext.Handled = true;
                    Console.WriteLine($"Warning: {args.ErrorContext.Path} in {shortPath} is invalid");
                };
                modinfo = JsonConvert.DeserializeObject<Modinfo>(File.ReadAllText(filePath), settings) ?? new Modinfo();
                Console.WriteLine($"Loaded Modinfo file from {shortPath}");
                return true;
            }
            catch (JsonSerializationException)
            {
                modinfo = null;
                Console.WriteLine("Json Serialization failed: {0}", shortPath);
            }
            catch (IOException)
            {
                modinfo = null;
                Console.WriteLine("File not found: {0}", shortPath);
            }
            return false;
        }

        public static bool AutofixModinfoArrays(string filePath)
        {
            if (!File.Exists(filePath)) return false;

            var fixToken = (JToken root, string name) =>
            {
                JToken? token = root.SelectToken(name);
                if (token is not null && token.Type != JTokenType.Array && token.Type != JTokenType.Null)
                {
                    var newToken = JToken.FromObject(new string[] { token.ToString() });
                    root[name] = newToken;
                    return true;
                }

                return false;
            };

            try
            {
                var modinfo = JObject.Parse(File.ReadAllText(filePath));
                bool fix = false;
                fix |= fixToken(modinfo.Root, "ModDependencies");
                fix |= fixToken(modinfo.Root, "IncompatibleIds");
                fix |= fixToken(modinfo.Root, "LoadAfterIds");
                fix |= fixToken(modinfo.Root, "DeprecateIds");

                if (fix)
                {
                    // backup, just in case
                    var backupFile = filePath + ".bak";
                    if (File.Exists(backupFile)) File.Delete(backupFile);
                    File.Move(filePath, backupFile);

                    File.WriteAllText(filePath, modinfo.ToString(Formatting.Indented));
                }
            }
            catch (Exception)
            {
                // There's nothing we can or should do if it's completely invalid.
                return false;
            }

            return false;
        }

        public Modinfo GetDummy(string name)
        {
            return new Modinfo()
            {
                Version = "1.0.0",
                ModName = new Localized()
                {
                    English = name,
                }
            };
        }

        public Modinfo GetModio(Modio.Models.Mod mod)
        {
            return new Modinfo()
            {
                ModID = mod.Id.ToString(),
                CreatorName = mod.SubmittedBy?.Username,
                Category = new Localized() 
                {
                    English = "mod.io" 
                },
                Description = new Localized() 
                {
                    English = mod.DescriptionPlaintext
                }
                
            };
        }
    }
}