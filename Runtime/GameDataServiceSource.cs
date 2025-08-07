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

    [CreateAssetMenu(menuName = "UniGame/Game DB/Game DB Source", fileName = "Game DB Source")]
    public class GameDataServiceSource : DataSourceAsset<IGameDatabase>
    {
        public GameDataBaseAsset dataBaseAsset;

        protected sealed override async UniTask<IGameDatabase> CreateInternalAsync(IContext context)
        {
            var lifeTime = context.LifeTime;
            var dbAsset = Instantiate(dataBaseAsset);

            var database = await dbAsset
                .gameDatabase
                .Initialize(lifeTime);
            
            context.Publish<IGameDatabase>(database);

            return database;
        }
    }
}