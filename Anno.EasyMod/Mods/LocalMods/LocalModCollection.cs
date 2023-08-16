using Anno.EasyMod.Utils;
using Microsoft.Extensions.Logging;
using PropertyChanged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Mods.LocalMods
{
    [AddINotifyPropertyChangedInterface]
    public class LocalModCollection : IModCollection<LocalMod>
    {
        #region UI related
        public int ActiveMods { get; private set; }
        public int ActiveSizeInMBs { get; private set; }
        public int InstalledSizeInMBs { get; private set; }
        #endregion

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public string ModsPath { get; internal set; }

        public IReadOnlyList<LocalMod> Mods { get => _mods; }
        private List<LocalMod> _mods;

        [DoNotNotify]
        public IEnumerable<string> ModIDs { get => _modids; }
        private List<string> _modids = new();

        private ILogger<LocalModCollection> _logger;
        private IModFactory<LocalMod> _modFactory;

        /// <summary>
        /// This constructor is internal. 
        /// To create a ModCollection, use <see cref="ModCollectionFactory"/>
        /// </summary>
        /// <param name="path">Path to mods.</param>
        /// <param name="normalize">Remove duplicate "-"</param>
        /// <param name="loadImages">Load image files into memory</param>
        /// <param name="autofixSubfolder">find data/ in subfolder and move up</param>
        internal LocalModCollection(
            IModFactory<LocalMod> modFactory,
            ILogger<LocalModCollection> logger,
            List<LocalMod> mods)
        {
            ModsPath = "";
            _mods = mods;
            _logger = logger;
            _modFactory = modFactory;
        }

        #region Validation
        public void ThrowIfDoesNotContain(LocalMod mod)
        {
            if (Mods.Contains(mod))
                throw new InvalidOperationException("Collection cannot change mods that are not in it.");
        }

        public void ThrowIfDoesNotContain(IEnumerable<LocalMod> mods)
        {
            if (mods.Any(x => !Mods.Contains(x)))
            {
                throw new InvalidOperationException("Collection cannot change mods that are not in it.");
            }
        }
        #endregion


        #region Change Activation
        public async Task ChangeActivationAsync(IEnumerable<LocalMod> mods, bool active, CancellationToken ct = default)
        {
            ThrowIfDoesNotContain(mods);

            var tasks = mods.Select(x => Task.Run(
                async () => await ChangeActivationAsync_NoEvents(x, active)))
                .ToList();
            await Task.WhenAll(tasks);

            OnActivationChanged(null);
        }

        public async Task ChangeActivationAsync(LocalMod mod, bool active, CancellationToken ct = default)
        {
            ThrowIfDoesNotContain(mod);
            await ChangeActivationAsync_NoEvents(mod, active, ct);
            OnActivationChanged(mod);
        }

        private async Task ChangeActivationAsync_NoEvents(LocalMod mod, bool active, CancellationToken ct = default)
        {
            ThrowIfDoesNotContain(mod);
            if (mod.IsActive == active || mod.IsRemoved)
                return;

            string sourcePath = Path.Combine(mod.BasePath, mod.FullFolderName);
            string targetPath = Path.Combine(mod.BasePath, (active ? "" : "-") + mod.FolderName);

            var verb = active ? "activate" : "deactivate";

            await Task.Run(() =>
            {
                try
                {
                    _logger.LogTrace($"{verb} {mod.Name}...");
                    DirectoryEx.CleanMove(sourcePath, targetPath);
                    mod.IsActive = active;
                    _logger.LogInformation($"{verb} {mod.Name}. Folder renamed to {mod.FullFolderName}");
                }
                catch (InvalidOperationException e)
                {
                    mod.IsRemoved = true;
                    _logger.LogError(e.Message);
                    _logger.LogTrace($"{e.StackTrace}");
                }
                catch (DirectoryNotFoundException e)
                {
                    mod.IsRemoved = true;
                    _logger.LogError($"Failed to access mod: {mod.Name}. Cause: The mod has been removed");
                    _logger.LogTrace($"{e.StackTrace}");
                }
                catch (Exception e)
                {
                    _logger.LogError($"Failed to access mod: {mod.Name}. Cause: {e.Message}");
                    _logger.LogTrace($"{e.StackTrace}");
                }
            }, ct);
        }

        private void OnActivationChanged(LocalMod? sender)
        {
            // remove mods with IssueModRemoved attribute
            int removedModCount = Mods.Count(x => x.IsRemoved);
            int newActiveCount = Mods.Count(x => x.IsActive);
            if (removedModCount > 0 || ActiveMods != newActiveCount)
            {
                ActiveMods = newActiveCount;
                ActiveSizeInMBs = (int)Math.Round(Mods.Sum(x => x.IsActive ? x.SizeInMB : 0));
                InstalledSizeInMBs = (int)Math.Round(Mods.Sum(x => x.SizeInMB));
                _logger.LogInformation($"{ActiveMods} active mods. {Mods.Count} total found.");

                // trigger changed events for activation/deactivation
                if (sender is null)
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                else
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, sender, sender));
            }
        }

        #endregion

        #region Remove
        public async Task RemoveAsync(IEnumerable<LocalMod> mods, CancellationToken ct = default)
        {
            ThrowIfDoesNotContain(mods);

            var deleted = mods.ToList();

            foreach (var mod in mods)
                await RemoveAsync_NoEvents(mod, ct);

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, deleted));
        }

        public async Task RemoveAsync(LocalMod mod, CancellationToken ct = default)
        {
            await RemoveAsync_NoEvents(mod, ct);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, mod));
        }

        private async Task RemoveAsync_NoEvents(LocalMod mod, CancellationToken ct = default)
        {
            await Task.Run(() =>
            {
                try
                {
                    Directory.Delete(mod.FullModPath, true);

                    // remove from the mod lists to prevent access.
                    _mods.Remove(mod);
                }
                catch (DirectoryNotFoundException)
                {
                    // remove from the mod lists to prevent access.
                    _mods.Remove(mod);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Failed to delete Mod: {mod.Name}. Cause: {e.Message}");
                }
            });
        }
        #endregion

        public async Task MakeObsoleteAsync(LocalMod mod, string path, CancellationToken ct = default)
        {
            ThrowIfDoesNotContain(mod);
            await ChangeActivationAsync_NoEvents(mod, false);
            mod.IsObsolete = true;
            _logger.LogInformation($"Obsolete: {mod.FolderName}");
        }

        [Obsolete("This Method is dangerous and not supported." +
            "Use AddAsync instead and clean the source mod directory up by yourself! Only adding this so iMYA can respect that later.")]
        public async Task MoveIntoAsync(IModCollection<LocalMod> source, bool allowOldToOverwrite, CancellationToken ct = default)
        {
            await AddAsync(source, allowOldToOverwrite, ct);
            Directory.Delete(source.ModsPath, true);
            _logger.LogDebug($"Removed Directory: {source.ModsPath}");
        }

        public async Task AddAsync(IEnumerable<LocalMod> source, bool allowOldToOverwrite, CancellationToken ct = default)
        {
            Directory.CreateDirectory(ModsPath);

            try
            {
                foreach (var sourceMod in source)
                {
                    await Task.Run(
                        async () => await AddAsync_NoEvents(sourceMod, allowOldToOverwrite, ct),
                        ct
                    );
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Move Error: {e.Message}");
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, source.ToList()));
        }

        public async Task AddAsync(LocalMod sourceMod, bool allowOldToOverwrite, CancellationToken ct = default)
        {
            try {

                await AddAsync_NoEvents(sourceMod, allowOldToOverwrite, ct);
            }
            catch (Exception e)
            {
                _logger.LogError($"Move Error: {e.Message}");
            }
            finally
            {
                Directory.Delete(sourceMod.FullModPath, true);
                _logger.LogDebug($"Removed Directory: {sourceMod.FullModPath}");
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, sourceMod));
        }

        private async Task AddAsync_NoEvents(LocalMod sourceMod, bool allowOldToOverwrite, CancellationToken ct = default)
        {
            var (targetMod, targetModPath) = SelectTargetMod(sourceMod);

            if (!allowOldToOverwrite && !sourceMod.IsUpdateOf(targetMod))
            {
                _logger.LogInformation($"Skip update of {sourceMod.FolderName}. Source version: {sourceMod.Modinfo.Version}, target version: {targetMod?.Modinfo.Version}");
                return;
            }

            // do it!
            var status = Directory.Exists(targetModPath) ? LocalModStatus.Updated : LocalModStatus.New;
            DirectoryEx.CleanMove(sourceMod.FullModPath, targetModPath);

            // mark all duplicate id mods as obsolete
            if (sourceMod.Modinfo.ModID != null)
            {
                var sameModIDs = WhereByModID(sourceMod.Modinfo.ModID).Where(x => x != targetMod);
                foreach (var mod in sameModIDs)
                    await MakeObsoleteAsync(mod, ModsPath);
                // mark mod as updated, since there was the same modid already there
                if (sameModIDs.Any())
                    status = LocalModStatus.Updated;
            }

            // mark deprecated ids as obsolete
            if (sourceMod.Modinfo.DeprecateIds != null)
            {
                var deprecateIDs = sourceMod.Modinfo.DeprecateIds.SelectMany(x => WhereByModID(x));
                foreach (var mod in deprecateIDs)
                    await MakeObsoleteAsync(mod, ModsPath);
            }

            // update mod list, only remove in case of same folder
            if (targetMod is not null)
            {
                _mods.Remove(targetMod);
            }
            var reparsed = _modFactory.GetFromFolder(targetModPath);
            _mods.Add(reparsed!);
        }

        private (LocalMod?, string) SelectTargetMod(LocalMod sourceMod)
        {
            // select target mod
            var targetMod = FirstByFolderName(sourceMod.FolderName);
            string targetModPath = Path.Combine(ModsPath, targetMod?.FullFolderName ?? sourceMod.FullFolderName);

            // re-select target mod when modids are different (safeguard after 9 tries)
            var iteration = 1;
            while (iteration < 10 &&
                sourceMod.Modinfo.ModID is not null &&
                targetMod?.Modinfo.ModID is not null &&
                sourceMod.Modinfo.ModID != targetMod.Modinfo.ModID)
            {
                targetMod = FirstByFolderName($"{sourceMod.FolderName}-{iteration}");
                targetModPath = Path.Combine(ModsPath, targetMod?.FullFolderName ?? $"{sourceMod.FullFolderName}-{iteration}");
                iteration++;
            }

            return (targetMod, targetModPath);
        }

        private LocalMod? FirstByFolderName(string folderName, bool ignoreActivation = true)
        {
            var match = Mods.Where(x => (ignoreActivation ? x.FolderName : x.FullFolderName) == folderName).ToArray();

            // prefer activated one in case of two
            if (ignoreActivation && match.Length == 2)
                return match[0].IsActive ? match[0] : match[1];

            return match.FirstOrDefault();
        }


        private IEnumerable<LocalMod> WhereByModID(string modID)
        {
            return Mods.Where(x => x.Modinfo.ModID == modID);
        }

        #region IReadOnlyCollection
        public int Count => Mods.Count;
        public IEnumerator<LocalMod> GetEnumerator() => Mods.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Mods.GetEnumerator();
        #endregion

    }
}
