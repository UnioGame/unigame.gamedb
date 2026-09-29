namespace Game.Code.DataBase.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using UniGame.GameDb.Runtime;
    using Cysharp.Threading.Tasks;
    using UniGame.Core.Runtime;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

#if UNITY_EDITOR
    using System.IO;
    using UnityEditor.AddressableAssets;
    using UniModules.Editor;
#endif

    /// <summary>
    /// Records mirror addressable entries: id = asset GUID, labels are searchable.
    /// Editor registration reads entry metadata only (guid, path, labels), it never loads the addressable assets.
    /// </summary>
    [CreateAssetMenu(menuName = "UniGame/Game DB/Addressable DB Category",
        fileName = "Addressable DB Category")]
    public class AddressableGameDataCategory : GameDataCategory, IGameDataCategory
    {
#if ODIN_INSPECTOR
        [TabGroup(CategoryGroupKey)]
        [Searchable(FilterOptions = SearchFilterOptions.ISearchFilterableInterface)]
#endif
        public List<AddressablesObjectRecord> records = new List<AddressablesObjectRecord>();

#if ODIN_INSPECTOR
        [TabGroup(SettingsGroupKey)]
#endif
        public AddressableFilterData filterData = new AddressableFilterData();

        private Dictionary<string, List<IGameResourceRecord>> _labelMap;

        public override IReadOnlyList<IGameResourceRecord> Records => records;

        public override UniTask<CategoryInitializeResult> InitializeAsync(ILifeTime lifeTime)
        {
            _labelMap = null;
            return base.InitializeAsync(lifeTime);
        }

        public override IGameResourceRecord Find(string filter)
        {
            var record = base.Find(filter);
            if (record != EmptyRecord.Value) return record;

            var byLabel = LabelMap;
            return byLabel.TryGetValue(filter ?? string.Empty, out var list) && list.Count > 0
                ? list[0]
                : EmptyRecord.Value;
        }

        public override IReadOnlyList<IGameResourceRecord> FindResources(string filter)
        {
            if (string.IsNullOrEmpty(filter)) return Array.Empty<IGameResourceRecord>();
            if (LabelMap.TryGetValue(filter, out var list)) return list;
            var single = base.Find(filter);
            return single == EmptyRecord.Value ? Array.Empty<IGameResourceRecord>() : new[] { single };
        }

        private Dictionary<string, List<IGameResourceRecord>> LabelMap
        {
            get
            {
                if (_labelMap != null) return _labelMap;
                _labelMap = new Dictionary<string, List<IGameResourceRecord>>(StringComparer.OrdinalIgnoreCase);
                foreach (var record in records)
                {
                    if (record.labels == null) continue;
                    foreach (var label in record.labels)
                    {
                        if (string.IsNullOrEmpty(label)) continue;
                        if (!_labelMap.TryGetValue(label, out var list))
                            _labelMap[label] = list = new List<IGameResourceRecord>();
                        list.Add(record);
                    }
                }
                return _labelMap;
            }
        }

#if UNITY_EDITOR

        protected override void OnValidate()
        {
            base.OnValidate();
            _labelMap = null;
        }

        public override IReadOnlyList<IGameResourceRecord> FillCategory()
        {
            var fresh = CollectRecords();
            if (!SameRecords(records, fresh))
            {
                records.Clear();
                records.AddRange(fresh);
                _labelMap = null;
                RebuildMaps();
                this.MarkDirty();
            }

            return records;
        }

        /// <summary>builds the record list from the addressable settings without loading any asset object</summary>
        public List<AddressablesObjectRecord> CollectRecords()
        {
            var result = new List<AddressablesObjectRecord>();
            var regexps = filterData.regex.Select(x => new Regex(x)).ToArray();

            var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
            if (addressableSettings == null) return result;

            foreach (var assetGroup in addressableSettings.groups)
            {
                if (assetGroup == null) continue;
                foreach (var assetEntry in assetGroup.entries)
                {
                    if (!ValidateLabels(assetEntry.labels)) continue;
                    if (!ValidateRegExp(assetEntry.AssetPath, regexps)) continue;

                    result.Add(new AddressablesObjectRecord()
                    {
                        name = Path.GetFileNameWithoutExtension(assetEntry.AssetPath),
                        assetReference = new AssetReference(assetEntry.guid),
                        labels = assetEntry.labels.OrderBy(x => x, StringComparer.Ordinal).ToArray(),
                    });
                }
            }

            return result;
        }

        public static bool SameRecords(List<AddressablesObjectRecord> a, List<AddressablesObjectRecord> b)
        {
            if (a.Count != b.Count) return false;
            for (var i = 0; i < a.Count; i++)
            {
                var x = a[i];
                var y = b[i];
                if (x.assetReference?.AssetGUID != y.assetReference?.AssetGUID) return false;
                if (x.name != y.name) return false;
                var xl = x.labels ?? Array.Empty<string>();
                var yl = y.labels ?? Array.Empty<string>();
                if (xl.Length != yl.Length) return false;
                for (var l = 0; l < xl.Length; l++)
                    if (xl[l] != yl[l]) return false;
            }
            return true;
        }

        public bool ValidateRegExp(string path, Regex[] regexps)
        {
            if (regexps.Length == 0)
                return true;

            foreach (var regex in regexps)
            {
                if (regex.IsMatch(path))
                    return true;
            }

            return false;
        }

        public bool ValidateLabels(ICollection<string> labels)
        {
            var filter = filterData.labels;
            if (filter.Length == 0)
                return true;

            if (labels == null || labels.Count == 0)
                return false;

            foreach (var filterLabel in filter)
            {
                if (labels.Contains(filterLabel))
                    return true;
            }

            return false;
        }

#endif
    }

    [Serializable]
    public class AddressableFilterData
    {
        public string[] folders = Array.Empty<string>();
        public string[] labels = Array.Empty<string>();
        public string[] regex = Array.Empty<string>();
    }
}
