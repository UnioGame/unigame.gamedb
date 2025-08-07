namespace UniGame.GameDb.Runtime
{
    using System;
    using Cysharp.Threading.Tasks;
    using Game.Code.DataBase.Runtime;
    using UniGame.Core.Runtime;

    public interface IGameDatabase
    {
        UniTask<GameResourceResult[]> LoadAllAsync<TResult>(string resource, ILifeTime lifeTime);
        
        bool IsValidResourceSource(string resource,Type resourceType);
        
        UniTask<GameResourceResult> LoadAsync(string resource, ILifeTime lifeTime);
        
        UniTask<GameResourceResult<TResult>> LoadAsync<TResult>(string resource, ILifeTime lifeTime);
    }
}