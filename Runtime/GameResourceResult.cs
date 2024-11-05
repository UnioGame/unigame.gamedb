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
        
        public object Result;
        public bool Complete;
        public string Error;
        public Exception Exception;
    }
}