using UnityEditor;
using UnityEngine;

namespace GAD213.Shotgun.Editor
{
    [CustomEditor(typeof(WeaponSystem))]
    public class WeaponSystemEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var weapon = (WeaponSystem)target;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Firing audio per shell", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Drag a firing clip into each shell below. These settings are saved on the shell assets and shared wherever those assets are used.", MessageType.Info);
            if (weapon.shells == null) return;
            foreach (var shell in weapon.shells)
            {
                if (!shell) continue;
                var settings = new SerializedObject(shell);
                settings.Update();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(shell.label, EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(settings.FindProperty("fireSound"), new GUIContent("Fire Sound"));
                EditorGUILayout.PropertyField(settings.FindProperty("fireVolume"), new GUIContent("Fire Volume"));
                EditorGUILayout.EndVertical();
                settings.ApplyModifiedProperties();
            }
        }
    }
}
