using Systems.SimpleDetection.Components.Detectors.Abstract;
using Systems.SimpleDetection.Components.Detectors.Zones;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Components.Detectors.Base
{
    // ReSharper disable once ClassCanBeSealed.Global
    public class FrustumDetector : ObjectDetectorBase
    {
        [SerializeField] private float angle = 45f;
        [SerializeField] private float radius = 2f;
        
        protected override IDetectionZone GetDetectionZone()
        {
            Transform objTransform = transform;
            float3 position = objTransform.position;
            float3 forward = objTransform.up;

            return new FrustumDetectionZone(position.xy, forward.xy, angle, radius);
        }
    }
}