#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Meta.Weapons.Editor
{
    public static class WeaponCatalogEditor
    {
        [MenuItem("Meta/Weapons/Create Weapon Catalog Asset", priority = 201)]
        public static void CreateCatalogAsset()
        {
            var asset = ScriptableObject.CreateInstance<WeaponCatalog>();
            var path = EditorUtility.SaveFilePanelInProject("Create Weapon Catalog", "WeaponCatalog", "asset", "Select location");
            if (string.IsNullOrEmpty(path)) return;

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        [MenuItem("Meta/Weapons/Refresh Selected Weapon Catalog(s)", priority = 202)]
        public static void RefreshSelectedCatalogs()
        {
            var objs = Selection.objects;
            int count = 0;
            foreach (var obj in objs)
            {
                if (obj is WeaponCatalog cat)
                {
                    cat.Editor_RefreshFromProject();
                    count++;
                }
            }
            if (count == 0)
                EditorUtility.DisplayDialog("Weapon Catalog", "Select one or more WeaponCatalog assets first.", "OK");
        }
    }
}
#endif