namespace Game.Code.DataBase.Runtime
{
    using System;
    using System.Collections.Generic;
    using Abstract;
    using Cysharp.Threading.Tasks;
    using UniGame.AddressableTools.Runtime;
    using UniGame.Core.Runtime;
    using UniGame.Runtime.Utils;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using Object = UnityEngine.Object;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
    [Serializable]
    public class GameDatabase : IGameDatabase
    {
        public const string SettingsKey = "Settings";
        public const string DatabaseKey = "Database";
        
        #region inspector

#if ODIN_INSPECTOR
        [TabGroup(DatabaseKey)]
#endif
        public DbData dbData = new();
        
#if ODIN_INSPECTOR
        [TabGroup(SettingsKey)]
#endif
        [SerializeReference]
        public List<IGameResourceProvider> fallBack = new() {
            new AddressableResourceProvider(),
        };
        
#if ODIN_INSPECTOR
        [TabGroup(SettingsKey)]
        [InlineEditor()]
#endif
        public List<GameResourceLocation> fallBackLocations = new();

#if ODIN_INSPECTOR
        [TabGroup(DatabaseKey)]
        [InlineProperty]
#endif
        public List<AssetReferenceT<GameDataCategory>> categories = new();

        #endregion

        private List<IGameDataCategory> _categories = new();
        private List<IGameResourceProvider> _fallBackLocations = new();
        private Dictionary<string, IGameResourceRecord> _records = new(256);
        private Dictionary<string,IGameDataCategory> _categoriesMap = new(16);
        
        private Dictionary<string,GameDbResource> _dbResourceCache = new(256);
        private Dictionary<string,GameDbResource[]> _dbResourcesCache = new(256);
        private List<GameDbResource> _dbCacheResources = new();

        public async UniTask<IGameDatabase> Initialize(ILifeTime lifeTime)
        {
            _dbResourceCache.Clear();
            _dbResourcesCache.Clear();
            _fallBackLocations.Clear();
            _fallBackLocations.AddRange(fallBack);
            _fallBackLocations.AddRange(fallBackLocations);
            
            var tasks = categories.Select(x =>
                x.LoadAssetInstanceTaskAsync(lifeTime, true));
            
            var categoriesAssets = await UniTask.WhenAll(tasks);

            var initializedCategories = categoriesAssets
                .Select(x => x.InitializeAsync(lifeTime));
            
            var initializeResults = await UniTask.WhenAll(initializedCategories);

            foreach (var categoryResult in initializeResults)
            {
                if (!categoryResult.complete)
                {
                    Debug.LogError($"[GameDB] filed to initialize category {categoryResult.categoryName} error: {categoryResult.error}");
                    continue;    
                }
                
                var category = categoryResult.category;
                _categories.Add(category);
                _categoriesMap[category.Category] = categoryResult.category;
            }
            
            return this;
        }

        public IGameDataCategory GetCategory(string category)
        {
            _categoriesMap.TryGetValue(category, out var value);
            return value;
        }
        
        public bool IsValidResourceSource(string resource, Type resourceType)
        {
            return true;
        }

        public async UniTask<GameResourceResult> LoadAsync(string resourceId, ILifeTime lifeTime)
        {
            return await LoadSourceAsync<Object>(resourceId, lifeTime);
        }

        public async UniTask<GameResourceResult<TAsset>> LoadAsync<TAsset>(string resourceId,ILifeTime lifeTime)
        {
            var assetResult = await LoadSourceAsync<TAsset>(resourceId, lifeTime);
            
            var resultAsset = default(TAsset);
            
            if (assetResult.Result is TAsset asset)
                resultAsset = asset;
            
            var result = new GameResourceResult<TAsset>()
            {
                Id = resourceId,
                Complete = assetResult.Complete,
                Error = assetResult.Error,
                Result = resultAsset,
                Exception = assetResult.Exception,
            };

            return result;
        }
        
        public async UniTask<GameResourceResult<TAsset>> LoadAsync<TAsset>(
            string resourceId,
            GameDbResource record, 
            ILifeTime lifeTime) where TAsset : class
        {
            var resource = record.resource;
            var category = record.category;
            var provider = category.ResourceProvider;
            
            var loadFallBack = resource == EmptyRecord.Value || provider == null;
            
            var assetResult = loadFallBack
                ? await LoadFallbackResourceAsync<TAsset>(resourceId,lifeTime) 
                : await provider.LoadAsync<TAsset>(resource.Id,lifeTime);

#if UNITY_EDITOR

            if (assetResult.Complete == false)
            {
                Debug.LogError($"Load resource failed: {resourceId} " +
                               $"from category: {category.Category} " +
                               $"with error: {assetResult.Error}");
            }
            
#endif
            
            var resultAsset = default(TAsset);
            if (assetResult.Result is TAsset asset)
                resultAsset = asset;
            
            var result = new GameResourceResult<TAsset>()
            {
                Id = resourceId,
                Complete = assetResult.Complete,
                Error = assetResult.Error,
                Result = resultAsset,
                Exception = assetResult.Exception,
            };

            return result;
        }

        public async UniTask<GameResourceResult[]> LoadAllAsync<TResult>(string resource, 
            ILifeTime lifeTime)
        {
            var resources = FindAll(resource);
            var tasks = resources
                .Select(x => LoadSourceAsync<TResult>(resource,lifeTime));
            var results = await UniTask.WhenAll(tasks);
            return results;
        }

        public GameDbResource[] FindAll(string filter)
        {
            if(_dbResourcesCache.TryGetValue(filter, out var value))
                return value;

            _dbCacheResources.Clear();
            
            var result = Array.Empty<GameDbResource>();
            
            foreach (var category in _categories)
            {
                var record = category.Find(filter);
                if(record == EmptyRecord.Value || string.IsNullOrEmpty(record.Id))
                    continue;
                
                var item = new GameDbResource()
                {
                    filter = filter,
                    success = true,
                    category = category,
                    resource = record
                };
                
                _dbCacheResources.Add(item);
                
                break;
            }

            result = _dbCacheResources.Count <= 0
                ? result
                : _dbCacheResources.ToArray();
            
            _dbResourcesCache[filter] =  result;
            
            return result;
        }

        public GameDbResource Find(string filter)
        {
            if(_dbResourceCache.TryGetValue(filter, out var value))
                return value;

            var result =  new GameDbResource()
            {
                success = false,
                filter = filter,
                category = null,
                resource = EmptyRecord.Value
            };
            
            foreach (var category in _categories)
            {
                var record = category.Find(filter);
                if(record == EmptyRecord.Value || string.IsNullOrEmpty(record.Id))
                    continue;
                
                result = new GameDbResource()
                {
                    filter = filter,
                    success = true,
                    category = category,
                    resource = record
                };
                
                break;
            }

            _dbResourceCache[filter] = result;
            
            return result;
        }

                
        public async UniTask<GameResourceResult> LoadSourceAsync<TAsset>(string resourceId,ILifeTime lifeTime)
        {
            resourceId = resourceId.TrimEnd(' ');
            
            var record = Find(resourceId);
            
            var resource = record.resource;
            var category = record.category;
            var resourcePath = resource.ResourcePath;
            var provider = category?.ResourceProvider;
            
            var loadFallBack = !record.success || 
                               resource == EmptyRecord.Value || 
                               provider == null;
            
            var assetResult = loadFallBack
                ? await LoadFallbackResourceAsync<TAsset>(resourceId,lifeTime) 
                : await provider.LoadAsync<TAsset>(resourcePath,lifeTime);
            
#if UNITY_EDITOR

            if (assetResult.Complete == false)
            {
                Debug.LogError($"Load resource failed: {resourceId} " +
                               $"from category: {category.Category} " +
                               $"with error: {assetResult.Error}");
            }
            
#endif
           
            return assetResult;
        }
        
        
        private async UniTask<GameResourceResult> LoadFallbackResourceAsync<TAsset>(
            string resourceId,
            ILifeTime lifeTime)
        {
            foreach (var resourceLocation in _fallBackLocations)
            {
                if(!resourceLocation.IsValidResourceSource(resourceId,typeof(TAsset)))
                    continue;
                
                var resource = await resourceLocation.LoadAsync<TAsset>(resourceId,lifeTime);
                if(!resource.Complete) continue;
                return resource;
            }
            
            return GameResourceResult.FailedResourceResult;
        }
        
        [Serializable]
        public struct GameDbResource
        {
            public string filter;
            public bool success;
            public IGameDataCategory category;
            public IGameResourceRecord resource;
        }

    }

    [Serializable]
    public class DbData
    {
#if ODIN_INSPECTOR
        [Searchable(FilterOptions = SearchFilterOptions.ISearchFilterableInterface)]
        [ListDrawerSettings(ListElementLabelName = nameof(DBRecord.recordId))]
#endif
        public List<DBRecord> records = new();
    }

    [Serializable]
    public class DBRecord : ISearchFilterable
    {
        public int id;
        public string category;
        public string recordId;
        
        public bool IsMatch(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                return true;
            
            if(id.ToStringFromCache().Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            
            return recordId.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                   category.Contains(searchString, StringComparison.OrdinalIgnoreCase);
        }
    }
}


