namespace UniGame.GameDb.Runtime
{
    using System;
    using Cysharp.Threading.Tasks;
    using Game.Code.DataBase.Runtime;
    using UniGame.Core.Runtime;

    public interface IGameResourceProvider
    {
        bool IsValidResourceSource(string resource,Type resourceType);
        
        UniTask<GameResourceResult> LoadAsync(string resource, ILifeTime lifeTime);
        
        UniTask<GameResourceResult> LoadAsync<TResult>(string resource, ILifeTime lifeTime);
    }
}