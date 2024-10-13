namespace Game.Code.DataBase.Runtime
{
    using System;
    using Abstract;
    using Object = UnityEngine.Object;

    [Serializable]
    public class UnityObjectResourceRecord : IGameResourceRecord
    {
        public Object asset;

        public string Name => asset.name;

        public string Id => asset == null ? DatabaseRecordConstants.EmptyId : asset.name;

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