namespace Game.Modules.game.packages.unigame.gamedb.Runtime
{
    using System;
    using System.Collections.Generic;
    using Code.DataBase.Runtime;
    using Code.DataBase.Runtime.Abstract;
    using Cysharp.Threading.Tasks;
    using UniGame.Core.Runtime;
    using UniModules;
    using UniModules.Editor;
    using UnityEditor;

    public abstract class ResourcesAssetsCategoryT<TAsset> : GameDataCategory
        where TAsset : UnityEngine.Object
    {
        public const string ResourcesPath = "Resources/";
        
        public List<ResourceDataRecord> records = new();

        private Dictionary<string, IGameResourceRecord> _map;
        private IGameResourceProvider _resourceProvider = new UnityResourcesDataProvider();

        public override IGameResourceProvider ResourceProvider => _resourceProvider;

        public override IReadOnlyList<IGameResourceRecord> Records => records;

        public override Dictionary<string, IGameResourceRecord> Map => _map;

        public override async UniTask<CategoryInitializeResult> InitializeAsync(ILifeTime lifeTime)
        {
            _map = new Dictionary<string, IGameResourceRecord>();

            foreach (var record in records)
            {
                _map[record.Id] = record;
            }

            return new CategoryInitializeResult()
            {
                category = this,
                categoryName = category,
                complete = true,
                error = string.Empty,
            };
        }

        public override IGameResourceRecord Find(string filter)
        {
            return _map.GetValueOrDefault(filter);
        }

        public override IReadOnlyList<IGameResourceRecord> FindResources(string filter)
        {
            var records = new List<IGameResourceRecord>();
            foreach (var record in this.records)
            {
                if (record.CheckRecord(filter))
                    records.Add(record);
            }

            return records;
        }

#if UNITY_EDITOR

        public override IReadOnlyList<IGameResourceRecord> FillCategory()
        {
            records.Clear();
            var itemData = AssetEditorTools.GetAssets<TAsset>();
            var recordLength = ResourcesPath.Length;

            foreach (var item in itemData)
            {
                if (item == null) continue;
                var itemPath = AssetDatabase.GetAssetPath(item);
                
                if(itemPath.Contains(ResourcesPath) == false) continue;
                    
                var index = itemPath.IndexOf(ResourcesPath, StringComparison.OrdinalIgnoreCase);
                if (index < 0) continue;

                var indexStart = index + recordLength;
                var resourcePath = itemPath.Substring(indexStart, itemPath.Length - indexStart);

                if (string.IsNullOrEmpty(resourcePath)) continue;

                resourcePath = resourcePath.GetDirectoryPath();
                resourcePath = resourcePath.CombinePath(item.name);

                var record = new ResourceDataRecord
                {
                    id = item.name,
                    resourcePath = resourcePath
                };

                records.Add(record);
            }

            return records;
        }
#endif
    }
}