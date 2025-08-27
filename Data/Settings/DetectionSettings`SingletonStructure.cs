using System.IO;
using UnityEngine;

namespace Systems.Detection2D.Data.Settings
{
    public sealed partial class DetectionSettings : ScriptableObject
    {
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
    }
}