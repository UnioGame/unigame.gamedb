namespace Game.Code.DataBase.Runtime
{
    using System;
    using System.Collections.Generic;
    using UniGame.GameDb.Runtime;
    using Cysharp.Threading.Tasks;
    using UniGame.AddressableTools.Runtime;
    using UniGame.Core.Runtime;
    using UniGame.Runtime.DataFlow;
    using UniGame.Runtime.Utils;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using Object = UnityEngine.Object;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

    /// <summary>
    /// The project registry of game data categories.
    /// Categories are AssetReferences (loaded through the lifetime aware addressable extensions, shared, never
    /// instantiated), the record index is an exact-id dictionary (O(1)), the legacy name/label filter is a per-category
    /// dictionary lookup that runs only when the exact id misses.
    /// </summary>
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

        private readonly List<IGameDataCategory> _categories = new();
        private readonly List<IGameResourceProvider> _fallBackLocations = new();
        private readonly Dictionary<string, GameDbResource> _index = new(512, StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, IGameDataCategory> _categoriesMap = new(16, StringComparer.OrdinalIgnoreCase);
        private LifeTime _lifeTime = new();

        public IReadOnlyList<IGameDataCategory> Categories => _categories;

        public int IndexedRecords => _index.Count;

        /// <summary>initialize with the database own lifetime (released by <see cref="Dispose"/>)</summary>
        public UniTask<IGameDatabase> Initialize()
        {
            _lifeTime.Restart();
            return Initialize(_lifeTime);
        }

        public async UniTask<IGameDatabase> Initialize(ILifeTime lifeTime)
        {
            _categories.Clear();
            _categoriesMap.Clear();
            _index.Clear();
            _fallBackLocations.Clear();
            _fallBackLocations.AddRange(fallBack);
            _fallBackLocations.AddRange(fallBackLocations);

            //load in parallel, register in the serialized order (first category wins on duplicate ids)
            var tasks = new UniTask<GameDataCategory>[categories.Count];
            for (var i = 0; i < categories.Count; i++)
                tasks[i] = LoadCategoryAsync(categories[i], lifeTime);

            var loaded = await UniTask.WhenAll(tasks);

            foreach (var asset in loaded)
            {
                if (asset == null) continue;
                await AddCategory(asset, lifeTime);
            }

            return this;
        }

        public async UniTask AddCategory(GameDataCategory category, ILifeTime lifeTime)
        {
            //categories are shared assets: their maps are runtime-only state, no Object.Instantiate copy of the records
            await category.InitializeAsync(lifeTime);

            _categories.Add(category);
            _categoriesMap[category.Category] = category;

            foreach (var pair in category.Map)
            {
                var record = pair.Value;
                if (record == null || string.IsNullOrEmpty(pair.Key)) continue;
                if (_index.ContainsKey(pair.Key)) continue;
                _index[pair.Key] = new GameDbResource()
                {
                    filter = pair.Key,
                    success = true,
                    category = category,
                    resource = record,
                };
            }
        }

        private static async UniTask<GameDataCategory> LoadCategoryAsync(AssetReferenceT<GameDataCategory> reference, ILifeTime lifeTime)
        {
            if (reference == null || !reference.RuntimeKeyIsValid()) return null;

            var result = await reference.AssetGUID.LoadReferenceAsync<GameDataCategory>(lifeTime, lifeTime.Token);
            if (!result.Success)
            {
                Debug.LogError($"Game DB: category {reference.AssetGUID} load failed: {result.Error}");
                return null;
            }

            return result.Result as GameDataCategory;
        }

        public IGameDataCategory GetCategory(string category)
        {
            _categoriesMap.TryGetValue(category ?? string.Empty, out var value);
            return value;
        }

        public bool IsValidResourceSource(string resource, Type resourceType)
        {
            return true;
        }

        /// <summary>exact id lookup: O(1)</summary>
        public bool TryGetRecord(string id, out IGameDataCategory category, out IGameResourceRecord record)
        {
            category = null;
            record = null;
            if (string.IsNullOrEmpty(id) || !_index.TryGetValue(id, out var value)) return false;
            category = value.category;
            record = value.resource;
            return true;
        }

        public async UniTask<GameResourceResult> LoadAsync(string resourceId, ILifeTime lifeTime)
        {
            return await LoadSourceAsync<Object>(resourceId, lifeTime);
        }

        public async UniTask<GameResourceResult<TAsset>> LoadAsync<TAsset>(string resourceId, ILifeTime lifeTime)
        {
            var assetResult = await LoadSourceAsync<TAsset>(resourceId, lifeTime);
            return ToTyped<TAsset>(resourceId, assetResult);
        }

        public async UniTask<GameResourceResult<TAsset>> LoadAsync<TAsset>(
            string resourceId,
            GameDbResource record,
            ILifeTime lifeTime) where TAsset : class
        {
            var assetResult = await LoadRecordAsync<TAsset>(resourceId, record, lifeTime);
            return ToTyped<TAsset>(resourceId, assetResult);
        }

        public async UniTask<GameResourceResult[]> LoadAllAsync<TResult>(string resource, ILifeTime lifeTime)
        {
            var resources = FindAll(resource);
            var tasks = new UniTask<GameResourceResult>[resources.Length];
            for (var i = 0; i < resources.Length; i++)
                tasks[i] = LoadRecordAsync<TResult>(resources[i].resource.Id, resources[i], lifeTime);
            return await UniTask.WhenAll(tasks);
        }

        /// <summary>every category's matches for the filter (exact id, name or label)</summary>
        public GameDbResource[] FindAll(string filter)
        {
            if (string.IsNullOrEmpty(filter)) return Array.Empty<GameDbResource>();

            var result = new List<GameDbResource>();
            foreach (var category in _categories)
            {
                var records = category.FindResources(filter);
                for (var i = 0; i < records.Count; i++)
                {
                    var record = records[i];
                    if (record == null || record == EmptyRecord.Value || string.IsNullOrEmpty(record.Id)) continue;
                    result.Add(new GameDbResource()
                    {
                        filter = filter,
                        success = true,
                        category = category,
                        resource = record,
                    });
                }
            }

            return result.ToArray();
        }

        /// <summary>exact id (O(1)) first, then the legacy name / label filter of each category</summary>
        public GameDbResource Find(string filter)
        {
            if (!string.IsNullOrEmpty(filter) && _index.TryGetValue(filter, out var exact))
                return exact;

            var result = new GameDbResource()
            {
                success = false,
                filter = filter,
                category = null,
                resource = EmptyRecord.Value
            };

            if (string.IsNullOrEmpty(filter)) return result;

            foreach (var category in _categories)
            {
                var record = category.Find(filter);

                if (record == null ||
                    record == EmptyRecord.Value ||
                    string.IsNullOrEmpty(record.Id)) continue;

                return new GameDbResource()
                {
                    filter = filter,
                    success = true,
                    category = category,
                    resource = record
                };
            }

            return result;
        }

        public async UniTask<GameResourceResult> LoadSourceAsync<TAsset>(string resourceId, ILifeTime lifeTime)
        {
            resourceId = resourceId?.TrimEnd(' ');
            return await LoadRecordAsync<TAsset>(resourceId, Find(resourceId), lifeTime);
        }

        private async UniTask<GameResourceResult> LoadRecordAsync<TAsset>(
            string resourceId,
            GameDbResource record,
            ILifeTime lifeTime)
        {
            var resource = record.resource;
            var category = record.category;
            var provider = category?.ResourceProvider;

            var loadFallBack = !record.success ||
                               resource == null ||
                               resource == EmptyRecord.Value ||
                               provider == null;

            //G3: the provider loads the record's resource path, not its id
            var assetResult = loadFallBack
                ? await LoadFallbackResourceAsync<TAsset>(resourceId, lifeTime)
                : await provider.LoadAsync<TAsset>(resource.ResourcePath, lifeTime);

#if UNITY_EDITOR
            if (assetResult.Complete == false)
            {
                Debug.LogError($"Load resource failed: {resourceId} " +
                               $"from category: {category?.Category ?? "<fallback>"} " +
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
                if (resourceLocation == null || !resourceLocation.IsValidResourceSource(resourceId, typeof(TAsset)))
                    continue;

                var resource = await resourceLocation.LoadAsync<TAsset>(resourceId, lifeTime);
                if (!resource.Complete) continue;
                return resource;
            }

            return GameResourceResult.FailedResourceResult;
        }

        private static GameResourceResult<TAsset> ToTyped<TAsset>(string resourceId, GameResourceResult assetResult)
        {
            var resultAsset = default(TAsset);
            if (assetResult.Result is TAsset asset)
                resultAsset = asset;

            return new GameResourceResult<TAsset>()
            {
                Id = resourceId,
                Complete = assetResult.Complete,
                Error = assetResult.Error,
                Result = resultAsset,
                Exception = assetResult.Exception,
            };
        }

        [Serializable]
        public struct GameDbResource
        {
            public string filter;
            public bool success;
            public IGameDataCategory category;
            public IGameResourceRecord resource;
        }

        public void Dispose()
        {
            _lifeTime.Terminate();
            _index.Clear();
            _categories.Clear();
            _categoriesMap.Clear();
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

            if (id.ToStringFromCache().Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return recordId.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                   category.Contains(searchString, StringComparison.OrdinalIgnoreCase);
        }
    }
}
