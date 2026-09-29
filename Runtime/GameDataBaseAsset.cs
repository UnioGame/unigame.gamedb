namespace Game.Code.DataBase.Runtime
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
#if ALCHEMY_INSPECTOR
    using Alchemy.Inspector;
#endif

#if UNITY_EDITOR
    using UnityEditor;
    using UniModules.Editor;
#endif

    [CreateAssetMenu(menuName = "UniGame/Game DB/Game DB Asset", fileName = "Game DB Asset")]
    public class GameDataBaseAsset : ScriptableObject
    {
        /// <summary>
        /// editor: refresh the addressable categories on asset imports. Off by default: the RPG categories are written by
        /// their builders and the registration is incremental (see GameDatabaseAssetProcessor).
        /// </summary>
        public bool enableAutoUpdate = false;

        public long lastImportTime;//in milliseconds since epoch

        public int importDelay = 1000;//in milliseconds

#if ODIN_INSPECTOR
        [InlineProperty]
        [HideLabel]
#endif
        [SerializeField]
        public GameDatabase gameDatabase;

#if UNITY_EDITOR

        private static GameDataBaseAsset _editorDatabase;

        /// <summary>
        /// the project's registry (the first database found). Cached: the dropdown drawers call this on every draw and
        /// must not run FindAssets each time (G7).
        /// </summary>
        public static GameDataBaseAsset EditorDatabase
        {
            get
            {
                if (_editorDatabase != null) return _editorDatabase;
                var guid = AssetDatabase.FindAssets($"t:{nameof(GameDataBaseAsset)}").FirstOrDefault();
                if (string.IsNullOrEmpty(guid)) return null;
                _editorDatabase = AssetDatabase.LoadAssetAtPath<GameDataBaseAsset>(AssetDatabase.GUIDToAssetPath(guid));
                return _editorDatabase;
            }
        }

        public static IEnumerable<ValueDropdownItem<GameResourceRecordId>> GetGameRecordIds(GameResourceCategoryId category)
        {
            var config = EditorDatabase;
            if (config == null)
                yield break;

            yield return new ValueDropdownItem<GameResourceRecordId>()
            {
                Text = "EMPTY",
                Value = GameResourceRecordId.Empty
            };

            var ids = config.gameDatabase
                .categories
                .Where(x => x != null && x.editorAsset != null)
                .Select(x => x.editorAsset)
                .Where(x => category == GameResourceCategoryId.Empty || x.Category == (string)category)
                .SelectMany(x => x.Records)
                .Select(x => new ValueDropdownItem<GameResourceRecordId>() {
                    Text = x.Name,
                    Value = (GameResourceRecordId)x.Id, }).ToList();

            foreach (var id in ids)
                yield return id;
        }

        public static IEnumerable<ValueDropdownItem<GameResourceRecordId>> GetGameRecordIds()
        {
            return GetGameRecordIds(GameResourceCategoryId.Empty);
        }

        public static IEnumerable<GameResourceCategoryId> GetGameRecordCategories()
        {
            var config = EditorDatabase;
            if (config == null)
                yield break;

            yield return GameResourceCategoryId.Empty;

            var records = config.gameDatabase
                .categories
                .Where(x => x != null && x.editorAsset != null)
                .Select(x => (GameResourceCategoryId)x.editorAsset.Category)
                .ToList();

            foreach (var record in records)
                yield return record;
        }

#if ODIN_INSPECTOR
        [Button(Icon = SdfIconType.ArchiveFill)]
        [PropertyOrder(-1)]
#endif
#if ALCHEMY_INSPECTOR
        [Button]
#endif
        public void UpdateData()
        {
            foreach (var reference in gameDatabase.categories)
            {
                var category = reference?.editorAsset;
                if (category == null) continue;
                category.FillCategory();
            }
        }

#endif

    }
}
