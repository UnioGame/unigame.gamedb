namespace Game.Code.DataBase.Runtime.Abstract
{
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
    public interface IGameResourceRecord : ISearchFilterable
    {
        public string Name { get; }
        public string Id { get; }
        
        bool CheckRecord(string filter);
    }
}