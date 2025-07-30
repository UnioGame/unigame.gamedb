namespace Game.Modules.game.packages.unigame.gamedb.Runtime
{
    using Code.DataBase.Runtime.Abstract;
    using Cysharp.Threading.Tasks;
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