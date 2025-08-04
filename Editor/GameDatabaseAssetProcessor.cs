namespace UniGame.GameDB
{
    using System;
    using Game.Code.DataBase.Runtime;
    using UniModules.Editor;
    using UnityEditor;

    public class GameDatabaseAssetProcessor: AssetPostprocessor
    {
        public static GameDataBaseAsset _dataBaseAsset;
        
        /// Этот метод вызывается при изменении ассетов
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var haveChanges = importedAssets.Length > 0 || deletedAssets.Length > 0 || movedAssets.Length > 0;
            if (!haveChanges) return;
            
            _dataBaseAsset ??= AssetEditorTools.GetAsset<GameDataBaseAsset>();
            if(_dataBaseAsset.enableAutoUpdate == false) return;

            try
            {
                AssetDatabase.StartAssetEditing();
                _dataBaseAsset.UpdateData();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

    }
}
