namespace UniGame.GameDb.Runtime
{
    using System;
    using Cysharp.Threading.Tasks;
    using Game.Code.DataBase.Runtime;
    using UniGame.Core.Runtime;
    using Object = UnityEngine.Object;

    [Serializable]
    public class UnityResourcesDataProvider : IGameResourceProvider
    {
        public const string NotFoundError = "Resource not found";
        
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
            var asset = await UnityEngine.Resources
                .LoadAsync(resource, typeof(TResult))
                .ToUniTask();
            
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