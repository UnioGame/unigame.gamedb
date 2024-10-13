namespace Game.Code.DataBase.Runtime
{
    using Abstract;

    public class EmptyRecord : IGameResourceRecord
    {
        public readonly static EmptyRecord Value = new EmptyRecord();

        public const string IdKey = nameof(EmptyRecord);

        public string Name => string.Empty;

        public string Id => IdKey;
        
        public bool CheckRecord(string filter) => false;

        public bool IsMatch(string searchString) => false;
    }
}