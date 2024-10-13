namespace Game.Code.DataBase.Runtime
{
    using System;
    using Abstract;
    using Cysharp.Threading.Tasks;
    using UniGame.AddressableTools.Runtime;
    using UniGame.Core.Runtime;
    using Object = UnityEngine.Object;

    [Serializable]
    public class AddressableResourceProvider : IGameResourceProvider
    {
        public Type unityObjectType = typeof(Object);
        
        public bool IsValidResourceSource(string resource, Type resourceType)
        {
            var isUnityObject = unityObjectType.IsAssignableFrom(resourceType);
            return isUnityObject;
        }

        public async UniTask<GameResourceResult> LoadAsync<TResult>(string resource,ILifeTime lifeTime)
        {
            var addressableResult = await resource
                .LoadAssetTaskAsync<TResult>(lifeTime);

            var result = new GameResourceResult()
            {
                Complete = addressableResult.Complete,
                Error = addressableResult.Error,
                Exception = addressableResult.Exception,
                Result = addressableResult.Result
            };
            
            return result;
        }
    }

}