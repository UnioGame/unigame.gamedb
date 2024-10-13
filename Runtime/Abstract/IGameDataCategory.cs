namespace Game.Code.DataBase.Runtime.Abstract
{
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using UniGame.Core.Runtime;

    public interface IGameDataCategory
    {
        public string Category { get; }
        
        public IGameResourceProvider ResourceProvider { get; }
        
        public IGameResourceRecord[] Records { get; }
        
        public Dictionary<string,IGameResourceRecord> Map { get; }
        
        public UniTask<CategoryInitializeResult> InitializeAsync(ILifeTime lifeTime);
        
        public bool Has(string id);
        
        public IGameResourceRecord Find(string filter);
        
        public IGameResourceRecord[] FindResources(string filter);

        /// <summary>
        /// editor only
        /// </summary>
        public IReadOnlyList<IGameResourceRecord> FillCategory();
    }
}