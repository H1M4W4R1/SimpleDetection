using System.Runtime.CompilerServices;
using Systems.SimpleDetection.Data.Enums;
using Systems.SimpleDetection.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Components.Detectors.Zones
{
    [BurstCompile] public readonly struct Circle2DDetectionZone : IDetectionZone
    {
        private readonly float2 position;
        private readonly float radius;
        private readonly float radiusSq;

        public Circle2DDetectionZone(float2 position, float radius)
        {
            this.position = position;
            this.radius = radius;
            radiusSq = radius * radius;
        }

        /// <summary>
        ///     Checks if a point is inside the zone.
        /// </summary>
        /// <param name="detectionPosition">Point to check</param>
        /// <returns>true if point is inside the zone, false otherwise</returns>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPointInZone(in float3 detectionPosition)
            => math.lengthsq(detectionPosition.xy - position) <= radiusSq;
        
        /// <summary>
        ///     Checks if a point is seen (i.e. not obscured) by the zone.
        /// </summary>
        /// <param name="detectionPosition">Point to check</param>
        /// <param name="layerMask">Layer mask to use for raycast</param>
        /// <returns>true if point is seen, false otherwise</returns>
        /// <remarks>
        ///     This method will first check if the point is inside the zone, and if not, will return false.
        ///     Then it will cast a ray from the point to the center of the zone and check if it hits anything.
        ///     If it does, then the point is obscured and the method will return false. Otherwise, it will return true.
        /// </remarks>
        public SpotResult IsPointSeen(in float3 detectionPosition, int layerMask)
        {
            // Ensure that point is in zone
            if (!IsPointInZone(detectionPosition)) return SpotResult.Outside;

            // Compute raycast data
            float2 detectedPosition = detectionPosition.xy;
            RaycastHit2D hitObj =
                Physics2D.Raycast(detectedPosition, math.normalize(position - detectedPosition),
                    math.min(math.distance(detectedPosition, position), radius),
                    layerMask);
            
            // Ensure that the raycast hit nothing
            return ReferenceEquals(hitObj.collider, null) ? SpotResult.InsideSeen : SpotResult.InsideObstructed;
        }

        /// <summary>
        ///     Draws a wire sphere gizmo at the position of the zone to represent it.
        ///     This is a burst-discarded method, meaning that it will not be compiled
        ///     by the Burst compiler, and will only be executed in the editor.
        /// </summary>
        [BurstDiscard] public void DrawGizmos(LayerMask raycastLayerMask)
        {
#if UNITY_EDITOR
            const int RESOLUTION = 180;

            UnsafeList<float3> targetPoints = new(RESOLUTION, Allocator.TempJob);
            float3 gizmosStartPoint = new(position, 0);
            float2 forward = new float2(0, 1);


            // Perform full circle rotation
            for (float drawAngle = 0f; drawAngle <= math.PI * 2f; drawAngle += math.PI * 2f / RESOLUTION)
            {
                float2 direction = MathExtensions.Rotate(forward, drawAngle);
                float3 point = new(position + direction * math.sqrt(radiusSq), 0);

                // Perform raycast
                RaycastHit2D raycastResult = Physics2D.Raycast(gizmosStartPoint.xy, direction, radius, raycastLayerMask);

                if (raycastResult.collider != null) point = new float3(raycastResult.point, 0);
                targetPoints.Add(point);
            }

            // Draw circle
            for (int n = 0; n < targetPoints.Length - 1; n++)
            {
                Gizmos.DrawLine(targetPoints[n], targetPoints[n + 1]);
            }

            // Draw finishing line to close the circle
            Gizmos.DrawLine(targetPoints[^1], targetPoints[0]);

            targetPoints.Dispose();
#endif
        }
    }
}