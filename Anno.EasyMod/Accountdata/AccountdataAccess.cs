using Microsoft.VisualBasic.FileIO;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using AnnoMods.BBDom;
using AnnoMods.BBDom.LookUps;
using AnnoMods.BBDom.ObjectSerializer;
using AnnoMods.BBDom.IO;

namespace Anno.EasyMod.Accountdata
{

    public class AccountdataAccess : IAccountdataAccess
    {
        BBDocument? accountdata;
        string AccountdataLocation;

        Attrib _activeModsNode;
        Tag _disabledLocalModsNode;

        public AccountdataAccess()
        {
            var accountsLocation = Path.Combine(SpecialDirectories.MyDocuments, "Anno 1800", "accounts");
            var directory = Directory.EnumerateDirectories(accountsLocation).FirstOrDefault();
            AccountdataLocation = Path.Combine(directory, "accountdata.a7s");
        }

        private void ThrowIfNotInitialized()
        {
            if (accountdata is null)
                throw new InvalidOperationException("No Accountdata loaded!");
        }

        public IEnumerable<String> GetDisabledLocalMods()
        {
            ThrowIfNotInitialized();

            var nodes = _disabledLocalModsNode
                .Children
                .Where(x => x is Attrib)
                .Select(x => new UTF8Encoding().GetString((x as Attrib)!.Content));
            return nodes;
        }

        public IEnumerable<int> GetActiveMods()
        {
            ThrowIfNotInitialized();

            var activeMods = _activeModsNode.Content
                .Chunk(sizeof(int))
                .Select(x => BitConverter.ToInt32(x));
            return activeMods ?? Enumerable.Empty<int>();
        }

        public void RemoveLocallyDisabledMod(string modId)
        {
            var bytes = new UTF8Encoding().GetBytes(modId);
            var toRemove = _disabledLocalModsNode.SelectNodes("None", x => (x as Attrib).Content.SequenceEqual(bytes));
            foreach (var item in toRemove)
                _disabledLocalModsNode.Children.Remove(item);
        }

        public void AddLocallyDisabledMod(string modId)
        {
            var bytes = new UTF8Encoding().GetBytes(modId);
            Attrib attrib = accountdata.CreateAttrib("None");
            attrib.Content = bytes;
            _disabledLocalModsNode.AddChild(attrib);
        }

        private byte[] BuildByteArray(int[] array)
        {
            var arrayInstance = array as Array;
            using (MemoryStream ContentStream = new MemoryStream())
            {
                for (int i = 0; i < arrayInstance.Length; i++)
                {
                    var singleVal = arrayInstance.GetValue(i);
                    ContentStream.Write(PrimitiveTypeConverter.GetBytes(singleVal));
                }
                return ContentStream.ToArray();
            }
        }

        public void AddModioActiveMod(IEnumerable<int> modIDs)
        {
            var activeMods = _activeModsNode.Content
                .Chunk(sizeof(int))
                .Select(x => BitConverter.ToInt32(x))
                .ToList();
            foreach (var modID in modIDs)
            {
                if (activeMods.Contains(modID))
                    continue;
                activeMods.Add(modID);
            }

            _activeModsNode.Content = BuildByteArray(activeMods.ToArray());
        }

        public void RemoveModioActiveMod(IEnumerable<int> modIDs)
        {
            var activeMods = _activeModsNode.Content
                .Chunk(sizeof(int))
                .Select(x => BitConverter.ToInt32(x))
                .ToList();

            foreach (var modID in modIDs)
                activeMods.Remove(modID);
            _activeModsNode.Content = BuildByteArray(activeMods.ToArray());
        }

        public void Save()
        {
            if (accountdata is null)
                return;

            using (var fs = File.Create(AccountdataLocation))
                accountdata.WriteToStream(fs);
        }

        public void Load()
        {
            using (var fs = File.OpenRead(AccountdataLocation))
            {
                var version = VersionDetector.GetCompressionVersion(fs);
                accountdata = BBDocument.LoadStream(fs);

                _activeModsNode = accountdata.SelectSingleNode("GameManager/ModManager/ActiveMods", x => x is Attrib) as Attrib;
                _disabledLocalModsNode = accountdata.SelectSingleNode("GameManager/ModManager/DisabledLocalMods", x => x is Tag) as Tag;
            }
        }

    }
}
