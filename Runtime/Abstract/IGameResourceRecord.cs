namespace Game.Code.DataBase.Runtime.Abstract
{
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
    public interface IGameResourceRecord
#if ODIN_INSPECTOR
        : ISearchFilterable
#endif
    {
        public string Name { get; }
        public string Id { get; }
        
        bool CheckRecord(string filter);
        
        #if !ODIN_INSPECTOR
        bool IsMatch(string searchString);
        #endif
    }
}