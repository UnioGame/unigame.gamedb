namespace UniGame.GameDb.Runtime
{
    using System;

    [Serializable]
    public struct CategoryInitializeResult
    {
        public bool complete;
        public string categoryName;
        public IGameDataCategory category;
        public string error;
    }
}