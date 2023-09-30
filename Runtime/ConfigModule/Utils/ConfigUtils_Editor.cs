#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LSCore.ConfigModule.Editor
{
    public static partial class ConfigUtils
    {
        [MenuItem(LSConsts.Path.MenuItem.Root + "/Delete Save Data")]
        private static void DeleteSaveData()
        {
            var path = System.IO.Path.Combine(Application.dataPath, FolderNames.Configs, FolderNames.SaveData);
            
            if (Directory.Exists(path))  
            {  
                Directory.Delete(path, true);
                AssetDatabase.Refresh();
            }
        }
    }
}
#endif