#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Meta.Weapons.Editor
{
    [CustomEditor(typeof(LoadoutSnapshot))]
    public class LoadoutSnapshotEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            DrawDefaultInspector();
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "LoadoutSnapshot is auto-populated at runtime.\n" +
                "This inspector is read-only by design.",
                MessageType.Info);
        }
    }
}
#endif