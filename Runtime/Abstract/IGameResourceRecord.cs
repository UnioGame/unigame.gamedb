namespace Game.Code.DataBase.Runtime.Abstract
{
    using Sirenix.OdinInspector;

    public interface IGameResourceRecord : ISearchFilterable
    {
        public string Name { get; }
        public string Id { get; }
        
        bool CheckRecord(string filter);
    }
}