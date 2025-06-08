namespace Game.Code.Services.GameDatabase
{
    using Cysharp.Threading.Tasks;
    using DataBase.Runtime;
    using DataBase.Runtime.Abstract;
    using UniGame.AddressableTools.Runtime;
    using UniGame.Core.Runtime;
    using UniGame.Core.Runtime.Extension;
    using UniGame.Context.Runtime;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    [CreateAssetMenu(menuName = "Game/Services/Game Data/Game Data Source", fileName = "Game Data Service")]
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