using System;
using System.IO;
using Systems.SimpleDetection.Data.Settings.Types;
using UnityEngine;

namespace Systems.SimpleDetection.Data.Settings
{
    [Serializable] public sealed class DetectionSettings : ScriptableObject
    {
        [SerializeField] [Tooltip("Object is outside of detection zone")]
        public Color gizmosColorObjectOutsideOfDetectionZone = Color.blue;

        [SerializeField] [Tooltip("Object is inside of detection zone and detected")]
        public Color gizmosColorObjectIndideZoneDetected = Color.red;

        [SerializeField] [Tooltip("Object is inside of detection zone but not detected due to occlusion")]
        public Color gizmosColorObjectInsideZoneUndetected = Color.green;

        [SerializeField]
        [Tooltip("Object is inside of detection zone but not detected as it cannot be detected / is ghost")]
        public Color gizmosColorObjectInsideZoneGhost = Color.yellow;

        [SerializeField]
        [Tooltip("Switch to selected mode to draw gizmos only for selected detectors, improves performance")]
        public GizmosDrawMode gizmosDrawModeForDetectors = GizmosDrawMode.Selected;

        [SerializeField] [Tooltip("Draw detection points for all detectors")] public bool drawDetectionPoints;

        [SerializeField] [Tooltip("Radius of detection points being drawn in units")] [Range(0.05f, 1f)]
        public float detectionPointRadius = 0.1f;

#region SINGLETON

        private const string RESOURCES_PATH = "DetectionSettings";
        private static DetectionSettings _instance;

        /// <summary>
        ///     Instance of <see cref="DetectionSettings"/>
        /// </summary>
        public static DetectionSettings Instance
        {
            get
            {
                if (!_instance) _instance = LoadOrCreateSettings();
                return _instance;
            }
        }

        /// <summary>
        ///     If settings are missing we attempt to load or create them
        /// </summary>
        private static DetectionSettings LoadOrCreateSettings()
        {
            const string PATH = "Assets/Resources/" + RESOURCES_PATH + ".asset";

            // Load from Resources in runtime
            DetectionSettings settings = Resources.Load<DetectionSettings>(RESOURCES_PATH);

#if UNITY_EDITOR
            // If not found, auto-create in Editor
            if (settings == null)
            {
                // Create instance of settings
                settings = CreateInstance<DetectionSettings>();
                if (!Directory.Exists("Assets/Resources")) Directory.CreateDirectory("Assets/Resources");
                UnityEditor.AssetDatabase.CreateAsset(settings, PATH);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
            }
#endif

            // If still null (e.g., stripped Resources), create a runtime default
            if (!settings) settings = CreateInstance<DetectionSettings>();

            return settings;
        }

#endregion
    }
}