using Systems.SimpleDetection.Components.Detectors.Abstract;
using Systems.SimpleDetection.Components.Detectors.Zones;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Components.Detectors.Base
{
    public abstract class Frustum3DDetector : ObjectDetectorBase
    {
        [SerializeField] private float angle = 45f;
        [SerializeField] private float aspectRatio = 16/9f;
        [SerializeField] private float nearPlaneDistance = 1f;
        [SerializeField] private float farPlaneDistance = 10f;
        
        protected override IDetectionZone GetDetectionZone()
        {
            Transform objTransform = transform;
            float3 position = objTransform.position;
            quaternion rotation = objTransform.rotation;

            return new Frustum3DDetectionZone(position, rotation, farPlaneDistance, nearPlaneDistance, angle,
                aspectRatio);
        }
    }
}