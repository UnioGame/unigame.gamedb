namespace UniGame.GameDb.Runtime
{
    using Cysharp.Threading.Tasks;
    using Runtime;
    using UniGame.Core.Runtime;

    public static class GameDatabaseExtensions
    {
        public static async UniTask<TAsset> LoadAssetAsync<TAsset>(this IGameDatabase database, string assetId, ILifeTime lifeTime)
        {
            var result = await database.LoadAsync<TAsset>(assetId,lifeTime);
            return result.Result;
        }
    }
}