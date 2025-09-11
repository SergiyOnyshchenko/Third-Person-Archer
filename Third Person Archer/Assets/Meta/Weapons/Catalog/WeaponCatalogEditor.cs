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
            var path = EditorUtility.SaveFilePanelInProject(
                "Create Weapon Catalog",
                "WeaponCatalog",
                "asset",
                "Select location"
            );
            if (string.IsNullOrEmpty(path)) return;

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}
#endif