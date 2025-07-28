namespace UniGame.GameDB
{
    using AddressableTools.Editor;
    using Game.Code.DataBase.Runtime;
    using UniModules.Editor;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    public class GameDbEditorTools
    {
        [MenuItem("Assets/UniGame/Game DB/Create Game DB",false, 100)]
        [MenuItem("Assets/Create/UniGame/Game DB/Create Game DB",false, 100)]
        public static void CreateDatabase()
        {
            var selection = Selection.activeObject;
            var contextPath = AssetDatabase.GetAssetPath(selection);
            var dbSourceAsset = ScriptableObject.CreateInstance<GameDataServiceSource>();
            var dbAddressableCategory = ScriptableObject.CreateInstance<AddressableGameDataCategory>();
            var dbAsset = ScriptableObject.CreateInstance<GameDataBaseAsset>();
            
            dbAsset.name = "GameDataBase";
            dbSourceAsset.name = "GameDataServiceSource";
            dbAddressableCategory.name = "GameDataAddressableCategory";
            
            Debug.Log("Game DB: path: " + contextPath);

            dbAsset = dbAsset.SaveAsset(contextPath);
            dbSourceAsset.dataBaseAsset = dbAsset;
            dbSourceAsset = dbSourceAsset.SaveAsset(contextPath);
            
            dbAddressableCategory = dbAddressableCategory.SaveAsset(contextPath);
            dbAddressableCategory.AddToDefaultAddressableGroup();
            var categoryGuid = dbAddressableCategory.GetGUID();
            
            dbAsset.gameDatabase.categories.Add(new AssetReferenceT<GameDataCategory>(categoryGuid));

            dbSourceAsset.MarkDirty();
            dbAddressableCategory.MarkDirty();
            dbAsset.MarkDirty();
        }
    }
}