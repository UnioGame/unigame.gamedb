namespace Game.Code.DataBase.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Abstract;
    using Cysharp.Threading.Tasks;
    using UniGame.Core.Runtime;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using Object = UnityEngine.Object;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
#if UNITY_EDITOR
    using UnityEditor.AddressableAssets;
    using UniModules.Editor;
    using UniGame.AddressableTools.Editor;
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

        private IGameResourceProvider _resourceProvider = new AddressableResourceProvider();
        private Dictionary<string, IGameResourceRecord> _map = new(128);
        private Dictionary<string, List<IGameResourceRecord>> _recordsMap = new(128);
        private Dictionary<string, IGameResourceRecord> _recordMap = new(128);
        
        private IGameResourceRecord[] _records;
        private List<IGameResourceRecord> _collectionBuffer = new();

        public override IGameResourceProvider ResourceProvider => _resourceProvider;
        
        public override Dictionary<string, IGameResourceRecord> Map => _map;
        
        public override IReadOnlyList<IGameResourceRecord> Records => _records;
        
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

        public override IReadOnlyList<IGameResourceRecord> FindResources(string filter)
        {
            if (_recordsMap.TryGetValue(filter, out var cached))
                return cached;
            
            _collectionBuffer.Clear();
            
            foreach (var record in records)
            {
                if(!ValidateRecord(filter,record)) continue;
                _collectionBuffer.Add(record);
            }

            var items = _collectionBuffer.ToList();
            _recordsMap[filter] = items;
            return items;
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

            if (folders.Length == 0) return records;

            var assets = AssetEditorTools.GetAssets<Object>(folders);

            foreach (var asset in assets)
            {
                if(!asset.IsAddressable()) continue;
                var record = new AddressablesObjectRecord()
                {
                    assetReference = new AssetReference(asset.GetGUID()),
                    name = asset.name,
                };
                records.Add(record);
            }

            this.MarkDirty();
            
            return records;
        }
        
#endif
    }
}