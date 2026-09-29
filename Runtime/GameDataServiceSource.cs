namespace UniGame.GameDB
{
    using Core.Runtime;
    using Core.Runtime.Extension;
    using Cysharp.Threading.Tasks;
    using Game.Code.DataBase.Runtime;
    using GameDb.Runtime;
    using UniGame.AddressableTools.Runtime;
    using UniGame.Context.Runtime;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    /// <summary>
    /// Bootstrap source of the game database. The database asset is an AssetReference (serialized as
    /// <c>_dataBaseAsset</c>, the schema of the project's existing source asset); the database is initialized with the
    /// context lifetime and published as <see cref="IGameDatabase"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "UniGame/Game DB/Game DB Source", fileName = "Game DB Source")]
    public class GameDataServiceSource : DataSourceAsset<IGameDataService>
    {
        public AssetReferenceT<GameDataBaseAsset> _dataBaseAsset;

        protected sealed override async UniTask<IGameDataService> CreateInternalAsync(IContext context)
        {
            var databaseAsset = await _dataBaseAsset
                .LoadAssetTaskAsync(context.LifeTime)
                .ToSharedInstanceAsync();

            var database = await databaseAsset
                .gameDatabase
                .Initialize(context.LifeTime);

            database.AddTo(context.LifeTime);
            context.Publish<IGameDatabase>(database);

            return new GameDataService();
        }
    }
}
