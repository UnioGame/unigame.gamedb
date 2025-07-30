namespace Game.Code.DataBase.Runtime
{
    using System;

    [Serializable]
    public struct GameResourceResult
    {
        public const string ResourceError = "Game Resource Not Found";
        public static GameResourceResult FailedResourceResult = new GameResourceResult()
        {
            Complete = false,
            Error = ResourceError,
            Result = null
        };

        public string Id;
        public object Result;
        public bool Complete;
        public string Error;
        public Exception Exception;
    }
    
    [Serializable]
    public struct GameResourceResult<TAsset>
    {
        public const string ResourceError = "Game Resource Not Found";
        public static GameResourceResult<TAsset> FailedResourceResult = new GameResourceResult<TAsset>()
        {
            Complete = false,
            Error = ResourceError,
            Result = default(TAsset)
        };

        public string Id;
        public TAsset Result;
        public bool Complete;
        public string Error;
        public Exception Exception;
    }
}