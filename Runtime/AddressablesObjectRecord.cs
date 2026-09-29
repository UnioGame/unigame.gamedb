namespace Game.Code.DataBase.Runtime
{
    using System;
    using UniGame.GameDb.Runtime;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    [Serializable]
    public class AddressablesObjectRecord : IGameResourceRecord
    {
        public string name;
        [SerializeField]
        public AssetReference assetReference;
        public string[] labels = Array.Empty<string>();

        public string Id => assetReference.AssetGUID;

        public string ResourcePath => assetReference.AssetGUID;

        public string Name
        {
            get
            {
#if UNITY_EDITOR
                //editor fallback for records created by hand: the name field is filled once, the object is loaded only then
                if (string.IsNullOrEmpty(name) && assetReference != null)
                {
                    var asset = assetReference.editorAsset;
                    name = asset == null ? string.Empty : asset.name;
                }
#endif
                return name;
            }
        }

        public bool CheckRecord(string filter)
        {
            if (string.IsNullOrEmpty(filter)) return false;
            if (!assetReference.RuntimeKeyIsValid()) return false;
            if (assetReference.AssetGUID.Equals(filter, StringComparison.OrdinalIgnoreCase)) return true;
            foreach (var label in labels)
            {
                if (label.Equals(filter, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public bool IsMatch(string searchString)
        {
            if (string.IsNullOrEmpty(searchString)) return true;
            if (Id != null && Id.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (Name != null && Name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            foreach (var label in labels)
            {
                if (label.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }
    }
}
