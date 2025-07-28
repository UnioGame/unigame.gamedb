namespace Game.Code.DataBase.Runtime
{
    using System.Collections.Generic;
    using Abstract;
    using Cysharp.Threading.Tasks;

    using UniGame.Core.Runtime;
    using UnityEngine;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
    public abstract class GameDataCategory : ScriptableObject, IGameDataCategory
    {
        public const string SettingsGroupKey = "settings";
        public const string CategoryGroupKey = "category";

        #region inspector
        
#if ODIN_INSPECTOR
        [TabGroup(CategoryGroupKey)]
#endif
        public string category;

#if ODIN_INSPECTOR
        [TabGroup(SettingsGroupKey)]
#endif
        public bool useAssetResourceProvider = false;
        
#if ODIN_INSPECTOR
        [TabGroup(SettingsGroupKey)]
        [HideIf(nameof(useAssetResourceProvider))]
#endif
        [SerializeReference]
        public IGameResourceProvider resourceProvider = new AddressableResourceProvider();
        
#if ODIN_INSPECTOR
        [TabGroup(SettingsGroupKey)]
        [InlineEditor()]
        [ShowIf(nameof(useAssetResourceProvider))]
#endif
        public GameResourceLocation resourceLocation;
        
        #endregion

        public virtual string Category => category;
        
        public virtual IGameResourceProvider ResourceProvider => resourceProvider;

        public abstract Dictionary<string, IGameResourceRecord> Map { get; }

        public abstract IReadOnlyList<IGameResourceRecord> Records { get; }
        
        public abstract UniTask<CategoryInitializeResult> InitializeAsync(ILifeTime lifeTime);

        public virtual bool Has(string id) => Find(id) != EmptyRecord.Value;
        
        public abstract IGameResourceRecord Find(string filter);
        
        public abstract IReadOnlyList<IGameResourceRecord> FindResources(string filter);

        public virtual IReadOnlyList<IGameResourceRecord> FillCategory()
        {
            return new List<IGameResourceRecord>();
        }

#if UNITY_EDITOR

#if ODIN_INSPECTOR
        [Button(ButtonSizes.Large,Icon = SdfIconType.ArchiveFill)]
#endif
#if ALCHEMY_INSPECTOR
        [Button]
#endif
        public virtual void UpdateCategory()
        {
            FillCategory();
        }
        
#endif

    }
}