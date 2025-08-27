#if UNITY_EDITOR
using JetBrains.Annotations;
using UnityEditor;

namespace Systems.Detection2D.Data.Settings
{

    internal static class DetectionSettingsEditor
    {
        private const string EDITOR_NAME = "Detection Settings";

        [SettingsProvider] [NotNull] public static SettingsProvider CreateSettingsProvider()
        {
            SettingsProvider provider = new($"Project/{EDITOR_NAME}", SettingsScope.Project)
            {
                label = EDITOR_NAME,

                guiHandler = (searchContext) =>
                {
                    DetectionSettings settings = DetectionSettings.Instance;
                    SerializedObject so = new(settings);
                 
                    EditorGUILayout.LabelField("Gizmos Colors", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(so.FindProperty(nameof(DetectionSettings.gizmosColorObjectOutsideOfDetectionZone)));
                    EditorGUILayout.PropertyField(so.FindProperty(nameof(DetectionSettings.gizmosColorObjectIndideZoneDetected)));
                    EditorGUILayout.PropertyField(so.FindProperty(nameof(DetectionSettings.gizmosColorObjectInsideZoneUndetected)));
                    EditorGUILayout.PropertyField(so.FindProperty(nameof(DetectionSettings.gizmosColorObjectInsideZoneGhost)));

                    EditorGUILayout.LabelField("Detection gizmos settings", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(so.FindProperty(nameof(DetectionSettings.gizmosDrawModeForDetectors)));
                    EditorGUILayout.PropertyField(so.FindProperty(nameof(DetectionSettings.drawDetectionPoints)));
                    EditorGUILayout.PropertyField(so.FindProperty(nameof(DetectionSettings.detectionPointRadius)));
                    
                    
                    so.ApplyModifiedProperties();
                }
            };

            return provider;
        }
    }
}
#endif