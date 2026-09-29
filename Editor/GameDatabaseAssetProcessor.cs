namespace UniGame.GameDB
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Game.Code.DataBase.Runtime;
    using UnityEditor;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEngine;

    /// <summary>
    /// Keeps the entry-driven categories (records mirror addressable entries) in sync, incrementally:
    /// <list type="bullet">
    /// <item>no <c>AssetPostprocessor</c>: raw imports never touch the database. The only trigger is a change of the
    /// addressable entries (add, remove, move, modify, labels), which is what a record depends on;</item>
    /// <item>events are coalesced into one refresh on the next editor tick;</item>
    /// <item>a refresh reads entry metadata only (guid, path, labels) and writes a category asset only when its record
    /// list actually changed; the database asset itself is never written;</item>
    /// <item>categories that are not entry-driven (builder-written, folder scans of prefabs) are refreshed by their own
    /// tools or the manual button, never from here.</item>
    /// </list>
    /// Off unless <see cref="GameDataBaseAsset.enableAutoUpdate"/> is set on the registry.
    /// </summary>
    [InitializeOnLoad]
    public static class GameDatabaseAssetProcessor
    {
        public struct RefreshStats
        {
            public int events;
            public int refreshes;
            public int categoriesChecked;
            public int categoriesWritten;
            public double lastMs;
        }

        private static GameDataBaseAsset _dataBaseAsset;
        private static readonly List<GameDataBaseAsset> Extra = new List<GameDataBaseAsset>();
        private static bool _pending;
        private static bool _searched;
        private static RefreshStats _stats;

        public static RefreshStats Stats => _stats;

        public static void ResetStats() => _stats = default;

        static GameDatabaseAssetProcessor()
        {
            AddressableAssetSettings.OnModificationGlobal -= OnEntriesChanged;
            AddressableAssetSettings.OnModificationGlobal += OnEntriesChanged;
        }

        /// <summary>additional registries to keep in sync (tools, tests); the project registry is always included</summary>
        public static void RegisterRegistry(GameDataBaseAsset registry)
        {
            if (registry != null && !Extra.Contains(registry)) Extra.Add(registry);
        }

        public static void UnregisterRegistry(GameDataBaseAsset registry) => Extra.Remove(registry);

        [MenuItem("UniGame/GameDB/Reimport Database", false, 2000)]
        public static void ReimportDatabase()
        {
            var registry = Registry();
            if (registry == null) return;
            registry.UpdateData();
            AssetDatabase.SaveAssets();
        }

        private static GameDataBaseAsset Registry()
        {
            //looked up once per domain: entry events can arrive by the thousand during a bulk import
            if (_dataBaseAsset == null && !_searched)
            {
                _searched = true;
                _dataBaseAsset = GameDataBaseAsset.EditorDatabase;
            }
            return _dataBaseAsset;
        }

        private static void OnEntriesChanged(AddressableAssetSettings settings,
            AddressableAssetSettings.ModificationEvent modification, object data)
        {
            switch (modification)
            {
                case AddressableAssetSettings.ModificationEvent.EntryAdded:
                case AddressableAssetSettings.ModificationEvent.EntryRemoved:
                case AddressableAssetSettings.ModificationEvent.EntryMoved:
                case AddressableAssetSettings.ModificationEvent.EntryModified:
                case AddressableAssetSettings.ModificationEvent.LabelAdded:
                case AddressableAssetSettings.ModificationEvent.LabelRemoved:
                case AddressableAssetSettings.ModificationEvent.GroupRemoved:
                case AddressableAssetSettings.ModificationEvent.BatchModification:
                    break;
                default:
                    return;
            }

            _stats.events++;
            if (_pending) return;
            //cheap gate before scheduling: the flag lives on the (cached) registry assets
            if (!AnyEnabled()) return;

            _pending = true;
            EditorApplication.delayCall += Refresh;
        }

        private static bool AnyEnabled()
        {
            var main = Registry();
            if (main != null && main.enableAutoUpdate) return true;
            foreach (var extra in Extra)
                if (extra != null && extra.enableAutoUpdate) return true;
            return false;
        }

        private static void Refresh()
        {
            _pending = false;
            RefreshNow();
        }

        /// <summary>runs the scheduled refresh now (tests and tools that do not wait for the next editor tick)</summary>
        public static bool FlushPending()
        {
            if (!_pending) return false;
            EditorApplication.delayCall -= Refresh;
            Refresh();
            return true;
        }

        /// <summary>refreshes every enabled registry now (what the coalesced editor tick runs); returns categories written</summary>
        public static int RefreshNow()
        {
            var watch = Stopwatch.StartNew();
            _stats.refreshes++;
            var written = 0;

            var main = Registry();
            if (main != null && main.enableAutoUpdate) written += RefreshRegistry(main);
            foreach (var extra in Extra)
                if (extra != null && extra.enableAutoUpdate) written += RefreshRegistry(extra);

            watch.Stop();
            _stats.lastMs = watch.Elapsed.TotalMilliseconds;
            return written;
        }

        /// <summary>refreshes the entry-driven categories of one registry; writes only categories whose records changed</summary>
        public static int RefreshRegistry(GameDataBaseAsset registry)
        {
            var written = 0;
            foreach (var reference in registry.gameDatabase.categories)
            {
                var category = reference?.editorAsset;
                if (category == null || !category.IsEntryDriven) continue;

                _stats.categoriesChecked++;
                var before = EditorUtility.GetDirtyCount(category);
                category.FillCategory();
                if (EditorUtility.GetDirtyCount(category) == before) continue;

                _stats.categoriesWritten++;
                written++;
                AssetDatabase.SaveAssetIfDirty(category);
            }

            return written;
        }
    }
}
