namespace UniGame.GameDb.Runtime
{
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using UniGame.Core.Runtime;

    public interface IGameDataCategory
    {
        public string Category { get; }
        
        public IGameResourceProvider ResourceProvider { get; }
        
        public IReadOnlyList<IGameResourceRecord> Records { get; }
        
        public Dictionary<string,IGameResourceRecord> Map { get; }
        
        public UniTask<CategoryInitializeResult> InitializeAsync(ILifeTime lifeTime);
        
        public bool Has(string id);
        
        public IGameResourceRecord Find(string filter);
        
        public IReadOnlyList<IGameResourceRecord> FindResources(string filter);

        /// <summary>
        /// editor only
        /// </summary>
        public IReadOnlyList<IGameResourceRecord> FillCategory();
    }
}