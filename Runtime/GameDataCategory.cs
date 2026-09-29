namespace Game.Code.DataBase.Runtime
{
    using System;
    using System.Collections.Generic;
    using UniGame.GameDb.Runtime;
    using Cysharp.Threading.Tasks;
    using UniGame.Core.Runtime;
    using UnityEngine;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

#if ALCHEMY_INSPECTOR
    using Alchemy.Inspector;
#endif

    /// <summary>
    /// Base class of a database category. Only <see cref="Records"/> is abstract: the exact-id map, the name map and the
    /// default resource provider are built from it (O(1) lookups, no per-filter caches, no Object.Instantiate copies).
    /// </summary>
    [Serializable]
    public abstract class GameDataCategory : ScriptableObject, IGameDataCategory
    {
        public const string SettingsGroupKey = "settings";
        public const string CategoryGroupKey = "category";

        private static readonly IGameResourceProvider DefaultProvider = new AddressableResourceProvider();

        public string category;

        /// <summary>optional custom provider asset (kept for the serialized schema of existing categories)</summary>
#if ODIN_INSPECTOR
        [InlineEditor()]
#endif
        public GameResourceLocation resourceLocation;

        private Dictionary<string, IGameResourceRecord> _map;
        private Dictionary<string, IGameResourceRecord> _nameMap;

        public virtual string Category => category;

        /// <summary>
        /// true when the records mirror addressable entries (guid, path, labels): the editor refreshes such a category from
        /// entry events. Other categories are filled by their builders or by the manual Update button only.
        /// </summary>
        public virtual bool IsEntryDriven => false;

        public virtual IGameResourceProvider ResourceProvider =>
            resourceLocation != null ? resourceLocation : DefaultProvider;

        public abstract IReadOnlyList<IGameResourceRecord> Records { get; }

        /// <summary>id -> record (case-insensitive, first record wins)</summary>
        public virtual Dictionary<string, IGameResourceRecord> Map
        {
            get
            {
                if (_map == null) RebuildMaps();
                return _map;
            }
        }

        public virtual UniTask<CategoryInitializeResult> InitializeAsync(ILifeTime lifeTime)
        {
            RebuildMaps();

            return UniTask.FromResult(new CategoryInitializeResult()
            {
                category = this,
                complete = true,
                error = string.Empty,
                categoryName = Category,
            });
        }

        /// <summary>exact id lookup, no fallback to names</summary>
        public virtual bool Has(string id) => !string.IsNullOrEmpty(id) && Map.ContainsKey(id);

        public bool TryGetRecord(string id, out IGameResourceRecord record)
        {
            record = null;
            if (string.IsNullOrEmpty(id)) return false;
            return Map.TryGetValue(id, out record);
        }

        /// <summary>exact id first, then the record name; <see cref="EmptyRecord.Value"/> when nothing matches</summary>
        public virtual IGameResourceRecord Find(string filter)
        {
            if (string.IsNullOrEmpty(filter)) return EmptyRecord.Value;
            if (Map.TryGetValue(filter, out var record)) return record;
            if (_nameMap.TryGetValue(filter, out record)) return record;
            return EmptyRecord.Value;
        }

        public virtual IReadOnlyList<IGameResourceRecord> FindResources(string filter)
        {
            var record = Find(filter);
            if (record == EmptyRecord.Value) return Array.Empty<IGameResourceRecord>();
            return new[] { record };
        }

        /// <summary>call after <see cref="Records"/> changed at runtime or in the editor</summary>
        public void RebuildMaps()
        {
            var records = Records;
            var count = records?.Count ?? 0;
            _map = new Dictionary<string, IGameResourceRecord>(count, StringComparer.OrdinalIgnoreCase);
            _nameMap = new Dictionary<string, IGameResourceRecord>(count, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < count; i++)
            {
                var record = records[i];
                if (record == null) continue;
                var id = record.Id;
                if (!string.IsNullOrEmpty(id)) _map.TryAdd(id, record);
                var name = record.Name;
                if (!string.IsNullOrEmpty(name)) _nameMap.TryAdd(name, record);
            }
        }

        public virtual IReadOnlyList<IGameResourceRecord> FillCategory()
        {
            return Records;
        }

#if UNITY_EDITOR

#if ODIN_INSPECTOR
        [Button(ButtonSizes.Large, Icon = SdfIconType.ArchiveFill)]
#endif
#if ALCHEMY_INSPECTOR
        [Button]
#endif
        public virtual void UpdateCategory()
        {
            FillCategory();
        }

        protected virtual void OnValidate()
        {
            _map = null;
            _nameMap = null;
        }

#endif
    }
}
