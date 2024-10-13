namespace Game.Code.DataBase.Runtime.Abstract
{
    using System;
    using Cysharp.Threading.Tasks;
    using UniGame.Core.Runtime;

    public interface IGameResourceProvider
    {
        bool IsValidResourceSource(string resource,Type resourceType);
        
        UniTask<GameResourceResult> LoadAsync<TResult>(string resource, ILifeTime lifeTime);
    }
}