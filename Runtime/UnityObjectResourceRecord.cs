namespace Game.Code.DataBase.Runtime
{
    using System;
    using UniGame.GameDb.Runtime;
    using Object = UnityEngine.Object;

    [Serializable]
    public class UnityObjectResourceRecord : IGameResourceRecord
    {
        public Object asset;
        private string _resourcePath;

        public string Name => Id;

        public string Id => asset == null ? DatabaseRecordConstants.EmptyId : asset.name;

        public string ResourcePath => _resourcePath;

        public bool CheckRecord(string filter)
        {
            if(asset == null) return false;

            return Id.Equals(filter, StringComparison.OrdinalIgnoreCase);
        }
        
        public bool IsMatch(string searchString)
        {
            if (string.IsNullOrEmpty(searchString)) return true;
            if (Id.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }
    }
}