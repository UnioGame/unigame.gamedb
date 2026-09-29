namespace Game.Code.DataBase.Runtime
{
    using System;
    using System.Collections.Generic;
    using UniGame.GameDb.Runtime;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

#if UNITY_EDITOR
    using System.IO;
    using UnityEditor;
    using UnityEditor.AddressableAssets;
    using UniModules.Editor;
#endif

    [CreateAssetMenu(menuName = "UniGame/Game DB/AddressableFolderCategory", fileName = "AddressableFolderCategory")]
    public class AddressableFolderCategory : GameDataCategory, IGameDataCategory
    {
#if ODIN_INSPECTOR
        [FolderPath]
#endif
        public string[] folders = Array.Empty<string>();

#if ODIN_INSPECTOR
        [Searchable(FilterOptions = SearchFilterOptions.ISearchFilterableInterface)]
#endif
        public List<AddressablesObjectRecord> records = new List<AddressablesObjectRecord>();

        public override IReadOnlyList<IGameResourceRecord> Records => records;

        public override bool IsEntryDriven => true;

#if UNITY_EDITOR

        /// <summary>records for the addressable assets under <see cref="folders"/>; no asset object is loaded</summary>
        public override IReadOnlyList<IGameResourceRecord> FillCategory()
        {
            if (folders.Length == 0)
            {
                if (records.Count > 0)
                {
                    records.Clear();
                    RebuildMaps();
                    this.MarkDirty();
                }
                return records;
            }

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var fresh = new List<AddressablesObjectRecord>();
            if (settings != null)
            {
                foreach (var guid in AssetDatabase.FindAssets(string.Empty, folders))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (AssetDatabase.IsValidFolder(path)) continue;
                    if (settings.FindAssetEntry(guid) == null) continue;
                    fresh.Add(new AddressablesObjectRecord()
                    {
                        assetReference = new AssetReference(guid),
                        name = Path.GetFileNameWithoutExtension(path),
                    });
                }
            }

            var same = records.Count == fresh.Count;
            for (var i = 0; same && i < fresh.Count; i++)
                same = records[i].assetReference.AssetGUID == fresh[i].assetReference.AssetGUID &&
                       records[i].name == fresh[i].name;

            if (!same)
            {
                records.Clear();
                records.AddRange(fresh);
                RebuildMaps();
                this.MarkDirty();
            }

            return records;
        }

#endif
    }
}
