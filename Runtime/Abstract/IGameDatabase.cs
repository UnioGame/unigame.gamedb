namespace UniGame.GameDb.Runtime
{
    using System;
    using Cysharp.Threading.Tasks;
    using Game.Code.DataBase.Runtime;
    using UniGame.Core.Runtime;

    public interface IGameDatabase : IDisposable
    {
        UniTask<GameResourceResult[]> LoadAllAsync<TResult>(string resource, ILifeTime lifeTime);

        bool IsValidResourceSource(string resource, Type resourceType);

        UniTask<GameResourceResult> LoadAsync(string resource, ILifeTime lifeTime);

        UniTask<GameResourceResult<TResult>> LoadAsync<TResult>(string resource, ILifeTime lifeTime);

        /// <summary>
        /// exact id lookup over every category: O(1), no filter matching, no allocation.
        /// Implementations that keep no index (test fakes) return false.
        /// </summary>
        bool TryGetRecord(string id, out IGameDataCategory category, out IGameResourceRecord record)
        {
            category = null;
            record = null;
            return false;
        }

        IGameDataCategory GetCategory(string category) => null;

        /// <summary>the categories loaded into the registry, in serialized order (empty for fakes)</summary>
        System.Collections.Generic.IReadOnlyList<IGameDataCategory> Categories => System.Array.Empty<IGameDataCategory>();
    }
}
