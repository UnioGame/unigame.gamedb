namespace Game.Code.DataBase.Runtime
{
    using System;
    using Abstract;
    using Cysharp.Threading.Tasks;
    using UniGame.Core.Runtime;
    using UnityEngine;

    public abstract class GameResourceLocation : ScriptableObject,IGameResourceProvider
    {
        public abstract bool IsValidResourceSource(string resource, Type resourceType);
        public abstract UniTask<GameResourceResult> LoadAsync(string resource, ILifeTime lifeTime);
        public abstract UniTask<GameResourceResult> LoadAsync<TAsset>(string resource, ILifeTime lifeTime);
        public abstract UniTask<GameResourceResult[]> LoadAllAsync<TResult>(string resource, ILifeTime lifeTime);
    }

    
}