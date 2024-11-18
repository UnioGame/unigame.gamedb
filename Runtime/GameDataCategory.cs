namespace Game.Code.DataBase.Runtime
{
    using System.Collections.Generic;
    using Abstract;
    using Cysharp.Threading.Tasks;
    using Sirenix.OdinInspector;
    using UniGame.Core.Runtime;
    using UnityEngine;

    public abstract class GameDataCategory : ScriptableObject, IGameDataCategory
    {
        public const string SettingsGroupKey = "settings";
        public const string CategoryGroupKey = "category";

        #region inspector
        
        [TabGroup(CategoryGroupKey)]
        public string category;

        [TabGroup(SettingsGroupKey)]
        public bool useAssetResourceProvider = false;
        
        [TabGroup(SettingsGroupKey)]
        [HideIf(nameof(useAssetResourceProvider))]
        [SerializeReference]
        public IGameResourceProvider resourceProvider = new AddressableResourceProvider();
        
        [TabGroup(SettingsGroupKey)]
        [InlineEditor()]
        [ShowIf(nameof(useAssetResourceProvider))]
        public GameResourceLocation resourceLocation;
        
        #endregion

        public virtual string Category => category;
        
        public virtual IGameResourceProvider ResourceProvider => resourceProvider;

        public abstract Dictionary<string, IGameResourceRecord> Map { get; }

        public abstract IGameResourceRecord[] Records { get; }
        
        public abstract UniTask<CategoryInitializeResult> InitializeAsync(ILifeTime lifeTime);

        public virtual bool Has(string id) => Find(id) != EmptyRecord.Value;
        
        public abstract IGameResourceRecord Find(string filter);
        
        public abstract IGameResourceRecord[] FindResources(string filter);
        

        public virtual IReadOnlyList<IGameResourceRecord> FillCategory()
        {
            return new List<IGameResourceRecord>();
        }

#if UNITY_EDITOR

        [Button(ButtonSizes.Large,Icon = SdfIconType.ArchiveFill)]
        public virtual void UpdateCategory()
        {
            FillCategory();
        }
        
#endif

    }
}