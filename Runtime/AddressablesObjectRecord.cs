namespace Game.Code.DataBase.Runtime
{
    using System;
    using System.Collections.Generic;
    using Abstract;
    using UnityEngine.AddressableAssets;

    [Serializable]
    public class AddressablesObjectRecord : IGameResourceRecord
    {
        public string name;
        public AssetReference assetReference;
        public string[] labels = Array.Empty<string>();
        

        public string Id => assetReference.AssetGUID;

        public string Name
        {
            get
            {
#if UNITY_EDITOR
                var asset = assetReference.editorAsset;
                name = asset == null ? string.Empty : asset.name;
                return name;
#endif
                return name;    
            }
        }

        public bool CheckRecord(string filter)
        {
            if(string.IsNullOrEmpty(filter)) return false;
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
            if (Id != null && Id.IndexOf(searchString,StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (Name != null && Name.IndexOf(searchString,StringComparison.OrdinalIgnoreCase) >= 0) return true;
            foreach (var label in labels)
            {
                if (label.IndexOf(searchString,StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }
        
        
        
    }
}
