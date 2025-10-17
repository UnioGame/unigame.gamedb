namespace UniGame.GameDb.Runtime
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using Game.Code.DataBase.Runtime;
    using UniGame.Core.Runtime;
    using UnityEngine;
    using Object = UnityEngine.Object;

    [Serializable]
    public class UnityResourcesDataProvider : IGameResourceProvider
    {
        #region static data
        
        public readonly static Dictionary<string,Object> CachedResources = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ResetProvider()
        {
            CachedResources.Clear();
        }
        
        #endregion
        
        public const string NotFoundError = "Resource not found";
        
        public bool cacheResources = true;
        
        public bool IsValidResourceSource(string resource, Type resourceType)
        {
            return string.IsNullOrEmpty(resource) == false;
        }

        public async UniTask<GameResourceResult> LoadAsync(string resource, ILifeTime lifeTime)
        {
            return await LoadAsync<Object>(resource, lifeTime);
        }

        public async UniTask<GameResourceResult> LoadAsync<TResult>(string resource, ILifeTime lifeTime)
        {
            Object asset = null;
            
            if (CachedResources.TryGetValue(resource, out var value))
            {
                asset = value;
            }

            if (asset == null)
            {
                asset = await UnityEngine.Resources
                    .LoadAsync(resource, typeof(TResult))
                    .ToUniTask();
                
                if(cacheResources)
                    CachedResources[resource] = asset;
            }
            
            var success = asset is TResult;
            
            var result = new GameResourceResult
            {
                Id = resource,
                Result = asset,
                Complete = success,
                Error = success ? string.Empty : NotFoundError,
            };
            
            return result;
        }
    }
}