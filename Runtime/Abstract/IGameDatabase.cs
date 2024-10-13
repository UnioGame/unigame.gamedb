namespace Game.Code.DataBase.Runtime.Abstract
{
    using Cysharp.Threading.Tasks;
    using UniGame.Core.Runtime;

    public interface IGameDatabase : IGameResourceProvider
    {
        UniTask<GameResourceResult[]> LoadAllAsync<TResult>(string resource, ILifeTime lifeTime);
    }
}