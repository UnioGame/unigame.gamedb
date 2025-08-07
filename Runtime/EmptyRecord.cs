namespace Game.Code.DataBase.Runtime
{
    using UniGame.GameDb.Runtime;

    public class EmptyRecord : IGameResourceRecord
    {
        public readonly static EmptyRecord Value = new EmptyRecord();
        private string _resourcePath;

        public const string IdKey = nameof(EmptyRecord);

        public string Name => string.Empty;

        public string Id => IdKey;

        public string ResourcePath => IdKey;

        public bool CheckRecord(string filter) => false;

        public bool IsMatch(string searchString) => false;
    }
}