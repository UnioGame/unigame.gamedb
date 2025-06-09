namespace Game.Code.DataBase.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UniGame.Core.Runtime;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
#if ODIN_INSPECTOR
    [InlineProperty]
    [BoxGroup(nameof(GameResourceId))]
#endif
    [Serializable]
    public struct GameResourceId
    {
#if ODIN_INSPECTOR
        [OnValueChanged(nameof(OnCategoryChanged))]
#endif
        public GameResourceCategoryId categoryId;
        
#if ODIN_INSPECTOR
        [ValueDropdown(nameof(GetCategoryIds))]
#endif
        public GameResourceRecordId id;

        public static implicit operator string(GameResourceId v) => v.id;

        public static explicit operator GameResourceRecordId(GameResourceId v) => v.id;
        
        public static explicit operator GameResourceCategoryId(GameResourceId v) => v.categoryId;
        
        public static explicit operator GameResourceId(string v) => new GameResourceId { id = (GameResourceRecordId)v };

        public override string ToString() => id;

        public override int GetHashCode() => string.IsNullOrEmpty(id) ? 0 : id.GetHashCode();

        public GameResourceId FromString(string value)
        {
            this.id = (GameResourceRecordId)value;
            return this;
        }

        public override bool Equals(object obj)
        {
            if (!string.IsNullOrEmpty(id) && obj is GameResourceId gameResourceId)
                return id.Equals(gameResourceId.id);

            return false;
        }
        

        public void OnCategoryChanged()
        {
#if UNITY_EDITOR
            var id = this.id;
            var ids = GetCategoryIds().ToList();
            var hasId = ids.Any(x => x.Value == id);
            if (!hasId) this.id = ids.FirstOrDefault().Value;
#endif
        }

        private IEnumerable<ValueDropdownItem<GameResourceRecordId>> GetCategoryIds()
        {
#if UNITY_EDITOR
            return GameDataBaseAsset.GetGameRecordIds(categoryId);
#endif
            return Enumerable.Empty<ValueDropdownItem<GameResourceRecordId>>();
        }

        
    }
}