using System;
using Systems.Detection2D.Data.Settings.Types;
using UnityEngine;

namespace Systems.Detection2D.Data.Settings
{
    [Serializable] public sealed partial class DetectionSettings
    {
        [SerializeField]
        [Tooltip("Object is outside of detection zone")]
        public Color gizmosColorObjectOutsideOfDetectionZone = Color.blue;
        [SerializeField]
        [Tooltip("Object is inside of detection zone and detected")]
        public Color gizmosColorObjectIndideZoneDetected = Color.red;
        [SerializeField] 
        [Tooltip("Object is inside of detection zone but not detected due to occlusion")]
        public Color gizmosColorObjectInsideZoneUndetected = Color.green;
        [SerializeField] 
        [Tooltip("Object is inside of detection zone but not detected as it cannot be detected / is ghost")]
        public Color gizmosColorObjectInsideZoneGhost = Color.yellow;

        [SerializeField]
        [Tooltip("Switch to selected mode to draw gizmos only for selected detectors, improves performance")]
        public GizmosDrawMode gizmosDrawModeForDetectors = GizmosDrawMode.Selected;

        [SerializeField]
        [Tooltip("Draw detection points for all detectors")]
        public bool drawDetectionPoints;
        
        [SerializeField]
        [Tooltip("Radius of detection points being drawn in units")]
        [Range(0.05f, 1f)]
        public float detectionPointRadius = 0.1f;
    }
}