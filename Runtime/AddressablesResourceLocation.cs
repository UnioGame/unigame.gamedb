namespace Game.Code.DataBase.Runtime
{
    using System;
    using Cysharp.Threading.Tasks;
    using UniGame.Core.Runtime;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>asset form of <see cref="AddressableResourceProvider"/> (existing categories reference it as a serialized asset)</summary>
    [CreateAssetMenu(menuName = "UniGame/Game DB/Locations/" + nameof(AddressablesResourceLocation), fileName = nameof(AddressablesResourceLocation))]
    public class AddressablesResourceLocation : GameResourceLocation
    {
        private readonly AddressableResourceProvider _provider = new AddressableResourceProvider();

        public override bool IsValidResourceSource(string resource, Type resourceType) =>
            _provider.IsValidResourceSource(resource, resourceType);

        public override UniTask<GameResourceResult> LoadAsync(string resource, ILifeTime lifeTime) =>
            _provider.LoadAsync<Object>(resource, lifeTime);

        public override UniTask<GameResourceResult> LoadAsync<TAsset>(string resource, ILifeTime lifeTime) =>
            _provider.LoadAsync<TAsset>(resource, lifeTime);

        public override async UniTask<GameResourceResult[]> LoadAllAsync<TResult>(string resource, ILifeTime lifeTime)
        {
            var single = await _provider.LoadAsync<TResult>(resource, lifeTime);
            return new[] { single };
        }
    }
}
