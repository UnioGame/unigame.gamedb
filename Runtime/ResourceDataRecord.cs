namespace Game.Modules.game.packages.unigame.gamedb.Runtime
{
    using System;
    using Code.DataBase.Runtime.Abstract;

    [Serializable]
    public class ResourceDataRecord : IGameResourceRecord
    {
        public string id = string.Empty;
        public string resourcePath;

        public string Name => id;

        public string Id => id;

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