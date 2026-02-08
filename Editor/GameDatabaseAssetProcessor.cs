namespace UniGame.GameDB
{
    using System;
    using Game.Code.DataBase.Runtime;
    using Runtime.DateTime;
    using UniModules.Editor;
    using UnityEditor;

    public class GameDatabaseAssetProcessor: AssetPostprocessor
    {
        public static GameDataBaseAsset _dataBaseAsset;
        public static long _lastImportTime;
        public static long _importDelay = 1000;

        [MenuItem("UniGame/GameDB/Reimport Database", false, 2000)]
        public static void ReimportDatabase()
        {
            _dataBaseAsset ??= AssetEditorTools.GetAsset<GameDataBaseAsset>();
            if(_dataBaseAsset == null) return;
            if(_dataBaseAsset.enableAutoUpdate == false) return;

            try
            {
                AssetDatabase.StartAssetEditing();
                
                _dataBaseAsset.UpdateData();
                _dataBaseAsset.lastImportTime = DateTime.UtcNow.ToUnixTimestamp();
                _dataBaseAsset.MarkDirty();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }
        
        /// Этот метод вызывается при изменении ассетов
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var haveChanges = importedAssets.Length > 0 || deletedAssets.Length > 0 || movedAssets.Length > 0;
            if (!haveChanges) return;
            
            var time = DateTime.UtcNow.ToUnixTimestamp();
            var timeDiff = time - _lastImportTime;
            if(timeDiff < _importDelay) return;
            
            _lastImportTime = time;
            
            ReimportDatabase();
        }

    }
}
