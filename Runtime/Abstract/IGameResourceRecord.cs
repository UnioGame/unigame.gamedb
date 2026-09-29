namespace UniGame.GameDb.Runtime
{
    using System;
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

    /// <summary>
    /// One record of a game data category.
    /// <see cref="Id"/> is the exact key the database indexes; <see cref="ResourcePath"/> is what the resource provider
    /// loads (the Addressables key). Both have default implementations so simple records only declare Name and Id.
    /// </summary>
    public interface IGameResourceRecord
#if ODIN_INSPECTOR
        : ISearchFilterable
#endif
    {
        public string Name { get; }

        public string Id { get; }

        /// <summary>key the resource provider loads; defaults to <see cref="Id"/></summary>
        public string ResourcePath => Id;

        /// <summary>exact id match (case-insensitive)</summary>
        bool CheckRecord(string filter)
        {
            var id = Id;
            return !string.IsNullOrEmpty(filter) && id != null &&
                   id.Equals(filter, StringComparison.OrdinalIgnoreCase);
        }

#if !ODIN_INSPECTOR
        bool IsMatch(string searchString);
#endif
    }
}
