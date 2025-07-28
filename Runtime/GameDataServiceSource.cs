namespace UniGame.GameDB
{
    using Core.Runtime;
    using Core.Runtime.Extension;
    using Cysharp.Threading.Tasks;
    using Game.Code.DataBase.Runtime;
    using Game.Code.DataBase.Runtime.Abstract;
    using UniGame.AddressableTools.Runtime;
    using UniGame.Context.Runtime;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    [CreateAssetMenu(menuName = "UniGame/Game DB/Game DB Source", fileName = "Game DB Source")]
    public class GameDataServiceSource : DataSourceAsset<IGameDataService>
    {
        public AssetReferenceT<GameDataBaseAsset> _dataBaseAsset;

        protected sealed override async UniTask<IGameDataService> CreateInternalAsync(IContext context)
        {
            var lifeTime = context.LifeTime;
            
            var databaseAsset = await _dataBaseAsset
                .LoadAssetTaskAsync(lifeTime)
                .ToSharedInstanceAsync();

            var database = await databaseAsset
                .gameDatabase
                .Initialize(lifeTime);
            
            context.Publish<IGameDatabase>(database);

            var service = new GameDataService();

            return service;
        }
    }
}