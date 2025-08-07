namespace Game.Code.DataBase.Runtime
{
    using System;
    using UniGame.GameDb.Runtime;
    using Cysharp.Threading.Tasks;
    using UniGame.AddressableTools.Runtime;
    using UniGame.Core.Runtime;
    using Object = UnityEngine.Object;

    [Serializable]
    public class AddressableResourceProvider : IGameResourceProvider
    {
        public const string LoadingError = "Asset {0} not found";
        
        public Type unityObjectType = typeof(Object);
        
        public bool IsValidResourceSource(string resource, Type resourceType)
        {
            var isUnityObject = unityObjectType.IsAssignableFrom(resourceType);
            return isUnityObject;
        }

        public async UniTask<GameResourceResult> LoadAsync(string resource, ILifeTime lifeTime)
        {
            return await LoadAsync<Object>(resource, lifeTime);
        }

        public async UniTask<GameResourceResult> LoadAsync<TResult>(string resource,ILifeTime lifeTime)
        {
            var addressableResult = await resource
                .LoadAssetTaskAsync<TResult>(lifeTime);

            var error = addressableResult == null ? string.Format(LoadingError, resource) : string.Empty;
            
            var result = new GameResourceResult()
            {
                Complete = addressableResult!=null,
                Error = error,
                Exception = null,
                Result = addressableResult
            };
            
            return result;
        }
        
    }

}