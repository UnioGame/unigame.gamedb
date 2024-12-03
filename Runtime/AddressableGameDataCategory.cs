namespace Game.Code.DataBase.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Abstract;
    using Cysharp.Threading.Tasks;
    using Sirenix.OdinInspector;
    using UniGame.Core.Runtime;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using Object = UnityEngine.Object;

#if UNITY_EDITOR
    using UnityEditor.AddressableAssets;
    using UniModules.Editor;
    using UniModules.UniGame.AddressableExtensions.Editor;
#endif
    
    [CreateAssetMenu(menuName = "Game/GameDatabase/Addressable Category",
        fileName = "Addressable Category Asset")]
    public class AddressableGameDataCategory : GameDataCategory, IGameDataCategory
    {
        [TabGroup(CategoryGroupKey)]
        [Searchable(FilterOptions = SearchFilterOptions.ISearchFilterableInterface)]
        public List<AddressablesObjectRecord> records = new List<AddressablesObjectRecord>();

        [TabGroup(SettingsGroupKey)]
        public AddressableFilterData filterData = new AddressableFilterData();
        
        private Dictionary<string, IGameResourceRecord> _map = new(128);
        private Dictionary<string, IGameResourceRecord[]> _recordsMap = new(128);
        private Dictionary<string, IGameResourceRecord> _recordMap = new(128);
        
        private IGameResourceRecord[] _records;
        private List<IGameResourceRecord> _collectionBuffer = new();

        public override Dictionary<string, IGameResourceRecord> Map => _map;
        
        public override IGameResourceRecord[] Records => _records;
        
        public override UniTask<CategoryInitializeResult> InitializeAsync(ILifeTime lifeTime)
        {
            var count = records.Count;
            
            _recordMap.Clear();
            _recordsMap.Clear();
            _map.Clear();
            
            _records = new IGameResourceRecord[count];
            
            for (var i = 0; i < records.Count; i++)
            {
                var record = records[i];
                _records[i] = record;
                _map[record.Id] = record;
            }

            return UniTask.FromResult(new CategoryInitializeResult()
            {
                category = this,
                complete = true,
                error = string.Empty,
                categoryName = Category,
            });
        }

        public override IGameResourceRecord Find(string filter)
        {
            if (_recordMap.TryGetValue(filter, out var cached))
                return cached;
            
            IGameResourceRecord result = EmptyRecord.Value;
            
            foreach (var record in records)
            {
                if(!ValidateRecord(filter,record)) continue;
                result = record;
            }

            _recordMap[filter] = result;
            
            return result;
        }

        public override IGameResourceRecord[] FindResources(string filter)
        {
            if (_recordsMap.TryGetValue(filter, out var cached))
                return cached;
            
            _collectionBuffer.Clear();
            
            foreach (var record in records)
            {
                if(!ValidateRecord(filter,record)) continue;
                _collectionBuffer.Add(record);
            }

            var array = _collectionBuffer.ToArray();
            _recordsMap[filter] = array;
            return array;
        }

        public bool ValidateRecord(string filter, AddressablesObjectRecord record)
        {
            if(string.IsNullOrEmpty(filter)) return false;
            if (!record.assetReference.RuntimeKeyIsValid()) return false;

            if (record.name.Equals(filter, StringComparison.OrdinalIgnoreCase))
                return true;            
            
            if (record.assetReference.AssetGUID
                .Equals(filter, StringComparison.OrdinalIgnoreCase)) return true;
            
            foreach (var label in record.labels)
            {
                if (label.Equals(filter, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            
            return false;
        }

#if UNITY_EDITOR
        
        public override IReadOnlyList<IGameResourceRecord> FillCategory()
        {
            records.Clear();
            
            var regexps = filterData.regex
                .Select(x => new Regex(x)).ToArray();

            var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
            var groups = addressableSettings.groups;

            foreach (var assetGroup in groups)
            {
                foreach (var assetEntry in assetGroup.entries)
                {
                    var asset = assetEntry.TargetAsset;
 
                    if(!ValidateLabels(assetEntry.labels)) continue;
                    if(!ValidateRegExp(assetEntry.AssetPath,regexps)) continue;
                
                    var entry = new AddressablesObjectRecord()
                    {
                        name = asset.name,
                        assetReference = new AssetReference(assetEntry.guid),
                        labels = assetEntry.labels.ToArray(),
                    };
                
                    records.Add(entry);
                }
            }

            return records;
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
        
        public bool ValidateLabels(HashSet<string> labels)
        {
            var filter = filterData.labels;
            if (filter.Length == 0)
                return true;
            
            if (labels == null || labels.Count == 0) 
                return false;
            
            foreach (var filterLabel in filter)
            {
                if(labels.Contains(filterLabel))
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
        public string[] regex =Array.Empty<string>();
    }
}