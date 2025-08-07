namespace UniGame.GameDb.Runtime
{
    using System;
    using Runtime;

    [Serializable]
    public class ResourceDataRecord : IGameResourceRecord
    {
        public string id = string.Empty;
        public string resourcePath;

        public string Name => id;

        public string Id => id;
        
        public string ResourcePath => resourcePath;

        public bool CheckRecord(string filter)
        {
            return id == filter;
        }

        public bool IsMatch(string filter)
        {
            if (string.IsNullOrEmpty(filter))
                return true;

            return id.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}