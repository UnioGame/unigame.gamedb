namespace Game.Code.DataBase.Runtime
{
    using System;
    using Cysharp.Threading.Tasks;
    using UniGame.AddressableTools.Runtime;
    using UniGame.Core.Runtime;
    using UniGame.GameDb.Runtime;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Loads a record's resource path (an Addressables key: GUID runtime key or address) through the lifetime aware
    /// <c>LoadReferenceAsync</c>: one handle acquire per lifetime, released when the lifetime ends, cancelled with it.
    /// </summary>
    [Serializable]
    public class AddressableResourceProvider : IGameResourceProvider
    {
        public const string LoadingError = "Asset {0} not found";
        public const string CancelledError = "Asset {0} load cancelled";

        public Type unityObjectType = typeof(Object);

        public bool IsValidResourceSource(string resource, Type resourceType)
        {
            return unityObjectType.IsAssignableFrom(resourceType);
        }

        public UniTask<GameResourceResult> LoadAsync(string resource, ILifeTime lifeTime)
        {
            return LoadAsync<Object>(resource, lifeTime);
        }

        public async UniTask<GameResourceResult> LoadAsync<TResult>(string resource, ILifeTime lifeTime)
        {
            if (string.IsNullOrEmpty(resource))
                return Failed(resource, string.Format(LoadingError, resource));

            //the typed key of the addressable cache is the requested type when it is an UnityEngine.Object, else Object
            var loadResult = typeof(Object).IsAssignableFrom(typeof(TResult))
                ? await LoadTyped<TResult>(resource, lifeTime)
                : await resource.LoadReferenceAsync<Object>(lifeTime, lifeTime.Token);

            var asset = loadResult.Success ? loadResult.Result : null;
            var complete = asset != null && (asset is TResult || typeof(TResult) == typeof(object));

            return new GameResourceResult()
            {
                Id = resource,
                Complete = complete,
                Error = complete ? string.Empty : (string.IsNullOrEmpty(loadResult.Error) ? string.Format(LoadingError, resource) : loadResult.Error),
                Exception = null,
                Result = complete ? asset : null,
            };
        }

        private static UniTask<AddressableLoadResult> LoadTyped<TResult>(string resource, ILifeTime lifeTime)
        {
            return resource.LoadReferenceAsync<TResult>(lifeTime, lifeTime.Token);
        }

        private static GameResourceResult Failed(string resource, string error)
        {
            return new GameResourceResult()
            {
                Id = resource,
                Complete = false,
                Error = error,
                Result = null,
            };
        }
    }
}
