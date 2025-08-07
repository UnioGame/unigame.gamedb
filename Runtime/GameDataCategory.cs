namespace Game.Code.DataBase.Runtime
{
    using System;
    using System.Collections.Generic;
    using UniGame.GameDb.Runtime;
    using Cysharp.Threading.Tasks;
    using UniGame.Core.Runtime;
    using UnityEngine;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

#if ALCHEMY_INSPECTOR
    using Alchemy.Inspector;
#endif

    [Serializable]
    public abstract class GameDataCategory : ScriptableObject, IGameDataCategory
    {
        public const string SettingsGroupKey = "settings";
        public const string CategoryGroupKey = "category";

        public string category;

        public virtual string Category => category;

        public abstract IGameResourceProvider ResourceProvider { get; }

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
        [Button(ButtonSizes.Large, Icon = SdfIconType.ArchiveFill)]
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