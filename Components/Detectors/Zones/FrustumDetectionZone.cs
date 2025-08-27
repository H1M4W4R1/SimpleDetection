using System.Runtime.CompilerServices;
using Systems.Detection2D.Data.Enums;
using Systems.Detection2D.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.Detection2D.Components.Detectors.Zones
{
    [BurstCompile] public readonly struct FrustumDetectionZone : IDetectionZone
    {
        private readonly float2 position;
        private readonly float2 forward;
        private readonly float angle;
        private readonly float radius;
        private readonly float radiusSq;

        public FrustumDetectionZone(float2 position, float2 forward, float angle, float radius)
        {
            this.position = position;
            this.forward = forward;
            this.angle = angle;
            this.radius = radius;
            radiusSq = radius * radius;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPointInZone(in float3 detectionPosition)
        {
            float2 detectedPosition = detectionPosition.xy;

            // Check if point is within radius
            if (math.lengthsq(detectedPosition - position) > radiusSq) return false;

            // Check if point is in front of the frustum
            float2 direction = math.normalize(detectedPosition - position);
            float dotProduct = math.dot(direction, forward);
            if (dotProduct < 0f) return false;

            // Check if point is within angle
            float angleCos = math.cos(angle);
            if (dotProduct < angleCos) return false;

            return true;
        }

        /// <summary>
        ///     Check if a point is seen (i.e. not obscured) from the zone.
        /// </summary>
        /// <param name="detectionPosition">Point to check</param>
        /// <param name="layerMask">Layer mask to use for raycast</param>
        /// <returns>true if point is seen, false otherwise</returns>
        /// <remarks>
        ///     This method first checks if the point is in the zone, and if not,
        ///     immediately returns false. Otherwise, it performs a raycast
        ///     from the point towards the position of the zone, and checks if
        ///     the raycast hit nothing. If nothing was hit, the point is seen.
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

        public void DrawGizmos(LayerMask raycastLayerMask)
        {
#if UNITY_EDITOR
            UnsafeList<float3> targetPoints = new(64, Allocator.TempJob);

            // Rotate direction by half angles to get side lines directions
            float2 rightDirection = MathExtensions.Rotate(forward, math.radians(angle * 0.5f));
            float2 leftDirection = MathExtensions.Rotate(forward, math.radians(-angle * 0.5f));

            float distance = math.sqrt(radiusSq);
            float3 rightPoint = new(position + rightDirection * distance, 0);
            float3 leftPoint = new(position + leftDirection * distance, 0);

            // Register side lines
            targetPoints.Add(leftPoint);
            targetPoints.Add(rightPoint);

            // Compute arc drawing parameters
            float startAngle = math.radians(-angle * 0.5f);
            float endAngle = math.radians(angle * 0.5f);
            float radianStep = math.radians(math.abs(endAngle - startAngle) / 8f);

            // Compute semi-circle (arc) points
            for (float drawAngle = startAngle; drawAngle <= endAngle; drawAngle += radianStep)
            {
                float2 direction = MathExtensions.Rotate(forward, drawAngle);
                float3 point = new(position + direction * distance, 0);
                targetPoints.Add(point);
            }

            float3 gizmosStartPoint = new(position, 0);

            // Perform raycasts
            for (int n = 0; n < targetPoints.Length; n++)
            {
                float2 raycastTarget = targetPoints[n].xy;
                float2 direction = math.normalize(raycastTarget - gizmosStartPoint.xy);
                RaycastHit2D raycastResult =
                    Physics2D.Raycast(gizmosStartPoint.xy, direction, distance, raycastLayerMask);

                if (raycastResult.point != Vector2.zero) targetPoints[n] = new float3(raycastResult.point, 0);
            }

            // Draw side lines
            Gizmos.DrawLine(gizmosStartPoint, targetPoints[0]);
            Gizmos.DrawLine(gizmosStartPoint, targetPoints[1]);

            // Draw arc
            for (int n = 2; n < targetPoints.Length - 1; n++)
            {
                Gizmos.DrawLine(targetPoints[n], targetPoints[n + 1]);
            }

            targetPoints.Dispose();
#endif
        }
    }
}