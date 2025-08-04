namespace Game.Modules.game.packages.unigame.gamedb.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Code.DataBase.Runtime;
    using Code.DataBase.Runtime.Abstract;
    using Cysharp.Threading.Tasks;
    using UniGame.Core.Runtime;
    using UniModules;
    using UniModules.Editor;
    using UnityEditor;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

    public abstract class ResourcesAssetsCategoryT<TAsset> : GameDataCategory
        where TAsset : UnityEngine.Object
    {
        public const string ResourcesPath = "Resources/";

#if ODIN_INSPECTOR
        [FolderPath]
#endif
        public string[] folders = new []{ "Assets/Resources" };
        
        public List<ResourceDataRecord> records = new();

        private List<string> _paths = new();
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
            
            _paths.Clear();
            
            AssetEditorTools.GetAssetsPaths<TAsset>(folders,_paths);
            
            var recordLength = ResourcesPath.Length;

            foreach (var item in _paths)
            {
                if (item == null) continue;
                var itemPath = item;
                
                if(itemPath.Contains(ResourcesPath) == false) continue;
                    
                var index = itemPath.IndexOf(ResourcesPath, StringComparison.OrdinalIgnoreCase);
                if (index < 0) continue;

                var indexStart = index + recordLength;
                var resourcePath = itemPath.Substring(indexStart, itemPath.Length - indexStart);

                if (string.IsNullOrEmpty(resourcePath)) continue;

                var fileName = Path.GetFileNameWithoutExtension(resourcePath);
                var directoryPath = resourcePath.GetDirectoryPath();
                
                resourcePath = directoryPath.CombinePath(fileName);

                var record = new ResourceDataRecord
                {
                    id = fileName,
                    resourcePath = resourcePath
                };

                records.Add(record);
            }

            return records;
        }
#endif
    }
}