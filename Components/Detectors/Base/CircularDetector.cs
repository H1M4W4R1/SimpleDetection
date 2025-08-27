using JetBrains.Annotations;
using Systems.Detection2D.Components.Detectors.Abstract;
using Systems.Detection2D.Components.Detectors.Zones;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.Detection2D.Components.Detectors.Base
{
    // ReSharper disable once ClassCanBeSealed.Global
    public class CircularDetector : ObjectDetectorBase
    {
        [SerializeField] private float radius = 2f;

        protected override IDetectionZone GetDetectionZone()
        {
            float3 position = transform.position;
            return new CircularDetectionZone(position.xy, radius);
        }
    }
}